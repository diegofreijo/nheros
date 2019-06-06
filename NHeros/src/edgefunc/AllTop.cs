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
namespace heros.edgefunc
{


	public class AllTop<V> : EdgeFunction<V>
	{

		private readonly V topElement;

		public AllTop(V topElement)
		{
			this.topElement = topElement;
		}

		public virtual V computeTarget(V source)
		{
			return topElement;
		}

		public virtual EdgeFunction<V> composeWith(EdgeFunction<V> secondFunction)
		{
			return this;
		}

		public virtual EdgeFunction<V> joinWith(EdgeFunction<V> otherFunction)
		{
			return otherFunction;
		}

		public virtual bool equalTo(EdgeFunction<V> other)
		{
			if (other is AllTop)
			{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("rawtypes") AllTop allTop = (AllTop) other;
				AllTop allTop = (AllTop) other;
				return allTop.topElement.Equals(topElement);
			}
			return false;
		}

		public override string ToString()
		{
			return "alltop";
		}

	}

}