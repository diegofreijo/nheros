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


//ORIGINAL LINE: @SuppressWarnings("rawtypes") private final static Identity instance = new Identity();
		private static readonly Identity instance = new Identity();

		private Identity()
		{
		} //use v() instead

		public virtual ISet<D> computeTargets(D source)
		{
			return singleton(source);
		}


//ORIGINAL LINE: @SuppressWarnings("unchecked") public static <D> Identity<D> v()
		public static Identity<D> v<D>()
		{
			return instance;
		}

	}

}