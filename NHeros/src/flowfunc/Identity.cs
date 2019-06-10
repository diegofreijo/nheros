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
namespace heros.flowfunc
{
	public class Identity<D> : FlowFunction<D>
	{
        private static readonly Identity<D> instance = new Identity<D>();

		private Identity()
		{
		} //use v() instead

		public virtual ISet<D> computeTargets(D source)
		{
			return new HashSet<D>(source);
		}

		public static Identity<D> v()
		{
			return instance;
		}
	}
}