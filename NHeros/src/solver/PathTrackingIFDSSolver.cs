﻿using NHeros.src.util;
using System;
using System.Collections.Generic;

/// <summary>
///*****************************************************************************
/// Copyright (c) 2013 Eric Bodden.
/// All rights reserved. This program and the accompanying materials
/// are made available under the terms of the GNU Lesser Public License v2.1
/// which accompanies this distribution, and is available at
/// http://www.gnu.org/licenses/old-licenses/gpl-2.0.html
/// 
/// Contributors:
///     Eric Bodden - initial API and implementation
/// *****************************************************************************
/// </summary>
namespace heros.solver
{
	/// <summary>
	/// An <seealso cref="IFDSSolver"/> that tracks paths for reporting. To do so, it requires that data-flow abstractions implement the LinkedNode interface.
	/// The solver implements a cache of data-flow facts for each statement and source value. If for the same statement and source value the same
	/// target value is seen again (as determined through a cache hit), then the solver propagates the cached value but at the same time links
	/// both target values with one another.
	/// 
	/// @author Eric Bodden </summary>
	/// @deprecated Use <seealso cref="JoinHandlingNodesIFDSSolver"/> instead. 
	[Obsolete("Use <seealso cref=\"JoinHandlingNodesIFDSSolver\"/> instead.")]
	public class PathTrackingIFDSSolver<N, D, M, I> : IFDSSolver<N, D, M, I>
        where D : LinkedNode<D> where I : heros.InterproceduralCFG<N, M>
	{

		public PathTrackingIFDSSolver(IFDSTabulationProblem<N, D, M, I> ifdsProblem) : base(ifdsProblem)
		{
		}

		readonly IDictionary<CacheEntry, LinkedNode<D>> cache = new Dictionary<CacheEntry, LinkedNode<D>>();

		protected internal override void propagate(D sourceVal, N target, D targetVal, EdgeFunction<IFDSSolver<N, D, M, I>.BinaryDomain> f, N relatedCallSite, bool isUnbalancedReturn)
		{
			CacheEntry currentCacheEntry = new CacheEntry(this, target, sourceVal, targetVal);

			bool propagate = false;
			lock (this)
			{
				if (cache.ContainsKey(currentCacheEntry))
				{
					LinkedNode<D> existingTargetVal = cache[currentCacheEntry];
                    throw new NotImplementedException("I don't know how to solve this comparison between LinkedNode and D");
					//if (existingTargetVal != targetVal)
					//{
					//	existingTargetVal.addNeighbor(targetVal);
					//}
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

		}


		private class CacheEntry
		{
			private readonly PathTrackingIFDSSolver<N, D, M, I> outerInstance;

			internal N n;
			internal D sourceVal;
			internal D targetVal;

			public CacheEntry(PathTrackingIFDSSolver<N, D, M, I> outerInstance, N n, D sourceVal, D targetVal) : base()
			{
				this.outerInstance = outerInstance;
				this.n = n;
				this.sourceVal = sourceVal;
				this.targetVal = targetVal;
			}

			public override int GetHashCode()
			{
				const int prime = 31;
				int result = 1;
				result = prime * result + (Utils.IsDefault((sourceVal)) ? 0 : sourceVal.GetHashCode());
				result = prime * result + (Utils.IsDefault((targetVal)) ? 0 : targetVal.GetHashCode());
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

//ORIGINAL LINE: @SuppressWarnings({ "unchecked" }) CacheEntry other = (CacheEntry) obj;
				CacheEntry other = (CacheEntry) obj;
				if (Utils.IsDefault(sourceVal))
				{
					if (!Utils.IsDefault(other.sourceVal))
					{
						return false;
					}
				}
				else if (!sourceVal.Equals(other.sourceVal))
				{
					return false;
				}
				if (Utils.IsDefault(targetVal))
				{
					if (!Utils.IsDefault(other.targetVal))
					{
						return false;
					}
				}
				else if (!targetVal.Equals(other.targetVal))
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