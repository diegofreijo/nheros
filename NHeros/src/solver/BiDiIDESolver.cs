using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

/// <summary>
///*****************************************************************************
/// Copyright (c) 2012 Eric Bodden.
/// All rights reserved. This program and the accompanying materials
/// are made available under the terms of the GNU Lesser Public License v2.1
/// which accompanies this distribution, and is available at
/// http://www.gnu.org/licenses/old-licenses/gpl-2.0.html
/// 
/// Contributors:
///     Eric Bodden - initial API and implementation
/// *****************************************************************************
/// </summary>
namespace heros.solver
{
	using BinaryDomain = heros.solver.IFDSSolver.BinaryDomain;


	using Maps = com.google.common.collect.Maps;

	/// <summary>
	/// This is a special IFDS solver that solves the analysis problem inside out, i.e., from further down the call stack to
	/// further up the call stack. This can be useful, for instance, for taint analysis problems that track flows in two directions.
	/// 
	/// The solver is instantiated with two analyses, one to be computed forward and one to be computed backward. Both analysis problems
	/// must be unbalanced, i.e., must return <code>true</code> for <seealso cref="IFDSTabulationProblem.followReturnsPastSeeds()"/>.
	/// The solver then executes both analyses in lockstep, i.e., when one of the analyses reaches an unbalanced return edge (signified
	/// by a ZERO source value) then the solver pauses this analysis until the other analysis reaches the same unbalanced return (if ever).
	/// The result is that the analyses will never diverge, i.e., will ultimately always only propagate into contexts in which both their
	/// computed paths are realizable at the same time.
	/// 
	/// This solver requires data-flow abstractions that implement the <seealso cref="LinkedNode"/> interface such that data-flow values can be linked to form
	/// reportable paths.  
	/// </summary>
	/// @param <N> see <seealso cref="IFDSSolver"/> </param>
	/// @param <D> A data-flow abstraction that must implement the <seealso cref="LinkedNode"/> interface such that data-flow values can be linked to form
	/// 				reportable paths. </param>
	/// @param <M> see <seealso cref="IFDSSolver"/> </param>
	/// @param <I> see <seealso cref="IFDSSolver"/> </param>
	public class BiDiIDESolver<N, D, M, V, I> where I : heros.InterproceduralCFG<N, M>
	{

		private readonly IDETabulationProblem<N, AbstractionWithSourceStmt, M, V, I> forwardProblem;
		private readonly IDETabulationProblem<N, AbstractionWithSourceStmt, M, V, I> backwardProblem;
		private readonly CountingThreadPoolExecutor sharedExecutor;
		protected internal SingleDirectionSolver fwSolver;
		protected internal SingleDirectionSolver bwSolver;

		/// <summary>
		/// Instantiates a <seealso cref="BiDiIDESolver"/> with the associated forward and backward problem.
		/// </summary>
		public BiDiIDESolver(IDETabulationProblem<N, D, M, V, I> forwardProblem, IDETabulationProblem<N, D, M, V, I> backwardProblem)
		{
			if (!forwardProblem.followReturnsPastSeeds() || !backwardProblem.followReturnsPastSeeds())
			{
				throw new System.ArgumentException("This solver is only meant for bottom-up problems, so followReturnsPastSeeds() should return true.");
			}
			this.forwardProblem = new AugmentedTabulationProblem(this, forwardProblem);
			this.backwardProblem = new AugmentedTabulationProblem(this, backwardProblem);
			this.sharedExecutor = new CountingThreadPoolExecutor(1, Math.Max(1,forwardProblem.numThreads()), 30, TimeUnit.SECONDS, new LinkedBlockingQueue<ThreadStart>());
		}

		public virtual void solve()
		{
			fwSolver = createSingleDirectionSolver(forwardProblem, "FW");
			bwSolver = createSingleDirectionSolver(backwardProblem, "BW");
			fwSolver.otherSolver = bwSolver;
			bwSolver.otherSolver = fwSolver;

			//start the bw solver
			bwSolver.submitInitialSeeds();

			//start the fw solver and block until both solvers have completed
			//(note that they both share the same executor, see below)
			//note to self: the order of the two should not matter
			fwSolver.solve();
		}

		/// <summary>
		/// Creates a solver to be used for each single analysis direction.
		/// </summary>
		protected internal virtual SingleDirectionSolver createSingleDirectionSolver(IDETabulationProblem<N, AbstractionWithSourceStmt, M, V, I> problem, string debugName)
		{
			return new SingleDirectionSolver(this, problem, debugName);
		}

		private class PausedEdge
		{
			private readonly BiDiIDESolver<N, D, M, V, I> outerInstance;

			internal N retSiteC;
			internal AbstractionWithSourceStmt targetVal;
			internal EdgeFunction<V> edgeFunction;
			internal N relatedCallSite;

			public PausedEdge(BiDiIDESolver<N, D, M, V, I> outerInstance, N retSiteC, AbstractionWithSourceStmt targetVal, EdgeFunction<V> edgeFunction, N relatedCallSite)
			{
				this.outerInstance = outerInstance;
				this.retSiteC = retSiteC;
				this.targetVal = targetVal;
				this.edgeFunction = edgeFunction;
				this.relatedCallSite = relatedCallSite;
			}
		}

		/// <summary>
		///  Data structure used to identify which edges can be unpaused by a <seealso cref="SingleDirectionSolver"/>. Each <seealso cref="SingleDirectionSolver"/> stores 
		///  its leaks using this structure. A leak always requires a flow from some <seealso cref="sourceStmt"/> (this is either the statement used as initial seed
		///  or a call site of an unbalanced return) to a return site. This return site is always different for the forward and backward solvers,
		///  but, the related call site of these return sites must be the same, if two entangled flows exist. 
		///  Moreover, this structure represents the pair of such a <seealso cref="sourceStmt"/> and the <seealso cref="relatedCallSite"/>.
		/// 
		/// </summary>
		private class LeakKey<N>
		{
			internal N sourceStmt;
			internal N relatedCallSite;

			public LeakKey(N sourceStmt, N relatedCallSite)
			{
				this.sourceStmt = sourceStmt;
				this.relatedCallSite = relatedCallSite;
			}

			public override int GetHashCode()
			{
				const int prime = 31;
				int result = 1;
				result = prime * result + ((relatedCallSite == default(N)) ? 0 : relatedCallSite.GetHashCode());
				result = prime * result + ((sourceStmt == default(N)) ? 0 : sourceStmt.GetHashCode());
				return result;
			}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("rawtypes") @Override public boolean equals(Object obj)
			public override bool Equals(object obj)
			{
				if (this == obj)
				{
					return true;
				}
				if (obj == null)
				{
					return false;
				}
				if (!(obj is LeakKey))
				{
					return false;
				}
				LeakKey other = (LeakKey) obj;
				if (relatedCallSite == default(N))
				{
					if (other.relatedCallSite != default(N))
					{
						return false;
					}
				}
				else if (!relatedCallSite.Equals(other.relatedCallSite))
				{
					return false;
				}
				if (sourceStmt == default(N))
				{
					if (other.sourceStmt != default(N))
					{
						return false;
					}
				}
				else if (!sourceStmt.Equals(other.sourceStmt))
				{
					return false;
				}
				return true;
			}
		}

		/// <summary>
		/// This is a modified IFDS solver that is capable of pausing and unpausing return-flow edges.
		/// </summary>
		protected internal class SingleDirectionSolver : IDESolver<N, AbstractionWithSourceStmt, M, V, I>
		{
			private readonly BiDiIDESolver<N, D, M, V, I> outerInstance;

			internal readonly string debugName;
			internal SingleDirectionSolver otherSolver;
			internal ISet<LeakKey<N>> leakedSources = Collections.newSetFromMap(Maps.newConcurrentMap<LeakKey<N>, bool>());
			internal ConcurrentMap<LeakKey<N>, ISet<PausedEdge>> pausedPathEdges = Maps.newConcurrentMap();

			public SingleDirectionSolver(BiDiIDESolver<N, D, M, V, I> outerInstance, IDETabulationProblem<N, AbstractionWithSourceStmt, M, V, I> ifdsProblem, string debugName) : base(ifdsProblem)
			{
				this.outerInstance = outerInstance;
				this.debugName = debugName;
			}

			protected internal override void propagateUnbalancedReturnFlow(N retSiteC, AbstractionWithSourceStmt targetVal, EdgeFunction<V> edgeFunction, N relatedCallSite)
			{
				//if an edge is originating from ZERO then to us this signifies an unbalanced return edge
				N sourceStmt = targetVal.SourceStmt;
				//we mark the fact that this solver would like to "leak" this edge to the caller
				LeakKey<N> leakKey = new LeakKey<N>(sourceStmt, relatedCallSite);
				leakedSources.Add(leakKey);
				if (otherSolver.hasLeaked(leakKey))
				{
					//if the other solver has leaked already then unpause its edges and continue
					otherSolver.unpausePathEdgesForSource(leakKey);
					base.propagateUnbalancedReturnFlow(retSiteC, targetVal, edgeFunction, relatedCallSite);
				}
				else
				{
					//otherwise we pause this solver's edge and don't continue
					ISet<PausedEdge> newPausedEdges = Collections.newSetFromMap(Maps.newConcurrentMap<PausedEdge, bool>());
					ISet<PausedEdge> existingPausedEdges = pausedPathEdges.putIfAbsent(leakKey, newPausedEdges);
					if (existingPausedEdges == null)
					{
						existingPausedEdges = newPausedEdges;
					}

					PausedEdge edge = new PausedEdge(outerInstance, retSiteC, targetVal, edgeFunction, relatedCallSite);
					existingPausedEdges.Add(edge);

					//if the other solver has leaked in the meantime, we have to make sure that the paused edge is unpaused
					if (otherSolver.hasLeaked(leakKey) && existingPausedEdges.remove(edge))
					{
						base.propagateUnbalancedReturnFlow(retSiteC, targetVal, edgeFunction, relatedCallSite);
					}

					logger.debug(" ++ PAUSE {}: {}", debugName, edge);
				}
			}

			protected internal override void propagate(AbstractionWithSourceStmt sourceVal, N target, AbstractionWithSourceStmt targetVal, EdgeFunction<V> f, N relatedCallSite, bool isUnbalancedReturn)
			{
				//the follwing branch will be taken only on an unbalanced return
				if (isUnbalancedReturn)
				{
					Debug.Assert(sourceVal.SourceStmt == default(N), "source value should have no statement attached");

					//attach target statement as new "source" statement to track
					targetVal = new AbstractionWithSourceStmt(outerInstance, targetVal.Abstraction, relatedCallSite);

					base.propagate(sourceVal, target, targetVal, f, relatedCallSite, isUnbalancedReturn);
				}
				else
				{
					base.propagate(sourceVal, target, targetVal, f, relatedCallSite, isUnbalancedReturn);
				}
			}

			protected internal override AbstractionWithSourceStmt restoreContextOnReturnedFact(N callSite, AbstractionWithSourceStmt d4, AbstractionWithSourceStmt d5)
			{
				return new AbstractionWithSourceStmt(outerInstance, d5.Abstraction, d4.SourceStmt);
			}

			/// <summary>
			/// Returns <code>true</code> if this solver has tried to leak an edge originating from the given source
			/// to its caller.
			/// </summary>
			internal virtual bool hasLeaked(LeakKey<N> leakKey)
			{
				return leakedSources.Contains(leakKey);
			}

			/// <summary>
			/// Unpauses all edges associated with the given source statement.
			/// </summary>
			internal virtual void unpausePathEdgesForSource(LeakKey<N> leakKey)
			{
				ISet<PausedEdge> pausedEdges = pausedPathEdges.get(leakKey);
				if (pausedEdges != null)
				{
					foreach (PausedEdge edge in pausedEdges)
					{
						if (pausedEdges.remove(edge))
						{
							if (DEBUG)
							{
								logger.debug("-- UNPAUSE {}: {}",debugName, edge);
							}
							base.propagateUnbalancedReturnFlow(edge.retSiteC, edge.targetVal, edge.edgeFunction, edge.relatedCallSite);
						}
					}
				}
			}

			/* we share the same executor; this will cause the call to solve() above to block
			 * until both solvers have finished
			 */ 
			protected internal override CountingThreadPoolExecutor Executor
			{
				get
				{
					return outerInstance.sharedExecutor;
				}
			}

			protected internal override string DebugName
			{
				get
				{
					return debugName;
				}
			}
		}

		/// <summary>
		/// This is an augmented abstraction propagated by the <seealso cref="SingleDirectionSolver"/>. It associates with the
		/// abstraction the source statement from which this fact originated. 
		/// </summary>
		public class AbstractionWithSourceStmt
		{
			private readonly BiDiIDESolver<N, D, M, V, I> outerInstance;


			protected internal readonly D abstraction;
			protected internal readonly N source;

			internal AbstractionWithSourceStmt(BiDiIDESolver<N, D, M, V, I> outerInstance, D abstraction, N source)
			{
				this.outerInstance = outerInstance;
				this.abstraction = abstraction;
				this.source = source;
			}

			public virtual D Abstraction
			{
				get
				{
					return abstraction;
				}
			}

			public virtual N SourceStmt
			{
				get
				{
					return source;
				}
			}

			public override string ToString()
			{
				if (source != default(N))
				{
					return "" + abstraction + "-@-" + source + "";
				}
				else
				{
					return abstraction.ToString();
				}
			}

			public override int GetHashCode()
			{
				const int prime = 31;
				int result = 1;
				result = prime * result + ((abstraction == default(D)) ? 0 : abstraction.GetHashCode());
				result = prime * result + ((source == default(N)) ? 0 : source.GetHashCode());
				return result;
			}

			public override bool Equals(object obj)
			{
				if (this == obj)
				{
					return true;
				}
				if (obj == null)
				{
					return false;
				}
				if (this.GetType() != obj.GetType())
				{
					return false;
				}
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") AbstractionWithSourceStmt other = (AbstractionWithSourceStmt) obj;
				AbstractionWithSourceStmt other = (AbstractionWithSourceStmt) obj;
				if (abstraction == default(D))
				{
					if (other.abstraction != default(D))
					{
						return false;
					}
				}
				else if (!abstraction.Equals(other.abstraction))
				{
					return false;
				}
				if (source == default(N))
				{
					if (other.source != default(N))
					{
						return false;
					}
				}
				else if (!source.Equals(other.source))
				{
					return false;
				}
				return true;
			}
		}

		/// <summary>
		/// This tabulation problem simply propagates augmented abstractions where the normal problem would propagate normal abstractions.
		/// </summary>
		private class AugmentedTabulationProblem : IDETabulationProblem<N, AbstractionWithSourceStmt, M, V, I>
		{
			private readonly BiDiIDESolver<N, D, M, V, I> outerInstance;


			internal readonly IDETabulationProblem<N, D, M, V, I> @delegate;
			internal readonly AbstractionWithSourceStmt ZERO;
			internal readonly FlowFunctions<N, D, M> originalFunctions;

			public AugmentedTabulationProblem(BiDiIDESolver<N, D, M, V, I> outerInstance, IDETabulationProblem<N, D, M, V, I> @delegate)
			{
				this.outerInstance = outerInstance;
				this.@delegate = @delegate;
				originalFunctions = this.@delegate.flowFunctions();
				ZERO = new AbstractionWithSourceStmt(outerInstance, @delegate.zeroValue(), default(N));
			}

			public override FlowFunctions<N, AbstractionWithSourceStmt, M> flowFunctions()
			{
				return new FlowFunctionsAnonymousInnerClass(this);
			}

			private class FlowFunctionsAnonymousInnerClass : FlowFunctions<N, AbstractionWithSourceStmt, M>
			{
				private readonly AugmentedTabulationProblem outerInstance;

				public FlowFunctionsAnonymousInnerClass(AugmentedTabulationProblem outerInstance)
				{
					this.outerInstance = outerInstance;
				}


//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public heros.FlowFunction<AbstractionWithSourceStmt> getNormalFlowFunction(final N curr, final N succ)
				public FlowFunction<AbstractionWithSourceStmt> getNormalFlowFunction(N curr, N succ)
				{
					return new FlowFunctionAnonymousInnerClass(this, curr, succ);
				}

				private class FlowFunctionAnonymousInnerClass : FlowFunction<AbstractionWithSourceStmt>
				{
					private readonly FlowFunctionsAnonymousInnerClass outerInstance;

					private N curr;
					private N succ;

					public FlowFunctionAnonymousInnerClass(FlowFunctionsAnonymousInnerClass outerInstance, N curr, N succ)
					{
						this.outerInstance = outerInstance;
						this.curr = curr;
						this.succ = succ;
					}

					public ISet<AbstractionWithSourceStmt> computeTargets(AbstractionWithSourceStmt source)
					{
						return copyOverSourceStmts(source, outerInstance.outerInstance.originalFunctions.getNormalFlowFunction(curr, succ));
					}
				}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public heros.FlowFunction<AbstractionWithSourceStmt> getCallFlowFunction(final N callStmt, final M destinationMethod)
				public FlowFunction<AbstractionWithSourceStmt> getCallFlowFunction(N callStmt, M destinationMethod)
				{
					return new FlowFunctionAnonymousInnerClass2(this, callStmt, destinationMethod);
				}

				private class FlowFunctionAnonymousInnerClass2 : FlowFunction<AbstractionWithSourceStmt>
				{
					private readonly FlowFunctionsAnonymousInnerClass outerInstance;

					private N callStmt;
					private M destinationMethod;

					public FlowFunctionAnonymousInnerClass2(FlowFunctionsAnonymousInnerClass outerInstance, N callStmt, M destinationMethod)
					{
						this.outerInstance = outerInstance;
						this.callStmt = callStmt;
						this.destinationMethod = destinationMethod;
					}

					public ISet<AbstractionWithSourceStmt> computeTargets(AbstractionWithSourceStmt source)
					{
						ISet<D> origTargets = outerInstance.outerInstance.originalFunctions.getCallFlowFunction(callStmt, destinationMethod).computeTargets(source.Abstraction);

						ISet<AbstractionWithSourceStmt> res = new HashSet<AbstractionWithSourceStmt>();
						foreach (D d in origTargets)
						{
							res.Add(new AbstractionWithSourceStmt(outerInstance.outerInstance, d, default(N)));
						}
						return res;
					}
				}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public heros.FlowFunction<AbstractionWithSourceStmt> getReturnFlowFunction(final N callSite, final M calleeMethod, final N exitStmt, final N returnSite)
				public FlowFunction<AbstractionWithSourceStmt> getReturnFlowFunction(N callSite, M calleeMethod, N exitStmt, N returnSite)
				{
					return new FlowFunctionAnonymousInnerClass3(this, callSite, calleeMethod, exitStmt, returnSite);
				}

				private class FlowFunctionAnonymousInnerClass3 : FlowFunction<AbstractionWithSourceStmt>
				{
					private readonly FlowFunctionsAnonymousInnerClass outerInstance;

					private N callSite;
					private M calleeMethod;
					private N exitStmt;
					private N returnSite;

					public FlowFunctionAnonymousInnerClass3(FlowFunctionsAnonymousInnerClass outerInstance, N callSite, M calleeMethod, N exitStmt, N returnSite)
					{
						this.outerInstance = outerInstance;
						this.callSite = callSite;
						this.calleeMethod = calleeMethod;
						this.exitStmt = exitStmt;
						this.returnSite = returnSite;
					}

					public ISet<AbstractionWithSourceStmt> computeTargets(AbstractionWithSourceStmt source)
					{
						return copyOverSourceStmts(source, outerInstance.outerInstance.originalFunctions.getReturnFlowFunction(callSite, calleeMethod, exitStmt, returnSite));
					}
				}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public heros.FlowFunction<AbstractionWithSourceStmt> getCallToReturnFlowFunction(final N callSite, final N returnSite)
				public FlowFunction<AbstractionWithSourceStmt> getCallToReturnFlowFunction(N callSite, N returnSite)
				{
					return new FlowFunctionAnonymousInnerClass4(this, callSite, returnSite);
				}

				private class FlowFunctionAnonymousInnerClass4 : FlowFunction<AbstractionWithSourceStmt>
				{
					private readonly FlowFunctionsAnonymousInnerClass outerInstance;

					private N callSite;
					private N returnSite;

					public FlowFunctionAnonymousInnerClass4(FlowFunctionsAnonymousInnerClass outerInstance, N callSite, N returnSite)
					{
						this.outerInstance = outerInstance;
						this.callSite = callSite;
						this.returnSite = returnSite;
					}

					public ISet<AbstractionWithSourceStmt> computeTargets(AbstractionWithSourceStmt source)
					{
						return copyOverSourceStmts(source, outerInstance.outerInstance.originalFunctions.getCallToReturnFlowFunction(callSite, returnSite));
					}
				}

				private ISet<AbstractionWithSourceStmt> copyOverSourceStmts(AbstractionWithSourceStmt source, FlowFunction<D> originalFunction)
				{
					D originalAbstraction = source.Abstraction;
					ISet<D> origTargets = originalFunction.computeTargets(originalAbstraction);

					ISet<AbstractionWithSourceStmt> res = new HashSet<AbstractionWithSourceStmt>();
					foreach (D d in origTargets)
					{
						res.Add(new AbstractionWithSourceStmt(outerInstance.outerInstance, d,source.SourceStmt));
					}
					return res;
				}
			}

			//delegate methods follow

			public virtual bool followReturnsPastSeeds()
			{
				return @delegate.followReturnsPastSeeds();
			}

			public virtual bool autoAddZero()
			{
				return @delegate.autoAddZero();
			}

			public virtual int numThreads()
			{
				return @delegate.numThreads();
			}

			public virtual bool computeValues()
			{
				return @delegate.computeValues();
			}

			public virtual I interproceduralCFG()
			{
				return @delegate.interproceduralCFG();
			}

			/* attaches the original seed statement to the abstraction
			 */
			public virtual IDictionary<N, ISet<AbstractionWithSourceStmt>> initialSeeds()
			{
				IDictionary<N, ISet<D>> originalSeeds = @delegate.initialSeeds();
				IDictionary<N, ISet<AbstractionWithSourceStmt>> res = new Dictionary<N, ISet<AbstractionWithSourceStmt>>();
				foreach (KeyValuePair<N, ISet<D>> entry in originalSeeds.SetOfKeyValuePairs())
				{
					N stmt = entry.Key;
					ISet<D> seeds = entry.Value;
					ISet<AbstractionWithSourceStmt> resSet = new HashSet<AbstractionWithSourceStmt>();
					foreach (D d in seeds)
					{
						//attach source stmt to abstraction
						resSet.Add(new AbstractionWithSourceStmt(outerInstance, d, stmt));
					}
					res[stmt] = resSet;
				}
				return res;
			}

			public virtual AbstractionWithSourceStmt zeroValue()
			{
				return ZERO;
			}

			public virtual EdgeFunctions<N, AbstractionWithSourceStmt, M, V> edgeFunctions()
			{
				return new EdgeFunctionsAnonymousInnerClass(this);
			}

			private class EdgeFunctionsAnonymousInnerClass : EdgeFunctions<N, AbstractionWithSourceStmt, M, V>
			{
				private readonly AugmentedTabulationProblem outerInstance;

				public EdgeFunctionsAnonymousInnerClass(AugmentedTabulationProblem outerInstance)
				{
					this.outerInstance = outerInstance;
				}


				public EdgeFunction<V> getNormalEdgeFunction(N curr, AbstractionWithSourceStmt currNode, N succ, AbstractionWithSourceStmt succNode)
				{
					return outerInstance.@delegate.edgeFunctions().getNormalEdgeFunction(curr, currNode.Abstraction, succ, succNode.Abstraction);
				}

				public EdgeFunction<V> getCallEdgeFunction(N callStmt, AbstractionWithSourceStmt srcNode, M destinationMethod, AbstractionWithSourceStmt destNode)
				{
					return outerInstance.@delegate.edgeFunctions().getCallEdgeFunction(callStmt, srcNode.Abstraction, destinationMethod, destNode.Abstraction);
				}

				public EdgeFunction<V> getReturnEdgeFunction(N callSite, M calleeMethod, N exitStmt, AbstractionWithSourceStmt exitNode, N returnSite, AbstractionWithSourceStmt retNode)
				{
					return outerInstance.@delegate.edgeFunctions().getReturnEdgeFunction(callSite, calleeMethod, exitStmt, exitNode.Abstraction, returnSite, retNode.Abstraction);
				}

				public EdgeFunction<V> getCallToReturnEdgeFunction(N callSite, AbstractionWithSourceStmt callNode, N returnSite, AbstractionWithSourceStmt returnSideNode)
				{
					return outerInstance.@delegate.edgeFunctions().getCallToReturnEdgeFunction(callSite, callNode.Abstraction, returnSite, returnSideNode.Abstraction);
				}
			}

			public virtual JoinLattice<V> joinLattice()
			{
				return @delegate.joinLattice();
			}

			public virtual EdgeFunction<V> allTopFunction()
			{
				return @delegate.allTopFunction();
			}

			public override bool recordEdges()
			{
				return @delegate.recordEdges();
			}

		}

	}

}