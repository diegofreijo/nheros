using heros.fieldsens;
using NHeros.src.util;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
///*****************************************************************************
/// Copyright (c) 2014 Johannes Lerch.
/// All rights reserved. This program and the accompanying materials
/// are made available under the terms of the GNU Lesser Public License v2.1
/// which accompanies this distribution, and is available at
/// http://www.gnu.org/licenses/old-licenses/gpl-2.0.html
/// 
/// Contributors:
///     Johannes Lerch - initial API and implementation
/// *****************************************************************************
/// </summary>
namespace heros.utilities
{
    public class FieldSensitiveTestHelper
	{
        //private Multimap<TestMethod, Statement> method2startPoint = HashMultimap.create();
        //private IList<NormalEdge<TestFact>> normalEdges = new List<NormalEdge<TestFact>>();
        //private IList<CallEdge<TestFact>> callEdges = new List<CallEdge<TestFact>>();
        //private IList<Call2ReturnEdge<TestFact>> call2retEdges = new List<Call2ReturnEdge<TestFact>>();
        //private IList<ReturnEdge<TestFact>> returnEdges = new List<ReturnEdge<TestFact>>();
        //private IDictionary<Statement, TestMethod> stmt2method = new Dictionary<Statement, TestMethod>();
        //private Multiset<ExpectedFlowFunction> remainingFlowFunctions = HashMultiset.create();
        private Multimap<TestMethod, Statement> method2startPoint = new Multimap<TestMethod, Statement>();
        private IList<NormalEdge<TestFact>> normalEdges = new List<NormalEdge<TestFact>>();
        private IList<CallEdge<TestFact>> callEdges = new List<CallEdge<TestFact>>();
        private IList<Call2ReturnEdge<TestFact>> call2retEdges = new List<Call2ReturnEdge<TestFact>>();
        private IList<ReturnEdge<TestFact>> returnEdges = new List<ReturnEdge<TestFact>>();
        private IDictionary<Statement, TestMethod> stmt2method = new Dictionary<Statement, TestMethod>();
        private Multiset<ExpectedFlowFunction<TestFact>> remainingFlowFunctions = new Multiset<ExpectedFlowFunction<TestFact>>();

        private TestDebugger<string, TestFact, Statement, TestMethod> debugger;

        public FieldSensitiveTestHelper(TestDebugger<string, TestFact, Statement, TestMethod> debugger)
		{
			this.debugger = debugger;
		}

		public virtual MethodHelper method(string methodName, Statement[] startingPoints, params EdgeBuilder<TestFact>[] edgeBuilders)
		{
			MethodHelper methodHelper = new MethodHelper(this, new TestMethod(methodName));
			methodHelper.startPoints(startingPoints);
			foreach (var edgeBuilder in edgeBuilders)
			{
				methodHelper.edges(edgeBuilder.edges());
			}
			return methodHelper;
		}

		public static Statement[] startPoints(params string[] startingPoints)
		{
			Statement[] result = new Statement[startingPoints.Length];
			for (int i = 0; i < result.Length; i++)
			{
				result[i] = new Statement(startingPoints[i]);
			}
			return result;
		}

		public static EdgeBuilder<TestFact>.NormalStmtBuilder normalStmt(string stmt, ExpectedFlowFunction<TestFact>[] flowFunctions)
		{
			return new EdgeBuilder<TestFact>.NormalStmtBuilder(new Statement(stmt), flowFunctions);
		}

		public static EdgeBuilder<TestFact>.CallSiteBuilder callSite(string callSite)
		{
			return new EdgeBuilder<TestFact>.CallSiteBuilder(new Statement(callSite));
		}

		public static EdgeBuilder<TestFact>.ExitStmtBuilder exitStmt(string exitStmt)
		{
			return new EdgeBuilder<TestFact>.ExitStmtBuilder(new Statement(exitStmt));
		}

		public static Statement over(string callSite)
		{
			return new Statement(callSite);
		}

		public static Statement to(string returnSite)
		{
			return new Statement(returnSite);
		}

		public static ExpectedFlowFunction<TestFact> kill(string source)
		{
			return kill(1, source);
		}

		public static ExpectedFlowFunction<TestFact> kill(int times, string source)
		{
			return new ExpectedFlowFunctionAnonymousInnerClass(times, new TestFact(source));
		}

		private class ExpectedFlowFunctionAnonymousInnerClass : ExpectedFlowFunction<TestFact>
		{
			public ExpectedFlowFunctionAnonymousInnerClass(int times, TestFact fact) : base(times, fact)
			{
			}

			public override FlowFunction_ConstrainedFact<string, TestFact, Statement, TestMethod> apply(TestFact target, AccessPathHandler<string, TestFact, Statement, TestMethod> accPathHandler)
			{
				throw new System.InvalidOperationException();
			}

			public override string transformerString()
			{
				return "";
			}
		}

        public static AccessPathTransformer readField(string fieldName)
		{
			return new AccessPathTransformerAnonymousInnerClass(fieldName);
		}

		private class AccessPathTransformerAnonymousInnerClass : AccessPathTransformer
		{
			private string fieldName;

			public AccessPathTransformerAnonymousInnerClass(string fieldName)
			{
				this.fieldName = fieldName;
			}

			public FlowFunction_ConstrainedFact<string, TestFact, Statement, TestMethod> apply(TestFact target, AccessPathHandler<string, TestFact, Statement, TestMethod> accPathHandler)
			{
				return accPathHandler.read(fieldName).generate(target);
			}

			public override string ToString()
			{
				return "read(" + fieldName + ")";
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public static AccessPathTransformer prependField(final String fieldName)
		public static AccessPathTransformer prependField(string fieldName)
		{
			return new AccessPathTransformerAnonymousInnerClass2(fieldName);
		}

		private class AccessPathTransformerAnonymousInnerClass2 : AccessPathTransformer
		{
			private string fieldName;

			public AccessPathTransformerAnonymousInnerClass2(string fieldName)
			{
				this.fieldName = fieldName;
			}

			public FlowFunction_ConstrainedFact<string, TestFact, Statement, TestMethod> apply(TestFact target, AccessPathHandler<string, TestFact, Statement, TestMethod> accPathHandler)
			{
				return accPathHandler.prepend(fieldName).generate(target);
			}

			public override string ToString()
			{
				return "prepend(" + fieldName + ")";
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public static AccessPathTransformer overwriteField(final String fieldName)
		public static AccessPathTransformer overwriteField(string fieldName)
		{
			return new AccessPathTransformerAnonymousInnerClass3(fieldName);
		}

		private class AccessPathTransformerAnonymousInnerClass3 : AccessPathTransformer
		{
			private string fieldName;

			public AccessPathTransformerAnonymousInnerClass3(string fieldName)
			{
				this.fieldName = fieldName;
			}

			public FlowFunction_ConstrainedFact<string, TestFact, Statement, TestMethod> apply(TestFact target, AccessPathHandler<string, TestFact, Statement, TestMethod> accPathHandler)
			{
				return accPathHandler.overwrite(fieldName).generate(target);
			}

			public override string ToString()
			{
				return "write(" + fieldName + ")";
			}
		}

		public static ExpectedFlowFunction<TestFact> flow(string source, AccessPathTransformer transformer, params string[] targets)
		{
			return flow(1, source, transformer, targets);
		}

		public static ExpectedFlowFunction<TestFact> flow(int times, string source, AccessPathTransformer transformer, params string[] targets)
		{
			TestFact[] targetFacts = new TestFact[targets.Length];
			for (int i = 0; i < targets.Length; i++)
			{
				targetFacts[i] = new TestFact(targets[i]);
			}
			return new ExpectedFlowFunctionAnonymousInnerClass2(times, new TestFact(source), targetFacts, transformer);
		}

		private class ExpectedFlowFunctionAnonymousInnerClass2 : ExpectedFlowFunction<TestFact>
		{
			private heros.utilities.FieldSensitiveTestHelper.AccessPathTransformer transformer;

			public ExpectedFlowFunctionAnonymousInnerClass2(int times, TestFact fact, heros.utilities.TestFact[] targetFacts, heros.utilities.FieldSensitiveTestHelper.AccessPathTransformer transformer) : base(times, fact, targetFacts)
			{
				this.transformer = transformer;
			}

			public override FlowFunction_ConstrainedFact<string, TestFact, Statement, TestMethod> apply(TestFact target, AccessPathHandler<string, TestFact, Statement, TestMethod> accPathHandler)
			{
				return transformer.apply(target, accPathHandler);
			}

			public override string transformerString()
			{
				return transformer.ToString();
			}
		}

		public interface AccessPathTransformer
		{
			FlowFunction_ConstrainedFact<string, TestFact, Statement, TestMethod> apply(TestFact target, AccessPathHandler<string, TestFact, Statement, TestMethod> accPathHandler);
		}

		public static ExpectedFlowFunction<TestFact> flow(string source, params string[] targets)
		{
			return flow(1, source, targets);
		}

		public static ExpectedFlowFunction<TestFact> flow(int times, string source, params string[] targets)
		{
			return flow(times, source, new AccessPathTransformerAnonymousInnerClass4()
		   , targets);
		}

		private class AccessPathTransformerAnonymousInnerClass4 : AccessPathTransformer
		{
			public FlowFunction_ConstrainedFact<string, TestFact, Statement, TestMethod> apply(TestFact target, AccessPathHandler<string, TestFact, Statement, TestMethod> accPathHandler)
			{
				return accPathHandler.generate(target);
			}

			public override string ToString()
			{
				return "";
			}

		}

		public static int times(int times)
		{
			return times;
		}

		public virtual InterproceduralCFG<Statement, TestMethod> buildIcfg()
		{
			return new InterproceduralCFGAnonymousInnerClass(this);
		}

		private class InterproceduralCFGAnonymousInnerClass : InterproceduralCFG<Statement, TestMethod>
		{
			private readonly FieldSensitiveTestHelper outerInstance;

			public InterproceduralCFGAnonymousInnerClass(FieldSensitiveTestHelper outerInstance)
			{
				this.outerInstance = outerInstance;
			}

            public bool isStartPoint(Statement stmt)
			{
                return outerInstance.method2startPoint.Any(
                    (kvp) => kvp.Value.Contains(stmt)
                );
			}

			public bool isFallThroughSuccessor(Statement stmt, Statement succ)
			{
				throw new InvalidOperationException();
			}

			public bool isExitStmt(Statement stmt)
			{
				foreach (var edge in outerInstance.returnEdges)
				{
					if (edge.exitStmt.Equals(stmt))
					{
						return true;
					}
				}
				return false;
			}

			public bool isCallStmt(Statement stmt)
			{
                return outerInstance.callEdges.Any(
                    (callEdge) => callEdge.callSite.Equals(stmt)
                );
			}

			public bool isBranchTarget(Statement stmt, Statement succ)
			{
				throw new System.InvalidOperationException();
			}

			public IList<Statement> getSuccsOf(Statement n)
			{
				var result = new List<Statement>();
				foreach (var edge in outerInstance.normalEdges)
				{
					if (edge.includeInCfg && edge.unit.Equals(n))
					{
						result.Add(edge.succUnit);
					}
				}
				return result;
			}

			public IList<Statement> getPredsOf(Statement stmt)
			{
				var result = new List<Statement>();
				foreach (var edge in outerInstance.normalEdges)
				{
					if (edge.includeInCfg && edge.succUnit.Equals(stmt))
					{
						result.Add(edge.unit);
					}
				}
				foreach (var edge in outerInstance.call2retEdges)
				{
					if (edge.includeInCfg && edge.returnSite.Equals(stmt))
					{
						result.Add(edge.callSite);
					}
				}
				return result;
			}

			public ICollection<Statement> getStartPointsOf(TestMethod m)
			{
				return outerInstance.method2startPoint[m];
			}

			public ICollection<Statement> getReturnSitesOfCallAt(Statement n)
			{
				ISet<Statement> result = new HashSet<Statement>();
				foreach (var edge in outerInstance.call2retEdges)
				{
					if (edge.includeInCfg && edge.callSite.Equals(n))
					{
						result.Add(edge.returnSite);
					}
				}
				foreach (var edge in outerInstance.returnEdges)
				{
					if (edge.includeInCfg && edge.callSite.Equals(n))
					{
						result.Add(edge.returnSite);
					}
				}
				return result;
			}

			public TestMethod getMethodOf(Statement n)
			{
				if (outerInstance.stmt2method.ContainsKey(n))
				{
					return outerInstance.stmt2method[n];
				}
				else
				{
					throw new System.ArgumentException("Statement " + n + " is not defined in any method.");
				}
			}

			public ISet<Statement> getCallsFromWithin(TestMethod m)
			{
				throw new System.InvalidOperationException();
			}

			public ICollection<Statement> getCallersOf(TestMethod m)
			{
                ISet<Statement> result = new HashSet<Statement>();
				foreach (var edge in outerInstance.callEdges)
				{
					if (edge.includeInCfg && edge.destinationMethod.Equals(m))
					{
						result.Add(edge.callSite);
					}
				}
				foreach (var edge in outerInstance.returnEdges)
				{
					if (edge.includeInCfg && edge.calleeMethod.Equals(m))
					{
						result.Add(edge.callSite);
					}
				}
				return result;
			}

			public ICollection<TestMethod> getCalleesOfCallAt(Statement n)
			{
				IList<TestMethod> result = new List<TestMethod>();
				foreach (var edge in outerInstance.callEdges)
				{
					if (edge.includeInCfg && edge.callSite.Equals(n))
					{
						result.Add(edge.destinationMethod);
					}
				}
				return result;
			}

			public ISet<Statement> allNonCallStartNodes()
			{
				throw new System.InvalidOperationException();
			}
		}

		//public virtual void assertAllFlowFunctionsUsed()
		//{
		//	assertTrue("These Flow Functions were expected, but never used: \n" + Joiner.on(",\n").join(remainingFlowFunctions), remainingFlowFunctions.Empty);
		//}

		private void addOrVerifyStmt2Method(Statement stmt, TestMethod m)
		{
			if (stmt2method.ContainsKey(stmt) && !stmt2method[stmt].Equals(m))
			{
				throw new System.ArgumentException("Statement " + stmt + " is used in multiple methods: " + m + " and " + stmt2method[stmt]);
			}
			stmt2method[stmt] = m;
		}

		public virtual MethodHelper method(TestMethod method)
		{
			MethodHelper h = new MethodHelper(this, method);
			return h;
		}

		public class MethodHelper
		{
			private readonly FieldSensitiveTestHelper outerInstance;
            internal TestMethod method;

			public MethodHelper(FieldSensitiveTestHelper outerInstance, TestMethod method)
			{
				this.outerInstance = outerInstance;
				this.method = method;
			}

			public virtual void edges(ICollection<Edge<TestFact>> edges)
			{
				foreach (var edge in edges)
				{
					foreach (ExpectedFlowFunction<TestFact> ff in edge.flowFunctions)
					{
						if (!outerInstance.remainingFlowFunctions.Contains(ff))
						{
							outerInstance.remainingFlowFunctions.Add(ff, ff.times);
						}
					}

					edge.accept(new EdgeVisitorAnonymousInnerClass(this, edge));
				}
			}

			private class EdgeVisitorAnonymousInnerClass : EdgeVisitor<TestFact>
			{
				private readonly MethodHelper outerInstance;

				private Edge<TestFact> edge;

				public EdgeVisitorAnonymousInnerClass(MethodHelper outerInstance, Edge<TestFact> edge)
				{
					this.outerInstance = outerInstance;
					this.edge = edge;
				}

				public void visit(ReturnEdge<TestFact> edge)
				{
					outerInstance.outerInstance.addOrVerifyStmt2Method(edge.exitStmt, outerInstance.method);
					edge.calleeMethod = outerInstance.method;
					outerInstance.outerInstance.returnEdges.Add(edge);
				}

				public void visit(Call2ReturnEdge<TestFact> edge)
				{
					outerInstance.outerInstance.addOrVerifyStmt2Method(edge.callSite, outerInstance.method);
					outerInstance.outerInstance.addOrVerifyStmt2Method(edge.returnSite, outerInstance.method);
					outerInstance.outerInstance.call2retEdges.Add(edge);
				}

				public void visit(CallEdge<TestFact> edge)
				{
					outerInstance.outerInstance.addOrVerifyStmt2Method(edge.callSite, outerInstance.method);
					outerInstance.outerInstance.callEdges.Add(edge);
				}

				public void visit(NormalEdge<TestFact> edge)
				{
					outerInstance.outerInstance.addOrVerifyStmt2Method(edge.unit, outerInstance.method);
					outerInstance.outerInstance.addOrVerifyStmt2Method(edge.succUnit, outerInstance.method);
					outerInstance.outerInstance.normalEdges.Add(edge);
				}
			}

			public virtual void startPoints(Statement[] startingPoints)
			{
				outerInstance.method2startPoint.PutAll(method, startingPoints);
			}
		}

		private static string expectedFlowFunctionsToString(ExpectedFlowFunction<TestFact>[] flowFunctions)
		{
			string result = "";
			foreach (ExpectedFlowFunction<TestFact> ff in flowFunctions)
			{
				result += ff.source + "->" + string.Join<TestFact>(",", ff.targets) + ff.transformerString() + ", ";
			}
			return result;
		}

		private static bool nullAwareEquals(object a, object b)
		{
			if (a == null)
			{
				return b == null;
			}
			else
			{
				return a.Equals(b);
			}
		}

		public virtual FlowFunctions<Statement, string, TestFact, TestMethod> flowFunctions()
		{
			return new FlowFunctionsAnonymousInnerClass(this);
		}

		private class FlowFunctionsAnonymousInnerClass : FlowFunctions<Statement, string, TestFact, TestMethod>
		{
			private readonly FieldSensitiveTestHelper outerInstance;

			public FlowFunctionsAnonymousInnerClass(FieldSensitiveTestHelper outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public FlowFunction<string, TestFact, Statement, TestMethod> getReturnFlowFunction(Statement callSite, TestMethod calleeMethod, Statement exitStmt, Statement returnSite)
			{
				foreach (var edge in outerInstance.returnEdges)
				{
					if (nullAwareEquals(callSite, edge.callSite) && edge.calleeMethod.Equals(calleeMethod) && edge.exitStmt.Equals(exitStmt) && nullAwareEquals(edge.returnSite, returnSite))
					{
						return createFlowFunction(edge);
					}
				}
				throw new Exception(string.Format("No Flow Function expected for return edge {0} -> {1} (call edge: {2} -> {3})", exitStmt, returnSite, callSite, calleeMethod));
			}

            public FlowFunction<string, TestFact, Statement, TestMethod> getNormalFlowFunction(Statement curr)
			{
				foreach (var edge in outerInstance.normalEdges)
				{
					if (edge.unit.Equals(curr))
					{
						return createFlowFunction(edge);
					}
				}
				throw new Exception(string.Format("No Flow Function expected for {0}", curr));
			}

			public FlowFunction<string, TestFact, Statement, TestMethod> getCallToReturnFlowFunction(Statement callSite, Statement returnSite)
			{
				foreach (var edge in outerInstance.call2retEdges)
				{
					if (edge.callSite.Equals(callSite) && edge.returnSite.Equals(returnSite))
					{
						return createFlowFunction(edge);
					}
				}
				throw new Exception(string.Format("No Flow Function expected for call to return edge {0} -> {1}", callSite, returnSite));
			}

			public FlowFunction<string, TestFact, Statement, TestMethod> getCallFlowFunction(Statement callStmt, TestMethod destinationMethod)
			{
				foreach (var edge in outerInstance.callEdges)
				{
					if (edge.callSite.Equals(callStmt) && edge.destinationMethod.Equals(destinationMethod))
					{
						return createFlowFunction(edge);
					}
				}
				throw new Exception(string.Format("No Flow Function expected for call {0} -> {1}", callStmt, destinationMethod));
			}

			private FlowFunction<string, TestFact, Statement, TestMethod> createFlowFunction(Edge<TestFact> edge)
			{
				return new FlowFunctionAnonymousInnerClass(this, edge);
			}

			private class FlowFunctionAnonymousInnerClass : FlowFunction<string, TestFact, Statement, TestMethod>
			{
				private readonly FlowFunctionsAnonymousInnerClass outerInstance;

				private Edge<TestFact> edge;

				public FlowFunctionAnonymousInnerClass(FlowFunctionsAnonymousInnerClass outerInstance, Edge<TestFact> edge)
				{
					this.outerInstance = outerInstance;
					this.edge = edge;
				}

				public ISet<FlowFunction_ConstrainedFact<string, TestFact, Statement, TestMethod>> computeTargets(TestFact source, AccessPathHandler<string, TestFact, Statement, TestMethod> accPathHandler)
				{
                    var result = new HashSet<FlowFunction_ConstrainedFact<string, TestFact, Statement, TestMethod>>();
					bool found = false;
					foreach (ExpectedFlowFunction<TestFact> ff in edge.flowFunctions)
					{
						if (ff.source.Equals(source))
						{
							if (outerInstance.outerInstance.remainingFlowFunctions.Remove(ff))
							{
								foreach (TestFact target in ff.targets)
								{
									result.Add(ff.apply(target, accPathHandler));
								}
								found = true;
							}
							else
							{
								throw new Exception(string.Format("Flow Function '{0}' was used multiple times on edge '{1}'", ff, edge));
							}
						}
					}
					if (found)
					{
						return result;
					}
					else
					{
						throw new Exception(string.Format("Fact '{0}' was not expected at edge '{1}'", source, edge));
					}
				}
			}
		}

		public virtual void runSolver(bool followReturnsPastSeeds, params string[] initialSeeds)
		{
			Scheduler scheduler = new Scheduler();
			FieldSensitiveIFDSSolver<string, TestFact, Statement, TestMethod, InterproceduralCFG<Statement, TestMethod>> solver = 
                new FieldSensitiveIFDSSolver<string, TestFact, Statement, TestMethod, InterproceduralCFG<Statement, TestMethod>>(
                    createTabulationProblem(followReturnsPastSeeds, initialSeeds), 
                    new FactMergeHandlerAnonymousInnerClass(this), debugger, scheduler);
			addExpectationsToDebugger();
			scheduler.runAndAwaitCompletion();

			//assertAllFlowFunctionsUsed();
		}

		private class FactMergeHandlerAnonymousInnerClass : FactMergeHandler<TestFact>
		{
			private readonly FieldSensitiveTestHelper outerInstance;

			public FactMergeHandlerAnonymousInnerClass(FieldSensitiveTestHelper outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void merge(TestFact previousFact, TestFact currentFact)
			{
			}

			public void restoreCallingContext(TestFact factAtReturnSite, TestFact factAtCallSite)
			{
			}

		}

		private void addExpectationsToDebugger()
		{
			foreach (var edge in normalEdges)
			{
				debugger.expectNormalFlow(edge.unit, expectedFlowFunctionsToString(edge.flowFunctions));
			}
			foreach (var edge in callEdges)
			{
				debugger.expectCallFlow(edge.callSite, edge.destinationMethod, expectedFlowFunctionsToString(edge.flowFunctions));
			}
			foreach (var edge in call2retEdges)
			{
				debugger.expectNormalFlow(edge.callSite, expectedFlowFunctionsToString(edge.flowFunctions));
			}
			foreach (var edge in returnEdges)
			{
				debugger.expectReturnFlow(edge.exitStmt, edge.returnSite, expectedFlowFunctionsToString(edge.flowFunctions));
			}
		}

        private IFDSTabulationProblem<Statement, string, TestFact, TestMethod, InterproceduralCFG<Statement, TestMethod>> createTabulationProblem(bool followReturnsPastSeeds, string[] initialSeeds)
		{
            InterproceduralCFG<Statement, TestMethod> icfg = buildIcfg();
            FlowFunctions<Statement, string, TestFact, TestMethod> flowFunctions = this.flowFunctions();

			return new IFDSTabulationProblemAnonymousInnerClass(this, followReturnsPastSeeds, initialSeeds, icfg, flowFunctions);
		}

		private class IFDSTabulationProblemAnonymousInnerClass : IFDSTabulationProblem<Statement, string, TestFact, TestMethod, InterproceduralCFG<Statement, TestMethod>>
		{
			private readonly FieldSensitiveTestHelper outerInstance;

			private bool _followReturnsPastSeeds;
			private string[] _initialSeeds;
			private InterproceduralCFG<Statement, TestMethod> _icfg;
			private FlowFunctions<Statement, string, TestFact, TestMethod> _flowFunctions;

			public IFDSTabulationProblemAnonymousInnerClass(FieldSensitiveTestHelper outerInstance, bool followReturnsPastSeeds, string[] initialSeeds, InterproceduralCFG<Statement, TestMethod> icfg, FlowFunctions<Statement, string, TestFact, TestMethod> flowFunctions)
			{
				this.outerInstance = outerInstance;
				this._followReturnsPastSeeds = followReturnsPastSeeds;
				this._initialSeeds = initialSeeds;
				this._icfg = icfg;
				this._flowFunctions = flowFunctions;
			}


			public bool followReturnsPastSeeds()
			{
				return _followReturnsPastSeeds;
			}

			public bool autoAddZero()
			{
				return false;
			}

			public int numThreads()
			{
				return 1;
			}

			public bool computeValues()
			{
				return false;
			}

			public FlowFunctions<Statement, string, TestFact, TestMethod> flowFunctions()
			{
				return _flowFunctions;
			}

			public InterproceduralCFG<Statement, TestMethod> interproceduralCFG()
			{
				return _icfg;
			}

			public IDictionary<Statement, ISet<TestFact>> initialSeeds()
			{
				var result = new Dictionary<Statement, ISet<TestFact>>();
				foreach (string stmt in _initialSeeds)
				{
					result[new Statement(stmt)] = new HashSet<TestFact>() { new TestFact("0") };
				}
				return result;
			}

			public TestFact zeroValue()
			{
				return new TestFact("0");
			}

			public ZeroHandler<string> zeroHandler()
			{
				return new ZeroHandlerAnonymousInnerClass(this);
			}

			private class ZeroHandlerAnonymousInnerClass : ZeroHandler<string>
			{
				private readonly IFDSTabulationProblemAnonymousInnerClass outerInstance;

				public ZeroHandlerAnonymousInnerClass(IFDSTabulationProblemAnonymousInnerClass outerInstance)
				{
					this.outerInstance = outerInstance;
				}

				public bool shouldGenerateAccessPath(AccessPath<string> accPath)
				{
					return true;
				}
			}

			public bool recordEdges()
			{
				return false;
			}
		}

		public enum TabulationProblemExchange
		{
			AsSpecified,
			ExchangeForwardAndBackward
		}
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public void runBiDiSolver(FieldSensitiveTestHelper backwardHelper, TabulationProblemExchange direction, final String...initialSeeds)
		public virtual void runBiDiSolver(FieldSensitiveTestHelper backwardHelper, TabulationProblemExchange direction, params string[] initialSeeds)
		{
			FactMergeHandler<TestFact> factMergeHandler = new FactMergeHandlerAnonymousInnerClass2(this);
			Scheduler scheduler = new Scheduler();
			BiDiFieldSensitiveIFDSSolver<string, TestFact, Statement, TestMethod, InterproceduralCFG<Statement, TestMethod>> solver = 
                (direction == TabulationProblemExchange.AsSpecified) 
                ? new BiDiFieldSensitiveIFDSSolver<string, TestFact, Statement, TestMethod, InterproceduralCFG<Statement, TestMethod>>(createTabulationProblem(true, initialSeeds), backwardHelper.createTabulationProblem(true, initialSeeds), factMergeHandler, debugger, scheduler) 
                : new BiDiFieldSensitiveIFDSSolver<string, TestFact, Statement, TestMethod, InterproceduralCFG<Statement, TestMethod>>(backwardHelper.createTabulationProblem(true, initialSeeds), createTabulationProblem(true, initialSeeds), factMergeHandler, debugger, scheduler);

			scheduler.runAndAwaitCompletion();
			//assertAllFlowFunctionsUsed();
			//backwardHelper.assertAllFlowFunctionsUsed();
		}

		private class FactMergeHandlerAnonymousInnerClass2 : FactMergeHandler<TestFact>
		{
			private readonly FieldSensitiveTestHelper outerInstance;

			public FactMergeHandlerAnonymousInnerClass2(FieldSensitiveTestHelper outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void merge(TestFact previousFact, TestFact currentFact)
			{
			}

			public void restoreCallingContext(TestFact factAtReturnSite, TestFact factAtCallSite)
			{
			}

		}
	}

}