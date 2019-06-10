using heros.solver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHeros.src.util
{
    public class Table<K1, K2, V> : Dictionary<Pair<K1, K2>, V>
    {
        internal V get(K1 key1, K2 key2)
        {
            var pair = new Pair<K1, K2>(key1, key2);
            return this[pair];
        }

        internal void put(K1 key1, K2 key2, V val)
        {
            var pair = new Pair<K1, K2>(key1, key2);
            this[pair] = val;
        }

        internal IEnumerable<Cell> cellSet()
        {
            return this.Select(kvp => new Cell(kvp.Key.O1, kvp.Key.O2, kvp.Value));
        }

        public class Cell
        {
            public Cell(K1 key1, K2 key2, V val)
            {
                RowKey = key1;
                ColumnKey = key2;
                Value = val;
            }

            public K1 RowKey { get; }
            public K2 ColumnKey { get; }
            public V Value { get; }
        }

        internal void remove<N, D>(N nHashN, D nHashD)
        {
            throw new NotImplementedException();
        }

        internal IDictionary<K2, V> row(K1 key1)
        {
            return this
                .Where(kvp => kvp.Key.O1.Equals(key1))
                .ToDictionary(kvp => kvp.Key.O2, kvp => kvp.Value);
        }
    }
}
