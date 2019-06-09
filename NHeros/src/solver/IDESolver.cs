using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using heros.edgefunc;

/// <summary>
///*****************************************************************************
/// Copyright (c) 2012 Eric Bodden.
/// Copyright (c) 2013 Tata Consultancy Services & Ecole Polytechnique de Montreal
/// All rights reserved. This program and the accompanying materials
/// are made available under the terms of the GNU Lesser Public License v2.1
/// which accompanies this distribution, and is available at
/// http://www.gnu.org/licenses/old-licenses/gpl-2.0.html
/// 
/// Contributors:
///     Eric Bodden - initial API and implementation
///     Marc-Andre Laverdiere-Papineau - Fixed race condition
///     John Toman - Adds edge recording
/// *****************************************************************************
/// </summary>
namespace heros.solver
{
	/// <summary>
	/// Solves the given <seealso cref="IDETabulationProblem"/> as described in the 1996 paper by Sagiv,
	/// Horwitz and Reps. To solve the problem, call <seealso cref="solve()"/>. Results can then be
	/// queried by using <seealso cref="resultAt(object, object)"/> and <seealso cref="resultsAt(object)"/>.
	/// 
	/// Note that this solver and its data structures internally use mostly <seealso cref="java.util.LinkedHashSet"/>s
	/// instead of normal <seealso cref="System.Collections.Generic.HashSet<object>"/>s to fix the iteration order as much as possible. This
	/// is to produce, as much as possible, reproducible benchmarking results. We have found
	/// that the iteration order can matter a lot in terms of speed.
	/// </summary>
	/// @param <N> The type of nodes in the interprocedural control-flow graph. </param>
	/// @param <D> The type of data-flow facts to be computed by the tabulation problem. </param>
	/// @param <M> The type of objects used to represent methods. </param>
	/// @param <V> The type of values to be computed along flow edges. </param>
	/// @param <I> The type of inter-procedural control-flow graph being used. </param>
	public class IDESolver<N, D, M, V, I> where I : heros.InterproceduralCFG<N, M>
	{
		public static CacheBuilder<object, object> DEFAULT_CACHE_BUILDER = CacheBuilder.newBuilder().concurrencyLevel(Runtime.Runtime.availableProcessors()).initialCapacity(10000).softValues();

		protected internal static readonly Logger logger = LoggerFactory.getLogger(typeof(IDESolver));

		[SynchronizedBy("consistent lock on field")]
		protected internal IDictionary<Pair<N, N>, IDictionary<D, ISet<D>>> computedIntraPEdges = HashBasedTable.create();

		[SynchronizedBy("consistent lock on field")]
		protected internal IDictionary<Pair<N, N>, IDictionary<D, ISet<D>>> computedInterPEdges = HashBasedTable.create();

		//enable with -Dorg.slf4j.simpleLogger.defaultLogLevel=trace
		public static bool DEBUG = logger.DebugEnabled;

		protected internal CountingThreadPoolExecutor executor;

		[DontSynchronize("only used by single thread")]
		protected internal int numThreads;

		[SynchronizedBy("thread safe data structure, consistent locking when used")]
		protected internal readonly JumpFunctions<N, D, V> jumpFn;

		[SynchronizedBy("thread safe data structure, only modified internally")]
		protected internal readonly I icfg;

		//stores summaries that were queried before they were computed
		//see CC 2010 paper by Naeem, Lhotak and Rodriguez
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		[SynchronizedBy("consistent lock on 'incoming'")]
		protected internal readonly Table<N, D, Table<N, D, EdgeFunction<V>>> endSummary_Conflict = HashBasedTable.create();

		//edges going along calls
		//see CC 2010 paper by Naeem, Lhotak and Rodriguez
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		[SynchronizedBy("consistent lock on field")]
		protected internal readonly Table<N, D, IDictionary<N, ISet<D>>> incoming_Conflict = HashBasedTable.create();

		//stores the return sites (inside callers) to which we have unbalanced returns
		//if followReturnPastSeeds is enabled
		[SynchronizedBy("use of ConcurrentHashMap")]
		protected internal readonly ISet<N> unbalancedRetSites;

		[DontSynchronize("stateless")]
		protected internal readonly FlowFunctions<N, D, M> flowFunctions;

		[DontSynchronize("stateless")]
		protected internal readonly EdgeFunctions<N, D, M, V> edgeFunctions;

		[DontSynchronize("only used by single thread")]
		protected internal readonly IDictionary<N, ISet<D>> initialSeeds;

		[DontSynchronize("stateless")]
		protected internal readonly JoinLattice<V> valueLattice;

		[DontSynchronize("stateless")]
		protected internal readonly EdgeFunction<V> allTop;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		[SynchronizedBy("consistent lock on field")]
		protected internal readonly Table<N, D, V> val_Conflict = HashBasedTable.create();

		[DontSynchronize("benign races")]
		public long flowFunctionApplicationCount;

		[DontSynchronize("benign races")]
		public long flowFunctionConstructionCount;

		[DontSynchronize("benign races")]
		public long propagationCount;

		[DontSynchronize("benign races")]
		public long durationFlowFunctionConstruction;

		[DontSynchronize("benign races")]
		public long durationFlowFunctionApplication;

		[DontSynchronize("stateless")]
		protected internal readonly D zeroValue;

		[DontSynchronize("readOnly")]
		protected internal readonly FlowFunctionCache<N, D, M> ffCache;

		[DontSynchronize("readOnly")]
		protected internal readonly EdgeFunctionCache<N, D, M, V> efCache;

		[DontSynchronize("readOnly")]
		protected internal readonly bool followReturnsPastSeeds;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		[DontSynchronize("readOnly")]
		protected internal readonly bool computeValues_Conflict;

		private bool recordEdges;

		/// <summary>
		/// Creates a solver for the given problem, which caches flow functions and edge functions.
		/// The solver must then be started by calling <seealso cref="solve()"/>.
		/// </summary>
		public IDESolver(IDETabulationProblem<N, D, M, V, I> tabulationProblem) : this(tabulationProblem, DEFAULT_CACHE_BUILDER, DEFAULT_CACHE_BUILDER)
		{
		}

		/// <summary>
		/// Creates a solver for the given problem, constructing caches with the given <seealso cref="CacheBuilder"/>. The solver must then be started by calling
		/// <seealso cref="solve()"/>. </summary>
		/// <param name="flowFunctionCacheBuilder"> A valid <seealso cref="CacheBuilder"/> or <code>null</code> if no caching is to be used for flow functions. </param>
		/// <param name="edgeFunctionCacheBuilder"> A valid <seealso cref="CacheBuilder"/> or <code>null</code> if no caching is to be used for edge functions. </param>
		public IDESolver(IDETabulationProblem<N, D, M, V, I> tabulationProblem, CacheBuilder flowFunctionCacheBuilder, CacheBuilder edgeFunctionCacheBuilder)
		{
			if (logger.DebugEnabled)
			{
				if (flowFunctionCacheBuilder != null)
				{
					flowFunctionCacheBuilder = flowFunctionCacheBuilder.recordStats();
				}
				if (edgeFunctionCacheBuilder != null)
				{
					edgeFunctionCacheBuilder = edgeFunctionCacheBuilder.recordStats();
				}
			}
			this.zeroValue = tabulationProblem.zeroValue();
			this.icfg = tabulationProblem.interproceduralCFG();
			FlowFunctions<N, D, M> flowFunctions = tabulationProblem.autoAddZero() ? new ZeroedFlowFunctions<N, D, M>(tabulationProblem.flowFunctions(), tabulationProblem.zeroValue()) : tabulationProblem.flowFunctions();
			EdgeFunctions<N, D, M, V> edgeFunctions = tabulationProblem.edgeFunctions();
			if (flowFunctionCacheBuilder != null)
			{
				ffCache = new FlowFunctionCache<N, D, M>(flowFunctions, flowFunctionCacheBuilder);
				flowFunctions = ffCache;
			}
			else
			{
				ffCache = null;
			}
			if (edgeFunctionCacheBuilder != null)
			{
				efCache = new EdgeFunctionCache<N, D, M, V>(edgeFunctions, edgeFunctionCacheBuilder);
				edgeFunctions = efCache;
			}
			else
			{
				efCache = null;
			}
			this.flowFunctions = flowFunctions;
			this.edgeFunctions = edgeFunctions;
			this.initialSeeds = tabulationProblem.initialSeeds();
			this.unbalancedRetSites = Collections.newSetFromMap(new ConcurrentDictionary<N, bool>());
			this.valueLattice = tabulationProblem.joinLattice();
			this.allTop = tabulationProblem.allTopFunction();
			this.jumpFn = new JumpFunctions<N, D, V>(allTop);
			this.followReturnsPastSeeds = tabulationProblem.followReturnsPastSeeds();
			this.numThreads = Math.Max(1,tabulationProblem.numThreads());
			this.computeValues_Conflict = tabulationProblem.computeValues();
			this.executor = Executor;
			this.recordEdges = tabulationProblem.recordEdges();
		}

		/// <summary>
		/// Runs the solver on the configured problem. This can take some time.
		/// </summary>
		public virtual void solve()
		{
			submitInitialSeeds();
			awaitCompletionComputeValuesAndShutdown();
		}

		/// <summary>
		/// Schedules the processing of initial seeds, initiating the analysis.
		/// Clients should only call this methods if performing synchronization on
		/// their own. Normally, <seealso cref="solve()"/> should be called instead.
		/// </summary>
		protected internal virtual void submitInitialSeeds()
		{
			foreach (KeyValuePair<N, ISet<D>> seed in initialSeeds.SetOfKeyValuePairs())
			{
				N startPoint = seed.Key;
				foreach (D val in seed.Value)
				{
					propagate(zeroValue, startPoint, val, EdgeIdentity.v<V>(), null, false);
				}
				jumpFn.addFunction(zeroValue, startPoint, zeroValue, EdgeIdentity.v<V>());
			}
		}

		/// <summary>
		/// Awaits the completion of the exploded super graph. When complete, computes result values,
		/// shuts down the executor and returns.
		/// </summary>
		protected internal virtual void awaitCompletionComputeValuesAndShutdown()
		{
			{

//ORIGINAL LINE: final long before = System.currentTimeMillis();
				long before = DateTimeHelper.CurrentUnixTimeMillis();
				//run executor and await termination of tasks
				runExecutorAndAwaitCompletion();
				durationFlowFunctionConstruction = DateTimeHelper.CurrentUnixTimeMillis() - before;
			}
			if (computeValues_Conflict)
			{

//ORIGINAL LINE: final long before = System.currentTimeMillis();
				long before = DateTimeHelper.CurrentUnixTimeMillis();
				computeValues();
				durationFlowFunctionApplication = DateTimeHelper.CurrentUnixTimeMillis() - before;
			}
			if (logger.DebugEnabled)
			{
				printStats();
			}

			//ask executor to shut down;
			//this will cause new submissions to the executor to be rejected,
			//but at this point all tasks should have completed anyway
			executor.shutdown();
			//similarly here: we await termination, but this should happen instantaneously,
			//as all tasks should have completed
			runExecutorAndAwaitCompletion();
		}

		/// <summary>
		/// Runs execution, re-throwing exceptions that might be thrown during its execution.
		/// </summary>
		private void runExecutorAndAwaitCompletion()
		{
			try
			{
				executor.awaitCompletion();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}

			Exception exception = executor.Exception;
			if (exception != null)
			{
				throw new Exception("There were exceptions during IDE analysis. Exiting.",exception);
			}
		}

		/// <summary>
		/// Dispatch the processing of a given edge. It may be executed in a different thread. </summary>
		/// <param name="edge"> the edge to process </param>
		protected internal virtual void scheduleEdgeProcessing(PathEdge<N, D> edge)
		{
			// If the executor has been killed, there is little point
			// in submitting new tasks
			if (executor.Terminating)
			{
				return;
			}
			executor.execute(new PathEdgeProcessingTask(this, edge));
			propagationCount++;
		}

		/// <summary>
		/// Dispatch the processing of a given value. It may be executed in a different thread. </summary>
		/// <param name="vpt"> </param>
		private void scheduleValueProcessing(ValuePropagationTask vpt)
		{
			// If the executor has been killed, there is little point
			// in submitting new tasks
			if (executor.Terminating)
			{
				return;
			}
			executor.execute(vpt);
		}

		/// <summary>
		/// Dispatch the computation of a given value. It may be executed in a different thread. </summary>
		/// <param name="task"> </param>
		private void scheduleValueComputationTask(ValueComputationTask task)
		{
			// If the executor has been killed, there is little point
			// in submitting new tasks
			if (executor.Terminating)
			{
				return;
			}
			executor.execute(task);
		}

		private void saveEdges(N sourceNode, N sinkStmt, D sourceVal, ISet<D> destVals, bool interP)
		{
			if (!this.recordEdges)
			{
				return;
			}

			Table<N, N, IDictionary<D, ISet<D>>> tgtMap = interP ? computedInterPEdges : computedIntraPEdges;
			lock (tgtMap)
			{
				IDictionary<D, ISet<D>> map = tgtMap.get(sourceNode, sinkStmt);
				if (map == null)
				{
					map = new Dictionary<D, ISet<D>>();
					tgtMap.put(sourceNode, sinkStmt, map);
				}
				map[sourceVal] = new HashSet<D>(destVals);
			}
		}

		/// <summary>
		/// Lines 13-20 of the algorithm; processing a call site in the caller's context.
		/// 
		/// For each possible callee, registers incoming call edges.
		/// Also propagates call-to-return flows and summarized callee flows within the caller. 
		/// </summary>
		/// <param name="edge"> an edge whose target node resembles a method call </param>
		private void processCall(PathEdge<N, D> edge)
		{
			D d1 = edge.factAtSource();
			N n = edge.Target; // a call node; line 14...

			logger.trace("Processing call to {}", n);

			D d2 = edge.factAtTarget();
			EdgeFunction<V> f = jumpFunction(edge);
			ICollection<N> returnSiteNs = icfg.getReturnSitesOfCallAt(n);

			//for each possible callee
			ICollection<M> callees = icfg.getCalleesOfCallAt(n);
			foreach (M sCalledProcN in callees)
			{ //still line 14

				//compute the call-flow function
				FlowFunction<D> function = flowFunctions.getCallFlowFunction(n, sCalledProcN);
				flowFunctionConstructionCount++;
				ISet<D> res = computeCallFlowFunction(function, d1, d2);
				//for each callee's start point(s)
				ICollection<N> startPointsOf = icfg.getStartPointsOf(sCalledProcN);
				foreach (N sP in startPointsOf)
				{
					saveEdges(n, sP, d2, res, true);
					//for each result node of the call-flow function
					foreach (D d3 in res)
					{
						//create initial self-loop
						propagate(d3, sP, d3, EdgeIdentity.v<V>(), n, false); //line 15

						//register the fact that <sp,d3> has an incoming edge from <n,d2>
						ISet<Table.Cell<N, D, EdgeFunction<V>>> endSumm;
						lock (incoming_Conflict)
						{
							//line 15.1 of Naeem/Lhotak/Rodriguez
							addIncoming(sP,d3,n,d2);
							//line 15.2, copy to avoid concurrent modification exceptions by other threads
							endSumm = new HashSet<Table.Cell<N, D, EdgeFunction<V>>>(endSummary(sP, d3));
						}

						//still line 15.2 of Naeem/Lhotak/Rodriguez
						//for each already-queried exit value <eP,d4> reachable from <sP,d3>,
						//create new caller-side jump functions to the return sites
						//because we have observed a potentially new incoming edge into <sP,d3>
						foreach (Table.Cell<N, D, EdgeFunction<V>> entry in endSumm)
						{
							N eP = entry.RowKey;
							D d4 = entry.ColumnKey;
							EdgeFunction<V> fCalleeSummary = entry.Value;
							//for each return site
							foreach (N retSiteN in returnSiteNs)
							{
								//compute return-flow function
								FlowFunction<D> retFunction = flowFunctions.getReturnFlowFunction(n, sCalledProcN, eP, retSiteN);
								flowFunctionConstructionCount++;
								ISet<D> returnedFacts = computeReturnFlowFunction(retFunction, d3, d4, n, Collections.singleton(d2));
								saveEdges(eP, retSiteN, d4, returnedFacts, true);
								//for each target value of the function
								foreach (D d5 in returnedFacts)
								{
									//update the caller-side summary function
									EdgeFunction<V> f4 = edgeFunctions.getCallEdgeFunction(n, d2, sCalledProcN, d3);
									EdgeFunction<V> f5 = edgeFunctions.getReturnEdgeFunction(n, sCalledProcN, eP, d4, retSiteN, d5);
									EdgeFunction<V> fPrime = f4.composeWith(fCalleeSummary).composeWith(f5);
									D d5_restoredCtx = restoreContextOnReturnedFact(n, d2, d5);
									propagate(d1, retSiteN, d5_restoredCtx, f.composeWith(fPrime), n, false);
								}
							}
						}
					}
				}
			}
			//line 17-19 of Naeem/Lhotak/Rodriguez		
			//process intra-procedural flows along call-to-return flow functions
			foreach (N returnSiteN in returnSiteNs)
			{
				FlowFunction<D> callToReturnFlowFunction = flowFunctions.getCallToReturnFlowFunction(n, returnSiteN);
				flowFunctionConstructionCount++;
				ISet<D> returnFacts = computeCallToReturnFlowFunction(callToReturnFlowFunction, d1, d2);
				saveEdges(n, returnSiteN, d2, returnFacts, false);
				foreach (D d3 in returnFacts)
				{
					EdgeFunction<V> edgeFnE = edgeFunctions.getCallToReturnEdgeFunction(n, d2, returnSiteN, d3);
					propagate(d1, returnSiteN, d3, f.composeWith(edgeFnE), n, false);
				}
			}
		}

		/// <summary>
		/// Computes the call flow function for the given call-site abstraction </summary>
		/// <param name="callFlowFunction"> The call flow function to compute </param>
		/// <param name="d1"> The abstraction at the current method's start node. </param>
		/// <param name="d2"> The abstraction at the call site </param>
		/// <returns> The set of caller-side abstractions at the callee's start node </returns>
		protected internal virtual ISet<D> computeCallFlowFunction(FlowFunction<D> callFlowFunction, D d1, D d2)
		{
			return callFlowFunction.computeTargets(d2);
		}

		/// <summary>
		/// Computes the call-to-return flow function for the given call-site
		/// abstraction </summary>
		/// <param name="callToReturnFlowFunction"> The call-to-return flow function to
		/// compute </param>
		/// <param name="d1"> The abstraction at the current method's start node. </param>
		/// <param name="d2"> The abstraction at the call site </param>
		/// <returns> The set of caller-side abstractions at the return site </returns>
		protected internal virtual ISet<D> computeCallToReturnFlowFunction(FlowFunction<D> callToReturnFlowFunction, D d1, D d2)
		{
			return callToReturnFlowFunction.computeTargets(d2);
		}

		/// <summary>
		/// Lines 21-32 of the algorithm.
		/// 
		/// Stores callee-side summaries.
		/// Also, at the side of the caller, propagates intra-procedural flows to return sites
		/// using those newly computed summaries.
		/// </summary>
		/// <param name="edge"> an edge whose target node resembles a method exits </param>
		protected internal virtual void processExit(PathEdge<N, D> edge)
		{
			N n = edge.Target; // an exit node; line 21...
			EdgeFunction<V> f = jumpFunction(edge);
			M methodThatNeedsSummary = icfg.getMethodOf(n);

			D d1 = edge.factAtSource();
			D d2 = edge.factAtTarget();

			//for each of the method's start points, determine incoming calls
			ICollection<N> startPointsOf = icfg.getStartPointsOf(methodThatNeedsSummary);
			IDictionary<N, ISet<D>> inc = new Dictionary<N, ISet<D>>();
			foreach (N sP in startPointsOf)
			{
				//line 21.1 of Naeem/Lhotak/Rodriguez

				//register end-summary
				lock (incoming_Conflict)
				{
					addEndSummary(sP, d1, n, d2, f);
					//copy to avoid concurrent modification exceptions by other threads
					foreach (KeyValuePair<N, ISet<D>> entry in incoming(d1, sP).SetOfKeyValuePairs())
					{
						inc[entry.Key] = new HashSet<D>(entry.Value);
					}
				}
			}

			//for each incoming call edge already processed
			//(see processCall(..))
			foreach (KeyValuePair<N, ISet<D>> entry in inc.SetOfKeyValuePairs())
			{
				//line 22
				N c = entry.Key;
				//for each return site
				foreach (N retSiteC in icfg.getReturnSitesOfCallAt(c))
				{
					//compute return-flow function
					FlowFunction<D> retFunction = flowFunctions.getReturnFlowFunction(c, methodThatNeedsSummary,n,retSiteC);
					flowFunctionConstructionCount++;
					//for each incoming-call value
					foreach (D d4 in entry.Value)
					{
						ISet<D> targets = computeReturnFlowFunction(retFunction, d1, d2, c, entry.Value);
						saveEdges(n, retSiteC, d2, targets, true);
						//for each target value at the return site
						//line 23
						foreach (D d5 in targets)
						{
							//compute composed function
							EdgeFunction<V> f4 = edgeFunctions.getCallEdgeFunction(c, d4, icfg.getMethodOf(n), d1);
							EdgeFunction<V> f5 = edgeFunctions.getReturnEdgeFunction(c, icfg.getMethodOf(n), n, d2, retSiteC, d5);
							EdgeFunction<V> fPrime = f4.composeWith(f).composeWith(f5);
							//for each jump function coming into the call, propagate to return site using the composed function
							lock (jumpFn)
							{ // some other thread might change jumpFn on the way
								foreach (KeyValuePair<D, EdgeFunction<V>> valAndFunc in jumpFn.reverseLookup(c,d4).SetOfKeyValuePairs())
								{
									EdgeFunction<V> f3 = valAndFunc.Value;
									if (!f3.equalTo(allTop))
									{
										D d3 = valAndFunc.Key;
										D d5_restoredCtx = restoreContextOnReturnedFact(c, d4, d5);
										propagate(d3, retSiteC, d5_restoredCtx, f3.composeWith(fPrime), c, false);
									}
								}
							}
						}
					}
				}
			}

			//handling for unbalanced problems where we return out of a method with a fact for which we have no incoming flow
			//note: we propagate that way only values that originate from ZERO, as conditionally generated values should only
			//be propagated into callers that have an incoming edge for this condition
			if (followReturnsPastSeeds && inc.Count == 0 && d1.Equals(zeroValue))
			{
				// only propagate up if we 
					ICollection<N> callers = icfg.getCallersOf(methodThatNeedsSummary);
					foreach (N c in callers)
					{
						foreach (N retSiteC in icfg.getReturnSitesOfCallAt(c))
						{
							FlowFunction<D> retFunction = flowFunctions.getReturnFlowFunction(c, methodThatNeedsSummary,n,retSiteC);
							flowFunctionConstructionCount++;
							ISet<D> targets = computeReturnFlowFunction(retFunction, d1, d2, c, Collections.singleton(zeroValue));
							saveEdges(n, retSiteC, d2, targets, true);
							foreach (D d5 in targets)
							{
								EdgeFunction<V> f5 = edgeFunctions.getReturnEdgeFunction(c, icfg.getMethodOf(n), n, d2, retSiteC, d5);
								propagateUnbalancedReturnFlow(retSiteC, d5, f.composeWith(f5), c);
								//register for value processing (2nd IDE phase)
								unbalancedRetSites.Add(retSiteC);
							}
						}
					}
					//in cases where there are no callers, the return statement would normally not be processed at all;
					//this might be undesirable if the flow function has a side effect such as registering a taint;
					//instead we thus call the return flow function will a null caller
					if (callers.Count == 0)
					{
						FlowFunction<D> retFunction = flowFunctions.getReturnFlowFunction(default(N), methodThatNeedsSummary, n, default(N));
						flowFunctionConstructionCount++;
						retFunction.computeTargets(d2);
					}
			}
		}

		protected internal virtual void propagateUnbalancedReturnFlow(N retSiteC, D targetVal, EdgeFunction<V> edgeFunction, N relatedCallSite)
		{
			propagate(zeroValue, retSiteC, targetVal, edgeFunction, relatedCallSite, true);
		}

		/// <summary>
		/// This method will be called for each incoming edge and can be used to
		/// transfer knowledge from the calling edge to the returning edge, without
		/// affecting the summary edges at the callee. </summary>
		/// <param name="callSite"> 
		/// </param>
		/// <param name="d4">
		///            Fact stored with the incoming edge, i.e., present at the
		///            caller side </param>
		/// <param name="d5">
		///            Fact that originally should be propagated to the caller. </param>
		/// <returns> Fact that will be propagated to the caller. </returns>

//ORIGINAL LINE: @SuppressWarnings("unchecked") protected D restoreContextOnReturnedFact(N callSite, D d4, D d5)
		protected internal virtual D restoreContextOnReturnedFact(N callSite, D d4, D d5)
		{
			if (d5 is LinkedNode)
			{
				((LinkedNode<D>) d5).CallingContext = d4;
			}
			if (d5 is JoinHandlingNode)
			{
				((JoinHandlingNode<D>) d5).CallingContext = d4;
			}
			return d5;
		}

		/// <summary>
		/// Computes the return flow function for the given set of caller-side
		/// abstractions. </summary>
		/// <param name="retFunction"> The return flow function to compute </param>
		/// <param name="d1"> The abstraction at the beginning of the callee </param>
		/// <param name="d2"> The abstraction at the exit node in the callee </param>
		/// <param name="callSite"> The call site </param>
		/// <param name="callerSideDs"> The abstractions at the call site </param>
		/// <returns> The set of caller-side abstractions at the return site </returns>
		protected internal virtual ISet<D> computeReturnFlowFunction(FlowFunction<D> retFunction, D d1, D d2, N callSite, ISet<D> callerSideDs)
		{
			return retFunction.computeTargets(d2);
		}

		/// <summary>
		/// Lines 33-37 of the algorithm.
		/// Simply propagate normal, intra-procedural flows. </summary>
		/// <param name="edge"> </param>
		private void processNormalFlow(PathEdge<N, D> edge)
		{

//ORIGINAL LINE: final D d1 = edge.factAtSource();
			D d1 = edge.factAtSource();

//ORIGINAL LINE: final N n = edge.getTarget();
			N n = edge.Target;

//ORIGINAL LINE: final D d2 = edge.factAtTarget();
			D d2 = edge.factAtTarget();

			EdgeFunction<V> f = jumpFunction(edge);
			foreach (N m in icfg.getSuccsOf(n))
			{
				FlowFunction<D> flowFunction = flowFunctions.getNormalFlowFunction(n,m);
				flowFunctionConstructionCount++;
				ISet<D> res = computeNormalFlowFunction(flowFunction, d1, d2);
				saveEdges(n, m, d2, res, false);
				foreach (D d3 in res)
				{
					EdgeFunction<V> fprime = f.composeWith(edgeFunctions.getNormalEdgeFunction(n, d2, m, d3));
					propagate(d1, m, d3, fprime, null, false);
				}
			}
		}

		/// <summary>
		/// Computes the normal flow function for the given set of start and end
		/// abstractions- </summary>
		/// <param name="flowFunction"> The normal flow function to compute </param>
		/// <param name="d1"> The abstraction at the method's start node </param>
		/// <param name="d1"> The abstraction at the current node </param>
		/// <returns> The set of abstractions at the successor node </returns>
		protected internal virtual ISet<D> computeNormalFlowFunction(FlowFunction<D> flowFunction, D d1, D d2)
		{
			return flowFunction.computeTargets(d2);
		}

		/// <summary>
		/// Propagates the flow further down the exploded super graph, merging any edge function that might
		/// already have been computed for targetVal at target. </summary>
		/// <param name="sourceVal"> the source value of the propagated summary edge </param>
		/// <param name="target"> the target statement </param>
		/// <param name="targetVal"> the target value at the target statement </param>
		/// <param name="f"> the new edge function computed from (s0,sourceVal) to (target,targetVal) </param>
		/// <param name="relatedCallSite"> for call and return flows the related call statement, <code>null</code> otherwise
		///        (this value is not used within this implementation but may be useful for subclasses of <seealso cref="IDESolver"/>) </param>
		/// <param name="isUnbalancedReturn"> <code>true</code> if this edge is propagating an unbalanced return
		///        (this value is not used within this implementation but may be useful for subclasses of <seealso cref="IDESolver"/>)  </param>
		protected internal virtual void propagate(D sourceVal, N target, D targetVal, EdgeFunction<V> f, N relatedCallSite, bool isUnbalancedReturn)
		{
			EdgeFunction<V> jumpFnE;
			EdgeFunction<V> fPrime;
			bool newFunction;
			lock (jumpFn)
			{
				jumpFnE = jumpFn.reverseLookup(target, targetVal)[sourceVal];
				if (jumpFnE == null)
				{
					jumpFnE = allTop; //JumpFn is initialized to all-top (see line [2] in SRH96 paper)
				}
				fPrime = jumpFnE.joinWith(f);
				newFunction = !fPrime.equalTo(jumpFnE);
				if (newFunction)
				{
					jumpFn.addFunction(sourceVal, target, targetVal, fPrime);
				}
			}

			if (newFunction)
			{
				PathEdge<N, D> edge = new PathEdge<N, D>(sourceVal, target, targetVal);
				scheduleEdgeProcessing(edge);

				if (targetVal != zeroValue)
				{
					logger.trace("{} - EDGE: <{},{}> -> <{},{}> - {}", DebugName, icfg.getMethodOf(target), sourceVal, target, targetVal, fPrime);
				}
			}
		}

		/// <summary>
		/// Computes the final values for edge functions.
		/// </summary>
		private void computeValues()
		{
			//Phase II(i)
			logger.debug("Computing the final values for the edge functions");
			//add caller seeds to initial seeds in an unbalanced problem
			IDictionary<N, ISet<D>> allSeeds = new Dictionary<N, ISet<D>>(initialSeeds);
			foreach (N unbalancedRetSite in unbalancedRetSites)
			{
				ISet<D> seeds = allSeeds[unbalancedRetSite];
				if (seeds == null)
				{
					seeds = new HashSet<D>();
					allSeeds[unbalancedRetSite] = seeds;
				}
				seeds.Add(zeroValue);
			}
			//do processing
			foreach (KeyValuePair<N, ISet<D>> seed in allSeeds.SetOfKeyValuePairs())
			{
				N startPoint = seed.Key;
				foreach (D val in seed.Value)
				{
					setVal(startPoint, val, valueLattice.bottomElement());
					Pair<N, D> superGraphNode = new Pair<N, D>(startPoint, val);
					scheduleValueProcessing(new ValuePropagationTask(this, superGraphNode));
				}
			}
			logger.debug("Computed the final values of the edge functions");
			//await termination of tasks
			try
			{
				executor.awaitCompletion();
			}
			catch (InterruptedException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}

			//Phase II(ii)
			//we create an array of all nodes and then dispatch fractions of this array to multiple threads
			ISet<N> allNonCallStartNodes = icfg.allNonCallStartNodes();

//ORIGINAL LINE: @SuppressWarnings("unchecked") N[] nonCallStartNodesArray = (N[]) new Object[allNonCallStartNodes.size()];
			N[] nonCallStartNodesArray = (N[]) new object[allNonCallStartNodes.Count];
			int i = 0;
			foreach (N n in allNonCallStartNodes)
			{
				nonCallStartNodesArray[i] = n;
				i++;
			}
			//No need to keep track of the number of tasks scheduled here, since we call shutdown
			for (int t = 0;t < numThreads; t++)
			{
				ValueComputationTask task = new ValueComputationTask(this, nonCallStartNodesArray, t);
				scheduleValueComputationTask(task);
			}
			//await termination of tasks
			try
			{
				executor.awaitCompletion();
			}
			catch (InterruptedException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
		}

		private void propagateValueAtStart(Pair<N, D> nAndD, N n)
		{
			D d = nAndD.O2;
			M p = icfg.getMethodOf(n);
			foreach (N c in icfg.getCallsFromWithin(p))
			{
				ISet<KeyValuePair<D, EdgeFunction<V>>> entries;
				lock (jumpFn)
				{
					entries = jumpFn.forwardLookup(d,c).SetOfKeyValuePairs();
					foreach (KeyValuePair<D, EdgeFunction<V>> dPAndFP in entries)
					{
						D dPrime = dPAndFP.Key;
						EdgeFunction<V> fPrime = dPAndFP.Value;
						N sP = n;
						propagateValue(c,dPrime,fPrime.computeTarget(val(sP,d)));
						flowFunctionApplicationCount++;
					}
				}
			}
		}

		private void propagateValueAtCall(Pair<N, D> nAndD, N n)
		{
			D d = nAndD.O2;
			foreach (M q in icfg.getCalleesOfCallAt(n))
			{
				FlowFunction<D> callFlowFunction = flowFunctions.getCallFlowFunction(n, q);
				flowFunctionConstructionCount++;
				foreach (D dPrime in callFlowFunction.computeTargets(d))
				{
					EdgeFunction<V> edgeFn = edgeFunctions.getCallEdgeFunction(n, d, q, dPrime);
					foreach (N startPoint in icfg.getStartPointsOf(q))
					{
						propagateValue(startPoint,dPrime, edgeFn.computeTarget(val(n,d)));
						flowFunctionApplicationCount++;
					}
				}
			}
		}

		protected internal virtual V joinValueAt(N unit, D fact, V curr, V newVal)
		{
			return valueLattice.join(curr, newVal);
		}

		private void propagateValue(N nHashN, D nHashD, V v)
		{
			lock (val_Conflict)
			{
				V valNHash = val(nHashN, nHashD);
				V vPrime = joinValueAt(nHashN, nHashD, valNHash,v);
				if (!vPrime.Equals(valNHash))
				{
					setVal(nHashN, nHashD, vPrime);
					scheduleValueProcessing(new ValuePropagationTask(this, new Pair<N, D>(nHashN,nHashD)));
				}
			}
		}

		private V val(N nHashN, D nHashD)
		{
			V l;
			lock (val_Conflict)
			{
				l = val_Conflict.get(nHashN, nHashD);
			}
			if (Utils.IsDefault(l))
			{
				return valueLattice.topElement(); //implicitly initialized to top; see line [1] of Fig. 7 in SRH96 paper
			}
			else
			{
				return l;
			}
		}

		private void setVal(N nHashN, D nHashD, V l)
		{
			// TOP is the implicit default value which we do not need to store.
			lock (val_Conflict)
			{
				if (l == valueLattice.topElement()) // do not store top values
				{
					val_Conflict.remove(nHashN, nHashD);
				}
				else
				{
					val_Conflict.put(nHashN, nHashD,l);
				}
			}
			logger.debug("VALUE: {} {} {} {}", icfg.getMethodOf(nHashN), nHashN, nHashD, l);
		}

		private EdgeFunction<V> jumpFunction(PathEdge<N, D> edge)
		{
			lock (jumpFn)
			{
				EdgeFunction<V> function = jumpFn.forwardLookup(edge.factAtSource(), edge.Target)[edge.factAtTarget()];
				if (function == null)
				{
					return allTop; //JumpFn initialized to all-top, see line [2] in SRH96 paper
				}
				return function;
			}
		}

		protected internal virtual ISet<Table.Cell<N, D, EdgeFunction<V>>> endSummary(N sP, D d3)
		{
			Table<N, D, EdgeFunction<V>> map = endSummary_Conflict.get(sP, d3);
			if (map == null)
			{
				return Collections.emptySet();
			}
			return map.cellSet();
		}

		private void addEndSummary(N sP, D d1, N eP, D d2, EdgeFunction<V> f)
		{
			Table<N, D, EdgeFunction<V>> summaries = endSummary_Conflict.get(sP, d1);
			if (summaries == null)
			{
				summaries = HashBasedTable.create();
				endSummary_Conflict.put(sP, d1, summaries);
			}
			//note: at this point we don't need to join with a potential previous f
			//because f is a jump function, which is already properly joined
			//within propagate(..)
			summaries.put(eP,d2,f);
		}

		protected internal virtual IDictionary<N, ISet<D>> incoming(D d1, N sP)
		{
			lock (incoming_Conflict)
			{
				IDictionary<N, ISet<D>> map = incoming_Conflict.get(sP, d1);
				if (map == null)
				{
					return Collections.emptyMap();
				}
				return map;
			}
		}

		protected internal virtual void addIncoming(N sP, D d3, N n, D d2)
		{
			lock (incoming_Conflict)
			{
				IDictionary<N, ISet<D>> summaries = incoming_Conflict.get(sP, d3);
				if (summaries == null)
				{
					summaries = new Dictionary<N, ISet<D>>();
					incoming_Conflict.put(sP, d3, summaries);
				}
				ISet<D> set = summaries[n];
				if (set == null)
				{
					set = new HashSet<D>();
					summaries[n] = set;
				}
				set.Add(d2);
			}
		}

		/// <summary>
		/// Returns the V-type result for the given value at the given statement.
		/// TOP values are never returned.
		/// </summary>
		public virtual V resultAt(N stmt, D value)
		{
			//no need to synchronize here as all threads are known to have terminated
			return val_Conflict.get(stmt, value);
		}

		/// <summary>
		/// Returns the resulting environment for the given statement.
		/// The artificial zero value is automatically stripped. TOP values are
		/// never returned.
		/// </summary>
		public virtual IDictionary<D, V> resultsAt(N stmt)
		{
			//filter out the artificial zero-value
			//no need to synchronize here as all threads are known to have terminated
			return Maps.filterKeys(val_Conflict.row(stmt), new PredicateAnonymousInnerClass(this));
		}

		private class PredicateAnonymousInnerClass : Predicate<D>
		{
			private readonly IDESolver<N, D, M, V, I> outerInstance;

			public PredicateAnonymousInnerClass(IDESolver<N, D, M, V, I> outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public bool apply(D val)
			{
				return val != outerInstance.zeroValue;
			}
		}

		/// <summary>
		/// Factory method for this solver's thread-pool executor.
		/// </summary>
		protected internal virtual CountingThreadPoolExecutor Executor
		{
			get
			{
				return new CountingThreadPoolExecutor(1, this.numThreads, 30, TimeUnit.SECONDS, new LinkedBlockingQueue<ThreadStart>());
			}
		}

		/// <summary>
		/// Returns a String used to identify the output of this solver in debug mode.
		/// Subclasses can overwrite this string to distinguish the output from different solvers.
		/// </summary>
		protected internal virtual string DebugName
		{
			get
			{
				return "";
			}
		}

		public virtual void printStats()
		{
			if (logger.DebugEnabled)
			{
				if (ffCache != null)
				{
					ffCache.printStats();
				}
				if (efCache != null)
				{
					efCache.printStats();
				}
			}
			else
			{
				logger.info("No statistics were collected, as DEBUG is disabled.");
			}
		}

		private class PathEdgeProcessingTask : ThreadStart
		{
			private readonly IDESolver<N, D, M, V, I> outerInstance;

			internal readonly PathEdge<N, D> edge;

			public PathEdgeProcessingTask(IDESolver<N, D, M, V, I> outerInstance, PathEdge<N, D> edge)
			{
				this.outerInstance = outerInstance;
				this.edge = edge;
			}

			public virtual void run()
			{
				if (outerInstance.icfg.isCallStmt(edge.Target))
				{
					outerInstance.processCall(edge);
				}
				else
				{
					//note that some statements, such as "throw" may be
					//both an exit statement and a "normal" statement
					if (outerInstance.icfg.isExitStmt(edge.Target))
					{
						outerInstance.processExit(edge);
					}
					if (!outerInstance.icfg.getSuccsOf(edge.Target).Empty)
					{
						outerInstance.processNormalFlow(edge);
					}
				}
			}
		}

		private class ValuePropagationTask : ThreadStart
		{
			private readonly IDESolver<N, D, M, V, I> outerInstance;

			internal readonly Pair<N, D> nAndD;

			public ValuePropagationTask(IDESolver<N, D, M, V, I> outerInstance, Pair<N, D> nAndD)
			{
				this.outerInstance = outerInstance;
				this.nAndD = nAndD;
			}

			public virtual void run()
			{
				N n = nAndD.O1;
				if (outerInstance.icfg.isStartPoint(n) || outerInstance.initialSeeds.ContainsKey(n) || outerInstance.unbalancedRetSites.Contains(n))
				{ //the same also for unbalanced return sites in an unbalanced problem
					outerInstance.propagateValueAtStart(nAndD, n);
				}
				if (outerInstance.icfg.isCallStmt(n))
				{
					outerInstance.propagateValueAtCall(nAndD, n);
				}
			}
		}

		private class ValueComputationTask : ThreadStart
		{
			private readonly IDESolver<N, D, M, V, I> outerInstance;

			internal readonly N[] values;
			internal readonly int num;

			public ValueComputationTask(IDESolver<N, D, M, V, I> outerInstance, N[] values, int num)
			{
				this.outerInstance = outerInstance;
				this.values = values;
				this.num = num;
			}

			public virtual void run()
			{
				int sectionSize = (int) Math.Floor(values.Length / outerInstance.numThreads) + outerInstance.numThreads;
				for (int i = sectionSize * num; i < Math.Min(sectionSize * (num + 1),values.Length); i++)
				{
					N n = values[i];
					foreach (N sP in outerInstance.icfg.getStartPointsOf(outerInstance.icfg.getMethodOf(n)))
					{
						ISet<Table.Cell<D, D, EdgeFunction<V>>> lookupByTarget;
						lookupByTarget = outerInstance.jumpFn.lookupByTarget(n);
						foreach (Table.Cell<D, D, EdgeFunction<V>> sourceValTargetValAndFunction in lookupByTarget)
						{
							D dPrime = sourceValTargetValAndFunction.RowKey;
							D d = sourceValTargetValAndFunction.ColumnKey;
							EdgeFunction<V> fPrime = sourceValTargetValAndFunction.Value;
							lock (outerInstance.val_Conflict)
							{
								outerInstance.setVal(n,d,outerInstance.valueLattice.join(outerInstance.val(n,d),fPrime.computeTarget(outerInstance.val(sP,dPrime))));
							}
							outerInstance.flowFunctionApplicationCount++;
						}
					}
				}
			}
		}

	}

}