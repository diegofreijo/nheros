using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHeros.src.util
{
    public class Multimap<K, V> : IEnumerable<KeyValuePair<K, ISet<V>>>
    {
        private readonly Dictionary<K, ISet<V>> data;

        public Multimap()
        {
            data = new Dictionary<K, ISet<V>>();
        }

        private Multimap(Dictionary<K, ISet<V>> data)
        {
            this.data = data;
        }

        public ISet<V> this[K key]
        {
            get { return data[key]; }
            set { data[key] = value; }
        }

        public void PutAll(K key, ICollection<V> values)
        {
            data.TryGetValue(key, out ISet<V> existing);
            existing.UnionWith(values);
            data[key] = existing;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return data.GetEnumerator();
        }

        public IEnumerator<KeyValuePair<K, ISet<V>>> GetEnumerator()
        {
            return data.GetEnumerator();
        }
    }
}
