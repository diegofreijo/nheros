using System.Collections.Generic;

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
