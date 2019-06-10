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
	/// <summary>
	/// Function that creates a new value (e.g. returns a set containing a fixed value when given
	/// a specific parameter), but acts like the identity function for all other parameters.
	/// </summary>
	/// @param <D> The type of data-flow facts to be computed by the tabulation problem. </param>
	public class Gen<D> : FlowFunction<D>
	{

		private readonly D genValue;
		private readonly D zeroValue;

		public Gen(D genValue, D zeroValue)
		{
			this.genValue = genValue;
			this.zeroValue = zeroValue;
		}

		public virtual ISet<D> computeTargets(D source)
		{
			if (source.Equals(zeroValue))
			{
				return new HashSet<D>() { source, genValue };
			}
			else
			{
				return new HashSet<D>() { source };
			}
		}

	}

}