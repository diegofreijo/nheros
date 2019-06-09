using System;

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
namespace heros.fieldsens
{


	using FieldSensitiveTestHelper = heros.utilities.FieldSensitiveTestHelper;
	using Statement = heros.utilities.Statement;
	using TestDebugger = heros.utilities.TestDebugger;
	using TestFact = heros.utilities.TestFact;
	using TestMethod = heros.utilities.TestMethod;

	using Before = org.junit.Before;
	using Ignore = org.junit.Ignore;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using TestWatcher = org.junit.rules.TestWatcher;

	using static heros.utilities.FieldSensitiveTestHelper;

	public class FieldSensitiveIFDSSolverTest
	{

		private FieldSensitiveTestHelper helper;
		private TestDebugger<string, TestFact, Statement, TestMethod> debugger;


//ORIGINAL LINE: @Before public void before()
		public virtual void before()
		{
			Console.Error.WriteLine("-----");
			debugger = new TestDebugger<string, TestFact, Statement, TestMethod>();
			helper = new FieldSensitiveTestHelper(debugger);
		}


//ORIGINAL LINE: @Rule public org.junit.rules.TestWatcher watcher = new org.junit.rules.TestWatcher()
		public TestWatcher watcher = new TestWatcherAnonymousInnerClass();

		private class TestWatcherAnonymousInnerClass : TestWatcher
		{
			protected internal void failed(Exception e, org.junit.runner.Description description)
			{
				outerInstance.debugger.writeJsonDebugFile("debug/" + description.MethodName + ".json");
				Console.Error.WriteLine("---failed: " + description.MethodName + " ----");
			};
		}


//ORIGINAL LINE: @Test public void fieldReadAndWrite()
		public virtual void fieldReadAndWrite()
		{
			helper.method("bar", startPoints("a"), normalStmt("a", flow("0", "1")).succ("b"), normalStmt("b", flow("1", prependField("f"), "2")).succ("c"), normalStmt("c", flow("2", "2")).succ("d"), normalStmt("d", flow("2", readField("f"), "4")).succ("e"), normalStmt("e", kill("4")).succ("f"));

			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void createSummaryForBaseValue()
		public virtual void createSummaryForBaseValue()
		{
			helper.method("bar", startPoints("a"), normalStmt("a", flow("0", "1")).succ("b"), normalStmt("b", flow("1", overwriteField("field"), "2")).succ("c"), callSite("c").calls("foo", flow("2", "3")));

			helper.method("foo",startPoints("d"), normalStmt("d", flow("3", "4")).succ("e"), normalStmt("e", flow("4","4")).succ("f"));
			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void reuseSummaryForBaseValue()
		public virtual void reuseSummaryForBaseValue()
		{
			helper.method("bar", startPoints("a"), normalStmt("a", flow("0", "1")).succ("b"), normalStmt("b", flow("1", overwriteField("field"), "2")).succ("c"), callSite("c").calls("foo", flow("2", "3")).retSite("retC", flow("2", "2")));

			helper.method("foo",startPoints("d"), normalStmt("d", flow("3", "4")).succ("e"), normalStmt("e", flow("4","4")).succ("f"), exitStmt("f").returns(over("c"), to("retC"), flow("4", "5")).returns(over("g"), to("retG"), flow("4", "6")));

			helper.method("xyz", startPoints("g"), callSite("g").calls("foo", flow("0", overwriteField("anotherField"), "3")).retSite("retG", kill("0")));

			helper.runSolver(false, "a", "g");
		}


//ORIGINAL LINE: @Test public void hold()
		public virtual void hold()
		{
			helper.method("bar", startPoints("a"), normalStmt("a", flow("0", "1")).succ("b"), normalStmt("b", flow("1", prependField("field"), "2")).succ("c"), callSite("c").calls("foo", flow("2", "3")));

			helper.method("foo", startPoints("d"), normalStmt("d", flow("3", readField("notfield"), "5"), flow("3", "3")).succ("e"), normalStmt("e", flow("3","4")).succ("f"));
			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void holdAndResume()
		public virtual void holdAndResume()
		{
			helper.method("bar", startPoints("a"), normalStmt("a", flow("0", "1")).succ("b"), normalStmt("b", flow("1", prependField("field"), "2")).succ("c"), callSite("c").calls("foo", flow("2", "3")).retSite("rs", kill("2")), callSite("rs").calls("foo", flow("5", prependField("notfield"), "3")));

			helper.method("foo",startPoints("d"), normalStmt("d", flow("3", "3"), flow("3", readField("notfield"), "6")).succ("e"), normalStmt("e", flow("3","4"), kill("6")).succ("f"), exitStmt("f").returns(over("c"), to("rs"), flow("4", "5")));

			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void doNotHoldIfInterestedTransitiveCallerExists()
		public virtual void doNotHoldIfInterestedTransitiveCallerExists()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", prependField("f"), "1")).succ("b"), callSite("b").calls("bar", flow("1", "2")));

			helper.method("bar", startPoints("c"), callSite("c").calls("xyz", flow("2", "3")));

			helper.method("xyz", startPoints("d"), normalStmt("d", flow("3", readField("f"), "4")).succ("e"), normalStmt("e", kill("4")).succ("f"));

			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void prefixFactOfSummaryIgnored()
		public virtual void prefixFactOfSummaryIgnored()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0","1")).succ("b"), callSite("b").calls("bar", flow("1", prependField("f"), "2")).retSite("e", kill("1")), callSite("e").calls("bar", flow("4", "2")).retSite("f", kill("4")), normalStmt("f", kill("5")).succ("g"));

			helper.method("bar", startPoints("c"), normalStmt("c", flow("2", readField("f"), "3")).succ("d"), exitStmt("d").returns(over("b"), to("e"), flow(2, "3", "4")).returns(over("e"), to("f"), flow(2, "3", "5")));

			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void doNotPauseZeroSources()
		public virtual void doNotPauseZeroSources()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", readField("f"), "1")).succ("b"), normalStmt("b", kill("1")).succ("c"));

			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void loopAndMerge()
		public virtual void loopAndMerge()
		{
			helper.method("foo", startPoints("a0"), normalStmt("a0", flow("0", "1")).succ("a1"), callSite("a1").calls("bar", flow("1", prependField("g"), "1")));

			helper.method("bar", startPoints("b"), normalStmt("b", flow("1", prependField("f"), "1")).succ("c"), normalStmt("c", flow("1", "1")).succ("b").succ("d"), normalStmt("d", flow("1", readField("f"), "2")).succ("e"), normalStmt("e", kill("2")).succ("f"));

			helper.runSolver(false, "a0");
		}


//ORIGINAL LINE: @Test @Ignore("not implemented optimization") public void loopAndMergeExclusion()
		public virtual void loopAndMergeExclusion()
		{
			helper.method("foo", startPoints("a0"), normalStmt("a0", flow("0", "1")).succ("a1"), callSite("a1").calls("bar", flow("1", "1.f")));

			helper.method("bar", startPoints("b"), normalStmt("b", flow("1", "1", "1^f")).succ("c"), normalStmt("c", flow("1", "1")).succ("d").succ("b"), normalStmt("d", flow("1", overwriteField("f"), "2")).succ("e"), normalStmt("e", kill("2")).succ("f"));


			helper.runSolver(false, "a0");
		}


//ORIGINAL LINE: @Test public void pauseOnOverwrittenFieldOfInterest()
		public virtual void pauseOnOverwrittenFieldOfInterest()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", prependField("f"), "1")).succ("b"), callSite("b").calls("bar", flow("1", "2")));

			helper.method("bar", startPoints("c"), normalStmt("c", flow("2", overwriteField("f"), "2")).succ("d"), normalStmt("d").succ("e")); //only interested in 2.f, but f excluded so this should not be reached

			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void pauseOnOverwrittenFieldOfInterest2()
		public virtual void pauseOnOverwrittenFieldOfInterest2()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", prependField("g"), "1")).succ("b"), callSite("b").calls("bar", flow("1", prependField("f"), "2")));

			helper.method("bar", startPoints("c"), normalStmt("c", flow("2", overwriteField("f"), "2")).succ("d"), normalStmt("d").succ("e")); //only interested in 2.f.g, but f excluded so this should not be reached

			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void doNotPauseOnOverwrittenFieldOfInterestedPrefix()
		public virtual void doNotPauseOnOverwrittenFieldOfInterestedPrefix()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", prependField("f"), "1")).succ("b"), callSite("b").calls("bar", flow("1", prependField("g"), "2")));

			helper.method("bar", startPoints("c"), normalStmt("c", flow("2", overwriteField("f"), "2")).succ("d"), normalStmt("d", kill("2")).succ("e"));

			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void pauseOnTransitiveExclusion()
		public virtual void pauseOnTransitiveExclusion()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", prependField("f"), "1")).succ("b"), callSite("b").calls("bar", flow("1", "2")));

			helper.method("bar", startPoints("c"), callSite("c").calls("xyz", flow("2", "3")));

			helper.method("xyz", startPoints("d"), normalStmt("d", flow("3", overwriteField("f"), "3")).succ("e"), normalStmt("e").succ("f"));

			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void resumePausedOnTransitiveExclusion()
		public virtual void resumePausedOnTransitiveExclusion()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", prependField("f"), "1")).succ("b"), callSite("b").calls("bar", flow("1", "2")));

			helper.method("bar", startPoints("c"), callSite("c").calls("xyz", flow("2", "3")));

			helper.method("xyz", startPoints("d"), normalStmt("d", flow("3", overwriteField("f"), "3"), flow("3", "4")).succ("e"), callSite("e").calls("bar", flow("4", prependField("g"), "2"), kill("3")));

			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void resumeEdgePausedOnOverwrittenField()
		public virtual void resumeEdgePausedOnOverwrittenField()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", prependField("f"), "1")).succ("b"), callSite("b").calls("bar", flow("1", "2")).retSite("e", kill("1")), callSite("e").calls("bar", flow("4", prependField("g"), "2")).retSite("f", kill("4")));

			helper.method("bar", startPoints("c"), normalStmt("c", flow("2", overwriteField("f"), "2"), flow("2", "3")).succ("d"), exitStmt("d").returns(over("b"), to("e"), flow("3", "4")).returns(over("e"), to("f"), kill("3"), kill("2")));

			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void resumeEdgePausedOnOverwrittenFieldForPrefixes()
		public virtual void resumeEdgePausedOnOverwrittenFieldForPrefixes()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", prependField("f"), "1")).succ("b"), callSite("b").calls("bar", flow("1", "2")).retSite("e", kill("1")), normalStmt("e", flow("4", readField("f"), "2")).succ("f"), callSite("f").calls("bar", flow("2", "2")).retSite("g", kill("2")));

			helper.method("bar", startPoints("c"), normalStmt("c", flow("2", overwriteField("f"), "2"), flow("2", "3")).succ("d"), exitStmt("d").returns(over("b"), to("e"), flow("3", "4")).returns(over("f"), to("g"), kill("3"), kill("2")));

			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void exclusionOnPotentiallyInterestedCaller()
		public virtual void exclusionOnPotentiallyInterestedCaller()
		{
			helper.method("foo", startPoints("sp"), normalStmt("sp", flow("0", "1")).succ("a"), callSite("a").calls("bar", flow("1", overwriteField("f"), "1")).retSite("d", kill("1")));

			helper.method("bar", startPoints("b"), normalStmt("b", flow("1", readField("f"), "2")).succ("c"), exitStmt("c").returns(over("a"), to("d")));

			helper.runSolver(false, "sp");
		}


//ORIGINAL LINE: @Test public void registerPausedEdgeInLateCallers()
		public virtual void registerPausedEdgeInLateCallers()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", prependField("g"), "1")).succ("b"), callSite("b").calls("bar", flow("1", "1")).retSite("e", kill("1")), normalStmt("e", flow("1", readField("g"), "3")).succ("f"), callSite("f").calls("bar", flow("3", "1")).retSite("g", kill("3")));

			helper.method("bar", startPoints("c"), normalStmt("c", flow("1", readField("f"), "2"), flow("1", "1")).succ("d"), exitStmt("d").returns(over("b"), to("e"), flow("1", "1")).returns(over("f"), to("g"), kill("1"), kill("2")));

			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test @Ignore("not implemented optimization") public void mergeExcludedField()
		public virtual void mergeExcludedField()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", "1")).succ("b"), normalStmt("b", flow("1", "2", "2^f")).succ("c"), normalStmt("c", kill("2")).succ("d"));

			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void resumeOnTransitiveInterestedCaller()
		public virtual void resumeOnTransitiveInterestedCaller()
		{
			helper.method("foo", startPoints("sp"), normalStmt("sp", flow("0", prependField("f"), "1")).succ("a"), callSite("a").calls("bar", flow("1", "1")).retSite("f", kill("1")), callSite("f").calls("bar", flow("2", prependField("g"), "1")));

			helper.method("bar", startPoints("b"), callSite("b").calls("xyz", flow("1", "1")).retSite("e", kill("1")), exitStmt("e").returns(over("a"), to("f"), flow("2", "2")));

			helper.method("xyz", startPoints("c"), normalStmt("c", flow("1", readField("g"), "3"), flow("1", readField("f"), "2")).succ("d"), exitStmt("d").returns(over("b"), to("e"), flow("2", "2"), kill("3")));


			helper.runSolver(false, "sp");
		}


//ORIGINAL LINE: @Test public void happyPath()
		public virtual void happyPath()
		{
			helper.method("bar", startPoints("a"), normalStmt("a", flow("0", "x")).succ("b"), normalStmt("b", flow("x", "x")).succ("c"), callSite("c").calls("foo", flow("x", "y")).retSite("f", flow("x", "x")));

			helper.method("foo", startPoints("d"), normalStmt("d", flow("y", "y", "z")).succ("e"), exitStmt("e").returns(over("c"), to("f"), flow("z", "u"), flow("y")));

			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void reuseSummary()
		public virtual void reuseSummary()
		{
			helper.method("foo", startPoints("a"), callSite("a").calls("bar", flow("0", "x")).retSite("b", kill("0")), callSite("b").calls("bar", flow("y", "x")).retSite("c", kill("y")), normalStmt("c", flow("w", "0")).succ("c0"));

			helper.method("bar", startPoints("d"), normalStmt("d", flow("x", "z")).succ("e"), exitStmt("e").returns(over("a"), to("b"), flow("z", "y")).returns(over("b"), to("c"), flow("z", "w")));

			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void reuseSummaryForRecursiveCall()
		public virtual void reuseSummaryForRecursiveCall()
		{
			helper.method("foo", startPoints("a"), callSite("a").calls("bar", flow("0", "1")).retSite("b", flow("0")), normalStmt("b", flow("2", "3")).succ("c"));

			helper.method("bar", startPoints("g"), normalStmt("g", flow("1", "1")).succ("i").succ("h"), callSite("i").calls("bar", flow("1", "1")).retSite("h", flow("1")), exitStmt("h").returns(over("a"), to("b"), flow("1"), flow("2","2")).returns(over("i"), to("h"), flow("1","2"), flow("2", "2")));

			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void branch()
		public virtual void branch()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", "x")).succ("b2").succ("b1"), normalStmt("b1", flow("x", "x", "y")).succ("c"), normalStmt("b2", flow("x", "x")).succ("c"), normalStmt("c", flow("x", "z"), flow("y", "w")).succ("d"), normalStmt("d", flow("z"), flow("w")).succ("e"));

			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void unbalancedReturn()
		public virtual void unbalancedReturn()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", "1")).succ("b"), exitStmt("b").returns(over("x"), to("y"), flow("1", "1")));

			helper.method("bar", startPoints("unused"), normalStmt("x").succ("y"), normalStmt("y", flow("1", "2")).succ("z"));

			helper.runSolver(true, "a");
		}


//ORIGINAL LINE: @Test public void artificalReturnEdgeForNoCallersCase()
		public virtual void artificalReturnEdgeForNoCallersCase()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", "1")).succ("b"), exitStmt("b").returns(null, null, flow("1", "1")));

			helper.runSolver(true, "a");
		}


//ORIGINAL LINE: @Test public void pauseEdgeMutuallyRecursiveCallers()
		public virtual void pauseEdgeMutuallyRecursiveCallers()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", prependField("x"), "1")).succ("b"), callSite("b").calls("bar",flow("1", "2")));

			helper.method("bar", startPoints("c"), callSite("c").calls("xyz", flow("2", "3")));

			helper.method("xyz", startPoints("d"), callSite("d").calls("bar", flow("3", "2")).retSite("e", flow("3", "3")), normalStmt("e", flow("3", readField("f"), "4")).succ("f"));

			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void pauseDiamondShapedCallerChain()
		public virtual void pauseDiamondShapedCallerChain()
		{
			helper.method("bar", startPoints("a"), normalStmt("a", flow("0", prependField("x"), "1")).succ("b"), callSite("b").calls("foo1", flow("1", "2")).calls("foo2", flow("1", "2")));

			helper.method("foo1", startPoints("c1"), callSite("c1").calls("xyz", flow("2", "3")));

			helper.method("foo2", startPoints("c2"), callSite("c2").calls("xyz", flow("2", "3")));

			helper.method("xyz", startPoints("d"), normalStmt("d", flow("3", readField("f"), "4")).succ("e"), normalStmt("e").succ("f"));

			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void dontPauseDiamondShapedCallerChain()
		public virtual void dontPauseDiamondShapedCallerChain()
		{
			helper.method("bar", startPoints("a"), normalStmt("a", flow("0", prependField("f"), "1")).succ("b"), callSite("b").calls("foo1", flow("1", "2")).calls("foo2", flow("1", "2")));

			helper.method("foo1", startPoints("c1"), callSite("c1").calls("xyz", flow("2", "3")));

			helper.method("foo2", startPoints("c2"), callSite("c2").calls("xyz", flow("2", "3")));

			helper.method("xyz", startPoints("d"), normalStmt("d", flow("3", readField("f"), "4")).succ("e"), normalStmt("e", kill("4")).succ("f"));

			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void correctDeltaConstraintApplication()
		public virtual void correctDeltaConstraintApplication()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", "1")).succ("b"), callSite("b").calls("bar", flow("1", "1")));

			helper.method("bar", startPoints("c"), normalStmt("c", flow("1", overwriteField("a"), "1")).succ("d"), callSite("d").calls("xyz", flow("1", "1")));

			helper.method("xyz", startPoints("e"), normalStmt("e", flow("1", readField("f"), "2")).succ("f"), callSite("f").calls("baz", flow("2", "3")));

			helper.method("baz", startPoints("g"), normalStmt("g", flow("3", readField("a"), "4")).succ("h"));

			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void pauseForSameSourceMultipleTimes()
		public virtual void pauseForSameSourceMultipleTimes()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", prependField("f"), "1")).succ("b"), callSite("b").calls("bar", flow("1", "2")));

			helper.method("bar", startPoints("c"), normalStmt("c", flow("2", readField("x"), "3"), flow("2", "2")).succ("d"), normalStmt("d", flow("2", readField("x"), "4")).succ("e"));

			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void pauseForSameSourceMultipleTimesTransitively()
		public virtual void pauseForSameSourceMultipleTimesTransitively()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", prependField("f"), "1")).succ("b"), callSite("b").calls("xyz", flow("1", "2")).retSite("f", flow("1", "1")), callSite("f").calls("xyz", flow("1", "2")));

			helper.method("xyz", startPoints("g"), callSite("g").calls("bar", flow("2", "2")));

			helper.method("bar", startPoints("c"), normalStmt("c", flow("2", readField("x"), "3"), flow("2", "2")).succ("d"), normalStmt("d", flow("2", readField("x"), "4")).succ("e"));

			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void multipleExclusions()
		public virtual void multipleExclusions()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", overwriteField("h"), "1")).succ("b"), callSite("b").calls("bar", flow("1", "2")));

			helper.method("bar", startPoints("c"), normalStmt("c", flow("2", overwriteField("f"), "3")).succ("d"), callSite("d").calls("xyz", flow("3", "4")));

			helper.method("xyz", startPoints("e"), normalStmt("e", flow("4", overwriteField("g"), "5")).succ("f"), normalStmt("f", kill("5")).succ("g"));

			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void unbalancedReturnWithFieldRead()
		public virtual void unbalancedReturnWithFieldRead()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", "1")).succ("b"), exitStmt("b").returns(over("cs"), to("c"), flow("1", "2")));

			helper.method("xyz", startPoints("n/a"), normalStmt("cs").succ("c"), exitStmt("c").returns(over("cs2"), to("d"), flow("2", "2")));

			helper.method("bar", startPoints("unused"), normalStmt("cs2").succ("d"), normalStmt("d", flow("2", readField("f"), "3")).succ("e"), normalStmt("e", kill("3")).succ("f"));

			helper.runSolver(true, "a");
		}


//ORIGINAL LINE: @Test public void unbalancedReturnAbstraction()
		public virtual void unbalancedReturnAbstraction()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", "1")).succ("b"), normalStmt("b", flow("1", prependField("f"), "2")).succ("c"), exitStmt("c").returns(over("cs"), to("rs"), flow("2", "2")));

			helper.method("bar", startPoints("unused"), normalStmt("cs").succ("rs"), normalStmt("rs", flow("2", "3")).succ("d"));

			helper.runSolver(true, "a");
		}


//ORIGINAL LINE: @Test public void unbalancedReturnReadAbstractedField()
		public virtual void unbalancedReturnReadAbstractedField()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", "1")).succ("b"), normalStmt("b", flow("1", prependField("f"), "2")).succ("c"), exitStmt("c").returns(over("cs"), to("rs"), flow("2", "2")));

			helper.method("bar", startPoints("unused"), normalStmt("cs").succ("rs"), normalStmt("rs", flow("2", "3")).succ("d"), normalStmt("d", flow("3", readField("f"), "4")).succ("e"), normalStmt("e", kill("4")).succ("f"));

			helper.runSolver(true, "a");
		}


//ORIGINAL LINE: @Test public void unbalancedReturnReadUnwrittenAbstractedField()
		public virtual void unbalancedReturnReadUnwrittenAbstractedField()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", "1")).succ("b"), normalStmt("b", flow("1", prependField("f"), "2")).succ("c"), exitStmt("c").returns(over("cs"), to("rs"), flow("2", "2")));

			helper.method("bar", startPoints("unused"), normalStmt("cs").succ("rs"), normalStmt("rs", flow("2", "3")).succ("d"), normalStmt("d", flow("3", readField("h"), "4")).succ("e"), normalStmt("e").succ("f"));

			helper.runSolver(true, "a");
		}


//ORIGINAL LINE: @Test public void unbalancedReturnTransitiveAbstraction()
		public virtual void unbalancedReturnTransitiveAbstraction()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", "1")).succ("b"), normalStmt("b", flow("1", prependField("f"), "2")).succ("c"), exitStmt("c").returns(over("cs1"), to("rs1"), flow("2", "2")));

			helper.method("bar", startPoints("unused1"), normalStmt("cs1").succ("rs1"), normalStmt("rs1", flow("2", prependField("g"), "3")).succ("d"), exitStmt("d").returns(over("cs2"), to("rs2"), flow("3", "4")));

			helper.method("xyz", startPoints("unused2"), normalStmt("cs2").succ("rs2"), normalStmt("rs2", flow("4", "5")).succ("e"), normalStmt("e", flow("5", readField("g"), "6")).succ("f"), normalStmt("f", flow("6", readField("f"), "7")).succ("g"), normalStmt("g", kill("7")).succ("h"));

			helper.runSolver(true, "a");
		}


//ORIGINAL LINE: @Test public void unbalancedReturnPauseAndResume()
		public virtual void unbalancedReturnPauseAndResume()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", "1")).succ("b"), exitStmt("b").returns(over("cs"), to("rs"), flow("1", prependField("g"), "2")));

			helper.method("bar", startPoints("unused"), normalStmt("cs").succ("rs"), normalStmt("rs", flow("2", "2")).succ("c").succ("d1"), exitStmt("c").returns(over("cs"), to("rs"), flow("2", prependField("f"), "2")), normalStmt("d1", flow("2", readField("f"), "3")).succ("d2"), normalStmt("d2", kill("3")).succ("e"));

			helper.runSolver(true, "a");
		}


//ORIGINAL LINE: @Test public void abstractedReturnUseCallerInterest()
		public virtual void abstractedReturnUseCallerInterest()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", "1")).succ("b"), callSite("b").calls("bar", flow("1", "2")).retSite("c", kill("1")), normalStmt("c", flow("2", readField("f"), "3")).succ("d"), normalStmt("d", kill("3")).succ("e"));

			helper.method("bar", startPoints("f"), exitStmt("f").returns(over("b"), to("c"), flow("2", "2")));

			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void abstractedReturnDeltaBlockingCallerInterest()
		public virtual void abstractedReturnDeltaBlockingCallerInterest()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", "1")).succ("b"), callSite("b").calls("bar", flow("1", prependField("h"), "2")).retSite("c", kill("1")), normalStmt("c", flow("2", readField("f"), "3")).succ("d"), normalStmt("d").succ("e"));

			helper.method("bar", startPoints("f"), exitStmt("f").returns(over("b"), to("c"), flow("2", "2")));

			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void abstractedReturnResolveThroughDelta()
		public virtual void abstractedReturnResolveThroughDelta()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", "1")).succ("b1"), callSite("b1").calls("xyz", flow("1", prependField("f"), "1")));

			helper.method("xyz", startPoints("b2"), callSite("b2").calls("bar", flow("1", prependField("h"), "2")).retSite("c", kill("1")), normalStmt("c", flow("2", readField("h"), "3")).succ("d"), normalStmt("d", kill("3")).succ("e"));

			helper.method("bar", startPoints("f"), exitStmt("f").returns(over("b2"), to("c"), flow("2", "2")));

			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void unbalancedAbstractedReturnRecursive()
		public virtual void unbalancedAbstractedReturnRecursive()
		{
			helper.method("bar", startPoints("a"), normalStmt("a", flow("0", "1")).succ("b"), normalStmt("cs1").succ("b"), exitStmt("b").returns(over("cs1"), to("b"), flow(2, "1", "1")).returns(over("cs2"), to("c"), flow(2, "1", "1")));

			helper.method("foo", startPoints("unused"), normalStmt("cs2").succ("c"), normalStmt("c", flow("1", readField("f"), "2")).succ("d"), normalStmt("d", kill("2")).succ("e"));

			helper.runSolver(true, "a");
		}


//ORIGINAL LINE: @Test public void includeResolversInCallDeltas()
		public virtual void includeResolversInCallDeltas()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", "1")).succ("b"), callSite("b").calls("bar", flow("1", "2")).retSite("e", kill("1")), callSite("e").calls("xyz", flow("3", "3")));

			helper.method("bar", startPoints("c"), normalStmt("c", flow("2", overwriteField("f"), "2")).succ("d"), exitStmt("d").returns(over("b"), to("e"), flow("2", "3")));

			helper.method("xyz", startPoints("f"), normalStmt("f", flow("3", readField("f"), "4")).succ("g"), normalStmt("g").succ("h"));

			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void includeResolversInCallDeltas2()
		public virtual void includeResolversInCallDeltas2()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", "1")).succ("b"), callSite("b").calls("bar", flow("1", "2")).retSite("e", kill("1")), callSite("e").calls("xyz", flow("3", "3")).retSite("g", kill("3")), normalStmt("g", flow("3", readField("f"), "4")).succ("h"), normalStmt("h").succ("i"));

			helper.method("bar", startPoints("c"), normalStmt("c", flow("2", overwriteField("f"), "2")).succ("d"), exitStmt("d").returns(over("b"), to("e"), flow("2", "3")));

			helper.method("xyz", startPoints("f"), exitStmt("f").returns(over("e"), to("g"), flow("3", "3")));

			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void includeResolversInCallDeltas3()
		public virtual void includeResolversInCallDeltas3()
		{
			helper.method("main", startPoints("m_a"), normalStmt("m_a", flow("0", "1")).succ("m_b"), callSite("m_b").calls("foo", flow("1", prependField("g"), "1")).retSite("m_c", kill("1")), callSite("m_c").calls("foo", flow("5", prependField("f"), "1")).retSite("m_d", kill("5")), normalStmt("m_d", kill("6")).succ("m_e"));

			helper.method("foo", startPoints("a"), normalStmt("a", flow("1", "1")).succ("b"), callSite("b").calls("bar", flow("1", "2")).retSite("e", kill("1")), callSite("e").calls("xyz", flow(2, "3", "3")).retSite("g", kill(2, "3")), normalStmt("g", flow(2, "3", readField("f"), "4"), flow(2, "3", readField("g"), "5")).succ("h"), exitStmt("h").returns(over("m_c"), to("m_d"), flow("4", "6")).returns(over("m_b"), to("m_c"), flow("5", "5")));

			helper.method("bar", startPoints("c"), normalStmt("c", flow("2", overwriteField("f"), "2"), flow("2", overwriteField("g"), "2")).succ("d"), exitStmt("d").returns(over("b"), to("e"), flow(2, "2", "3")));

			helper.method("xyz", startPoints("f"), exitStmt("f").returns(over("e"), to("g"), flow(2, "3", "3")));

			helper.runSolver(false, "m_a");
		}


//ORIGINAL LINE: @Test public void includeResolversInCallDeltas4()
		public virtual void includeResolversInCallDeltas4()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", "1")).succ("b"), callSite("b").calls("bar", flow("1", "2")).retSite("e1", kill("1")), normalStmt("e1", flow("3", prependField("g"), "3")).succ("e2"), callSite("e2").calls("xyz", flow("3", "3")).retSite("g", kill("3")), normalStmt("g", flow("3", readField("h"), "4")).succ("h"), normalStmt("h", flow("4", readField("g"), "5")).succ("i"), normalStmt("i", flow("5", readField("f"), "6")).succ("j"));

			helper.method("bar", startPoints("c"), normalStmt("c", flow("2", prependField("f"), "2")).succ("d"), exitStmt("d").returns(over("b"), to("e1"), flow("2", "3")));

			helper.method("xyz", startPoints("f1"), normalStmt("f1", flow("3", prependField("h"), "3")).succ("f2"), exitStmt("f2").returns(over("e2"), to("g"), flow("3", "3")));

			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void includeResolversInCallDeltas5()
		public virtual void includeResolversInCallDeltas5()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", prependField("z"), "1")).succ("b"), callSite("b").calls("bar", flow("1", "2")).retSite("l", kill("1")), normalStmt("l").succ("m"));

			helper.method("bar", startPoints("c"), normalStmt("c", flow("2", prependField("f"), "2")).succ("d"), callSite("d").calls("xyz", flow("2", "3")).retSite("i", kill("2")), normalStmt("i", flow("4", readField("g"), "5")).succ("j"), normalStmt("j", flow("5", readField("f"), "6")).succ("k"), exitStmt("k").returns(over("b"), to("l"), kill("6")));

			helper.method("xyz", startPoints("e"), callSite("e").calls("baz", flow("3", "3")).retSite("h", kill("3")), exitStmt("h").returns(over("d"), to("i"), flow("4", "4")));

			helper.method("baz", startPoints("f"), normalStmt("f", flow("3", prependField("g"), "4")).succ("g"), exitStmt("g").returns(over("e"), to("h"), flow("4", "4")));

			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void includeResolversInCallDeltas6()
		public virtual void includeResolversInCallDeltas6()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", prependField("f"), "1")).succ("a2"), callSite("a2").calls("xyz", flow("1", "3")).retSite("b", kill("1")), callSite("b").calls("bar", flow("1", "2")).retSite("l", kill("1")), normalStmt("l").succ("m"));

			helper.method("bar", startPoints("c"), normalStmt("c", flow("2", prependField("z"), "2")).succ("d"), callSite("d").calls("xyz", flow("2", "3")).retSite("i", kill("2")), normalStmt("i", flow("4", readField("g"), "5")).succ("j"), normalStmt("j", flow("5", readField("f"), "6")).succ("k"), exitStmt("k").returns(over("b"), to("l")));

			helper.method("xyz", startPoints("e"), callSite("e").calls("baz", flow("3", "3")).retSite("h", kill("3")), exitStmt("h").returns(over("d"), to("i"), flow("4", "4")).returns(over("a2"), to("b"), flow("4", "1")));

			helper.method("baz", startPoints("f"), normalStmt("f", flow("3", prependField("g"), "4")).succ("g"), exitStmt("g").returns(over("e"), to("h"), flow("4", "4")));

			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void recursiveCallReturnCase()
		public virtual void recursiveCallReturnCase()
		{
			helper.method("xyz", startPoints("x"), normalStmt("x", flow("0", "1")).succ("y"), callSite("y").calls("foo", flow("1", prependField("g"), "1")));

			helper.method("foo", startPoints("a"), normalStmt("a", flow("1", "1")).succ("b"), callSite("b").calls("bar", flow("1", "2")).retSite("c", kill("1")), callSite("c").calls("bar", flow("2", "2")));

			helper.method("bar", startPoints("d"), normalStmt("d", flow("2", "2")).succ("d1").succ("d2"), normalStmt("d1", flow("2", readField("f"), "3")).succ("e"), normalStmt("d2", flow("2", "2")).succ("f"), exitStmt("f").returns(over("b"), to("c"), flow("2", "2")));

			helper.runSolver(false, "x");
		}


//ORIGINAL LINE: @Test public void recursivelyUseIncompatibleReturnResolver()
		public virtual void recursivelyUseIncompatibleReturnResolver()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", "1")).succ("b"), callSite("b").calls("bar", flow("1", "1")).retSite("f", kill("1")), normalStmt("f", flow("2", readField("f"), "3")).succ("g"), normalStmt("h").succ("i"));

			helper.method("bar", startPoints("c"), callSite("c").calls("xyz", flow("1", "1")).retSite("d", kill("1")), normalStmt("d", flow("1", overwriteField("f"), "1")).succ("e"), exitStmt("e").returns(over("b"), to("f"), flow("1", "2")));

			helper.method("xyz", startPoints("x"), exitStmt("x").returns(over("c"), to("d"), flow("1", "1")));

			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void unbalancedUseIncompatibleReturnResolver()
		public virtual void unbalancedUseIncompatibleReturnResolver()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", "1")).succ("b"), exitStmt("b").returns(over("cs"), to("c"), flow("1", "1")));

			helper.method("bar", startPoints("unused1"), normalStmt("cs").succ("c"), normalStmt("c", flow("1", readField("g"), "1")).succ("d"), normalStmt("d", flow("1", overwriteField("f"), "2")).succ("e"), exitStmt("e").returns(over("cs2"), to("f"), flow("2", "2")));

			helper.method("xyz", startPoints("unused2"), normalStmt("cs2").succ("f"), normalStmt("f", flow("2", readField("f"), "3")).succ("g"), normalStmt("g").succ("h"));

			helper.runSolver(true, "a");
		}


//ORIGINAL LINE: @Test public void recursiveReadField()
		public virtual void recursiveReadField()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", "1")).succ("b"), callSite("b").calls("bar", flow("1", "1")));

			helper.method("bar", startPoints("c"), normalStmt("c", flow("1", readField("f"), "1")).succ("d"), callSite("d").calls("bar", flow("1", "1")));

			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void recursiveReadField2()
		public virtual void recursiveReadField2()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", "1")).succ("b"), callSite("b").calls("bar", flow("1", "1")));

			helper.method("bar", startPoints("c"), normalStmt("c", flow("1", readField("f"), "1")).succ("d"), callSite("d").calls("xyz", flow("1", "1")));

			helper.method("xyz", startPoints("e"), callSite("e").calls("bar", flow("1", "1")));

			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void recursiveReadField3()
		public virtual void recursiveReadField3()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", "1")).succ("b"), callSite("b").calls("bar", flow("1", "1")));

			helper.method("bar", startPoints("c"), normalStmt("c", flow("1", readField("f"), "1")).succ("d"), callSite("d").calls("xyz", flow("1", "1")));

			helper.method("xyz", startPoints("e"), normalStmt("e", flow("1", readField("g"), "1")).succ("f"), callSite("f").calls("bar", flow("1", "1")));

			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void recursiveReadField4()
		public virtual void recursiveReadField4()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", "1")).succ("b1"), callSite("b1").calls("bar", flow("1", "1")).retSite("b2", flow("1", "1")), callSite("b2").calls("xyz", flow("1", "1")));

			helper.method("bar", startPoints("c"), normalStmt("c", flow("1", readField("f"), "1")).succ("d"), callSite("d").calls("xyz", flow("1", "1")));

			helper.method("xyz", startPoints("e"), normalStmt("e", flow("1", readField("g"), "1")).succ("f"), callSite("f").calls("bar", flow("1", "1")));

			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void loopReadingField()
		public virtual void loopReadingField()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", "1")).succ("b"), normalStmt("b", flow("1", readField("f"), "1")).succ("b").succ("c"), normalStmt("c", kill("1")).succ("d"));

			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void loopReadingField2()
		public virtual void loopReadingField2()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", "1")).succ("b"), normalStmt("b", flow("1", "1")).succ("c1").succ("c2"), normalStmt("c1", flow("1", readField("f"), "1")).succ("b").succ("d"), normalStmt("c2", flow("1", readField("g"), "1")).succ("b").succ("d"), normalStmt("d", kill("1")).succ("e"));

			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void loopReadingField3()
		public virtual void loopReadingField3()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", "1")).succ("b"), normalStmt("b", flow("1", "1")).succ("c1").succ("c2"), normalStmt("c1", flow("1", readField("f"), "1")).succ("d"), normalStmt("c2", flow("1", readField("g"), "1")).succ("d"), normalStmt("d", flow("1", "1")).succ("e").succ("b"), normalStmt("e", kill("1")).succ("f"));

			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void recursiveReturn()
		public virtual void recursiveReturn()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", "1")).succ("b"), callSite("b").calls("bar", flow("1", "2")));

			helper.method("bar", startPoints("c"), callSite("c").calls("bar", flow("2", "2")).retSite("d", flow("2", "2")), normalStmt("d", flow(2, "2", "2")).succ("e").succ("f"), exitStmt("e").returns(over("c"), to("d"), flow(2, "2", "2")), normalStmt("f", flow(2, "2", readField("f"), "3")).succ("g"), normalStmt("g", kill("3")).succ("h"));

			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void joinStmtIsCallSite()
		public virtual void joinStmtIsCallSite()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", "1")).succ("b"), callSite("b").calls("bar", kill(2,"1")).retSite("c", flow("1", "1")), normalStmt("c", flow("1", prependField("f"), "1")).succ("b"));

			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void readMultipleAbstractedFields()
		public virtual void readMultipleAbstractedFields()
		{
			helper.method("foo", startPoints("a"), normalStmt("a", flow("0", prependField("f"), "1")).succ("b1").succ("b2"), normalStmt("b1", kill("1")).succ("c"), normalStmt("b2", flow("1", prependField("g"), "1")).succ("c"), normalStmt("c", flow("1", "2")).succ("d"), normalStmt("d", flow("2", "2")).succ("e1").succ("e2"), normalStmt("e1", flow("2", "3")).succ("f"), normalStmt("e2", kill("2")).succ("f"), normalStmt("f", flow("3", readField("g"), "4")).succ("g"), normalStmt("g", flow("4", readField("f"), "5")).succ("h"), normalStmt("h", kill("5")).succ("i"));

			helper.runSolver(false, "a");
		}


//ORIGINAL LINE: @Test public void intraproceduralStateExplosionInline0Resolver()
		public virtual void intraproceduralStateExplosionInline0Resolver()
		{
			helper.method("main", startPoints("m_a"), normalStmt("m_a", flow("0", "1")).succ("m_b"), callSite("m_b").calls("foo", flow("1", "1")));

			helper.method("foo", startPoints("a"), normalStmt("a", flow("1", "1")).succ("b"), normalStmt("b", flow("1", "1")).succ("c1").succ("c2"), normalStmt("c1", flow("1", "1")).succ("d"), normalStmt("c2", flow("1", overwriteField("f"), "1")).succ("d"), normalStmt("d", flow(2, "1", "1")).succ("e1").succ("e2"), normalStmt("e1", flow(2, "1", "1")).succ("f"), normalStmt("e2", flow(2, "1", overwriteField("g"), "1")).succ("f"), normalStmt("f", flow(4, "1", "1")).succ("g1").succ("g2"), normalStmt("g1", flow(4, "1", "1")).succ("h"), normalStmt("g2", flow(4, "1", overwriteField("h"), "1")).succ("h"), normalStmt("h", flow(8, "1", "1")).succ("i"));

			helper.runSolver(false, "m_a");
		}


//ORIGINAL LINE: @Test public void intraproceduralStateExplosion()
		public virtual void intraproceduralStateExplosion()
		{
			helper.method("main", startPoints("m_a"), normalStmt("m_a", flow("0", "1")).succ("m_b"), callSite("m_b").calls("foo", flow("1", prependField("x"), "1")));

			helper.method("foo", startPoints("a"), normalStmt("a", flow("1", "1")).succ("b"), normalStmt("b", flow("1", "1")).succ("c1").succ("c2"), normalStmt("c1", flow("1", "1")).succ("d"), normalStmt("c2", flow("1", overwriteField("f"), "1")).succ("d"), normalStmt("d", flow(2, "1", "1")).succ("e1").succ("e2"), normalStmt("e1", flow(2, "1", "1")).succ("f"), normalStmt("e2", flow(2, "1", overwriteField("g"), "1")).succ("f"), normalStmt("f", flow(4, "1", "1")).succ("g1").succ("g2"), normalStmt("g1", flow(4, "1", "1")).succ("h"), normalStmt("g2", flow(4, "1", overwriteField("h"), "1")).succ("h"), normalStmt("h", flow(8, "1", "1")).succ("i"));

			helper.runSolver(false, "m_a");
		}


//ORIGINAL LINE: @Test public void nestedResolversShouldFormAGraph()
		public virtual void nestedResolversShouldFormAGraph()
		{
			helper.method("main", startPoints("m_a"), normalStmt("m_a", flow("0", "1")).succ("m_b"), callSite("m_b").calls("foo", flow("1", prependField("f"), "1")));

			helper.method("foo", startPoints("a"), normalStmt("a", flow("1", "1")).succ("b1").succ("b2"), normalStmt("b1", flow("1", overwriteField("x"), "1")).succ("c"), normalStmt("b2", flow("1", overwriteField("y"), "1")).succ("c"), normalStmt("c", flow(2, "1", "1")).succ("d1").succ("d2"), normalStmt("d1", flow(2, "1", overwriteField("x"), "1")).succ("e"), normalStmt("d2", flow(2, "1", overwriteField("y"), "1")).succ("e"), normalStmt("e", flow(3, "1", "1")).succ("f"));

			helper.runSolver(false, "m_a");
		}


//ORIGINAL LINE: @Test public void maintainCallingContextInCtrlFlowJoin()
		public virtual void maintainCallingContextInCtrlFlowJoin()
		{
			helper.method("main", startPoints("a"), normalStmt("a", flow("0", "1")).succ("b1").succ("b2"), callSite("b1").calls("foo", flow("1", prependField("f"), "2")).retSite("c1", kill("1")), callSite("b2").calls("foo", flow("1", "2")).retSite("c2", kill("1")), normalStmt("c1", kill("3")).succ("d"), normalStmt("c2", flow("3", readField("f"), "4")).succ("d"), normalStmt("d", kill("4")).succ("e"));

			helper.method("foo", startPoints("f"), normalStmt("f", flow("2", "2")).succ("g").succ("f"), exitStmt("g").returns(over("b1"), to("c1"), flow("2", "3")).returns(over("b2"), to("c2"), flow("2", "3")));

			helper.runSolver(false, "a");
		}
	}

}