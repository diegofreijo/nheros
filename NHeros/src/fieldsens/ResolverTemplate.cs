using System.Collections.Generic;
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
	public abstract class ResolverTemplate<Field, Fact, Stmt, Method, Incoming> : Resolver<Field, Fact, Stmt, Method>
	{
		private bool recursionLock = false;
		protected internal ISet<Incoming> incomingEdges = new HashSet<Incoming>();
		private ResolverTemplate<Field, Fact, Stmt, Method, Incoming> parent;
		private IDictionary<AccessPath<Field>, ResolverTemplate<Field, Fact, Stmt, Method, Incoming>> nestedResolvers = new Dictionary<AccessPath<Field>, ResolverTemplate<Field, Fact, Stmt, Method, Incoming>>();
		private IDictionary<AccessPath<Field>, ResolverTemplate<Field, Fact, Stmt, Method, Incoming>> allResolversInExclHierarchy;
		protected internal AccessPath<Field> resolvedAccessPath;
		protected internal Debugger<Field, Fact, Stmt, Method> debugger;

		public ResolverTemplate(PerAccessPathMethodAnalyzer<Field, Fact, Stmt, Method> analyzer, AccessPath<Field> resolvedAccessPath, ResolverTemplate<Field, Fact, Stmt, Method, Incoming> parent, Debugger<Field, Fact, Stmt, Method> debugger) : base(analyzer)
		{
			this.resolvedAccessPath = resolvedAccessPath;
			this.parent = parent;
			this.debugger = debugger;
			if (parent == null || resolvedAccessPath.Exclusions.Count == 0)
			{
				allResolversInExclHierarchy = new Dictionary<AccessPath<Field>, ResolverTemplate<Field, Fact, Stmt, Method, Incoming>>();
			}
			else
			{
				allResolversInExclHierarchy = parent.allResolversInExclHierarchy;
			}
			debugger.newResolver(analyzer, this);
		}

		protected internal virtual bool Locked
		{
			get
			{
				if (recursionLock)
				{
					return true;
				}
				if (parent == null)
				{
					return false;
				}
				return parent.Locked;
			}
		}

		protected internal virtual void @lock()
		{
			recursionLock = true;
		}

		protected internal virtual void unlock()
		{
			recursionLock = false;
		}

		protected internal abstract AccessPath<Field> getAccessPathOf(Incoming inc);

		public virtual void addIncoming(Incoming inc)
		{
			if (resolvedAccessPath.isPrefixOf(getAccessPathOf(inc)) == AccessPath<Field>.PrefixTestResult.GUARANTEED_PREFIX)
			{
				log("Incoming Edge: " + inc);
				if (!incomingEdges.Add(inc))
				{
					return;
				}

				interest(this);

				foreach (ResolverTemplate<Field, Fact, Stmt, Method, Incoming> nestedResolver in nestedResolvers.Values)
				{
					nestedResolver.addIncoming(inc);
				}

				processIncomingGuaranteedPrefix(inc);
			}
            else if (getAccessPathOf(inc).isPrefixOf(resolvedAccessPath).atLeast(AccessPath<Field>.PrefixTestResult.POTENTIAL_PREFIX))
			{
				processIncomingPotentialPrefix(inc);
			}
		}

		protected internal abstract void processIncomingPotentialPrefix(Incoming inc);

		protected internal abstract void processIncomingGuaranteedPrefix(Incoming inc);

		public override void resolve(FlowFunction_Constraint<Field> constraint, InterestCallback<Field, Fact, Stmt, Method> callback)
		{
			log("Resolve: " + constraint);
			debugger.askedToResolve(this, constraint);
			if (constraint.canBeAppliedTo(resolvedAccessPath) && !Locked)
			{
				AccessPath<Field> newAccPath = constraint.applyToAccessPath(resolvedAccessPath);
				ResolverTemplate<Field, Fact, Stmt, Method, Incoming> nestedResolver = getOrCreateNestedResolver(newAccPath);
				Debug.Assert(nestedResolver.resolvedAccessPath.Equals(constraint.applyToAccessPath(resolvedAccessPath)));
				nestedResolver.registerCallback(callback);
			}
		}

		protected internal virtual ResolverTemplate<Field, Fact, Stmt, Method, Incoming> getOrCreateNestedResolver(AccessPath<Field> newAccPath)
		{
			if (resolvedAccessPath.Equals(newAccPath))
			{
				return this;
			}

			if (!nestedResolvers.ContainsKey(newAccPath))
			{
				Debug.Assert(resolvedAccessPath.getDeltaTo(newAccPath).accesses.Length <= 1);
				if (allResolversInExclHierarchy.ContainsKey(newAccPath))
				{
					return allResolversInExclHierarchy[newAccPath];
				}
				else
				{
					ResolverTemplate<Field, Fact, Stmt, Method, Incoming> nestedResolver = createNestedResolver(newAccPath);
					if (resolvedAccessPath.Exclusions.Count > 0 || newAccPath.Exclusions.Count > 0)
					{
						allResolversInExclHierarchy[newAccPath] = nestedResolver;
					}
					nestedResolvers[newAccPath] = nestedResolver;
					foreach (Incoming inc in incomingEdges)
					{
						nestedResolver.addIncoming(inc);
					}
					return nestedResolver;
				}
			}
			return nestedResolvers[newAccPath];
		}

		protected internal abstract ResolverTemplate<Field, Fact, Stmt, Method, Incoming> createNestedResolver(AccessPath<Field> newAccPath);
	}
}