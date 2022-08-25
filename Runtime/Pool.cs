using System;
using System.Collections.Generic;
using UnityEngine;

namespace solacerxt.Pooling
{
    [CreateAssetMenu(menuName = "Pool", order = 51)]
    public partial class Pool : ScriptableObject
    {
        private static Dictionary<Type, Pool> _typeToPool = new Dictionary<Type, Pool>();
        private static bool _staticInit;
#if UNITY_EDITOR
        private static Transform _hMainPoolsObj;
#endif

        protected const int DefaultReserve = 4;

        protected static Pool GetPool<T>() where T : Poolable
        {
            StaticInit();
            
            if (_typeToPool.TryGetValue(typeof(T), out var pool))
                return pool;
            else throw new PoolNotDefinedException();
        }

        private static void StaticInit()
        {
            if (_staticInit || !Application.isPlaying) return;
            _staticInit = true;

            var pools = Resources.LoadAll<Pool>("Pools");
            int n = pools.Length;

            for (int i = 0; i < n; ++i)
            {
                var type = pools[i].Type;
                if (type is not null)
                {
                    pools[i]._instances = new List<Poolable>(0);
#if UNITY_EDITOR
                    if (pools[i]._groupInHierarchy)
                    {
                        var gameObj = new GameObject();

                        if (_hMainPoolsObj is null) 
                        {
                            _hMainPoolsObj = new GameObject("Pools").transform;
                            DontDestroyOnLoad(_hMainPoolsObj.gameObject);
                        }

                        gameObj.transform.SetParent(_hMainPoolsObj);
                        pools[i]._hParentPoolObj = gameObj.transform;

                        pools[i].UpdatePoolStateName();
                    }
#endif
                    _typeToPool[type] = pools[i];
                }
            }
        }

        [SerializeField] private Poolable _prefab;

#if UNITY_EDITOR
        [Tooltip("Puts all reserved objects in `Pools > Pool<Type>[Count/Capacity]` in Hierarchy when they are inactive")]
        [SerializeField] private bool _groupInHierarchy;

        private Transform _hParentPoolObj;
#endif

        private List<Poolable> _instances;
        private Type _type;

        private event Action _clear;
        
        public Type Type
        {
            get 
            {
                if (_type is null) _type = _prefab?.GetType();
                return _type;
            }
            set => _type = value;
        }

        public int Capacity => _instances.Capacity;
        public int Count => _instances.Count;

        private void OnEnable()
        {
            StaticInit();
        }

        public int Reserve(int count)
        {
            _instances.Capacity += count;

            for (int i = 0; i < count; ++i)
            {
                var obj = GameObject.Instantiate(_prefab, Vector3.zero, Quaternion.identity);
                DontDestroyOnLoad(obj.gameObject);
#if UNITY_EDITOR
                if (_groupInHierarchy)
                    obj.transform.SetParent(_hParentPoolObj);
#endif
                _instances.Add(obj);
            }

#if UNITY_EDITOR
            if (_groupInHierarchy) 
                UpdatePoolStateName();
#endif

            return _instances.Capacity;
        }

        public void Clear()
        {
            _instances.Clear();
            _instances.Capacity = 0;
            _clear?.Invoke();
        }

        public Poolable Next(int reserveIfEmpty = DefaultReserve)
        {
            if (_instances.Count == 0)
            {
                if (reserveIfEmpty == 0) 
                    throw new EmptyPoolException();

                Reserve(reserveIfEmpty);
            }

            var last = _instances[^1];
            last.Activate();
            
            return last;
        }

        public Poolable Next(Vector3 position, int reserveIfEmpty = DefaultReserve)
        {
            var obj = Next(reserveIfEmpty);
            obj.transform.position = position;

            return obj;
        }

        public Poolable Next(Vector3 position, Quaternion rotation, int reserveIfEmpty = DefaultReserve)
        {
            var obj = Next(reserveIfEmpty);
            obj.transform.position = position;
            obj.transform.rotation = rotation;

            return obj;
        }

#if UNITY_EDITOR
        private void UpdatePoolStateName() => 
            _hParentPoolObj.name = $"Pool<{Type}> [{Count}/{Capacity}]";

        private void RemoveInHierarchy(Poolable poolable) =>
            poolable.transform.SetParent(null);

        private void AddInHierarchy(Poolable poolable) =>
            poolable.transform.SetParent(_hParentPoolObj);
#endif
    }
}
