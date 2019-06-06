using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

/// <summary>
///*****************************************************************************
/// Copyright (c) 2015 Johannes Lerch.
/// All rights reserved. This program and the accompanying materials
/// are made available under the terms of the GNU Lesser Public License v2.1
/// which accompanies this distribution, and is available at
/// http://www.gnu.org/licenses/old-licenses/gpl-2.0.html
/// 
/// Contributors:
///     Johannes Lerch - initial API and implementation
/// *****************************************************************************
/// </summary>
namespace heros.fieldsens
{
	using Delta = heros.fieldsens.AccessPath.Delta;
	using PrefixTestResult = heros.fieldsens.AccessPath.PrefixTestResult;
	using FactAtStatement = heros.fieldsens.structs.FactAtStatement;
	using WrappedFact = heros.fieldsens.structs.WrappedFact;
	using WrappedFactAtStatement = heros.fieldsens.structs.WrappedFactAtStatement;
	using DefaultValueMap = heros.utilities.DefaultValueMap;


	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;

	using Lists = com.google.common.collect.Lists;
	using Maps = com.google.common.collect.Maps;
	using Sets = com.google.common.collect.Sets;

	public class PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>
	{

		private static readonly Logger logger = LoggerFactory.getLogger(typeof(PerAccessPathMethodAnalyzer));
		private Fact sourceFact;
		private readonly AccessPath<System.Reflection.FieldInfo> accessPath;
		private IDictionary<WrappedFactAtStatement<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>, WrappedFactAtStatement<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>> reachableStatements = new Dictionary();
		private IList<WrappedFactAtStatement<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>> summaries = new List();
		private Context<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> context;
		private System.Reflection.MethodInfo method;
		private DefaultValueMap<FactAtStatement<Fact, Stmt>, ReturnSiteResolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>> returnSiteResolvers = new DefaultValueMapAnonymousInnerClass();

		private class DefaultValueMapAnonymousInnerClass : DefaultValueMap<FactAtStatement<Fact, Stmt>, ReturnSiteResolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>>
		{
			protected internal override ReturnSiteResolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> createItem(FactAtStatement<Fact, Stmt> key)
			{
				return new ReturnSiteResolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(outerInstance.context.factHandler, outerInstance, key.stmt, outerInstance.debugger);
			}
		}
		private DefaultValueMap<FactAtStatement<Fact, Stmt>, ControlFlowJoinResolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>> ctrFlowJoinResolvers = new DefaultValueMapAnonymousInnerClass2();

		private class DefaultValueMapAnonymousInnerClass2 : DefaultValueMap<FactAtStatement<Fact, Stmt>, ControlFlowJoinResolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>>
		{
			protected internal override ControlFlowJoinResolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> createItem(FactAtStatement<Fact, Stmt> key)
			{
				return new ControlFlowJoinResolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(outerInstance.context.factHandler, outerInstance, key.stmt, outerInstance.debugger);
			}
		}
		private CallEdgeResolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> callEdgeResolver;
		private PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> parent;
		private Debugger<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> debugger;

		public PerAccessPathMethodAnalyzer(System.Reflection.MethodInfo method, Fact sourceFact, Context<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> context, Debugger<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> debugger) : this(method, sourceFact, context, debugger, new AccessPath<System.Reflection.FieldInfo>(), null)
		{
		}

		private PerAccessPathMethodAnalyzer(System.Reflection.MethodInfo method, Fact sourceFact, Context<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> context, Debugger<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> debugger, AccessPath<System.Reflection.FieldInfo> accPath, PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> parent)
		{
			this.debugger = debugger;
			if (method == default(System.Reflection.MethodInfo))
			{
				throw new System.ArgumentException("Method must be not null");
			}
			this.parent = parent;
			this.method = method;
			this.sourceFact = sourceFact;
			this.accessPath = accPath;
			this.context = context;
			if (parent == null)
			{
				this.callEdgeResolver = ZeroSource ? new ZeroCallEdgeResolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(this, context.zeroHandler, debugger) : new CallEdgeResolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(this, debugger);
			}
			else
			{
				this.callEdgeResolver = ZeroSource ? parent.callEdgeResolver : new CallEdgeResolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(this, debugger, parent.callEdgeResolver);
			}
			log("initialized");
		}

		public virtual PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> createWithAccessPath(AccessPath<System.Reflection.FieldInfo> accPath)
		{
			return new PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(method, sourceFact, context, debugger, accPath, this);
		}

		internal virtual WrappedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> wrappedSource()
		{
			return new WrappedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(sourceFact, accessPath, callEdgeResolver);
		}

		public virtual AccessPath<System.Reflection.FieldInfo> AccessPath
		{
			get
			{
				return accessPath;
			}
		}

		private bool BootStrapped
		{
			get
			{
				return callEdgeResolver.hasIncomingEdges() || !accessPath.Empty;
			}
		}

		private void bootstrapAtMethodStartPoints()
		{
			callEdgeResolver.interest(callEdgeResolver);
			foreach (Stmt startPoint in context.icfg.getStartPointsOf(method))
			{
				WrappedFactAtStatement<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> target = new WrappedFactAtStatement<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(startPoint, wrappedSource());
				if (!reachableStatements.ContainsKey(target))
				{
					scheduleEdgeTo(target);
				}
			}
		}

		public virtual void addInitialSeed(Stmt stmt)
		{
			scheduleEdgeTo(new WrappedFactAtStatement<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(stmt, wrappedSource()));
		}

		private void scheduleEdgeTo(ICollection<Stmt> successors, WrappedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> fact)
		{
			foreach (Stmt stmt in successors)
			{
				scheduleEdgeTo(new WrappedFactAtStatement<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(stmt, fact));
			}
		}

		internal virtual void scheduleEdgeTo(WrappedFactAtStatement<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> factAtStmt)
		{
			Debug.Assert(context.icfg.getMethodOf(factAtStmt.Statement).Equals(method));
			if (reachableStatements.ContainsKey(factAtStmt))
			{
				log("Merging " + factAtStmt);
				context.factHandler.merge(reachableStatements[factAtStmt].WrappedFact.Fact, factAtStmt.WrappedFact.Fact);
			}
			else
			{
				log("Edge to " + factAtStmt);
				reachableStatements[factAtStmt] = factAtStmt;
				context.scheduler.schedule(new Job(this, factAtStmt));
				debugger.edgeTo(this, factAtStmt);
			}
		}

		internal virtual void log(string message)
		{
			logger.trace("[{}; {}{}: " + message + "]", method, sourceFact, accessPath);
		}

		public override string ToString()
		{
			return method + "; " + sourceFact + accessPath;
		}

		internal virtual void processCall(WrappedFactAtStatement<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> factAtStmt)
		{
			ICollection<System.Reflection.MethodInfo> calledMethods = context.icfg.getCalleesOfCallAt(factAtStmt.Statement);
			foreach (System.Reflection.MethodInfo calledMethod in calledMethods)
			{
				FlowFunction<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> flowFunction = context.flowFunctions.getCallFlowFunction(factAtStmt.Statement, calledMethod);
				ICollection<FlowFunction_ConstrainedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>> targetFacts = flowFunction.computeTargets(factAtStmt.Fact, new AccessPathHandler<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(factAtStmt.AccessPath, factAtStmt.Resolver, debugger));
				foreach (FlowFunction_ConstrainedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> targetFact in targetFacts)
				{
					//TODO handle constraint
					MethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> analyzer = context.getAnalyzer(calledMethod);
					analyzer.addIncomingEdge(new CallEdge<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(this, factAtStmt, targetFact.Fact));
				}
			}

			processCallToReturnEdge(factAtStmt);
		}

		internal virtual void processExit(WrappedFactAtStatement<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> factAtStmt)
		{
			log("New Summary: " + factAtStmt);
			if (!summaries.Add(factAtStmt))
			{
				throw new AssertionError();
			}

			callEdgeResolver.applySummaries(factAtStmt);

			if (context.followReturnsPastSeeds && ZeroSource)
			{
				ICollection<Stmt> callSites = context.icfg.getCallersOf(method);
				foreach (Stmt callSite in callSites)
				{
					ICollection<Stmt> returnSites = context.icfg.getReturnSitesOfCallAt(callSite);
					foreach (Stmt returnSite in returnSites)
					{
						FlowFunction<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> flowFunction = context.flowFunctions.getReturnFlowFunction(callSite, method, factAtStmt.Statement, returnSite);
						ICollection<FlowFunction_ConstrainedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>> targetFacts = flowFunction.computeTargets(factAtStmt.Fact, new AccessPathHandler<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(factAtStmt.AccessPath, factAtStmt.Resolver, debugger));
						foreach (FlowFunction_ConstrainedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> targetFact in targetFacts)
						{
							//TODO handle constraint
							context.getAnalyzer(context.icfg.getMethodOf(callSite)).addUnbalancedReturnFlow(new WrappedFactAtStatement<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(returnSite, targetFact.Fact), callSite);
						}
					}
				}
				//in cases where there are no callers, the return statement would normally not be processed at all;
				//this might be undesirable if the flow function has a side effect such as registering a taint;
				//instead we thus call the return flow function will a null caller
				if (callSites.Count == 0)
				{
					FlowFunction<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> flowFunction = context.flowFunctions.getReturnFlowFunction(default(Stmt), method, factAtStmt.Statement, default(Stmt));
					flowFunction.computeTargets(factAtStmt.Fact, new AccessPathHandler<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(factAtStmt.AccessPath, factAtStmt.Resolver, debugger));
				}
			}
		}

		private void processCallToReturnEdge(WrappedFactAtStatement<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> factAtStmt)
		{
			if (isLoopStart(factAtStmt.Statement))
			{
				ctrFlowJoinResolvers.getOrCreate(factAtStmt.AsFactAtStatement).addIncoming(factAtStmt.WrappedFact);
			}
			else
			{
				processNonJoiningCallToReturnFlow(factAtStmt);
			}
		}

		private void processNonJoiningCallToReturnFlow(WrappedFactAtStatement<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> factAtStmt)
		{
			ICollection<Stmt> returnSites = context.icfg.getReturnSitesOfCallAt(factAtStmt.Statement);
			foreach (Stmt returnSite in returnSites)
			{
				FlowFunction<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> flowFunction = context.flowFunctions.getCallToReturnFlowFunction(factAtStmt.Statement, returnSite);
				ICollection<FlowFunction_ConstrainedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>> targetFacts = flowFunction.computeTargets(factAtStmt.Fact, new AccessPathHandler<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(factAtStmt.AccessPath, factAtStmt.Resolver, debugger));
				foreach (FlowFunction_ConstrainedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> targetFact in targetFacts)
				{
					//TODO handle constraint
					scheduleEdgeTo(new WrappedFactAtStatement<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(returnSite, targetFact.Fact));
				}
			}
		}

		private void processNormalFlow(WrappedFactAtStatement<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> factAtStmt)
		{
			if (isLoopStart(factAtStmt.Statement))
			{
				ctrFlowJoinResolvers.getOrCreate(factAtStmt.AsFactAtStatement).addIncoming(factAtStmt.WrappedFact);
			}
			else
			{
				processNormalNonJoiningFlow(factAtStmt);
			}
		}

		private bool isLoopStart(Stmt stmt)
		{
			int numberOfPredecessors = context.icfg.getPredsOf(stmt).Count;
			if ((numberOfPredecessors > 1 && !context.icfg.isExitStmt(stmt)) || (context.icfg.isStartPoint(stmt) && numberOfPredecessors > 0))
			{
				ISet<Stmt> visited = Sets.newHashSet();
				IList<Stmt> worklist = new List();
				((IList<Stmt>)worklist).AddRange(context.icfg.getPredsOf(stmt));
				while (worklist.Count > 0)
				{
					Stmt current = worklist.RemoveAt(0);
					if (current.Equals(stmt))
					{
						return true;
					}
					if (!visited.Add(current))
					{
						continue;
					}
					((IList<Stmt>)worklist).AddRange(context.icfg.getPredsOf(current));
				}
			}
			return false;
		}

		internal virtual void processFlowFromJoinStmt(WrappedFactAtStatement<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> factAtStmt)
		{
			if (context.icfg.isCallStmt(factAtStmt.Statement))
			{
				processNonJoiningCallToReturnFlow(factAtStmt);
			}
			else
			{
				processNormalNonJoiningFlow(factAtStmt);
			}
		}

		private void processNormalNonJoiningFlow(WrappedFactAtStatement<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> factAtStmt)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<Stmt> successors = context.icfg.getSuccsOf(factAtStmt.getStatement());
			IList<Stmt> successors = context.icfg.getSuccsOf(factAtStmt.Statement);
			FlowFunction<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> flowFunction = context.flowFunctions.getNormalFlowFunction(factAtStmt.Statement);
			ICollection<FlowFunction_ConstrainedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>> targetFacts = flowFunction.computeTargets(factAtStmt.Fact, new AccessPathHandler<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(factAtStmt.AccessPath, factAtStmt.Resolver, debugger));
			foreach (FlowFunction_ConstrainedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> targetFact in targetFacts)
			{
				if (targetFact.Constraint == null)
				{
					scheduleEdgeTo(successors, targetFact.Fact);
				}
				else
				{
					targetFact.Fact.Resolver.resolve(targetFact.Constraint, new InterestCallbackAnonymousInnerClass(this, successors));
				}
			}
		}

		private class InterestCallbackAnonymousInnerClass : InterestCallback<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>
		{
			private readonly PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> outerInstance;

			private IList<Stmt> successors;

			public InterestCallbackAnonymousInnerClass(PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> outerInstance, IList<Stmt> successors)
			{
				this.outerInstance = outerInstance;
				this.successors = successors;
			}

			public void interest(PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> analyzer, Resolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> resolver)
			{
				analyzer.scheduleEdgeTo(successors, new WrappedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(targetFact.Fact.Fact, targetFact.Fact.AccessPath, resolver));
			}

			public void canBeResolvedEmpty()
			{
				outerInstance.callEdgeResolver.resolve(targetFact.Constraint, this);
			}
		}

		public virtual void addIncomingEdge(CallEdge<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> incEdge)
		{
			if (BootStrapped)
			{
				context.factHandler.merge(sourceFact, incEdge.CalleeSourceFact.Fact);
			}
			else
			{
				bootstrapAtMethodStartPoints();
			}
			callEdgeResolver.addIncoming(incEdge);
		}

		internal virtual void applySummary(CallEdge<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> incEdge, WrappedFactAtStatement<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> exitFact)
		{
			ICollection<Stmt> returnSites = context.icfg.getReturnSitesOfCallAt(incEdge.CallSite);
			foreach (Stmt returnSite in returnSites)
			{
				FlowFunction<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> flowFunction = context.flowFunctions.getReturnFlowFunction(incEdge.CallSite, method, exitFact.Statement, returnSite);
				ISet<FlowFunction_ConstrainedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>> targets = flowFunction.computeTargets(exitFact.Fact, new AccessPathHandler<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(exitFact.AccessPath, exitFact.Resolver, debugger));
				foreach (FlowFunction_ConstrainedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> targetFact in targets)
				{
					context.factHandler.restoreCallingContext(targetFact.Fact.Fact, incEdge.CallerCallSiteFact.Fact);
					//TODO handle constraint
					scheduleReturnEdge(incEdge, targetFact.Fact, returnSite);
				}
			}
		}

		public virtual void scheduleUnbalancedReturnEdgeTo(WrappedFactAtStatement<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> fact)
		{
			ReturnSiteResolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> resolver = returnSiteResolvers.getOrCreate(fact.AsFactAtStatement);
			resolver.addIncoming(new WrappedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(fact.WrappedFact.Fact, fact.WrappedFact.AccessPath, fact.WrappedFact.Resolver), null, Delta.empty<System.Reflection.FieldInfo>());
		}

		private void scheduleReturnEdge(CallEdge<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> incEdge, WrappedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> fact, Stmt returnSite)
		{
			Delta<System.Reflection.FieldInfo> delta = accessPath.getDeltaTo(incEdge.CalleeSourceFact.AccessPath);
			ReturnSiteResolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> returnSiteResolver = incEdge.CallerAnalyzer.returnSiteResolvers.getOrCreate(new FactAtStatement<Fact, Stmt>(fact.Fact, returnSite));
			returnSiteResolver.addIncoming(fact, incEdge.CalleeSourceFact.Resolver, delta);
		}

		internal virtual void applySummaries(CallEdge<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> incEdge)
		{
			foreach (WrappedFactAtStatement<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> summary in summaries)
			{
				applySummary(incEdge, summary);
			}
		}

		public virtual bool ZeroSource
		{
			get
			{
				return sourceFact.Equals(context.zeroValue);
			}
		}

		private class Job : ThreadStart
		{
			private readonly PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> outerInstance;


			internal WrappedFactAtStatement<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> factAtStmt;

			public Job(PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> outerInstance, WrappedFactAtStatement<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> factAtStmt)
			{
				this.outerInstance = outerInstance;
				this.factAtStmt = factAtStmt;
				outerInstance.debugger.newJob(outerInstance, factAtStmt);
			}

			public override void run()
			{
				outerInstance.debugger.jobStarted(outerInstance, factAtStmt);
				if (outerInstance.context.icfg.isCallStmt(factAtStmt.Statement))
				{
					outerInstance.processCall(factAtStmt);
				}
				else
				{
					if (outerInstance.context.icfg.isExitStmt(factAtStmt.Statement))
					{
						outerInstance.processExit(factAtStmt);
					}
					if (outerInstance.context.icfg.getSuccsOf(factAtStmt.Statement).Count > 0)
					{
						outerInstance.processNormalFlow(factAtStmt);
					}
				}
				outerInstance.debugger.jobFinished(outerInstance, factAtStmt);
			}

			public override string ToString()
			{
				return "Job: " + factAtStmt;
			}
		}

		public virtual CallEdgeResolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> CallEdgeResolver
		{
			get
			{
				return callEdgeResolver;
			}
		}

		public virtual System.Reflection.MethodInfo Method
		{
			get
			{
				return method;
			}
		}

	//	public void debugReachables() {
	//		JsonDocument root = new JsonDocument();
	//		
	//		for(WrappedFactAtStatement<Field, Fact, Stmt, Method> fact : reachableStatements.keySet()) {
	//			JsonDocument doc = root.doc(fact.getStatement().toString()).doc(fact.getFact().toString()).doc(fact.getResolver().toString()).doc(String.valueOf(fact.hashCode()));
	//			doc.keyValue("fact", String.valueOf(fact.getFact().hashCode()));
	//			doc.keyValue("resolver", String.valueOf(fact.getResolver().hashCode()));
	//			doc.keyValue("resolver-analyzer", String.valueOf(fact.getResolver().analyzer.hashCode()));
	//			doc.keyValue("resolver-class", String.valueOf(fact.getResolver().getClass().toString()));
	//		}
	//		try {
	//			FileWriter writer = new FileWriter("debug/reachables.json");
	//			StringBuilder builder = new StringBuilder();
	//			builder.append("var root=");
	//			root.write(builder, 0);
	//			writer.write(builder.toString());
	//			writer.close();
	//		} catch (IOException e) {
	//			e.printStackTrace();
	//		}
	//	}

	//	public void debugInterest() {
	//		JsonDocument root = new JsonDocument();
	//		
	//		List<PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method>> worklist = new List();
	//		worklist.add(this);
	//		Set<PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method>> visited = Sets.newHashSet();
	//		
	//		while(!worklist.isEmpty()) {
	//			PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> current = worklist.remove(0);
	//			if(!visited.add(current))
	//				continue;
	//			
	//			JsonDocument currentMethodDoc = root.doc(current.method.toString()+ "___"+current.sourceFact);
	//			JsonDocument currentDoc = currentMethodDoc.doc("accPath").doc("_"+current.accessPath.toString());
	//			
	//			for(CallEdge<Field, Fact, Stmt, Method> incEdge : current.getCallEdgeResolver().incomingEdges) {
	//				currentDoc.doc("incoming").doc(incEdge.getCallerAnalyzer().method+"___"+incEdge.getCallerAnalyzer().sourceFact).doc("_"+incEdge.getCallerAnalyzer().accessPath.toString());
	//				worklist.add(incEdge.getCallerAnalyzer());
	//			}
	//		}
	//		
	//		try {
	//			FileWriter writer = new FileWriter("debug/incoming.json");
	//			StringBuilder builder = new StringBuilder();
	//			builder.append("var root=");
	//			root.write(builder, 0);
	//			writer.write(builder.toString());
	//			writer.close();
	//		} catch (IOException e) {
	//			e.printStackTrace();
	//		}
	//	}
	//	
	//	public void debugNestings() {
	//		PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> current = this;
	//		while(current.parent != null)
	//			current = current.parent;
	//		
	//		JsonDocument root = new JsonDocument();
	//		debugNestings(current, root);
	//		
	//		try {
	//			FileWriter writer = new FileWriter("debug/nestings.json");
	//			StringBuilder builder = new StringBuilder();
	//			builder.append("var root=");
	//			root.write(builder, 0);
	//			writer.write(builder.toString());
	//			writer.close();
	//		} catch (IOException e) {
	//			e.printStackTrace();
	//		}
	//	}
	//
	//	private void debugNestings(PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> current, JsonDocument parentDoc) {
	//		JsonDocument currentDoc = parentDoc.doc(current.accessPath.toString());
	//		for(ResolverTemplate<Field, Fact, Stmt, Method, CallEdge<Field, Fact, Stmt, Method>> nestedAnalyzer : current.getCallEdgeResolver().nestedResolvers.values()) {
	//			debugNestings(nestedAnalyzer.analyzer, currentDoc);
	//		}
	//	}
	}

}