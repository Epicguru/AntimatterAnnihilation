using System;
using System.Collections.Generic;

namespace AntimatterAnnihilation.Utils
{
    public class ObjectPool<T> : IDisposable where T : IResettable
    {
        public Func<T> CreateFunction { get; set; }

        private bool disposed;
        private Queue<T> pool = new Queue<T>();

        public T GetOrCreate()
        {
            if (pool.Count == 0)
            {
                if (CreateFunction != null)
                {
                    var created = CreateFunction.Invoke();
                    created?.Reset();
                    return created;
                }
                return default;
            }
            else
            {
                var fromPool = pool.Dequeue();
                fromPool.Reset();
                return fromPool;
            }
        }

        public void Return(T obj)
        {
            if (obj == null)
                return;

            if (!pool.Contains(obj))
            {
                obj.Reset();
                pool.Enqueue(obj);
            }
        }

        public void Dispose()
        {
            if (!disposed)
            {
                pool.Clear();
                pool = null;
            }
            disposed = true;
        }
    }
}
