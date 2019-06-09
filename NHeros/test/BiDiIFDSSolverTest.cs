using heros.utilities;
using System.Collections.Generic;

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
namespace heros
{
	using static heros.utilities.TestHelper;
	using TabulationProblemExchange = heros.utilities.TestHelper.TabulationProblemExchange;


//ORIGINAL LINE: @RunWith(Parameterized.class) public class BiDiIFDSSolverTest
	public class BiDiIFDSSolverTest
	{
		private TestHelper forwardHelper;
		private TestHelper backwardHelper;
		private TestHelper.TabulationProblemExchange exchange;

		public BiDiIFDSSolverTest(TestHelper.TabulationProblemExchange exchange)
		{
			this.exchange = exchange;
			forwardHelper = new TestHelper();
			backwardHelper = new TestHelper();
		}


//ORIGINAL LINE: @Parameters(name="{0}") public static java.util.Collection<Object[]> parameters()
		public static ICollection<object[]> parameters()
		{
            var result = new List<object[]>
            {
                new object[] { TestHelper.TabulationProblemExchange.AsSpecified },
                new object[] { TestHelper.TabulationProblemExchange.ExchangeForwardAndBackward }
            };
            return result;
		}


//ORIGINAL LINE: @Test public void happyPath()
		public virtual void happyPath()
		{
			forwardHelper.method("foo", startPoints("a"), normalStmt("a").succ("b"), normalStmt("b", flow("0", "1")).succ("c"), exitStmt("c").expectArtificalFlow(flow("1")));

			backwardHelper.method("foo", startPoints("c"), normalStmt("c").succ("b"), normalStmt("b", flow("0", "2")).succ("a"), exitStmt("a").expectArtificalFlow(flow("2")));

			forwardHelper.runBiDiSolver(backwardHelper, exchange, "b");
		}


//ORIGINAL LINE: @Test public void unbalancedReturnsInBothDirections()
		public virtual void unbalancedReturnsInBothDirections()
		{
			forwardHelper.method("foo", startPoints("a"), normalStmt("a").succ("b"), normalStmt("b", flow("0", "1")).succ("c"), exitStmt("c").returns(over("y"), to("z"), flow("1", "2")));

			forwardHelper.method("bar", startPoints(), exitStmt("z").expectArtificalFlow(kill("2")));

			backwardHelper.method("foo", startPoints("c"), normalStmt("c").succ("b"), normalStmt("b", flow("0", "2")).succ("a"), exitStmt("a").returns(over("y"), to("x"), flow("2", "3")));

			backwardHelper.method("bar", startPoints(), exitStmt("x").expectArtificalFlow(kill("3")));

			forwardHelper.runBiDiSolver(backwardHelper, exchange, "b");
		}


//ORIGINAL LINE: @Test public void unbalancedReturnsNonMatchingCallSites()
		public virtual void unbalancedReturnsNonMatchingCallSites()
		{
			forwardHelper.method("foo", startPoints("a"), normalStmt("a").succ("b"), normalStmt("b", flow("0", "1")).succ("c"), exitStmt("c").returns(over("y1"), to("z"), flow("1", "2")));

			forwardHelper.method("bar", startPoints(), exitStmt("z").expectArtificalFlow());

			backwardHelper.method("foo", startPoints("c"), normalStmt("c").succ("b"), normalStmt("b", flow("0", "2")).succ("a"), exitStmt("a").returns(over("y2"), to("x"), flow("2", "3")));

			backwardHelper.method("bar", startPoints(), exitStmt("x").expectArtificalFlow());

			forwardHelper.runBiDiSolver(backwardHelper, exchange, "b");
		}


//ORIGINAL LINE: @Test public void returnsOnlyOneDirectionAndStops()
		public virtual void returnsOnlyOneDirectionAndStops()
		{
			forwardHelper.method("foo", startPoints("a"), normalStmt("a").succ("b"), normalStmt("b", flow("0", "1")).succ("c"), exitStmt("c").returns(over("y"), to("z"), flow("1", "2")));

			forwardHelper.method("bar", startPoints(), exitStmt("z").expectArtificalFlow());

			backwardHelper.method("foo", startPoints("c"), normalStmt("c").succ("b"), normalStmt("b", kill("0")).succ("a"), exitStmt("a").returns(over("y"), to("x")));

			backwardHelper.method("bar", startPoints(), exitStmt("x").expectArtificalFlow());

			forwardHelper.runBiDiSolver(backwardHelper, exchange, "b");
		}


//ORIGINAL LINE: @Test public void reuseSummary()
		public virtual void reuseSummary()
		{
			forwardHelper.method("foo", startPoints(), normalStmt("a", flow("0", "1")).succ("b"), callSite("b").calls("bar", flow("1", "2")).retSite("c", kill("1")), callSite("c").calls("bar", flow("1", "2")).retSite("d", kill("1")), exitStmt("d").expectArtificalFlow(kill("1")));

			forwardHelper.method("bar", startPoints("x"), normalStmt("x", flow("2", "2")).succ("y"), exitStmt("y").returns(over("b"), to("c"), flow("2", "1")).returns(over("c"), to("d"), flow("2", "1")));

			backwardHelper.method("foo", startPoints(), exitStmt("a").expectArtificalFlow(kill("0")));

			forwardHelper.runBiDiSolver(backwardHelper, exchange, "a");
		}


//ORIGINAL LINE: @Test public void multipleSeedsPreventReusingSummary()
		public virtual void multipleSeedsPreventReusingSummary()
		{
			forwardHelper.method("foo", startPoints(), normalStmt("a1", flow("0", "1")).succ("b"), normalStmt("a2", flow("0", "1")).succ("b"), callSite("b").calls("bar", flow(times(2), "1", "2")).retSite("c", kill(times(2), "1")), callSite("c").calls("bar", flow(times(2), "1", "2")).retSite("d", kill(times(2), "1")), exitStmt("d").expectArtificalFlow(kill(times(2), "1")));

			forwardHelper.method("bar", startPoints("x"), normalStmt("x", flow("2", "2")).succ("y"), exitStmt("y").returns(over("b"), to("c"), flow(times(2), "2", "1")).returns(over("c"), to("d"), flow(times(2), "2", "1")));

			backwardHelper.method("foo", startPoints(), exitStmt("a1").expectArtificalFlow(kill("0")), exitStmt("a2").expectArtificalFlow(kill("0")));

			forwardHelper.runBiDiSolver(backwardHelper, exchange, "a1", "a2");
		}


//ORIGINAL LINE: @Test public void dontResumeIfReturnFlowIsKilled()
		public virtual void dontResumeIfReturnFlowIsKilled()
		{
			forwardHelper.method("foo", startPoints(), normalStmt("a", flow("0", "1")).succ("b"), exitStmt("b").returns(over("cs"), to("y"), kill("1")));

			forwardHelper.method("bar", startPoints(), normalStmt("y").succ("z"));

			backwardHelper.method("foo", startPoints(), normalStmt("a", flow("0", "1")).succ("c"), exitStmt("c").returns(over("cs"), to("x"), flow("1", "2")));

			backwardHelper.method("bar", startPoints(), normalStmt("x").succ("z"));

			forwardHelper.runBiDiSolver(backwardHelper, exchange, "a");
		}
	}

}