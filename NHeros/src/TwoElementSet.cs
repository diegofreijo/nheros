using System.Collections.Generic;

/// <summary>
///*****************************************************************************
/// Copyright (c) 2013 Eric Bodden.
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
	/// An unmodifiable set holding exactly two elements. Particularly useful within flow functions.
	/// </summary>
	/// @param <E> </param>
	/// <seealso cref= FlowFunction </seealso>
	public class TwoElementSet<E> : AbstractSet<E>
	{

		protected internal readonly E first, second;

		public TwoElementSet(E first, E second)
		{
			this.first = first;
			this.second = second;
		}

		public static TwoElementSet<E> twoElementSet<E>(E first, E second)
		{
			return new TwoElementSet<E>(first, second);
		}

		public override IEnumerator<E> iterator()
		{
			return new IteratorAnonymousInnerClass(this);
		}

		private class IteratorAnonymousInnerClass : IEnumerator<E>
		{
			private readonly TwoElementSet<E> outerInstance;

			public IteratorAnonymousInnerClass(TwoElementSet<E> outerInstance)
			{
				this.outerInstance = outerInstance;
				elementsRead = 0;
			}

			internal int elementsRead;

			public bool hasNext()
			{
				return elementsRead < 2;
			}

			public E next()
			{
				switch (elementsRead)
				{
				case 0:
					elementsRead++;
					return outerInstance.first;
				case 1:
					elementsRead++;
					return outerInstance.second;
				default:
					throw new NoSuchElementException();
				}
			}

			public void remove()
			{
				throw new System.NotSupportedException();
			}
		}

		public override int size()
		{
			return 2;
		}
	}

}