using System.Collections.Generic;

/// <summary>
///*****************************************************************************
/// Copyright (c) 2015 Johannes Lerch.
/// All rights reserved. This program and the accompanying materials
/// are made available under the terms of the GNU Lesser Public License v2.1
/// which accompanies this distribution, and is available at
/// http://www.gnu.org/licenses/old-licenses/gpl-2.0.html
/// 
/// Contributors:
///     Johannes Lerch - initial API and implementation
/// *****************************************************************************
/// </summary>
namespace heros.utilities
{

	public abstract class DefaultValueMap<K, V> : IDictionary<K, V>
	{

		private Dictionary<K, V> map;

		public DefaultValueMap()
		{
			map = new Dictionary<K, V>();
		}

		public virtual int Count
		{
			get
			{
				return map.Count;
			}
		}

		public override bool Empty
		{
			get
			{
				return map.Count == 0;
			}
		}

		public virtual bool ContainsKey(object key)
		{
			return map.ContainsKey(key);
		}

		public override bool containsValue(object value)
		{
			return map.ContainsValue(value);
		}

		protected internal abstract V createItem(K key);

		public virtual V getOrCreate(K key)
		{
			if (!map.ContainsKey(key))
			{
				V value = createItem((K) key);
				map[(K) key] = value;
				return value;
			}

			return map[key];
		}

		public override V get(object key)
		{
			return map[key];
		}

		public override V put(K key, V value)
		{
			return map[key] = value;
		}

		public override V remove(object key)
		{
			return map.Remove(key);
		}

		public override void putAll<T1>(IDictionary<T1> m) where T1 : K
		{
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
			map.putAll(m);
		}

		public virtual void Clear()
		{
			map.Clear();
		}

		public override ISet<K> keySet()
		{
			return map.Keys;
		}

		public virtual ICollection<V> Values
		{
			get
			{
				return map.Values;
			}
		}

		public override ISet<KeyValuePair<K, V>> entrySet()
		{
			return map.SetOfKeyValuePairs();
		}

	}

}