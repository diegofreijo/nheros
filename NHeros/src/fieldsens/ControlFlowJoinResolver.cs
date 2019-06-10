using heros.fieldsens.structs;
using static heros.fieldsens.AccessPath<T>;

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
	public class ControlFlowJoinResolver<Field, Fact, Stmt, Method> : ResolverTemplate<Field, Fact, Stmt, Method, WrappedFact<Field, Fact, Stmt, Method>>
	{

		private Stmt joinStmt;
		private bool propagated = false;
		private Fact sourceFact;
		private FactMergeHandler factMergeHandler;

		public ControlFlowJoinResolver(FactMergeHandler factMergeHandler, PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> analyzer, Stmt joinStmt, Debugger<Field, Fact, Stmt, Method> debugger) : this(factMergeHandler, analyzer, joinStmt, default, new AccessPath<Field>(), debugger, null)
		{
			this.factMergeHandler = factMergeHandler;
			propagated = false;
		}

		private ControlFlowJoinResolver(FactMergeHandler factMergeHandler, PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> analyzer, Stmt joinStmt, Fact sourceFact, AccessPath<Field> resolvedAccPath, Debugger<Field, Fact, Stmt, Method> debugger, ControlFlowJoinResolver<Field, Fact, Stmt, Method> parent) : base(analyzer, resolvedAccPath, parent, debugger)
		{
			this.factMergeHandler = factMergeHandler;
			this.joinStmt = joinStmt;
			this.sourceFact = sourceFact;
			propagated = true;
		}

		protected internal override AccessPath<Field> getAccessPathOf(WrappedFact<Field, Fact, Stmt, Method> inc)
		{
			return inc.AccessPath;
		}

		protected internal virtual void processIncomingGuaranteedPrefix(WrappedFact<Field, Fact, Stmt, Method> fact)
		{
			if (propagated)
			{
				factMergeHandler.merge(sourceFact, fact.Fact);
			}
			else
			{
				propagated = true;
				sourceFact = fact.Fact;
				analyzer.processFlowFromJoinStmt(new WrappedFactAtStatement<Field, Fact, Stmt, Method>(joinStmt, new WrappedFact<Field, Fact, Stmt, Method>(fact.Fact, new AccessPath<Field>(), this)));
			}
		}

		private bool isNullOrCallEdgeResolver(Resolver<Field, Fact, Stmt, Method> resolver)
		{
			if (resolver == null)
			{
				return true;
			}
			if (resolver is CallEdgeResolver<Field, Fact, Stmt, Method>)
			{
				return !(resolver is ZeroCallEdgeResolver<Field, Fact, Stmt, Method>);
			}
			return false;
		}

    	protected internal override void processIncomingPotentialPrefix(WrappedFact<Field, Fact, Stmt, Method> fact)
		{
			if (isNullOrCallEdgeResolver(fact.Resolver))
			{
				canBeResolvedEmpty();
			}
			else
			{
				@lock();
				Delta<Field> delta = fact.AccessPath.getDeltaTo(resolvedAccessPath);
				fact.Resolver.resolve(new DeltaConstraint<Field>(delta), new InterestCallbackAnonymousInnerClass(this));
				unlock();
			}
		}

		private class InterestCallbackAnonymousInnerClass : InterestCallback<Field, Fact, Stmt, Method>
		{
			private readonly ControlFlowJoinResolver<Field, Fact, Stmt, Method> outerInstance;

			public InterestCallbackAnonymousInnerClass(ControlFlowJoinResolver<Field, Fact, Stmt, Method> outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void interest(PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> analyzer, Resolver<Field, Fact, Stmt, Method> resolver)
			{
				outerInstance.interest(resolver);
			}

			public void canBeResolvedEmpty()
			{
				outerInstance.canBeResolvedEmpty();
			}
		}

		protected internal override ResolverTemplate<Field, Fact, Stmt, Method, WrappedFact<Field, Fact, Stmt, Method>> createNestedResolver(AccessPath<Field> newAccPath)
		{
			return new ControlFlowJoinResolver<Field, Fact, Stmt, Method>(factMergeHandler, analyzer, joinStmt, sourceFact, newAccPath, debugger, this);
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