using System.Diagnostics;

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
	using Delta = heros.fieldsens.AccessPath.Delta;
	using PrefixTestResult = heros.fieldsens.AccessPath.PrefixTestResult;
	using DeltaConstraint = heros.fieldsens.structs.DeltaConstraint;
	using WrappedFact = heros.fieldsens.structs.WrappedFact;
	using WrappedFactAtStatement = heros.fieldsens.structs.WrappedFactAtStatement;

	public class CallEdge<Field, Fact, Stmt, Method>
	{

		private WrappedFact<Field, Fact, Stmt, Method> calleeSourceFact;
		private PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> callerAnalyzer;
		private WrappedFactAtStatement<Field, Fact, Stmt, Method> factAtCallSite;

		public CallEdge(PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> callerAnalyzer, WrappedFactAtStatement<Field, Fact, Stmt, Method> factAtCallSite, WrappedFact<Field, Fact, Stmt, Method> calleeSourceFact)
		{
			this.callerAnalyzer = callerAnalyzer;
			this.factAtCallSite = factAtCallSite;
			this.calleeSourceFact = calleeSourceFact;
		}

		public virtual WrappedFact<Field, Fact, Stmt, Method> CalleeSourceFact
		{
			get
			{
				return calleeSourceFact;
			}
		}

		public virtual WrappedFact<Field, Fact, Stmt, Method> CallerCallSiteFact
		{
			get
			{
				return factAtCallSite.WrappedFact;
			}
		}

		public virtual WrappedFact<Field, Fact, Stmt, Method> CallerSourceFact
		{
			get
			{
				return callerAnalyzer.wrappedSource();
			}
		}

		public virtual Stmt CallSite
		{
			get
			{
				return factAtCallSite.Statement;
			}
		}

		public virtual PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> CallerAnalyzer
		{
			get
			{
				return callerAnalyzer;
			}
		}


//ORIGINAL LINE: public void registerInterestCallback(final PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> interestedAnalyzer)
		public virtual void registerInterestCallback(PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> interestedAnalyzer)
		{

//ORIGINAL LINE: final heros.fieldsens.AccessPath.Delta<Field> delta = calleeSourceFact.getAccessPath().getDeltaTo(interestedAnalyzer.getAccessPath());
			Delta<Field> delta = calleeSourceFact.AccessPath.getDeltaTo(interestedAnalyzer.AccessPath);

			if (!factAtCallSite.canDeltaBeApplied(delta))
			{
				return;
			}

			factAtCallSite.WrappedFact.Resolver.resolve(new DeltaConstraint<Field>(delta), new InterestCallbackAnonymousInnerClass(this, interestedAnalyzer, delta));
		}

		private class InterestCallbackAnonymousInnerClass : InterestCallback<Field, Fact, Stmt, Method>
		{
			private readonly CallEdge<Field, Fact, Stmt, Method> outerInstance;

			private heros.fieldsens.PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> interestedAnalyzer;
			private Delta<Field> delta;

			public InterestCallbackAnonymousInnerClass(CallEdge<Field, Fact, Stmt, Method> outerInstance, heros.fieldsens.PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> interestedAnalyzer, Delta<Field> delta)
			{
				this.outerInstance = outerInstance;
				this.interestedAnalyzer = interestedAnalyzer;
				this.delta = delta;
			}


			public void interest(PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> analyzer, Resolver<Field, Fact, Stmt, Method> resolver)
			{
				WrappedFact<Field, Fact, Stmt, Method> calleeSourceFactWithDelta = new WrappedFact<Field, Fact, Stmt, Method>(outerInstance.calleeSourceFact.Fact, delta.applyTo(outerInstance.calleeSourceFact.AccessPath), resolver);
				Debug.Assert(interestedAnalyzer.AccessPath.isPrefixOf(calleeSourceFactWithDelta.AccessPath) == PrefixTestResult.GUARANTEED_PREFIX);

				CallEdge<Field, Fact, Stmt, Method> newCallEdge = new CallEdge<Field, Fact, Stmt, Method>(analyzer, new WrappedFactAtStatement<Field, Fact, Stmt, Method>(outerInstance.factAtCallSite.Statement, new WrappedFact<Field, Fact, Stmt, Method>(outerInstance.factAtCallSite.WrappedFact.Fact, delta.applyTo(outerInstance.factAtCallSite.WrappedFact.AccessPath), resolver)), calleeSourceFactWithDelta);

				if (resolver is ZeroCallEdgeResolver)
				{
					interestedAnalyzer.CallEdgeResolver.incomingEdges.Add(newCallEdge);
					interestedAnalyzer.CallEdgeResolver.interest(((ZeroCallEdgeResolver<Field, Fact, Stmt, Method>) resolver).copyWithAnalyzer(interestedAnalyzer));
					interestedAnalyzer.CallEdgeResolver.processIncomingGuaranteedPrefix(newCallEdge);
				}
				else
				{
					interestedAnalyzer.addIncomingEdge(newCallEdge);
				}
			}

			public void canBeResolvedEmpty()
			{
				outerInstance.callerAnalyzer.CallEdgeResolver.resolve(new DeltaConstraint<Field>(delta), this);
			}
		}

		public override string ToString()
		{
			return "[IncEdge CSite:" + CallSite + ", Caller-Edge: " + CallerSourceFact + "->" + CallerCallSiteFact + ",  CalleeFact: " + calleeSourceFact + "]";
		}

		public override int GetHashCode()
		{
			const int prime = 31;
			int result = 1;
			result = prime * result + ((calleeSourceFact == null) ? 0 : calleeSourceFact.GetHashCode());
			result = prime * result + ((callerAnalyzer == null) ? 0 : callerAnalyzer.GetHashCode());
			result = prime * result + ((factAtCallSite == null) ? 0 : factAtCallSite.GetHashCode());
			return result;
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			if (obj == null)
			{
				return false;
			}
			if (this.GetType() != obj.GetType())
			{
				return false;
			}
			CallEdge other = (CallEdge) obj;
			if (calleeSourceFact == null)
			{
				if (other.calleeSourceFact != null)
				{
					return false;
				}
			}
			else if (!calleeSourceFact.Equals(other.calleeSourceFact))
			{
				return false;
			}
			if (callerAnalyzer == null)
			{
				if (other.callerAnalyzer != null)
				{
					return false;
				}
			}
			else if (!callerAnalyzer.Equals(other.callerAnalyzer))
			{
				return false;
			}
			if (factAtCallSite == null)
			{
				if (other.factAtCallSite != null)
				{
					return false;
				}
			}
			else if (!factAtCallSite.Equals(other.factAtCallSite))
			{
				return false;
			}
			return true;
		}


	}

}