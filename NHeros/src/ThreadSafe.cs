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
	/// This annotation tells that the class was designed to be used by multiple threads, with concurrent updates. 
	/// </summary>
	public class ThreadSafe : System.Attribute
	{

		internal string value;


		public ThreadSafe(String value = "")
		{
			this.value = value;
		}
	}

}