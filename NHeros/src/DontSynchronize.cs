using System;
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
	///	Semantic annotation stating that the annotated field can remain unsynchronized.
	///  This annotation is meant as a structured comment only, and has no immediate effect. 
	/// </summary>
    //[AttributeUsage(<missing>, AllowMultiple = false, Inherited = false)]
	public class DontSynchronize : Attribute
	{
		internal string value;

		public DontSynchronize(string value = "")
		{
			this.value = value;
		}
	}
}