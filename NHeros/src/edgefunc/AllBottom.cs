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


	public class AllBottom<V> : EdgeFunction<V>
	{

		private readonly V bottomElement;

		public AllBottom(V bottomElement)
		{
			this.bottomElement = bottomElement;
		}

		public virtual V computeTarget(V source)
		{
			return bottomElement;
		}

		public virtual EdgeFunction<V> composeWith(EdgeFunction<V> secondFunction)
		{
			if (secondFunction is EdgeIdentity)
			{
				return this;
			}
			return secondFunction;
		}

		public virtual EdgeFunction<V> joinWith(EdgeFunction<V> otherFunction)
		{
			if (otherFunction == this || otherFunction.equalTo(this))
			{
				return this;
			}
			if (otherFunction is AllTop)
			{
				return this;
			}
			if (otherFunction is EdgeIdentity)
			{
				return this;
			}
			throw new System.InvalidOperationException("unexpected edge function: " + otherFunction);
		}

		public virtual bool equalTo(EdgeFunction<V> other)
		{
			if (other is AllBottom)
			{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("rawtypes") AllBottom allBottom = (AllBottom) other;
				AllBottom allBottom = (AllBottom) other;
				return allBottom.bottomElement.Equals(bottomElement);
			}
			return false;
		}

		public override string ToString()
		{
			return "allbottom";
		}

	}

}