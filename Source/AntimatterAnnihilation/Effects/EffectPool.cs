using System;
using System.Collections.Generic;

namespace AntimatterAnnihilation.Effects
{
    static class EffectPool<T> where T : class
    {
        private static List<T> pool;

        public static T Get(Func<T> create)
        {
            var fromPool = GetFromPool();
            return fromPool ?? create();
        }

        public static void Return(T t)
        {
            if (t == null || (pool?.Contains(t) ?? false))
                return;

            if (pool == null)
                pool = new List<T>();

            pool.Add(t);
        }

        private static T GetFromPool()
        {
            if (pool == null || pool.Count == 0)
                return null;

            var obj = pool[0];
            pool.RemoveAt(0);

            return obj;
        }
    }
}
