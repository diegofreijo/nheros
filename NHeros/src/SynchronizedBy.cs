﻿/// <summary>
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
	///	Semantic annotation that the annotated field is synchronized.
	///  This annotation is meant as a structured comment only, and has no immediate effect. 
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: There is no attribute target in .NET corresponding to FIELD:
//ORIGINAL LINE: @Target(FIELD) public class SynchronizedBy extends System.Attribute
	[AttributeUsage(<missing>, AllowMultiple = false, Inherited = false)]
	public class SynchronizedBy : System.Attribute
	{
		internal string value;

		public SynchronizedBy(String value = "")
		{
			this.value = value;
		}
	}

}