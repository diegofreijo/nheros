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

	using Lists = com.google.common.collect.Lists;
	using Maps = com.google.common.collect.Maps;
	using Sets = com.google.common.collect.Sets;

	using PrefixTestResult = heros.fieldsens.AccessPath.PrefixTestResult;


	public abstract class ResolverTemplate<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo, Incoming> : Resolver<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo>
	{

		private bool recursionLock = false;
		protected internal ISet<Incoming> incomingEdges = Sets.newHashSet();
		private ResolverTemplate<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo, Incoming> parent;
		private IDictionary<AccessPath<System.Reflection.FieldInfo>, ResolverTemplate<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo, Incoming>> nestedResolvers = new Dictionary();
		private IDictionary<AccessPath<System.Reflection.FieldInfo>, ResolverTemplate<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo, Incoming>> allResolversInExclHierarchy;
		protected internal AccessPath<System.Reflection.FieldInfo> resolvedAccessPath;
		protected internal Debugger<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> debugger;

		public ResolverTemplate(PerAccessPathMethodAnalyzer<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> analyzer, AccessPath<System.Reflection.FieldInfo> resolvedAccessPath, ResolverTemplate<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo, Incoming> parent, Debugger<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> debugger) : base(analyzer)
		{
			this.resolvedAccessPath = resolvedAccessPath;
			this.parent = parent;
			this.debugger = debugger;
			if (parent == null || resolvedAccessPath.Exclusions.Count == 0)
			{
				allResolversInExclHierarchy = new Dictionary();
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

		protected internal abstract AccessPath<System.Reflection.FieldInfo> getAccessPathOf(Incoming inc);

		public virtual void addIncoming(Incoming inc)
		{
			if (resolvedAccessPath.isPrefixOf(getAccessPathOf(inc)) == PrefixTestResult.GUARANTEED_PREFIX)
			{
				log("Incoming Edge: " + inc);
				if (!incomingEdges.Add(inc))
				{
					return;
				}

				interest(this);

				foreach (ResolverTemplate<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo, Incoming> nestedResolver in Lists.newLinkedList(nestedResolvers.Values))
				{
					nestedResolver.addIncoming(inc);
				}

				processIncomingGuaranteedPrefix(inc);
			}
			else if (getAccessPathOf(inc).isPrefixOf(resolvedAccessPath).atLeast(PrefixTestResult.POTENTIAL_PREFIX))
			{
				processIncomingPotentialPrefix(inc);
			}
		}

		protected internal abstract void processIncomingPotentialPrefix(Incoming inc);

		protected internal abstract void processIncomingGuaranteedPrefix(Incoming inc);

		public override void resolve(FlowFunction_Constraint<System.Reflection.FieldInfo> constraint, InterestCallback<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo> callback)
		{
			log("Resolve: " + constraint);
			debugger.askedToResolve(this, constraint);
			if (constraint.canBeAppliedTo(resolvedAccessPath) && !Locked)
			{
				AccessPath<System.Reflection.FieldInfo> newAccPath = constraint.applyToAccessPath(resolvedAccessPath);
				ResolverTemplate<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo, Incoming> nestedResolver = getOrCreateNestedResolver(newAccPath);
				Debug.Assert(nestedResolver.resolvedAccessPath.Equals(constraint.applyToAccessPath(resolvedAccessPath)));
				nestedResolver.registerCallback(callback);
			}
		}

		protected internal virtual ResolverTemplate<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo, Incoming> getOrCreateNestedResolver(AccessPath<System.Reflection.FieldInfo> newAccPath)
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
					ResolverTemplate<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo, Incoming> nestedResolver = createNestedResolver(newAccPath);
					if (resolvedAccessPath.Exclusions.Count > 0 || newAccPath.Exclusions.Count > 0)
					{
						allResolversInExclHierarchy[newAccPath] = nestedResolver;
					}
					nestedResolvers[newAccPath] = nestedResolver;
					foreach (Incoming inc in Lists.newLinkedList(incomingEdges))
					{
						nestedResolver.addIncoming(inc);
					}
					return nestedResolver;
				}
			}
			return nestedResolvers[newAccPath];
		}

		protected internal abstract ResolverTemplate<System.Reflection.FieldInfo, Fact, Stmt, System.Reflection.MethodInfo, Incoming> createNestedResolver(AccessPath<System.Reflection.FieldInfo> newAccPath);
	}
}