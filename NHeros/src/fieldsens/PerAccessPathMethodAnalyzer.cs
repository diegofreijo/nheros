using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using heros.fieldsens.structs;
using heros.utilities;
using NHeros.src.util;
using System.Linq;

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
	public class PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method>
	{
		//private static readonly Logger logger = LoggerFactory.getLogger(typeof(PerAccessPathMethodAnalyzer));

		private Fact sourceFact;
		private readonly AccessPath<Field> accessPath;
		private IDictionary<WrappedFactAtStatement<Field, Fact, Stmt, Method>, WrappedFactAtStatement<Field, Fact, Stmt, Method>> reachableStatements = new Dictionary();
		private IList<WrappedFactAtStatement<Field, Fact, Stmt, Method>> summaries = new List<WrappedFactAtStatement<Field, Fact, Stmt, Method>>();
		private Context<Field, Fact, Stmt, Method> context;
		private Method method;
		private DefaultValueMap<FactAtStatement<Fact, Stmt>, ReturnSiteResolver<Field, Fact, Stmt, Method>> returnSiteResolvers = new DefaultValueMapAnonymousInnerClass();

		private class DefaultValueMapAnonymousInnerClass : DefaultValueMap<FactAtStatement<Fact, Stmt>, ReturnSiteResolver<Field, Fact, Stmt, Method>>
		{
			protected internal override ReturnSiteResolver<Field, Fact, Stmt, Method> createItem(FactAtStatement<Fact, Stmt> key)
			{
				return new ReturnSiteResolver<Field, Fact, Stmt, Method>(outerInstance.context.factHandler, outerInstance, key.stmt, outerInstance.debugger);
			}
		}
		private DefaultValueMap<FactAtStatement<Fact, Stmt>, ControlFlowJoinResolver<Field, Fact, Stmt, Method>> ctrFlowJoinResolvers = new DefaultValueMapAnonymousInnerClass2();

		private class DefaultValueMapAnonymousInnerClass2 : DefaultValueMap<FactAtStatement<Fact, Stmt>, ControlFlowJoinResolver<Field, Fact, Stmt, Method>>
		{
			protected internal override ControlFlowJoinResolver<Field, Fact, Stmt, Method> createItem(FactAtStatement<Fact, Stmt> key)
			{
				return new ControlFlowJoinResolver<Field, Fact, Stmt, Method>(outerInstance.context.factHandler, outerInstance, key.stmt, outerInstance.debugger);
			}
		}
		private CallEdgeResolver<Field, Fact, Stmt, Method> callEdgeResolver;
		private PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> parent;
		private Debugger<Field, Fact, Stmt, Method> debugger;

		public PerAccessPathMethodAnalyzer(Method method, Fact sourceFact, Context<Field, Fact, Stmt, Method> context, Debugger<Field, Fact, Stmt, Method> debugger) : this(method, sourceFact, context, debugger, new AccessPath<Field>(), null)
		{
		}

		private PerAccessPathMethodAnalyzer(Method method, Fact sourceFact, Context<Field, Fact, Stmt, Method> context, Debugger<Field, Fact, Stmt, Method> debugger, AccessPath<Field> accPath, PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> parent)
		{
			this.debugger = debugger;
			if (Utils.IsDefault(method))
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
				this.callEdgeResolver = ZeroSource ? new ZeroCallEdgeResolver<Field, Fact, Stmt, Method>(this, context.zeroHandler, debugger) : new CallEdgeResolver<Field, Fact, Stmt, Method>(this, debugger);
			}
			else
			{
				this.callEdgeResolver = ZeroSource ? parent.callEdgeResolver : new CallEdgeResolver<Field, Fact, Stmt, Method>(this, debugger, parent.callEdgeResolver);
			}
			log("initialized");
		}

		public virtual PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> createWithAccessPath(AccessPath<Field> accPath)
		{
			return new PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method>(method, sourceFact, context, debugger, accPath, this);
		}

		internal virtual WrappedFact<Field, Fact, Stmt, Method> wrappedSource()
		{
			return new WrappedFact<Field, Fact, Stmt, Method>(sourceFact, accessPath, callEdgeResolver);
		}

		public virtual AccessPath<Field> AccessPath
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
				WrappedFactAtStatement<Field, Fact, Stmt, Method> target = new WrappedFactAtStatement<Field, Fact, Stmt, Method>(startPoint, wrappedSource());
				if (!reachableStatements.ContainsKey(target))
				{
					scheduleEdgeTo(target);
				}
			}
		}

		public virtual void addInitialSeed(Stmt stmt)
		{
			scheduleEdgeTo(new WrappedFactAtStatement<Field, Fact, Stmt, Method>(stmt, wrappedSource()));
		}

		private void scheduleEdgeTo(ICollection<Stmt> successors, WrappedFact<Field, Fact, Stmt, Method> fact)
		{
			foreach (Stmt stmt in successors)
			{
				scheduleEdgeTo(new WrappedFactAtStatement<Field, Fact, Stmt, Method>(stmt, fact));
			}
		}

		internal virtual void scheduleEdgeTo(WrappedFactAtStatement<Field, Fact, Stmt, Method> factAtStmt)
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
			//logger.trace("[{}; {}{}: " + message + "]", method, sourceFact, accessPath);
		}

		public override string ToString()
		{
			return method + "; " + sourceFact + accessPath;
		}

		internal virtual void processCall(WrappedFactAtStatement<Field, Fact, Stmt, Method> factAtStmt)
		{
			ICollection<Method> calledMethods = context.icfg.getCalleesOfCallAt(factAtStmt.Statement);
			foreach (Method calledMethod in calledMethods)
			{
				FlowFunction<Field, Fact, Stmt, Method> flowFunction = context.flowFunctions.getCallFlowFunction(factAtStmt.Statement, calledMethod);
				ICollection<FlowFunction_ConstrainedFact<Field, Fact, Stmt, Method>> targetFacts = flowFunction.computeTargets(factAtStmt.Fact, new AccessPathHandler<Field, Fact, Stmt, Method>(factAtStmt.AccessPath, factAtStmt.Resolver, debugger));
				foreach (FlowFunction_ConstrainedFact<Field, Fact, Stmt, Method> targetFact in targetFacts)
				{
					//TODO handle constraint
					MethodAnalyzer<Field, Fact, Stmt, Method> analyzer = context.getAnalyzer(calledMethod);
					analyzer.addIncomingEdge(new CallEdge<Field, Fact, Stmt, Method>(this, factAtStmt, targetFact.Fact));
				}
			}

			processCallToReturnEdge(factAtStmt);
		}

		internal virtual void processExit(WrappedFactAtStatement<Field, Fact, Stmt, Method> factAtStmt)
		{
			log("New Summary: " + factAtStmt);
            //if (!summaries.Add(factAtStmt))
            //{
            //    throw new AssertionError();
            //}
            summaries.Add(factAtStmt);

			callEdgeResolver.applySummaries(factAtStmt);

			if (context.followReturnsPastSeeds && ZeroSource)
			{
				ICollection<Stmt> callSites = context.icfg.getCallersOf(method);
				foreach (Stmt callSite in callSites)
				{
					ICollection<Stmt> returnSites = context.icfg.getReturnSitesOfCallAt(callSite);
					foreach (Stmt returnSite in returnSites)
					{
						FlowFunction<Field, Fact, Stmt, Method> flowFunction = context.flowFunctions.getReturnFlowFunction(callSite, method, factAtStmt.Statement, returnSite);
						ICollection<FlowFunction_ConstrainedFact<Field, Fact, Stmt, Method>> targetFacts = flowFunction.computeTargets(factAtStmt.Fact, new AccessPathHandler<Field, Fact, Stmt, Method>(factAtStmt.AccessPath, factAtStmt.Resolver, debugger));
						foreach (FlowFunction_ConstrainedFact<Field, Fact, Stmt, Method> targetFact in targetFacts)
						{
							//TODO handle constraint
							context.getAnalyzer(context.icfg.getMethodOf(callSite)).addUnbalancedReturnFlow(new WrappedFactAtStatement<Field, Fact, Stmt, Method>(returnSite, targetFact.Fact), callSite);
						}
					}
				}
				//in cases where there are no callers, the return statement would normally not be processed at all;
				//this might be undesirable if the flow function has a side effect such as registering a taint;
				//instead we thus call the return flow function will a null caller
				if (callSites.Count == 0)
				{
					FlowFunction<Field, Fact, Stmt, Method> flowFunction = context.flowFunctions.getReturnFlowFunction(default(Stmt), method, factAtStmt.Statement, default(Stmt));
					flowFunction.computeTargets(factAtStmt.Fact, new AccessPathHandler<Field, Fact, Stmt, Method>(factAtStmt.AccessPath, factAtStmt.Resolver, debugger));
				}
			}
		}

		private void processCallToReturnEdge(WrappedFactAtStatement<Field, Fact, Stmt, Method> factAtStmt)
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

		private void processNonJoiningCallToReturnFlow(WrappedFactAtStatement<Field, Fact, Stmt, Method> factAtStmt)
		{
			ICollection<Stmt> returnSites = context.icfg.getReturnSitesOfCallAt(factAtStmt.Statement);
			foreach (Stmt returnSite in returnSites)
			{
				FlowFunction<Field, Fact, Stmt, Method> flowFunction = context.flowFunctions.getCallToReturnFlowFunction(factAtStmt.Statement, returnSite);
				ICollection<FlowFunction_ConstrainedFact<Field, Fact, Stmt, Method>> targetFacts = flowFunction.computeTargets(factAtStmt.Fact, new AccessPathHandler<Field, Fact, Stmt, Method>(factAtStmt.AccessPath, factAtStmt.Resolver, debugger));
				foreach (FlowFunction_ConstrainedFact<Field, Fact, Stmt, Method> targetFact in targetFacts)
				{
					//TODO handle constraint
					scheduleEdgeTo(new WrappedFactAtStatement<Field, Fact, Stmt, Method>(returnSite, targetFact.Fact));
				}
			}
		}

		private void processNormalFlow(WrappedFactAtStatement<Field, Fact, Stmt, Method> factAtStmt)
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
                ISet<Stmt> visited = new HashSet<Stmt>();
                IEnumerable<Stmt> worklist = new List<Stmt>(context.icfg.getPredsOf(stmt));
				while (worklist.Count() > 0)
				{
                    Stmt current = worklist.First();
                    worklist = worklist.Skip(1);
                    if (current.Equals(stmt))
						return true;
					if (!visited.Add(current))
						continue;
					worklist = worklist.Concat(context.icfg.getPredsOf(current));
				}
			}
			return false;
		}

		internal virtual void processFlowFromJoinStmt(WrappedFactAtStatement<Field, Fact, Stmt, Method> factAtStmt)
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

		private void processNormalNonJoiningFlow(WrappedFactAtStatement<Field, Fact, Stmt, Method> factAtStmt)
		{

//ORIGINAL LINE: final java.util.List<Stmt> successors = context.icfg.getSuccsOf(factAtStmt.getStatement());
			IList<Stmt> successors = context.icfg.getSuccsOf(factAtStmt.Statement);
			FlowFunction<Field, Fact, Stmt, Method> flowFunction = context.flowFunctions.getNormalFlowFunction(factAtStmt.Statement);
			ICollection<FlowFunction_ConstrainedFact<Field, Fact, Stmt, Method>> targetFacts = flowFunction.computeTargets(factAtStmt.Fact, new AccessPathHandler<Field, Fact, Stmt, Method>(factAtStmt.AccessPath, factAtStmt.Resolver, debugger));
			foreach (FlowFunction_ConstrainedFact<Field, Fact, Stmt, Method> targetFact in targetFacts)
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

		private class InterestCallbackAnonymousInnerClass : InterestCallback<Field, Fact, Stmt, Method>
		{
			private readonly PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> outerInstance;

			private IList<Stmt> successors;

			public InterestCallbackAnonymousInnerClass(PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> outerInstance, IList<Stmt> successors)
			{
				this.outerInstance = outerInstance;
				this.successors = successors;
			}

			public void interest(PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> analyzer, Resolver<Field, Fact, Stmt, Method> resolver)
			{
				analyzer.scheduleEdgeTo(successors, new WrappedFact<Field, Fact, Stmt, Method>(targetFact.Fact.Fact, targetFact.Fact.AccessPath, resolver));
			}

			public void canBeResolvedEmpty()
			{
				outerInstance.callEdgeResolver.resolve(targetFact.Constraint, this);
			}
		}

		public virtual void addIncomingEdge(CallEdge<Field, Fact, Stmt, Method> incEdge)
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

		internal virtual void applySummary(CallEdge<Field, Fact, Stmt, Method> incEdge, WrappedFactAtStatement<Field, Fact, Stmt, Method> exitFact)
		{
			ICollection<Stmt> returnSites = context.icfg.getReturnSitesOfCallAt(incEdge.CallSite);
			foreach (Stmt returnSite in returnSites)
			{
				FlowFunction<Field, Fact, Stmt, Method> flowFunction = context.flowFunctions.getReturnFlowFunction(incEdge.CallSite, method, exitFact.Statement, returnSite);
				ISet<FlowFunction_ConstrainedFact<Field, Fact, Stmt, Method>> targets = flowFunction.computeTargets(exitFact.Fact, new AccessPathHandler<Field, Fact, Stmt, Method>(exitFact.AccessPath, exitFact.Resolver, debugger));
				foreach (FlowFunction_ConstrainedFact<Field, Fact, Stmt, Method> targetFact in targets)
				{
					context.factHandler.restoreCallingContext(targetFact.Fact.Fact, incEdge.CallerCallSiteFact.Fact);
					//TODO handle constraint
					scheduleReturnEdge(incEdge, targetFact.Fact, returnSite);
				}
			}
		}

		public virtual void scheduleUnbalancedReturnEdgeTo(WrappedFactAtStatement<Field, Fact, Stmt, Method> fact)
		{
			ReturnSiteResolver<Field, Fact, Stmt, Method> resolver = returnSiteResolvers.getOrCreate(fact.AsFactAtStatement);
			resolver.addIncoming(new WrappedFact<Field, Fact, Stmt, Method>(fact.WrappedFact.Fact, fact.WrappedFact.AccessPath, fact.WrappedFact.Resolver), null, Delta.empty<Field>());
		}

		private void scheduleReturnEdge(CallEdge<Field, Fact, Stmt, Method> incEdge, WrappedFact<Field, Fact, Stmt, Method> fact, Stmt returnSite)
		{
			var delta = accessPath.getDeltaTo(incEdge.CalleeSourceFact.AccessPath);
			ReturnSiteResolver<Field, Fact, Stmt, Method> returnSiteResolver = incEdge.CallerAnalyzer.returnSiteResolvers.getOrCreate(new FactAtStatement<Fact, Stmt>(fact.Fact, returnSite));
			returnSiteResolver.addIncoming(fact, incEdge.CalleeSourceFact.Resolver, delta);
		}

		internal virtual void applySummaries(CallEdge<Field, Fact, Stmt, Method> incEdge)
		{
			foreach (WrappedFactAtStatement<Field, Fact, Stmt, Method> summary in summaries)
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
			private readonly PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> outerInstance;
            internal WrappedFactAtStatement<Field, Fact, Stmt, Method> factAtStmt;

			public Job(PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> outerInstance, WrappedFactAtStatement<Field, Fact, Stmt, Method> factAtStmt)
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

        public virtual CallEdgeResolver<Field, Fact, Stmt, Method> CallEdgeResolver => callEdgeResolver;
        public virtual Method Method => method;

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