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

	public class ReturnSiteResolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> : ResolverTemplate<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo, ReturnEdge<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>>
	{

		private Stmt returnSite;
		private bool propagated = false;
		private Fact sourceFact;
		private FactMergeHandler<Fact> factMergeHandler;

		public ReturnSiteResolver(FactMergeHandler<Fact> factMergeHandler, PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> analyzer, Stmt returnSite, Debugger<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> debugger) : this(factMergeHandler, analyzer, returnSite, null, debugger, new AccessPath<System.Reflection.FieldInfo>(), null)
		{
			this.factMergeHandler = factMergeHandler;
			propagated = false;
		}

		private ReturnSiteResolver(FactMergeHandler<Fact> factMergeHandler, PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> analyzer, Stmt returnSite, Fact sourceFact, Debugger<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> debugger, AccessPath<System.Reflection.FieldInfo> resolvedAccPath, ReturnSiteResolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> parent) : base(analyzer, resolvedAccPath, parent, debugger)
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

		protected internal virtual AccessPath<System.Reflection.FieldInfo> getAccessPathOf(ReturnEdge<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> inc)
		{
			return inc.usedAccessPathOfIncResolver.applyTo(inc.incAccessPath);
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public void addIncoming(final heros.fieldsens.structs.WrappedFact<Field, Fact, Stmt, Method> fact, Resolver<Field, Fact, Stmt, Method> resolverAtCaller, heros.fieldsens.AccessPath.Delta<Field> callDelta)
		public virtual void addIncoming(WrappedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> fact, Resolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> resolverAtCaller, Delta<System.Reflection.FieldInfo> callDelta)
		{

			addIncoming(new ReturnEdge<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(fact, resolverAtCaller, callDelta));
		}

		protected internal virtual void processIncomingGuaranteedPrefix(ReturnEdge<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> retEdge)
		{
			if (propagated)
			{
				factMergeHandler.merge(sourceFact, retEdge.incFact);
			}
			else
			{
				propagated = true;
				sourceFact = retEdge.incFact;
				analyzer.scheduleEdgeTo(new WrappedFactAtStatement<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(returnSite, new WrappedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(retEdge.incFact, new AccessPath<System.Reflection.FieldInfo>(), this)));
			}
		};

		protected internal virtual void processIncomingPotentialPrefix(ReturnEdge<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> retEdge)
		{
			log("Incoming potential prefix:  " + retEdge);
			resolveViaDelta(retEdge);
		};

		protected internal override void log(string message)
		{
			analyzer.log("Return Site " + ToString() + ": " + message);
		}

		protected internal override ResolverTemplate<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo, ReturnEdge<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>> createNestedResolver(AccessPath<System.Reflection.FieldInfo> newAccPath)
		{
			return new ReturnSiteResolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(factMergeHandler, analyzer, returnSite, sourceFact, debugger, newAccPath, this);
		}

		public virtual Stmt ReturnSite
		{
			get
			{
				return returnSite;
			}
		}

		private bool isNullOrCallEdgeResolver(Resolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> resolver)
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

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private void resolveViaDelta(final heros.fieldsens.structs.ReturnEdge<Field, Fact, Stmt, Method> retEdge)
		private void resolveViaDelta(ReturnEdge<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> retEdge)
		{
			if (isNullOrCallEdgeResolver(retEdge.incResolver))
			{
				resolveViaDeltaAndPotentiallyDelegateToCallSite(retEdge);
			}
			else
			{
				//resolve via incoming facts resolver
				Delta<System.Reflection.FieldInfo> delta = retEdge.usedAccessPathOfIncResolver.applyTo(retEdge.incAccessPath).getDeltaTo(resolvedAccessPath);
				Debug.Assert(delta.accesses.Length <= 1);
				retEdge.incResolver.resolve(new DeltaConstraint<System.Reflection.FieldInfo>(delta), new InterestCallbackAnonymousInnerClass(this, retEdge));
			}
		}

		private class InterestCallbackAnonymousInnerClass : InterestCallback<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>
		{
			private readonly ReturnSiteResolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> outerInstance;

			private ReturnEdge<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> retEdge;

			public InterestCallbackAnonymousInnerClass(ReturnSiteResolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> outerInstance, ReturnEdge<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> retEdge)
			{
				this.outerInstance = outerInstance;
				this.retEdge = retEdge;
			}


			public void interest(PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> analyzer, Resolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> resolver)
			{
				if (resolver is ZeroCallEdgeResolver)
				{
					outerInstance.interest(((ZeroCallEdgeResolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>) resolver).copyWithAnalyzer(outerInstance.analyzer));
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

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private void resolveViaDeltaAndPotentiallyDelegateToCallSite(final heros.fieldsens.structs.ReturnEdge<Field, Fact, Stmt, Method> retEdge)
		private void resolveViaDeltaAndPotentiallyDelegateToCallSite(ReturnEdge<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> retEdge)
		{
			AccessPath<System.Reflection.FieldInfo> inc = retEdge.usedAccessPathOfIncResolver.applyTo(retEdge.incAccessPath);
			if (!retEdge.callDelta.canBeAppliedTo(inc))
			{
				return;
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final AccessPath<Field> currAccPath = retEdge.callDelta.applyTo(inc);
			AccessPath<System.Reflection.FieldInfo> currAccPath = retEdge.callDelta.applyTo(inc);
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

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: protected void resolveViaCallSiteResolver(final heros.fieldsens.structs.ReturnEdge<Field, Fact, Stmt, Method> retEdge, AccessPath<Field> currAccPath)
		protected internal virtual void resolveViaCallSiteResolver(ReturnEdge<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> retEdge, AccessPath<System.Reflection.FieldInfo> currAccPath)
		{
			if (isNullOrCallEdgeResolver(retEdge.resolverAtCaller))
			{
				canBeResolvedEmpty();
			}
			else
			{
				retEdge.resolverAtCaller.resolve(new DeltaConstraint<System.Reflection.FieldInfo>(currAccPath.getDeltaTo(resolvedAccessPath)), new InterestCallbackAnonymousInnerClass2(this));
			}
		}

		private class InterestCallbackAnonymousInnerClass2 : InterestCallback<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>
		{
			private readonly ReturnSiteResolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> outerInstance;

			public InterestCallbackAnonymousInnerClass2(ReturnSiteResolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void interest(PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> analyzer, Resolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> resolver)
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