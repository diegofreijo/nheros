using System.Collections.Generic;

/// <summary>
///*****************************************************************************
/// Copyright (c) 2012 Eric Bodden.
/// All rights reserved. This program and the accompanying materials
/// are made available under the terms of the GNU Lesser Public License v2.1
/// which accompanies this distribution, and is available at
/// http://www.gnu.org/licenses/old-licenses/gpl-2.0.html
/// 
/// Contributors:
///     Eric Bodden - initial API and implementation
/// *****************************************************************************
/// </summary>
namespace heros
{

	/// <summary>
	/// A utility class for creating default seeds that cause an analysis to simply start at a given statement.
	/// This is useful if seeding is performed entirely through flow functions as used to be the case in 
	/// earlier versions of Heros.
	/// </summary>
	public class DefaultSeeds
	{

		public static IDictionary<N, ISet<D>> make<N, D>(IEnumerable<N> units, D zeroNode)
		{
			IDictionary<N, ISet<D>> res = new Dictionary<N, ISet<D>>();
			foreach (N n in units)
			{
				res[n] = Collections.singleton(zeroNode);
			}
			return res;
		}

	}

}