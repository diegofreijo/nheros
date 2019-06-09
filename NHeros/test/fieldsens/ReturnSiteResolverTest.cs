using System.Collections.Generic;

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

//	import static org.junit.Assert.assertEquals;

//	import static org.junit.Assert.assertTrue;

//	import static org.mockito.Matchers.any;

//	import static org.mockito.Matchers.argThat;

//	import static org.mockito.Matchers.eq;

//	import static org.mockito.Mockito.RETURNS_DEEP_STUBS;

//	import static org.mockito.Mockito.RETURNS_MOCKS;

//	import static org.mockito.Mockito.RETURNS_SMART_NULLS;

//	import static org.mockito.Mockito.doAnswer;

//	import static org.mockito.Mockito.mock;

//	import static org.mockito.Mockito.never;

//	import static org.mockito.Mockito.verify;

//	import static org.mockito.Mockito.when;

//	import static org.mockito.Mockito.withSettings;
	using Delta = heros.fieldsens.AccessPath.Delta;
	using DeltaConstraint = heros.fieldsens.structs.DeltaConstraint;
	using WrappedFact = heros.fieldsens.structs.WrappedFact;
	using WrappedFactAtStatement = heros.fieldsens.structs.WrappedFactAtStatement;
	using Statement = heros.utilities.Statement;
	using TestFact = heros.utilities.TestFact;
	using TestMethod = heros.utilities.TestMethod;

	using Before = org.junit.Before;
	using Test = org.junit.Test;
	using ArgumentMatcher = org.mockito.ArgumentMatcher;
	using Mockito = org.mockito.Mockito;
	using InvocationOnMock = org.mockito.invocation.InvocationOnMock;
	using Answer = org.mockito.stubbing.Answer;

	using Lists = com.google.common.collect.Lists;

	public class ReturnSiteResolverTest
	{

		private static DeltaConstraint<string> getDeltaConstraint(params string[] fieldRefs)
		{
			return new DeltaConstraint<string>(getDelta(fieldRefs));
		}

		private static Delta<string> getDelta(params string[] fieldRefs)
		{
			AccessPath<string> accPath = createAccessPath(fieldRefs);
			return (new AccessPath<string>()).getDeltaTo(accPath);
		}

		protected internal static AccessPath<string> createAccessPath(params string[] fieldRefs)
		{
			AccessPath<string> accPath = new AccessPath<string>();
			foreach (string fieldRef in fieldRefs)
			{
				accPath = accPath.append(fieldRef);
			}
			return accPath;
		}

		private PerAccessPathMethodAnalyzer<string, TestFact, Statement, TestMethod> analyzer;
		private Statement returnSite;
		private ReturnSiteResolver<string, TestFact, Statement, TestMethod> sut;
		private TestFact fact;
		private InterestCallback<string, TestFact, Statement, TestMethod> callback;
		private Resolver<string, TestFact, Statement, TestMethod> callEdgeResolver;


//ORIGINAL LINE: @Before public void before()
		public virtual void before()
		{
			analyzer = mock(typeof(PerAccessPathMethodAnalyzer));
			returnSite = new Statement("returnSite");
			sut = new ReturnSiteResolver<string, TestFact, Statement, TestMethod>(mock(typeof(FactMergeHandler)), analyzer, returnSite, new Debugger_NullDebugger<string, TestFact, Statement, TestMethod>());
			fact = new TestFact("value");
			callback = mock(typeof(InterestCallback));
			callEdgeResolver = mock(typeof(CallEdgeResolver));
		}


//ORIGINAL LINE: @Test public void emptyIncomingFact()
		public virtual void emptyIncomingFact()
		{
			sut.addIncoming(new WrappedFact<string, TestFact, Statement, TestMethod>(fact, createAccessPath(), callEdgeResolver), callEdgeResolver, Delta);
			verify(analyzer).scheduleEdgeTo(eq(new WrappedFactAtStatement<string, TestFact, Statement, TestMethod>(returnSite, new WrappedFact<string, TestFact, Statement, TestMethod>(fact, createAccessPath(), sut))));
			assertTrue(sut.InterestGiven);
		}


//ORIGINAL LINE: @Test public void resolveViaIncomingFact()
		public virtual void resolveViaIncomingFact()
		{
			sut.resolve(getDeltaConstraint("a"), callback);
			sut.addIncoming(new WrappedFact<string, TestFact, Statement, TestMethod>(fact, createAccessPath("a"), callEdgeResolver), callEdgeResolver, Delta);
			verify(callback).interest(eq(analyzer), argThat(new ReturnSiteResolverArgumentMatcher(this, createAccessPath("a"))));
		}


//ORIGINAL LINE: @Test public void registerCallbackAtIncomingResolver()
		public virtual void registerCallbackAtIncomingResolver()
		{
			Resolver<string, TestFact, Statement, TestMethod> resolver = mock(typeof(Resolver));
			sut.addIncoming(new WrappedFact<string, TestFact, Statement, TestMethod>(fact, createAccessPath(), resolver), callEdgeResolver, Delta);
			sut.resolve(getDeltaConstraint("a"), callback);
			verify(resolver).resolve(eq(getDeltaConstraint("a")), any(typeof(InterestCallback)));
		}


//ORIGINAL LINE: @Test public void resolveViaIncomingResolver()
		public virtual void resolveViaIncomingResolver()
		{
			Resolver<string, TestFact, Statement, TestMethod> resolver = mock(typeof(Resolver));

//ORIGINAL LINE: final Resolver<String, heros.utilities.TestFact, heros.utilities.Statement, heros.utilities.TestMethod> nestedResolver = mock(Resolver.class);
			Resolver<string, TestFact, Statement, TestMethod> nestedResolver = mock(typeof(Resolver));
			Mockito.doAnswer(new AnswerAnonymousInnerClass(this, nestedResolver))
		   .when(resolver).resolve(eq(getDeltaConstraint("a")), any(typeof(InterestCallback)));

			sut.addIncoming(new WrappedFact<string, TestFact, Statement, TestMethod>(fact, createAccessPath(), resolver), callEdgeResolver, Delta);
			sut.resolve(getDeltaConstraint("a"), callback);

			verify(callback).interest(eq(analyzer), argThat(new ReturnSiteResolverArgumentMatcher(this, createAccessPath("a"))));
		}

		private class AnswerAnonymousInnerClass : Answer
		{
			private readonly ReturnSiteResolverTest outerInstance;

			private heros.fieldsens.Resolver<string, TestFact, Statement, TestMethod> nestedResolver;

			public AnswerAnonymousInnerClass(ReturnSiteResolverTest outerInstance, heros.fieldsens.Resolver<string, TestFact, Statement, TestMethod> nestedResolver)
			{
				this.outerInstance = outerInstance;
				this.nestedResolver = nestedResolver;
			}


//ORIGINAL LINE: @Override public Object answer(org.mockito.invocation.InvocationOnMock invocation) throws Throwable
			public override object answer(InvocationOnMock invocation)
			{
				InterestCallback<string, TestFact, Statement, TestMethod> argCallback = (InterestCallback<string, TestFact, Statement, TestMethod>) invocation.Arguments[1];
				argCallback.interest(outerInstance.analyzer, nestedResolver);
				return null;
			}
		}


//ORIGINAL LINE: @Test public void resolveViaLateInterestAtIncomingResolver()
		public virtual void resolveViaLateInterestAtIncomingResolver()
		{
			Resolver<string, TestFact, Statement, TestMethod> resolver = mock(typeof(Resolver));

//ORIGINAL LINE: final Resolver<String, heros.utilities.TestFact, heros.utilities.Statement, heros.utilities.TestMethod> nestedResolver = mock(Resolver.class);
			Resolver<string, TestFact, Statement, TestMethod> nestedResolver = mock(typeof(Resolver));

//ORIGINAL LINE: final java.util.List<InterestCallback> callbacks = com.google.common.collect.new List();
			IList<InterestCallback> callbacks = new List();

			Mockito.doAnswer(new AnswerAnonymousInnerClass2(this, callbacks))
		   .when(resolver).resolve(eq(getDeltaConstraint("a")), any(typeof(InterestCallback)));

			sut.addIncoming(new WrappedFact<string, TestFact, Statement, TestMethod>(fact, createAccessPath(), resolver), callEdgeResolver, Delta);
			sut.resolve(getDeltaConstraint("a"), callback);

			verify(callback, never()).interest(any(typeof(PerAccessPathMethodAnalyzer)), any(typeof(Resolver)));

			assertEquals(1, callbacks.Count);
			Resolver transitiveResolver = mock(typeof(Resolver));
			callbacks[0].interest(analyzer, transitiveResolver);
			verify(callback).interest(eq(analyzer), argThat(new ReturnSiteResolverArgumentMatcher(this, createAccessPath("a"))));
		}

		private class AnswerAnonymousInnerClass2 : Answer
		{
			private readonly ReturnSiteResolverTest outerInstance;

			private IList<InterestCallback> callbacks;

			public AnswerAnonymousInnerClass2(ReturnSiteResolverTest outerInstance, IList<InterestCallback> callbacks)
			{
				this.outerInstance = outerInstance;
				this.callbacks = callbacks;
			}


//ORIGINAL LINE: @Override public Object answer(org.mockito.invocation.InvocationOnMock invocation) throws Throwable
			public override object answer(InvocationOnMock invocation)
			{
				InterestCallback<string, TestFact, Statement, TestMethod> argCallback = (InterestCallback<string, TestFact, Statement, TestMethod>) invocation.Arguments[1];
				callbacks.Add(argCallback);
				return null;
			}
		}


//ORIGINAL LINE: @Test public void resolveViaDelta()
		public virtual void resolveViaDelta()
		{
			sut.addIncoming(new WrappedFact<string, TestFact, Statement, TestMethod>(fact, createAccessPath(), callEdgeResolver), callEdgeResolver, getDelta("a"));
			sut.resolve(getDeltaConstraint("a"), callback);
			verify(callback).interest(eq(analyzer), argThat(new ReturnSiteResolverArgumentMatcher(this, createAccessPath("a"))));
		}


//ORIGINAL LINE: @Test public void resolveViaDeltaTwice()
		public virtual void resolveViaDeltaTwice()
		{

//ORIGINAL LINE: final InterestCallback<String, heros.utilities.TestFact, heros.utilities.Statement, heros.utilities.TestMethod> innerCallback = mock(InterestCallback.class);
			InterestCallback<string, TestFact, Statement, TestMethod> innerCallback = mock(typeof(InterestCallback));
			doAnswer(new AnswerAnonymousInnerClass3(this, innerCallback))
		   .when(callback).interest(eq(analyzer), argThat(new ReturnSiteResolverArgumentMatcher(this, createAccessPath("a"))));

			sut.addIncoming(new WrappedFact<string, TestFact, Statement, TestMethod>(fact, createAccessPath(), callEdgeResolver), callEdgeResolver, getDelta("a", "b"));
			sut.resolve(getDeltaConstraint("a"), callback);

			verify(innerCallback).interest(eq(analyzer), argThat(new ReturnSiteResolverArgumentMatcher(this, createAccessPath("a", "b"))));
		}

		private class AnswerAnonymousInnerClass3 : Answer
		{
			private readonly ReturnSiteResolverTest outerInstance;

			private heros.fieldsens.InterestCallback<string, TestFact, Statement, TestMethod> innerCallback;

			public AnswerAnonymousInnerClass3(ReturnSiteResolverTest outerInstance, heros.fieldsens.InterestCallback<string, TestFact, Statement, TestMethod> innerCallback)
			{
				this.outerInstance = outerInstance;
				this.innerCallback = innerCallback;
			}


//ORIGINAL LINE: @Override public Object answer(org.mockito.invocation.InvocationOnMock invocation) throws Throwable
			public override object answer(InvocationOnMock invocation)
			{
				ReturnSiteResolver<string, TestFact, Statement, TestMethod> resolver = (ReturnSiteResolver<string, TestFact, Statement, TestMethod>) invocation.Arguments[1];
				resolver.resolve(getDeltaConstraint("b"), innerCallback);
				return null;
			}
		}


//ORIGINAL LINE: @Test public void resolveViaDeltaAndThenViaCallSite()
		public virtual void resolveViaDeltaAndThenViaCallSite()
		{

//ORIGINAL LINE: final InterestCallback<String, heros.utilities.TestFact, heros.utilities.Statement, heros.utilities.TestMethod> innerCallback = mock(InterestCallback.class);
			InterestCallback<string, TestFact, Statement, TestMethod> innerCallback = mock(typeof(InterestCallback));
			doAnswer(new AnswerAnonymousInnerClass4(this, innerCallback))
		   .when(callback).interest(eq(analyzer), argThat(new ReturnSiteResolverArgumentMatcher(this, createAccessPath("a"))));

			sut.addIncoming(new WrappedFact<string, TestFact, Statement, TestMethod>(fact, createAccessPath(), callEdgeResolver), callEdgeResolver, getDelta("a"));
			sut.resolve(getDeltaConstraint("a"), callback);
			verify(innerCallback).canBeResolvedEmpty();
		}

		private class AnswerAnonymousInnerClass4 : Answer
		{
			private readonly ReturnSiteResolverTest outerInstance;

			private heros.fieldsens.InterestCallback<string, TestFact, Statement, TestMethod> innerCallback;

			public AnswerAnonymousInnerClass4(ReturnSiteResolverTest outerInstance, heros.fieldsens.InterestCallback<string, TestFact, Statement, TestMethod> innerCallback)
			{
				this.outerInstance = outerInstance;
				this.innerCallback = innerCallback;
			}


//ORIGINAL LINE: @Override public Object answer(org.mockito.invocation.InvocationOnMock invocation) throws Throwable
			public override object answer(InvocationOnMock invocation)
			{
				ReturnSiteResolver<string, TestFact, Statement, TestMethod> resolver = (ReturnSiteResolver<string, TestFact, Statement, TestMethod>) invocation.Arguments[1];
				resolver.resolve(getDeltaConstraint("b"), innerCallback);
				return null;
			}
		}


//ORIGINAL LINE: @Test public void resolveViaCallEdgeResolverAtCallSite()
		public virtual void resolveViaCallEdgeResolverAtCallSite()
		{
			sut.addIncoming(new WrappedFact<string, TestFact, Statement, TestMethod>(fact, createAccessPath(), callEdgeResolver), callEdgeResolver, Delta);
			sut.resolve(getDeltaConstraint("a"), callback);
			verify(callback).canBeResolvedEmpty();
		}


//ORIGINAL LINE: @Test public void resolveViaResolverAtCallSite()
		public virtual void resolveViaResolverAtCallSite()
		{
			Resolver<string, TestFact, Statement, TestMethod> resolver = mock(typeof(Resolver));
			sut.addIncoming(new WrappedFact<string, TestFact, Statement, TestMethod>(fact, createAccessPath(), callEdgeResolver), resolver, Delta);
			sut.resolve(getDeltaConstraint("a"), callback);
			verify(resolver).resolve(eq(getDeltaConstraint("a")), any(typeof(InterestCallback)));
		}


//ORIGINAL LINE: @Test public void resolveViaResolverAtCallSiteTwice()
		public virtual void resolveViaResolverAtCallSiteTwice()
		{
			Resolver<string, TestFact, Statement, TestMethod> resolver = mock(typeof(Resolver));

//ORIGINAL LINE: final Resolver<String, heros.utilities.TestFact, heros.utilities.Statement, heros.utilities.TestMethod> nestedResolver = mock(Resolver.class);
			Resolver<string, TestFact, Statement, TestMethod> nestedResolver = mock(typeof(Resolver));
			doAnswer(new AnswerAnonymousInnerClass5(this, nestedResolver))
		   .when(resolver).resolve(eq(getDeltaConstraint("a")), any(typeof(InterestCallback)));
			doAnswer(new AnswerAnonymousInnerClass6(this, nestedResolver))
		   .when(nestedResolver).resolve(eq(getDeltaConstraint("b")), any(typeof(InterestCallback)));


//ORIGINAL LINE: final InterestCallback<String, heros.utilities.TestFact, heros.utilities.Statement, heros.utilities.TestMethod> secondCallback = mock(InterestCallback.class);
			InterestCallback<string, TestFact, Statement, TestMethod> secondCallback = mock(typeof(InterestCallback));
			doAnswer(new AnswerAnonymousInnerClass7(this, resolver, secondCallback))
		   .when(callback).interest(eq(analyzer), eq(nestedResolver));

			sut.addIncoming(new WrappedFact<string, TestFact, Statement, TestMethod>(fact, createAccessPath(), callEdgeResolver), resolver, Delta);
			sut.resolve(getDeltaConstraint("a"), callback);

			verify(secondCallback).interest(eq(analyzer), eq(nestedResolver));
		}

		private class AnswerAnonymousInnerClass5 : Answer
		{
			private readonly ReturnSiteResolverTest outerInstance;

			private heros.fieldsens.Resolver<string, TestFact, Statement, TestMethod> nestedResolver;

			public AnswerAnonymousInnerClass5(ReturnSiteResolverTest outerInstance, heros.fieldsens.Resolver<string, TestFact, Statement, TestMethod> nestedResolver)
			{
				this.outerInstance = outerInstance;
				this.nestedResolver = nestedResolver;
			}


//ORIGINAL LINE: @Override public Object answer(org.mockito.invocation.InvocationOnMock invocation) throws Throwable
			public override object answer(InvocationOnMock invocation)
			{
				InterestCallback innerCallback = (InterestCallback) invocation.Arguments[1];
				innerCallback.interest(outerInstance.analyzer, nestedResolver);
				return null;
			}
		}

		private class AnswerAnonymousInnerClass6 : Answer
		{
			private readonly ReturnSiteResolverTest outerInstance;

			private heros.fieldsens.Resolver<string, TestFact, Statement, TestMethod> nestedResolver;

			public AnswerAnonymousInnerClass6(ReturnSiteResolverTest outerInstance, heros.fieldsens.Resolver<string, TestFact, Statement, TestMethod> nestedResolver)
			{
				this.outerInstance = outerInstance;
				this.nestedResolver = nestedResolver;
			}


//ORIGINAL LINE: @Override public Object answer(org.mockito.invocation.InvocationOnMock invocation) throws Throwable
			public override object answer(InvocationOnMock invocation)
			{
				InterestCallback innerCallback = (InterestCallback) invocation.Arguments[1];
				innerCallback.interest(outerInstance.analyzer, nestedResolver);
				return null;
			}
		}

		private class AnswerAnonymousInnerClass7 : Answer
		{
			private readonly ReturnSiteResolverTest outerInstance;

			private heros.fieldsens.Resolver<string, TestFact, Statement, TestMethod> resolver;
			private heros.fieldsens.InterestCallback<string, TestFact, Statement, TestMethod> secondCallback;

			public AnswerAnonymousInnerClass7(ReturnSiteResolverTest outerInstance, heros.fieldsens.Resolver<string, TestFact, Statement, TestMethod> resolver, heros.fieldsens.InterestCallback<string, TestFact, Statement, TestMethod> secondCallback)
			{
				this.outerInstance = outerInstance;
				this.resolver = resolver;
				this.secondCallback = secondCallback;
			}


//ORIGINAL LINE: @Override public Object answer(org.mockito.invocation.InvocationOnMock invocation) throws Throwable
			public override object answer(InvocationOnMock invocation)
			{
				Resolver<string, TestFact, Statement, TestMethod> resolver = (Resolver) invocation.Arguments[1];
				resolver.resolve(getDeltaConstraint("b"), secondCallback);
				return null;
			}

		}


//ORIGINAL LINE: @Test public void resolveAsEmptyViaIncomingResolver()
		public virtual void resolveAsEmptyViaIncomingResolver()
		{
			Resolver<string, TestFact, Statement, TestMethod> resolver = mock(typeof(Resolver));
			Delta<string> delta = (new AccessPath<string>()).getDeltaTo((new AccessPath<string>()).appendExcludedFieldReference("a"));

			doAnswer(new AnswerAnonymousInnerClass8(this))
		   .when(resolver).resolve(eq(getDeltaConstraint("a")), any(typeof(InterestCallback)));

			sut.addIncoming(new WrappedFact<string, TestFact, Statement, TestMethod>(fact, createAccessPath(), resolver), callEdgeResolver, delta);
			sut.resolve(getDeltaConstraint("a"), callback);

			verify(callback, never()).canBeResolvedEmpty();
			verify(callback, never()).interest(any(typeof(PerAccessPathMethodAnalyzer)), any(typeof(Resolver)));
		}

		private class AnswerAnonymousInnerClass8 : Answer
		{
			private readonly ReturnSiteResolverTest outerInstance;

			public AnswerAnonymousInnerClass8(ReturnSiteResolverTest outerInstance)
			{
				this.outerInstance = outerInstance;
			}


//ORIGINAL LINE: @Override public Object answer(org.mockito.invocation.InvocationOnMock invocation) throws Throwable
			public override object answer(InvocationOnMock invocation)
			{
				InterestCallback innerCallback = (InterestCallback) invocation.Arguments[1];
				innerCallback.canBeResolvedEmpty();
				return null;
			}
		}


//ORIGINAL LINE: @Test public void resolveViaCallSiteResolver()
		public virtual void resolveViaCallSiteResolver()
		{
			Resolver<string, TestFact, Statement, TestMethod> resolver = mock(typeof(Resolver));

			sut.addIncoming(new WrappedFact<string, TestFact, Statement, TestMethod>(fact, createAccessPath(), callEdgeResolver), resolver, Delta);
			sut.resolve(getDeltaConstraint("a"), callback);

			verify(resolver).resolve(eq(getDeltaConstraint("a")), any(typeof(InterestCallback)));
		}


//ORIGINAL LINE: @Test public void incomingZeroCallEdgeResolver()
		public virtual void incomingZeroCallEdgeResolver()
		{
			Resolver<string, TestFact, Statement, TestMethod> resolver = mock(typeof(Resolver));
			ZeroCallEdgeResolver<string, TestFact, Statement, TestMethod> zeroResolver = mock(typeof(ZeroCallEdgeResolver));
			sut.addIncoming(new WrappedFact<string, TestFact, Statement, TestMethod>(fact, createAccessPath(), zeroResolver), resolver, Delta);
			sut.resolve(getDeltaConstraint("a"), callback);

			verify(resolver, never()).resolve(any(typeof(FlowFunction_Constraint)), any(typeof(InterestCallback)));
			verify(callback, never()).interest(any(typeof(PerAccessPathMethodAnalyzer)), any(typeof(Resolver)));
			verify(callback, never()).canBeResolvedEmpty();
		}

		private class ReturnSiteResolverArgumentMatcher : ArgumentMatcher<ReturnSiteResolver<string, TestFact, Statement, TestMethod>>
		{
			private readonly ReturnSiteResolverTest outerInstance;


			internal AccessPath<string> accPath;

			public ReturnSiteResolverArgumentMatcher(ReturnSiteResolverTest outerInstance, AccessPath<string> accPath)
			{
				this.outerInstance = outerInstance;
				this.accPath = accPath;
			}

			public override bool matches(object argument)
			{
				ReturnSiteResolver resolver = (ReturnSiteResolver) argument;
				return resolver.InterestGiven && resolver.resolvedAccessPath.Equals(accPath) && resolver.ReturnSite.Equals(outerInstance.returnSite);
			}
		}
	}

}