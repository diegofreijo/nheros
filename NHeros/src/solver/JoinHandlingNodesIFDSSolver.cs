using System.Collections.Generic;

/// <summary>
///*****************************************************************************
/// Copyright (c) 2013Johannes Lerch.
/// All rights reserved. This program and the accompanying materials
/// are made available under the terms of the GNU Lesser Public License v2.1
/// which accompanies this distribution, and is available at
/// http://www.gnu.org/licenses/old-licenses/gpl-2.0.html
/// 
/// Contributors:
///     Johannes Lerch - initial API and implementation
/// *****************************************************************************
/// </summary>
namespace heros.solver
{

	using Maps = com.google.common.collect.Maps;

	/// <summary>
	/// An <seealso cref="IFDSSolver"/> that tracks paths for reporting. To do so, it requires that data-flow abstractions implement the LinkedNode interface.
	/// The solver implements a cache of data-flow facts for each statement and source value. If for the same statement and source value the same
	/// target value is seen again (as determined through a cache hit), then the solver propagates the cached value but at the same time links
	/// both target values with one another.
	/// 
	/// @author Johannes Lerch
	/// </summary>
	public class JoinHandlingNodesIFDSSolver<N, D, M, I> : IFDSSolver<N, D, M, I> where D : JoinHandlingNode<D> where I : heros.InterproceduralCFG<N, M>
	{

		public JoinHandlingNodesIFDSSolver(IFDSTabulationProblem<N, D, M, I> ifdsProblem) : base(ifdsProblem)
		{
		}

		protected internal readonly IDictionary<CacheEntry, JoinHandlingNode<D>> cache = new Dictionary();

		protected internal override void propagate(D sourceVal, N target, D targetVal, EdgeFunction<IFDSSolver.BinaryDomain> f, N relatedCallSite, bool isUnbalancedReturn)
		{
			CacheEntry currentCacheEntry = new CacheEntry(this, target, sourceVal.createJoinKey(), targetVal.createJoinKey());

			bool propagate = false;
			lock (this)
			{
				if (cache.ContainsKey(currentCacheEntry))
				{
					JoinHandlingNode<D> existingTargetVal = cache[currentCacheEntry];
					if (!existingTargetVal.handleJoin(targetVal))
					{
						propagate = true;
					}
				}
				else
				{
					cache[currentCacheEntry] = targetVal;
					propagate = true;
				}
			}

			if (propagate)
			{
				base.propagate(sourceVal, target, targetVal, f, relatedCallSite, isUnbalancedReturn);
			}

		};


		private class CacheEntry
		{
			private readonly JoinHandlingNodesIFDSSolver<N, D, M, I> outerInstance;

			internal N n;
			internal JoinHandlingNode_JoinKey sourceKey;
			internal JoinHandlingNode_JoinKey targetKey;

			public CacheEntry(JoinHandlingNodesIFDSSolver<N, D, M, I> outerInstance, N n, JoinHandlingNode_JoinKey sourceKey, JoinHandlingNode_JoinKey targetKey) : base()
			{
				this.outerInstance = outerInstance;
				this.n = n;
				this.sourceKey = sourceKey;
				this.targetKey = targetKey;
			}

			public override int GetHashCode()
			{
				const int prime = 31;
				int result = 1;
				result = prime * result + ((sourceKey == null) ? 0 : sourceKey.GetHashCode());
				result = prime * result + ((targetKey == null) ? 0 : targetKey.GetHashCode());
				result = prime * result + (Utils.IsDefault((n)) ? 0 : n.GetHashCode());
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
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings({ "unchecked" }) CacheEntry other = (CacheEntry) obj;
				CacheEntry other = (CacheEntry) obj;
				if (sourceKey == null)
				{
					if (other.sourceKey != null)
					{
						return false;
					}
				}
				else if (!sourceKey.Equals(other.sourceKey))
				{
					return false;
				}
				if (targetKey == null)
				{
					if (other.targetKey != null)
					{
						return false;
					}
				}
				else if (!targetKey.Equals(other.targetKey))
				{
					return false;
				}
				if (Utils.IsDefault(n))
				{
					if (!Utils.IsDefault(other.n))
					{
						return false;
					}
				}
				else if (!n.Equals(other.n))
				{
					return false;
				}
				return true;
			}
		}



	}

}