﻿using System.Collections.Generic;

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
    /// The empty function, i.e. a function which returns an empty set for all points
    /// in the definition space.
    /// </summary>
    /// @param <D> The type of data-flow facts to be computed by the tabulation problem. </param>
    public class KillAll<D> : FlowFunction<D>
	{
        private static readonly KillAll<D> instance = new KillAll<D>();

		private KillAll()
		{
		} //use v() instead

		public virtual ISet<D> computeTargets(D source)
		{
            return new HashSet<D>();
		}

		public static KillAll<D> v()
		{
			return instance;
        }
	}
}