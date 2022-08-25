using UnityEngine;

namespace solacerxt.Pooling
{
    public sealed class Pool<T> : Pool where T : Pool.Poolable
    {
        private static Pool _pool = GetPool<T>();
        
        private Pool() : base() {}

        public static new int Count => _pool.Count;
        public static new int Capacity => _pool.Capacity;

        // Increases Capacity on count
        public static new int Reserve(int count) => _pool.Reserve(count);

        // Removes and destroys all objects, so you can be sure that Capacity == Count == 0
        public static new void Clear() => _pool.Clear();

        public static new T Next(int reserveIfEmpty = DefaultReserve) => 
            (T) _pool.Next(reserveIfEmpty);

        public static new T Next(Vector3 position, int reserveIfEmpty = DefaultReserve) =>
            (T) _pool.Next(position, reserveIfEmpty);

        public static new T Next(Vector3 position, Quaternion rotation, int reserveIfEmpty = DefaultReserve) =>
            (T) _pool.Next(position, rotation, reserveIfEmpty);
    }
}
