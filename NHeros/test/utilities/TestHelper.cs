using heros.fieldsens;
using System.Collections.Generic;
using static heros.utilities.Edge;

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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class TestHelper
	{
		private Multimap<TestMethod, Statement> method2startPoint = HashMultimap.create();
		private IList<NormalEdge> normalEdges = new List<NormalEdge>();
		private IList<CallEdge> callEdges = new List<CallEdge>();
		private IList<Call2ReturnEdge> call2retEdges = new List<Call2ReturnEdge>();
		private IList<ReturnEdge> returnEdges = new List<ReturnEdge>();
		private IDictionary<Statement, TestMethod> stmt2method = new Dictionary<Statement, TestMethod>();
		private Multiset<ExpectedFlowFunction<JoinableFact>> remainingFlowFunctions = HashMultiset.create();

		public virtual MethodHelper method(string methodName, Statement[] startingPoints, params EdgeBuilder[] edgeBuilders)
		{
			MethodHelper methodHelper = new MethodHelper(this, new TestMethod(methodName));
			methodHelper.startPoints(startingPoints);
			foreach (EdgeBuilder edgeBuilder in edgeBuilders)
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

		public static EdgeBuilder.NormalStmtBuilder normalStmt(string stmt, params ExpectedFlowFunction<JoinableFact>[] flowFunctions)
		{
			return new NormalStmtBuilder(new Statement(stmt), flowFunctions);
		}

		public static EdgeBuilder.CallSiteBuilder callSite(string callSite)
		{
			return new EdgeBuilder.CallSiteBuilder(new Statement(callSite));
		}

		public static EdgeBuilder.ExitStmtBuilder exitStmt(string exitStmt)
		{
			return new EdgeBuilder.ExitStmtBuilder(new Statement(exitStmt));
		}

		public static Statement over(string callSite)
		{
			return new Statement(callSite);
		}

		public static Statement to(string returnSite)
		{
			return new Statement(returnSite);
		}

		public static ExpectedFlowFunction<JoinableFact> kill(string source)
		{
			return kill(1, source);
		}

		public static ExpectedFlowFunction<JoinableFact> kill(int times, string source)
		{
			return new ExpectedFlowFunctionAnonymousInnerClass(times, new JoinableFact(source));
		}

		private class ExpectedFlowFunctionAnonymousInnerClass : ExpectedFlowFunction<JoinableFact>
		{
			public ExpectedFlowFunctionAnonymousInnerClass(int times, JoinableFact fact) : base(times, fact)
			{
			}


			public override string transformerString()
			{
				throw new System.InvalidOperationException();
			}

            public override FlowFunction_ConstrainedFact<string, TestFact, Statement, TestMethod> apply(TestFact target, AccessPathHandler<string, TestFact, Statement, TestMethod> accPathHandler)
            {
                throw new System.InvalidOperationException();
            }
        }

		public static ExpectedFlowFunction<JoinableFact> flow(string source, params string[] targets)
		{
			return flow(1, source, targets);
		}

		public static ExpectedFlowFunction<JoinableFact> flow(int times, string source, params string[] targets)
		{
			JoinableFact[] targetFacts = new JoinableFact[targets.Length];
			for (int i = 0; i < targets.Length; i++)
			{
				targetFacts[i] = new JoinableFact(targets[i]);
			}
			return new ExpectedFlowFunctionAnonymousInnerClass2(times, new JoinableFact(source), targetFacts);
		}

		private class ExpectedFlowFunctionAnonymousInnerClass2 : ExpectedFlowFunction<JoinableFact>
		{
			public ExpectedFlowFunctionAnonymousInnerClass2(int times, JoinableFact fact, heros.utilities.JoinableFact[] targetFacts) : base(times, fact, targetFacts)
			{
			}

			public override string transformerString()
			{
				throw new System.InvalidOperationException();
			}

			public override FlowFunction_ConstrainedFact<string, TestFact, Statement, TestMethod> apply(TestFact target, AccessPathHandler<string, TestFact, Statement, TestMethod> accPathHandler)
			{
				throw new System.InvalidOperationException();
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
			private readonly TestHelper outerInstance;

			public InterproceduralCFGAnonymousInnerClass(TestHelper outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public bool isStartPoint(Statement stmt)
			{
				return outerInstance.method2startPoint.values().contains(stmt);
			}

			public bool isFallThroughSuccessor(Statement stmt, Statement succ)
			{
				throw new System.InvalidOperationException();
			}

			public bool isExitStmt(Statement stmt)
			{
				foreach (ReturnEdge edge in outerInstance.returnEdges)
				{
					if (edge.exitStmt.Equals(stmt))
					{
						return true;
					}
				}
				return false;
			}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public boolean isCallStmt(final Statement stmt)
			public bool isCallStmt(Statement stmt)
			{
				return Iterables.any(outerInstance.callEdges, new PredicateAnonymousInnerClass(this, stmt));
			}

			private class PredicateAnonymousInnerClass : Predicate<CallEdge>
			{
				private readonly InterproceduralCFGAnonymousInnerClass outerInstance;

				private heros.utilities.Statement stmt;

				public PredicateAnonymousInnerClass(InterproceduralCFGAnonymousInnerClass outerInstance, heros.utilities.Statement stmt)
				{
					this.outerInstance = outerInstance;
					this.stmt = stmt;
				}

				public bool apply(CallEdge edge)
				{
					return edge.callSite.Equals(stmt);
				}
			}

			public bool isBranchTarget(Statement stmt, Statement succ)
			{
				throw new System.InvalidOperationException();
			}

			public IList<Statement> getSuccsOf(Statement n)
			{
				LinkedList<Statement> result = new List();
				foreach (NormalEdge edge in outerInstance.normalEdges)
				{
					if (edge.includeInCfg && edge.unit.Equals(n))
					{
						result.AddLast(edge.succUnit);
					}
				}
				return result;
			}

			public IList<Statement> getPredsOf(Statement stmt)
			{
				LinkedList<Statement> result = new List();
				foreach (NormalEdge edge in outerInstance.normalEdges)
				{
					if (edge.includeInCfg && edge.succUnit.Equals(stmt))
					{
						result.AddLast(edge.unit);
					}
				}
				return result;
			}

			public ICollection<Statement> getStartPointsOf(TestMethod m)
			{
				return outerInstance.method2startPoint.get(m);
			}

			public ICollection<Statement> getReturnSitesOfCallAt(Statement n)
			{
				ISet<Statement> result = Sets.newHashSet();
				foreach (Call2ReturnEdge edge in outerInstance.call2retEdges)
				{
					if (edge.includeInCfg && edge.callSite.Equals(n))
					{
						result.Add(edge.returnSite);
					}
				}
				foreach (ReturnEdge edge in outerInstance.returnEdges)
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
				return outerInstance.stmt2method[n];
			}

			public ISet<Statement> getCallsFromWithin(TestMethod m)
			{
				throw new System.InvalidOperationException();
			}

			public ICollection<Statement> getCallersOf(TestMethod m)
			{
				ISet<Statement> result = Sets.newHashSet();
				foreach (CallEdge edge in outerInstance.callEdges)
				{
					if (edge.includeInCfg && edge.destinationMethod.Equals(m))
					{
						result.Add(edge.callSite);
					}
				}
				foreach (ReturnEdge edge in outerInstance.returnEdges)
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
				IList<TestMethod> result = new List();
				foreach (CallEdge edge in outerInstance.callEdges)
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

		public virtual void assertAllFlowFunctionsUsed()
		{
			assertTrue("These Flow Functions were expected, but never used: \n" + Joiner.on(",\n").join(remainingFlowFunctions), remainingFlowFunctions.Empty);
		}

		private void addOrVerifyStmt2Method(Statement stmt, TestMethod m)
		{
			if (stmt2method.ContainsKey(stmt) && !stmt2method[stmt].Equals(m))
			{
				throw new System.ArgumentException("Statement " + stmt + " is used in multiple TestMethods: " + m + " and " + stmt2method[stmt]);
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
			private readonly TestHelper outerInstance;


			internal TestMethod method;

			public MethodHelper(TestHelper outerInstance, TestMethod method)
			{
				this.outerInstance = outerInstance;
				this.method = method;
			}

			public virtual void edges(ICollection<Edge> edges)
			{
				foreach (Edge edge in edges)
				{
					foreach (ExpectedFlowFunction<JoinableFact> ff in edge.flowFunctions)
					{
						outerInstance.remainingFlowFunctions.add(ff, ff.times);
					}

					edge.accept(new EdgeVisitorAnonymousInnerClass(this, edge));
				}
			}

			private class EdgeVisitorAnonymousInnerClass : EdgeVisitor
			{
				private readonly MethodHelper outerInstance;

				private heros.utilities.Edge edge;

				public EdgeVisitorAnonymousInnerClass(MethodHelper outerInstance, heros.utilities.Edge edge)
				{
					this.outerInstance = outerInstance;
					this.edge = edge;
				}

				public void visit(ReturnEdge edge)
				{
					outerInstance.outerInstance.addOrVerifyStmt2Method(edge.exitStmt, outerInstance.method);
					edge.calleeMethod = outerInstance.method;
					outerInstance.outerInstance.returnEdges.Add(edge);
				}

				public void visit(Call2ReturnEdge edge)
				{
					outerInstance.outerInstance.addOrVerifyStmt2Method(edge.callSite, outerInstance.method);
					outerInstance.outerInstance.addOrVerifyStmt2Method(edge.returnSite, outerInstance.method);
					outerInstance.outerInstance.call2retEdges.Add(edge);
				}

				public void visit(CallEdge edge)
				{
					outerInstance.outerInstance.addOrVerifyStmt2Method(edge.callSite, outerInstance.method);
					outerInstance.outerInstance.callEdges.Add(edge);
				}

				public void visit(NormalEdge edge)
				{
					outerInstance.outerInstance.addOrVerifyStmt2Method(edge.unit, outerInstance.method);
					outerInstance.outerInstance.addOrVerifyStmt2Method(edge.succUnit, outerInstance.method);
					outerInstance.outerInstance.normalEdges.Add(edge);
				}
			}

			public virtual void startPoints(Statement[] startingPoints)
			{
				outerInstance.method2startPoint.putAll(method, Lists.newArrayList(startingPoints));
			}
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

		public virtual FlowFunctions<Statement, JoinableFact, TestMethod> flowFunctions()
		{
			return new FlowFunctionsAnonymousInnerClass(this);
		}

		private class FlowFunctionsAnonymousInnerClass : FlowFunctions<Statement, JoinableFact, TestMethod>
		{
			private readonly TestHelper outerInstance;

			public FlowFunctionsAnonymousInnerClass(TestHelper outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public FlowFunction<JoinableFact> getReturnFlowFunction(Statement callSite, TestMethod calleeMethod, Statement exitStmt, Statement returnSite)
			{
				foreach (ReturnEdge edge in outerInstance.returnEdges)
				{
					if (nullAwareEquals(callSite, edge.callSite) && edge.calleeMethod.Equals(calleeMethod) && edge.exitStmt.Equals(exitStmt) && nullAwareEquals(edge.returnSite, returnSite))
					{
						return createFlowFunction(edge);
					}
				}
				throw new AssertionError(string.Format("No Flow Function expected for return edge {0} -> {1} (call edge: {2} -> {3})", exitStmt, returnSite, callSite, calleeMethod));
			}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public heros.FlowFunction<JoinableFact> getNormalFlowFunction(final Statement curr, final Statement succ)
			public FlowFunction<JoinableFact> getNormalFlowFunction(Statement curr, Statement succ)
			{
				foreach (NormalEdge edge in outerInstance.normalEdges)
				{
					if (edge.unit.Equals(curr) && edge.succUnit.Equals(succ))
					{
						return createFlowFunction(edge);
					}
				}
				throw new AssertionError(string.Format("No Flow Function expected for {0} -> {1}", curr, succ));
			}

			public FlowFunction<JoinableFact> getCallToReturnFlowFunction(Statement callSite, Statement returnSite)
			{
				foreach (Call2ReturnEdge edge in outerInstance.call2retEdges)
				{
					if (edge.callSite.Equals(callSite) && edge.returnSite.Equals(returnSite))
					{
						return createFlowFunction(edge);
					}
				}
				throw new AssertionError(string.Format("No Flow Function expected for call to return edge {0} -> {1}", callSite, returnSite));
			}

			public FlowFunction<JoinableFact> getCallFlowFunction(Statement callStmt, TestMethod destinationMethod)
			{
				foreach (CallEdge edge in outerInstance.callEdges)
				{
					if (edge.callSite.Equals(callStmt) && edge.destinationMethod.Equals(destinationMethod))
					{
						return createFlowFunction(edge);
					}
				}
				throw new AssertionError(string.Format("No Flow Function expected for call {0} -> {1}", callStmt, destinationMethod));
			}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private heros.FlowFunction<JoinableFact> createFlowFunction(final Edge edge)
			private FlowFunction<JoinableFact> createFlowFunction(Edge edge)
			{
				return new FlowFunctionAnonymousInnerClass(this, edge);
			}

			private class FlowFunctionAnonymousInnerClass : FlowFunction<JoinableFact>
			{
				private readonly FlowFunctionsAnonymousInnerClass outerInstance;

				private heros.utilities.Edge edge;

				public FlowFunctionAnonymousInnerClass(FlowFunctionsAnonymousInnerClass outerInstance, heros.utilities.Edge edge)
				{
					this.outerInstance = outerInstance;
					this.edge = edge;
				}

				public ISet<JoinableFact> computeTargets(JoinableFact source)
				{
					foreach (ExpectedFlowFunction<JoinableFact> ff in edge.flowFunctions)
					{
						if (ff.source.Equals(source))
						{
							if (outerInstance.outerInstance.remainingFlowFunctions.remove(ff))
							{
								return Sets.newHashSet(ff.targets);
							}
							else
							{
								throw new AssertionError(string.Format("Flow Function '{0}' was used multiple times on edge '{1}'", ff, edge));
							}
						}
					}
					throw new AssertionError(string.Format("Fact '{0}' was not expected at edge '{1}'", source, edge));
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public void runSolver(final boolean followReturnsPastSeeds, final String...initialSeeds)
		public virtual void runSolver(bool followReturnsPastSeeds, params string[] initialSeeds)
		{
			IFDSSolver<Statement, JoinableFact, TestMethod, InterproceduralCFG<Statement, TestMethod>> solver = new IFDSSolver<Statement, JoinableFact, TestMethod, InterproceduralCFG<Statement, TestMethod>>(createTabulationProblem(followReturnsPastSeeds, initialSeeds));

			solver.solve();
			assertAllFlowFunctionsUsed();
		}


		public enum TabulationProblemExchange
		{
			AsSpecified,
			ExchangeForwardAndBackward
		}
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public void runBiDiSolver(TestHelper backwardHelper, TabulationProblemExchange direction, final String...initialSeeds)
		public virtual void runBiDiSolver(TestHelper backwardHelper, TabulationProblemExchange direction, params string[] initialSeeds)
		{
			BiDiIFDSSolver<Statement, JoinableFact, TestMethod, InterproceduralCFG<Statement, TestMethod>> solver = direction == TabulationProblemExchange.AsSpecified ? new BiDiIFDSSolver<Statement, JoinableFact, TestMethod, InterproceduralCFG<Statement, TestMethod>>(createTabulationProblem(true, initialSeeds), backwardHelper.createTabulationProblem(true, initialSeeds)) : new BiDiIFDSSolver<Statement, JoinableFact, TestMethod, InterproceduralCFG<Statement, TestMethod>>(backwardHelper.createTabulationProblem(true, initialSeeds), createTabulationProblem(true, initialSeeds));

			solver.solve();
			assertAllFlowFunctionsUsed();
			backwardHelper.assertAllFlowFunctionsUsed();
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private heros.IFDSTabulationProblem<Statement, JoinableFact, TestMethod, heros.InterproceduralCFG<Statement, TestMethod>> createTabulationProblem(final boolean followReturnsPastSeeds, final String[] initialSeeds)
		private IFDSTabulationProblem<Statement, JoinableFact, TestMethod, InterproceduralCFG<Statement, TestMethod>> createTabulationProblem(bool followReturnsPastSeeds, string[] initialSeeds)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final heros.InterproceduralCFG<Statement, TestMethod> icfg = buildIcfg();
			InterproceduralCFG<Statement, TestMethod> icfg = buildIcfg();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final heros.FlowFunctions<Statement, JoinableFact, TestMethod> flowFunctions = flowFunctions();
			FlowFunctions<Statement, JoinableFact, TestMethod> flowFunctions = flowFunctions();

			return new IFDSTabulationProblemAnonymousInnerClass(this, followReturnsPastSeeds, initialSeeds, icfg, flowFunctions);
		}

		private class IFDSTabulationProblemAnonymousInnerClass : IFDSTabulationProblem<Statement, JoinableFact, TestMethod, InterproceduralCFG<Statement, TestMethod>>
		{
			private readonly TestHelper outerInstance;

			private bool followReturnsPastSeeds;
			private string[] initialSeeds;
			private InterproceduralCFG<Statement, TestMethod> icfg;
			private FlowFunctions<Statement, JoinableFact, TestMethod> flowFunctions;

			public IFDSTabulationProblemAnonymousInnerClass(TestHelper outerInstance, bool followReturnsPastSeeds, string[] initialSeeds, InterproceduralCFG<Statement, TestMethod> icfg, FlowFunctions<Statement, JoinableFact, TestMethod> flowFunctions)
			{
				this.outerInstance = outerInstance;
				this.followReturnsPastSeeds = followReturnsPastSeeds;
				this.initialSeeds = initialSeeds;
				this.icfg = icfg;
				this.flowFunctions = flowFunctions;
			}


			public bool followReturnsPastSeeds()
			{
				return followReturnsPastSeeds;
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

			public FlowFunctions<Statement, JoinableFact, TestMethod> flowFunctions()
			{
				return flowFunctions;
			}

			public InterproceduralCFG<Statement, TestMethod> interproceduralCFG()
			{
				return icfg;
			}

			public IDictionary<Statement, ISet<JoinableFact>> initialSeeds()
			{
				IDictionary<Statement, ISet<JoinableFact>> result = new Dictionary();
				foreach (string stmt in initialSeeds)
				{
					result[new Statement(stmt)] = Sets.newHashSet(new JoinableFact("0"));
				}
				return result;
			}

			public JoinableFact zeroValue()
			{
				return new JoinableFact("0");
			}

			public bool recordEdges()
			{
				return false;
			}
		}
	}

}