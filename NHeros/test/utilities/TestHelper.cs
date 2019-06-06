using heros.fieldsens;
using heros.solver;
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
	public class TestHelper
	{
		private IDictionary<TestMethod, ISet<Statement>> method2startPoint = new Dictionary<TestMethod, ISet<Statement>>();
		private IList<NormalEdge<JoinableFact>> normalEdges = new List<NormalEdge<JoinableFact>>();
		private IList<CallEdge<JoinableFact>> callEdges = new List<CallEdge<JoinableFact>>();
		private IList<Call2ReturnEdge<JoinableFact>> call2retEdges = new List<Call2ReturnEdge<JoinableFact>>();
		private IList<ReturnEdge<JoinableFact>> returnEdges = new List<ReturnEdge<JoinableFact>>();
		private IDictionary<Statement, TestMethod> stmt2method = new Dictionary<Statement, TestMethod>();
        private IDictionary<ExpectedFlowFunction<JoinableFact>, int> remainingFlowFunctions = new Dictionary<ExpectedFlowFunction<JoinableFact>, int>();


        public virtual MethodHelper method(string methodName, Statement[] startingPoints, params EdgeBuilder<JoinableFact>[] edgeBuilders)
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

		public static EdgeBuilder<JoinableFact>.NormalStmtBuilder normalStmt(string stmt, params ExpectedFlowFunction<JoinableFact>[] flowFunctions)
		{
			return new EdgeBuilder<JoinableFact>.NormalStmtBuilder(new Statement(stmt), flowFunctions);
		}

		public static EdgeBuilder<JoinableFact>.CallSiteBuilder callSite(string callSite)
		{
			return new EdgeBuilder<JoinableFact>.CallSiteBuilder(new Statement(callSite));
		}

		public static EdgeBuilder<JoinableFact>.ExitStmtBuilder exitStmt(string exitStmt)
		{
			return new EdgeBuilder<JoinableFact>.ExitStmtBuilder(new Statement(exitStmt));
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

            public override FlowFunction_ConstrainedFact<string, JoinableFact, Statement, TestMethod> apply(JoinableFact target, AccessPathHandler<string, JoinableFact, Statement, TestMethod> accPathHandler)
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

			public override FlowFunction_ConstrainedFact<string, JoinableFact, Statement, TestMethod> apply(JoinableFact target, AccessPathHandler<string, JoinableFact, Statement, TestMethod> accPathHandler)
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
                return outerInstance.method2startPoint.Any(
                    (kvp) => kvp.Value.Contains(stmt)
                );
			}

			public bool isFallThroughSuccessor(Statement stmt, Statement succ)
			{
				throw new System.InvalidOperationException();
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
                    (edge) => edge.callSite.Equals(stmt)
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
				return result;
			}

			public ICollection<Statement> getStartPointsOf(TestMethod m)
			{
                if (!outerInstance.method2startPoint.ContainsKey(m))
                {
                    var ret = new HashSet<Statement>();
                    outerInstance.method2startPoint.Add(m, ret);
                    return ret;
                }
                else
                {
                    return outerInstance.method2startPoint[m];
                }
			}

			public ICollection<Statement> getReturnSitesOfCallAt(Statement n)
			{
				var result = new HashSet<Statement>();
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
				return outerInstance.stmt2method[n];
			}

			public ISet<Statement> getCallsFromWithin(TestMethod m)
			{
				throw new System.InvalidOperationException();
			}

			public ICollection<Statement> getCallersOf(TestMethod m)
			{
                var result = new HashSet<Statement>();
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
				var result = new List<TestMethod>();
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

			public virtual void edges(ICollection<Edge<JoinableFact>> edges)
			{
				foreach (var edge in edges)
				{
					foreach (var ff in edge.flowFunctions)
					{
                        if (outerInstance.remainingFlowFunctions.ContainsKey(ff))
                        {
                            outerInstance.remainingFlowFunctions[ff] = outerInstance.remainingFlowFunctions[ff] + ff.times;
                        }
                        else
                        {
                            outerInstance.remainingFlowFunctions.Add(
                                ff,
                                ff.times + outerInstance.remainingFlowFunctions[ff]
                            );
                        }
					}

					edge.accept(new EdgeVisitorAnonymousInnerClass(this, edge));
				}
			}

			private class EdgeVisitorAnonymousInnerClass : EdgeVisitor<JoinableFact>
            {
				private readonly MethodHelper outerInstance;

				private heros.utilities.Edge<JoinableFact> edge;

				public EdgeVisitorAnonymousInnerClass(MethodHelper outerInstance, heros.utilities.Edge<JoinableFact> edge)
				{
					this.outerInstance = outerInstance;
					this.edge = edge;
				}

				public void visit(ReturnEdge<JoinableFact> edge)
				{
					outerInstance.outerInstance.addOrVerifyStmt2Method(edge.exitStmt, outerInstance.method);
					edge.calleeMethod = outerInstance.method;
					outerInstance.outerInstance.returnEdges.Add(edge);
				}

				public void visit(Call2ReturnEdge<JoinableFact> edge)
				{
					outerInstance.outerInstance.addOrVerifyStmt2Method(edge.callSite, outerInstance.method);
					outerInstance.outerInstance.addOrVerifyStmt2Method(edge.returnSite, outerInstance.method);
					outerInstance.outerInstance.call2retEdges.Add(edge);
				}

				public void visit(CallEdge<JoinableFact> edge)
				{
					outerInstance.outerInstance.addOrVerifyStmt2Method(edge.callSite, outerInstance.method);
					outerInstance.outerInstance.callEdges.Add(edge);
				}

				public void visit(NormalEdge<JoinableFact> edge)
                {
					outerInstance.outerInstance.addOrVerifyStmt2Method(edge.unit, outerInstance.method);
					outerInstance.outerInstance.addOrVerifyStmt2Method(edge.succUnit, outerInstance.method);
					outerInstance.outerInstance.normalEdges.Add(edge);
				}
			}

			public virtual void startPoints(Statement[] startingPoints)
			{
                ISet<Statement> statements;
                if (!outerInstance.method2startPoint.ContainsKey(method))
                {
                    statements = new HashSet<Statement>();
                    outerInstance.method2startPoint.Add(method, statements);
                }
                else
                {
                    statements = outerInstance.method2startPoint[method];
                }

                statements.UnionWith(startingPoints);
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
				foreach (var edge in outerInstance.returnEdges)
				{
					if (nullAwareEquals(callSite, edge.callSite) && edge.calleeMethod.Equals(calleeMethod) && edge.exitStmt.Equals(exitStmt) && nullAwareEquals(edge.returnSite, returnSite))
					{
						return createFlowFunction(edge);
					}
				}
				throw new Exception(string.Format("No Flow Function expected for return edge {0} -> {1} (call edge: {2} -> {3})", exitStmt, returnSite, callSite, calleeMethod));
			}

			public FlowFunction<JoinableFact> getNormalFlowFunction(Statement curr, Statement succ)
			{
				foreach (var edge in outerInstance.normalEdges)
				{
					if (edge.unit.Equals(curr) && edge.succUnit.Equals(succ))
					{
						return createFlowFunction(edge);
					}
				}
				throw new Exception(string.Format("No Flow Function expected for {0} -> {1}", curr, succ));
			}

			public FlowFunction<JoinableFact> getCallToReturnFlowFunction(Statement callSite, Statement returnSite)
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

			public FlowFunction<JoinableFact> getCallFlowFunction(Statement callStmt, TestMethod destinationMethod)
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

			private FlowFunction<JoinableFact> createFlowFunction(Edge<JoinableFact> edge)
			{
				return new FlowFunctionAnonymousInnerClass(this, edge);
			}

			private class FlowFunctionAnonymousInnerClass : FlowFunction<JoinableFact>
			{
				private readonly FlowFunctionsAnonymousInnerClass outerInstance;

				private Edge<JoinableFact> edge;

				public FlowFunctionAnonymousInnerClass(FlowFunctionsAnonymousInnerClass outerInstance, Edge<JoinableFact> edge)
				{
					this.outerInstance = outerInstance;
					this.edge = edge;
				}

				public ISet<JoinableFact> computeTargets(JoinableFact source)
				{
					foreach (var ff in edge.flowFunctions)
					{
						if (ff.source.Equals(source))
						{
							if (outerInstance.outerInstance.remainingFlowFunctions.ContainsKey(ff))
							{
                                outerInstance.outerInstance.remainingFlowFunctions.Remove(ff);
                                return new HashSet<JoinableFact>(ff.targets);
							}
							else
							{
								throw new Exception(string.Format("Flow Function '{0}' was used multiple times on edge '{1}'", ff, edge));
							}
						}
					}
					throw new Exception(string.Format("Fact '{0}' was not expected at edge '{1}'", source, edge));
				}
			}
		}

        public virtual void runSolver(bool followReturnsPastSeeds, params string[] initialSeeds)
		{
			IFDSSolver<Statement, JoinableFact, TestMethod, InterproceduralCFG<Statement, TestMethod>> solver = new IFDSSolver<Statement, JoinableFact, TestMethod, InterproceduralCFG<Statement, TestMethod>>(createTabulationProblem(followReturnsPastSeeds, initialSeeds));

			solver.solve();
			//assertAllFlowFunctionsUsed();
		}


		public enum TabulationProblemExchange
		{
			AsSpecified,
			ExchangeForwardAndBackward
		}

        public virtual void runBiDiSolver(TestHelper backwardHelper, TabulationProblemExchange direction, params string[] initialSeeds)
		{
            var solver = (direction == TabulationProblemExchange.AsSpecified) 
                ? new BiDiIFDSSolver<Statement, JoinableFact, TestMethod, InterproceduralCFG<Statement, TestMethod>>(createTabulationProblem(true, initialSeeds), backwardHelper.createTabulationProblem(true, initialSeeds)) 
                : new BiDiIFDSSolver<Statement, JoinableFact, TestMethod, InterproceduralCFG<Statement, TestMethod>>(backwardHelper.createTabulationProblem(true, initialSeeds), createTabulationProblem(true, initialSeeds));

			solver.solve();
			//assertAllFlowFunctionsUsed();
			//backwardHelper.assertAllFlowFunctionsUsed();
		}

		private IFDSTabulationProblem<Statement, JoinableFact, TestMethod, InterproceduralCFG<Statement, TestMethod>> createTabulationProblem(bool followReturnsPastSeeds, string[] initialSeeds)
		{
			InterproceduralCFG<Statement, TestMethod> icfg = buildIcfg();

			return new IFDSTabulationProblemAnonymousInnerClass(this, followReturnsPastSeeds, initialSeeds, icfg, this.flowFunctions());
		}

		private class IFDSTabulationProblemAnonymousInnerClass 
            : IFDSTabulationProblem<Statement, JoinableFact, TestMethod, InterproceduralCFG<Statement, TestMethod>>
		{
			private readonly TestHelper outerInstance;

			private bool _followReturnsPastSeeds;
			private string[] _initialSeeds;
			private InterproceduralCFG<Statement, TestMethod> icfg;
			private FlowFunctions<Statement, JoinableFact, TestMethod> _flowFunctions;

			public IFDSTabulationProblemAnonymousInnerClass(
                TestHelper outerInstance, 
                bool followReturnsPastSeeds, 
                string[] initialSeeds, 
                InterproceduralCFG<Statement, TestMethod> icfg, 
                FlowFunctions<Statement, JoinableFact, TestMethod> flowFunctions
            )
			{
				this.outerInstance = outerInstance;
				this._followReturnsPastSeeds = followReturnsPastSeeds;
				this._initialSeeds = initialSeeds;
				this.icfg = icfg;
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

			public FlowFunctions<Statement, JoinableFact, TestMethod> flowFunctions()
			{
				return this._flowFunctions;
			}

			public InterproceduralCFG<Statement, TestMethod> interproceduralCFG()
			{
				return icfg;
			}

			public IDictionary<Statement, ISet<JoinableFact>> initialSeeds()
			{
				IDictionary<Statement, ISet<JoinableFact>> result = new Dictionary<Statement, ISet<JoinableFact>>();
				foreach (string stmt in _initialSeeds)
				{
					result[new Statement(stmt)] = new HashSet<JoinableFact> { new JoinableFact("0") };
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