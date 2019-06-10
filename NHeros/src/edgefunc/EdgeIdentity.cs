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

	/// <summary>
	/// The identity function on graph edges </summary>
	/// @param <V> The type of values to be computed along flow edges. </param>
	public class EdgeIdentity<V> : EdgeFunction<V>
	{
        private static readonly EdgeIdentity<V> instance = new EdgeIdentity<V>();

        //use v() instead
        private EdgeIdentity()
		{
		} 
        public static EdgeIdentity<V> v()
        {
            return instance;
        }

        public virtual V computeTarget(V source)
		{
			return source;
		}

		public virtual EdgeFunction<V> composeWith(EdgeFunction<V> secondFunction)
		{
			return secondFunction;
		}

		public virtual EdgeFunction<V> joinWith(EdgeFunction<V> otherFunction)
		{
			if (otherFunction == this || otherFunction.equalTo(this))
			{
				return this;
			}
			if (otherFunction is AllBottom)
			{
				return otherFunction;
			}
			if (otherFunction is AllTop)
			{
				return this;
			}
			//do not know how to join; hence ask other function to decide on this
			return otherFunction.joinWith(this);
		}

		public virtual bool equalTo(EdgeFunction<V> other)
		{
			//singleton
			return other == this;
		}

		public override string ToString()
		{
			return "id";
		}


	}

}