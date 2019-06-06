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
	using DeltaConstraint = heros.fieldsens.structs.DeltaConstraint;
	using WrappedFact = heros.fieldsens.structs.WrappedFact;
	using WrappedFactAtStatement = heros.fieldsens.structs.WrappedFactAtStatement;

	public class ControlFlowJoinResolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> : ResolverTemplate<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo, WrappedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>>
	{

		private Stmt joinStmt;
		private bool propagated = false;
		private Fact sourceFact;
		private FactMergeHandler<Fact> factMergeHandler;

		public ControlFlowJoinResolver(FactMergeHandler<Fact> factMergeHandler, PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> analyzer, Stmt joinStmt, Debugger<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> debugger) : this(factMergeHandler, analyzer, joinStmt, null, new AccessPath<System.Reflection.FieldInfo>(), debugger, null)
		{
			this.factMergeHandler = factMergeHandler;
			propagated = false;
		}

		private ControlFlowJoinResolver(FactMergeHandler<Fact> factMergeHandler, PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> analyzer, Stmt joinStmt, Fact sourceFact, AccessPath<System.Reflection.FieldInfo> resolvedAccPath, Debugger<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> debugger, ControlFlowJoinResolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> parent) : base(analyzer, resolvedAccPath, parent, debugger)
		{
			this.factMergeHandler = factMergeHandler;
			this.joinStmt = joinStmt;
			this.sourceFact = sourceFact;
			propagated = true;
		}

		protected internal override AccessPath<System.Reflection.FieldInfo> getAccessPathOf(WrappedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> inc)
		{
			return inc.AccessPath;
		}

		protected internal virtual void processIncomingGuaranteedPrefix(WrappedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> fact)
		{
			if (propagated)
			{
				factMergeHandler.merge(sourceFact, fact.Fact);
			}
			else
			{
				propagated = true;
				sourceFact = fact.Fact;
				analyzer.processFlowFromJoinStmt(new WrappedFactAtStatement<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(joinStmt, new WrappedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(fact.Fact, new AccessPath<System.Reflection.FieldInfo>(), this)));
			}
		};

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
//ORIGINAL LINE: @Override protected void processIncomingPotentialPrefix(final heros.fieldsens.structs.WrappedFact<Field, Fact, Stmt, Method> fact)
		protected internal override void processIncomingPotentialPrefix(WrappedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> fact)
		{
			if (isNullOrCallEdgeResolver(fact.Resolver))
			{
				canBeResolvedEmpty();
			}
			else
			{
				@lock();
				Delta<System.Reflection.FieldInfo> delta = fact.AccessPath.getDeltaTo(resolvedAccessPath);
				fact.Resolver.resolve(new DeltaConstraint<System.Reflection.FieldInfo>(delta), new InterestCallbackAnonymousInnerClass(this));
				unlock();
			}
		}

		private class InterestCallbackAnonymousInnerClass : InterestCallback<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>
		{
			private readonly ControlFlowJoinResolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> outerInstance;

			public InterestCallbackAnonymousInnerClass(ControlFlowJoinResolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void interest(PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> analyzer, Resolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> resolver)
			{
				outerInstance.interest(resolver);
			}

			public void canBeResolvedEmpty()
			{
				outerInstance.canBeResolvedEmpty();
			}
		}

		protected internal override ResolverTemplate<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo, WrappedFact<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>> createNestedResolver(AccessPath<System.Reflection.FieldInfo> newAccPath)
		{
			return new ControlFlowJoinResolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>(factMergeHandler, analyzer, joinStmt, sourceFact, newAccPath, debugger, this);
		}

		protected internal override void log(string message)
		{
			analyzer.log("Join Stmt " + ToString() + ": " + message);
		}

		public override string ToString()
		{
			return "<" + resolvedAccessPath + ":" + joinStmt + " in " + analyzer.Method + ">";
		}

		public virtual Stmt JoinStmt
		{
			get
			{
				return joinStmt;
			}
		}
	}

}