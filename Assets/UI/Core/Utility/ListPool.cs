
using System.Collections.Generic;

namespace NEW_UI
{
    internal static class ListPool<T>
    {
        private static readonly ObjectPool<List<T>> s_ListPool = new ObjectPool<List<T>>(null, Clear);

        static void Clear(List<T> l) 
        {
            l.Clear();
        }

        public static List<T> Get() 
        {
            return s_ListPool.Get();
        }

        public static void Release(List<T> l) 
        {
            s_ListPool.Release(l);
        }
    }
}  