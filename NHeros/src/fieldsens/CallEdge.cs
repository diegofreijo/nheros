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

	public class CallEdge<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>
	{

		private WrappedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> calleeSourceFact;
		private PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> callerAnalyzer;
		private WrappedFactAtStatement<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> factAtCallSite;

		public CallEdge(PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> callerAnalyzer, WrappedFactAtStatement<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> factAtCallSite, WrappedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> calleeSourceFact)
		{
			this.callerAnalyzer = callerAnalyzer;
			this.factAtCallSite = factAtCallSite;
			this.calleeSourceFact = calleeSourceFact;
		}

		public virtual WrappedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> CalleeSourceFact
		{
			get
			{
				return calleeSourceFact;
			}
		}

		public virtual WrappedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> CallerCallSiteFact
		{
			get
			{
				return factAtCallSite.WrappedFact;
			}
		}

		public virtual WrappedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> CallerSourceFact
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

		public virtual PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> CallerAnalyzer
		{
			get
			{
				return callerAnalyzer;
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public void registerInterestCallback(final PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> interestedAnalyzer)
		public virtual void registerInterestCallback(PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> interestedAnalyzer)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final heros.fieldsens.AccessPath.Delta<Field> delta = calleeSourceFact.getAccessPath().getDeltaTo(interestedAnalyzer.getAccessPath());
			Delta<System.Reflection.FieldInfo> delta = calleeSourceFact.AccessPath.getDeltaTo(interestedAnalyzer.AccessPath);

			if (!factAtCallSite.canDeltaBeApplied(delta))
			{
				return;
			}

			factAtCallSite.WrappedFact.Resolver.resolve(new DeltaConstraint<System.Reflection.FieldInfo>(delta), new InterestCallbackAnonymousInnerClass(this, interestedAnalyzer, delta));
		}

		private class InterestCallbackAnonymousInnerClass : InterestCallback<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>
		{
			private readonly CallEdge<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> outerInstance;

			private heros.fieldsens.PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> interestedAnalyzer;
			private Delta<System.Reflection.FieldInfo> delta;

			public InterestCallbackAnonymousInnerClass(CallEdge<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> outerInstance, heros.fieldsens.PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> interestedAnalyzer, Delta<System.Reflection.FieldInfo> delta)
			{
				this.outerInstance = outerInstance;
				this.interestedAnalyzer = interestedAnalyzer;
				this.delta = delta;
			}


			public void interest(PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> analyzer, Resolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> resolver)
			{
				WrappedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> calleeSourceFactWithDelta = new WrappedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(outerInstance.calleeSourceFact.Fact, delta.applyTo(outerInstance.calleeSourceFact.AccessPath), resolver);
				Debug.Assert(interestedAnalyzer.AccessPath.isPrefixOf(calleeSourceFactWithDelta.AccessPath) == PrefixTestResult.GUARANTEED_PREFIX);

				CallEdge<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> newCallEdge = new CallEdge<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(analyzer, new WrappedFactAtStatement<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(outerInstance.factAtCallSite.Statement, new WrappedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(outerInstance.factAtCallSite.WrappedFact.Fact, delta.applyTo(outerInstance.factAtCallSite.WrappedFact.AccessPath), resolver)), calleeSourceFactWithDelta);

				if (resolver is ZeroCallEdgeResolver)
				{
					interestedAnalyzer.CallEdgeResolver.incomingEdges.Add(newCallEdge);
					interestedAnalyzer.CallEdgeResolver.interest(((ZeroCallEdgeResolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>) resolver).copyWithAnalyzer(interestedAnalyzer));
					interestedAnalyzer.CallEdgeResolver.processIncomingGuaranteedPrefix(newCallEdge);
				}
				else
				{
					interestedAnalyzer.addIncomingEdge(newCallEdge);
				}
			}

			public void canBeResolvedEmpty()
			{
				outerInstance.callerAnalyzer.CallEdgeResolver.resolve(new DeltaConstraint<System.Reflection.FieldInfo>(delta), this);
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