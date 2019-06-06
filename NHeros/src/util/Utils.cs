using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHeros.src.util
{
    public static class Utils
    {
        public static bool IsDefault<T>(T val)
        {
            return EqualityComparer<T>.Default.Equals(val, default(T));
        }
    }
}
