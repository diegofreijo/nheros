using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHeros.src.util
{
    public delegate V CacheLoader<K, V>(K key);

    public class LoadingCache<K,V>
    {
        private readonly Dictionary<K, V> dict = new Dictionary<K, V>();
        private readonly CacheLoader<K, V> loader;

        public LoadingCache(CacheLoader<K,V> loader)
        {
            this.loader = loader;
        }

        public V Get(K key)
        {
            if(!Exists(key))
            {
                var ret = loader(key);
                dict.Add(key, ret);
                return ret;
            }

            return dict[key];
        }

        public bool Exists(K key)
        {
            return dict.ContainsKey(key);
        }
    }
}
