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
	using DeltaConstraint = heros.fieldsens.structs.DeltaConstraint;
	using ReturnEdge = heros.fieldsens.structs.ReturnEdge;
	using WrappedFact = heros.fieldsens.structs.WrappedFact;
	using WrappedFactAtStatement = heros.fieldsens.structs.WrappedFactAtStatement;

	public class ReturnSiteResolver<Field, Fact, Stmt, Method> : ResolverTemplate<Field, Fact, Stmt, Method, ReturnEdge<Field, Fact, Stmt, Method>>
	{

		private Stmt returnSite;
		private bool propagated = false;
		private Fact sourceFact;
		private FactMergeHandler factMergeHandler;

		public ReturnSiteResolver(FactMergeHandler factMergeHandler, PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> analyzer, Stmt returnSite, Debugger<Field, Fact, Stmt, Method> debugger) : this(factMergeHandler, analyzer, returnSite, null, debugger, new AccessPath<Field>(), null)
		{
			this.factMergeHandler = factMergeHandler;
			propagated = false;
		}

		private ReturnSiteResolver(FactMergeHandler factMergeHandler, PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> analyzer, Stmt returnSite, Fact sourceFact, Debugger<Field, Fact, Stmt, Method> debugger, AccessPath<Field> resolvedAccPath, ReturnSiteResolver<Field, Fact, Stmt, Method> parent) : base(analyzer, resolvedAccPath, parent, debugger)
		{
			this.factMergeHandler = factMergeHandler;
			this.returnSite = returnSite;
			this.sourceFact = sourceFact;
			propagated = true;
		}

		public override string ToString()
		{
			return "<" + resolvedAccessPath + ":" + returnSite + " in " + analyzer.Method + ">";
		}

		protected internal virtual AccessPath<Field> getAccessPathOf(ReturnEdge<Field, Fact, Stmt, Method> inc)
		{
			return inc.usedAccessPathOfIncResolver.applyTo(inc.incAccessPath);
		}


//ORIGINAL LINE: public void addIncoming(final heros.fieldsens.structs.WrappedFact<Field, Fact, Stmt, Method> fact, Resolver<Field, Fact, Stmt, Method> resolverAtCaller, heros.fieldsens.AccessPath.Delta<Field> callDelta)
		public virtual void addIncoming(WrappedFact<Field, Fact, Stmt, Method> fact, Resolver<Field, Fact, Stmt, Method> resolverAtCaller, Delta<Field> callDelta)
		{

			addIncoming(new ReturnEdge<Field, Fact, Stmt, Method>(fact, resolverAtCaller, callDelta));
		}

		protected internal virtual void processIncomingGuaranteedPrefix(ReturnEdge<Field, Fact, Stmt, Method> retEdge)
		{
			if (propagated)
			{
				factMergeHandler.merge(sourceFact, retEdge.incFact);
			}
			else
			{
				propagated = true;
				sourceFact = retEdge.incFact;
				analyzer.scheduleEdgeTo(new WrappedFactAtStatement<Field, Fact, Stmt, Method>(returnSite, new WrappedFact<Field, Fact, Stmt, Method>(retEdge.incFact, new AccessPath<Field>(), this)));
			}
		};

		protected internal virtual void processIncomingPotentialPrefix(ReturnEdge<Field, Fact, Stmt, Method> retEdge)
		{
			log("Incoming potential prefix:  " + retEdge);
			resolveViaDelta(retEdge);
		};

		protected internal override void log(string message)
		{
			analyzer.log("Return Site " + ToString() + ": " + message);
		}

		protected internal override ResolverTemplate<Field, Fact, Stmt, Method, ReturnEdge<Field, Fact, Stmt, Method>> createNestedResolver(AccessPath<Field> newAccPath)
		{
			return new ReturnSiteResolver<Field, Fact, Stmt, Method>(factMergeHandler, analyzer, returnSite, sourceFact, debugger, newAccPath, this);
		}

		public virtual Stmt ReturnSite
		{
			get
			{
				return returnSite;
			}
		}

		private bool isNullOrCallEdgeResolver(Resolver<Field, Fact, Stmt, Method> resolver)
		{
			if (resolver == null)
			{
				return true;
			}
			if (resolver is CallEdgeResolver)
			{
				return !(resolver is ZeroCallEdgeResolver);
			}
			return false;
		}


//ORIGINAL LINE: private void resolveViaDelta(final heros.fieldsens.structs.ReturnEdge<Field, Fact, Stmt, Method> retEdge)
		private void resolveViaDelta(ReturnEdge<Field, Fact, Stmt, Method> retEdge)
		{
			if (isNullOrCallEdgeResolver(retEdge.incResolver))
			{
				resolveViaDeltaAndPotentiallyDelegateToCallSite(retEdge);
			}
			else
			{
				//resolve via incoming facts resolver
				Delta<Field> delta = retEdge.usedAccessPathOfIncResolver.applyTo(retEdge.incAccessPath).getDeltaTo(resolvedAccessPath);
				Debug.Assert(delta.accesses.Length <= 1);
				retEdge.incResolver.resolve(new DeltaConstraint<Field>(delta), new InterestCallbackAnonymousInnerClass(this, retEdge));
			}
		}

		private class InterestCallbackAnonymousInnerClass : InterestCallback<Field, Fact, Stmt, Method>
		{
			private readonly ReturnSiteResolver<Field, Fact, Stmt, Method> outerInstance;

			private ReturnEdge<Field, Fact, Stmt, Method> retEdge;

			public InterestCallbackAnonymousInnerClass(ReturnSiteResolver<Field, Fact, Stmt, Method> outerInstance, ReturnEdge<Field, Fact, Stmt, Method> retEdge)
			{
				this.outerInstance = outerInstance;
				this.retEdge = retEdge;
			}


			public void interest(PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> analyzer, Resolver<Field, Fact, Stmt, Method> resolver)
			{
				if (resolver is ZeroCallEdgeResolver)
				{
					outerInstance.interest(((ZeroCallEdgeResolver<Field, Fact, Stmt, Method>) resolver).copyWithAnalyzer(outerInstance.analyzer));
				}
				else
				{
					outerInstance.incomingEdges.Add(retEdge.copyWithIncomingResolver(resolver, retEdge.incAccessPath.getDeltaTo(outerInstance.resolvedAccessPath)));
					outerInstance.interest(outerInstance);
				}
			}

			public void canBeResolvedEmpty()
			{
				outerInstance.resolveViaDeltaAndPotentiallyDelegateToCallSite(retEdge);
			}
		}


//ORIGINAL LINE: private void resolveViaDeltaAndPotentiallyDelegateToCallSite(final heros.fieldsens.structs.ReturnEdge<Field, Fact, Stmt, Method> retEdge)
		private void resolveViaDeltaAndPotentiallyDelegateToCallSite(ReturnEdge<Field, Fact, Stmt, Method> retEdge)
		{
			AccessPath<Field> inc = retEdge.usedAccessPathOfIncResolver.applyTo(retEdge.incAccessPath);
			if (!retEdge.callDelta.canBeAppliedTo(inc))
			{
				return;
			}


//ORIGINAL LINE: final AccessPath<Field> currAccPath = retEdge.callDelta.applyTo(inc);
			AccessPath<Field> currAccPath = retEdge.callDelta.applyTo(inc);
			if (resolvedAccessPath.isPrefixOf(currAccPath) == PrefixTestResult.GUARANTEED_PREFIX)
			{
				incomingEdges.Add(retEdge.copyWithIncomingResolver(null, retEdge.usedAccessPathOfIncResolver));
				interest(this);
			}
			else if (currAccPath.isPrefixOf(resolvedAccessPath).atLeast(PrefixTestResult.POTENTIAL_PREFIX))
			{
				resolveViaCallSiteResolver(retEdge, currAccPath);
			}
		}


//ORIGINAL LINE: protected void resolveViaCallSiteResolver(final heros.fieldsens.structs.ReturnEdge<Field, Fact, Stmt, Method> retEdge, AccessPath<Field> currAccPath)
		protected internal virtual void resolveViaCallSiteResolver(ReturnEdge<Field, Fact, Stmt, Method> retEdge, AccessPath<Field> currAccPath)
		{
			if (isNullOrCallEdgeResolver(retEdge.resolverAtCaller))
			{
				canBeResolvedEmpty();
			}
			else
			{
				retEdge.resolverAtCaller.resolve(new DeltaConstraint<Field>(currAccPath.getDeltaTo(resolvedAccessPath)), new InterestCallbackAnonymousInnerClass2(this));
			}
		}

		private class InterestCallbackAnonymousInnerClass2 : InterestCallback<Field, Fact, Stmt, Method>
		{
			private readonly ReturnSiteResolver<Field, Fact, Stmt, Method> outerInstance;

			public InterestCallbackAnonymousInnerClass2(ReturnSiteResolver<Field, Fact, Stmt, Method> outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void interest(PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> analyzer, Resolver<Field, Fact, Stmt, Method> resolver)
			{
		//					incomingEdges.add(retEdge.copyWithResolverAtCaller(resolver, retEdge.incAccessPath.getDeltaTo(getResolvedAccessPath())));
				outerInstance.interest(resolver);
			}

			public void canBeResolvedEmpty()
			{
				outerInstance.canBeResolvedEmpty();
			}
		}
	}

}