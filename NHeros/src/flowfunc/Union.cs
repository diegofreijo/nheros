using System.Collections.Generic;
using System.Linq;

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
namespace heros.flowfunc
{
	/// <summary>
	/// Represents the union of a set of flow functions.
	/// </summary>
	public class Union<D> : FlowFunction<D>
	{
		private readonly FlowFunction<D>[] funcs;

		private Union(params FlowFunction<D>[] funcs)
		{
			this.funcs = funcs;
		}

		public virtual ISet<D> computeTargets(D source)
		{
			ISet<D> res = new HashSet<D>();
			foreach (FlowFunction<D> func in funcs)
			{
				res.UnionWith(func.computeTargets(source));
			}
			return res;
		}

        public static FlowFunction<D> union<D>(params FlowFunction<D>[] funcs)
		{
			IList<FlowFunction<D>> list = new List<FlowFunction<D>>();
			foreach (FlowFunction<D> f in funcs)
			{
				if (f != Identity<D>.v())
				{
					list.Add(f);
				}
			}
			if (list.Count == 1)
			{
				return list[0];
			}
			else if (list.Count == 0)
			{
				return Identity<D>.v();
			}
			return new Union<D>(list.ToArray());
		}

	}

}