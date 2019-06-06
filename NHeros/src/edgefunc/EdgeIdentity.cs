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

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("rawtypes") private final static EdgeIdentity instance = new EdgeIdentity();
		private static readonly EdgeIdentity instance = new EdgeIdentity();

		private EdgeIdentity()
		{
		} //use v() instead

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

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static <A> EdgeIdentity<A> v()
		public static EdgeIdentity<A> v<A>()
		{
			return instance;
		}

		public override string ToString()
		{
			return "id";
		}


	}

}