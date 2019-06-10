//using heros.fieldsens.structs;
//using heros.utilities;
//using heros.fieldsens;

///// <summary>
/////*****************************************************************************
///// Copyright (c) 2015 Johannes Lerch.
///// All rights reserved. This program and the accompanying materials
///// are made available under the terms of the GNU Lesser Public License v2.1
///// which accompanies this distribution, and is available at
///// http://www.gnu.org/licenses/old-licenses/gpl-2.0.html
///// 
///// Contributors:
/////     Johannes Lerch - initial API and implementation
///// *****************************************************************************
///// </summary>
//namespace heros.fieldsens
//{ 
//	public class ControlFlowJoinResolverTest
//	{
//		private static DeltaConstraint<string> getDeltaConstraint(params string[] fieldRefs)
//		{
//			return new DeltaConstraint<string>(getDelta(fieldRefs));
//		}

//		private static AccessPath<string>.Delta<string> getDelta(params string[] fieldRefs)
//		{
//			AccessPath<string> accPath = createAccessPath(fieldRefs);
//			return (new AccessPath<string>()).getDeltaTo(accPath);
//		}

//		protected internal static AccessPath<string> createAccessPath(params string[] fieldRefs)
//		{
//			AccessPath<string> accPath = new AccessPath<string>();
//			foreach (string fieldRef in fieldRefs)
//			{
//				accPath = accPath.append(fieldRef);
//			}
//			return accPath;
//		}

//		private PerAccessPathMethodAnalyzer<string, TestFact, Statement, TestMethod> analyzer;
//		private Statement joinStmt;
//		private ControlFlowJoinResolver<string, TestFact, Statement, TestMethod> sut;
//		private TestFact fact;
//		private InterestCallback<string, TestFact, Statement, TestMethod> callback;
//		private Resolver<string, TestFact, Statement, TestMethod> callEdgeResolver;


////ORIGINAL LINE: @Before public void before()
//		public virtual void before()
//		{
//			analyzer = mock(typeof(PerAccessPathMethodAnalyzer));
//			joinStmt = new Statement("joinStmt");
//			sut = new ControlFlowJoinResolver<string, TestFact, Statement, TestMethod>(mock(typeof(FactMergeHandler)), analyzer, joinStmt, new Debugger_NullDebugger<string, TestFact, Statement, TestMethod>());
//			fact = new TestFact("value");
//			callback = mock(typeof(InterestCallback));
//			callEdgeResolver = mock(typeof(CallEdgeResolver));
//		}


////ORIGINAL LINE: @Test public void emptyIncomingFact()
//		public virtual void emptyIncomingFact()
//		{
//			sut.addIncoming(new WrappedFact<string, TestFact, Statement, TestMethod>(fact, createAccessPath(), callEdgeResolver));
//			verify(analyzer).processFlowFromJoinStmt(eq(new WrappedFactAtStatement<string, TestFact, Statement, TestMethod>(joinStmt, new WrappedFact<string, TestFact, Statement, TestMethod>(fact, createAccessPath(), sut))));
//			assertTrue(sut.InterestGiven);
//		}


////ORIGINAL LINE: @Test public void resolveViaIncomingFact()
//		public virtual void resolveViaIncomingFact()
//		{
//			sut.resolve(getDeltaConstraint("a"), callback);
//			sut.addIncoming(new WrappedFact<string, TestFact, Statement, TestMethod>(fact, createAccessPath("a"), callEdgeResolver));
//			verify(callback).interest(eq(analyzer), argThat(new ResolverArgumentMatcher(this, createAccessPath("a"))));
//		}


////ORIGINAL LINE: @Test public void registerCallbackAtIncomingResolver()
//		public virtual void registerCallbackAtIncomingResolver()
//		{
//			Resolver<string, TestFact, Statement, TestMethod> resolver = mock(typeof(Resolver));
//			sut.addIncoming(new WrappedFact<string, TestFact, Statement, TestMethod>(fact, createAccessPath(), resolver));
//			sut.resolve(getDeltaConstraint("a"), callback);
//			verify(resolver).resolve(eq(getDeltaConstraint("a")), any(typeof(InterestCallback)));
//		}


////ORIGINAL LINE: @Test public void resolveViaIncomingResolver()
//		public virtual void resolveViaIncomingResolver()
//		{
//			Resolver<string, TestFact, Statement, TestMethod> resolver = mock(typeof(Resolver));

////ORIGINAL LINE: final heros.fieldsens.Resolver<String, heros.utilities.TestFact, heros.utilities.Statement, heros.utilities.TestMethod> nestedResolver = mock(heros.fieldsens.Resolver.class);
//			Resolver<string, TestFact, Statement, TestMethod> nestedResolver = mock(typeof(Resolver));
//			Mockito.doAnswer(new AnswerAnonymousInnerClass(this, nestedResolver))
//		   .when(resolver).resolve(eq(getDeltaConstraint("a")), any(typeof(InterestCallback)));

//			sut.addIncoming(new WrappedFact<string, TestFact, Statement, TestMethod>(fact, createAccessPath(), resolver));
//			sut.resolve(getDeltaConstraint("a"), callback);

//			verify(callback).interest(eq(analyzer), eq(nestedResolver));
//		}

//		private class AnswerAnonymousInnerClass : Answer
//		{
//			private readonly ControlFlowJoinResolverTest outerInstance;

//			private Resolver<string, TestFact, Statement, TestMethod> nestedResolver;

//			public AnswerAnonymousInnerClass(ControlFlowJoinResolverTest outerInstance, Resolver<string, TestFact, Statement, TestMethod> nestedResolver)
//			{
//				this.outerInstance = outerInstance;
//				this.nestedResolver = nestedResolver;
//			}


////ORIGINAL LINE: @Override public Object answer(org.mockito.invocation.InvocationOnMock invocation) throws Throwable
//			public override object answer(InvocationOnMock invocation)
//			{
//				InterestCallback<string, TestFact, Statement, TestMethod> argCallback = (InterestCallback<string, TestFact, Statement, TestMethod>) invocation.Arguments[1];
//				argCallback.interest(outerInstance.analyzer, nestedResolver);
//				return null;
//			}
//		}


//		private class ResolverArgumentMatcher : ArgumentMatcher<ReturnSiteResolver<string, TestFact, Statement, TestMethod>>
//		{
//			private readonly ControlFlowJoinResolverTest outerInstance;


//			internal AccessPath<string> accPath;

//			public ResolverArgumentMatcher(ControlFlowJoinResolverTest outerInstance, AccessPath<string> accPath)
//			{
//				this.outerInstance = outerInstance;
//				this.accPath = accPath;
//			}

//			public override bool matches(object argument)
//			{
//				ControlFlowJoinResolver resolver = (ControlFlowJoinResolver) argument;
//				return resolver.InterestGiven && resolver.resolvedAccessPath.Equals(accPath) && resolver.JoinStmt.Equals(outerInstance.joinStmt);
//			}
//		}
//	}

//}