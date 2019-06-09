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

//	import static heros.TwoElementSet.twoElementSet;




	public class Transfer<D> : FlowFunction<D>
	{

		private readonly D toValue;
		private readonly D fromValue;

		public Transfer(D toValue, D fromValue)
		{
			this.toValue = toValue;
			this.fromValue = fromValue;
		}

		public virtual ISet<D> computeTargets(D source)
		{
			if (source.Equals(fromValue))
			{
				return twoElementSet(source, toValue);
			}
			else if (source.Equals(toValue))
			{
				return Collections.emptySet();
			}
			else
			{
				return Collections.singleton(source);
			}
		}

	}

}