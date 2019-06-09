using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHeros.src.util
{
    public class Multiset<T> 
    {
        private readonly Dictionary<T, int> data;

        public Multiset()
        {
            data = new Dictionary<T, int>();
        }

        private Multiset(Dictionary<T, int> data)
        {
            this.data = data;
        }

        public void Add(T item)
        {
            this.Add(item, 1);
        }

        public void Add(T item, int times)
        {
            data.TryGetValue(item, out int count);
            count += times;
            data[item] = count;
        }

        public void Clear()
        {
            data.Clear();
        }

        public bool Remove(T item)
        {
            int count;
            if (!data.TryGetValue(item, out count))
            {
                return false;
            }
            count--;
            if (count == 0)
            {
                data.Remove(item);
            }
            else
            {
                data[item] = count;
            }
            return true;
        }

        public bool Contains(T item)
        {
            data.TryGetValue(item, out int count);
            return count > 0;
        }

        public int Count
        {
            get
            {
                return data.Values.Sum();
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

    }
}
