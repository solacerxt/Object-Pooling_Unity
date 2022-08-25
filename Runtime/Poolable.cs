using UnityEngine;

namespace solacerxt.Pooling
{
    public partial class Pool
    {
        [DisallowMultipleComponent]
        public abstract class Poolable : MonoBehaviour
        {
            private Pool _pool;

            protected void Awake() 
            {
                _pool = _typeToPool[GetType()];
                _pool._clear += DestroySelf;

                Init();
                gameObject.SetActive(false);

                print($"[Awaken] {name}");
            }
            
            private void DestroySelf()
            {
                _pool._clear -= DestroySelf;
                Destroy(gameObject);
            }

            protected void Start() {}

            /// <summary> 
            /// You should not use this directly, because this can be ineffecient. Use Pool .Next() instead 
            /// </summary>
            public bool Activate()
            {
                if (gameObject.activeSelf) return false;

                _pool._instances.Remove(this);
                gameObject.SetActive(true);
    #if UNITY_EDITOR
                if (_pool._groupInHierarchy)
                {
                    _pool.RemoveInHierarchy(this);
                    _pool.UpdatePoolStateName();
                }
    #endif
                OnActivate();

                return true;
            }

            public bool Deactivate()
            {
                if (!gameObject.activeSelf) return false;

                OnDeactivate();

                gameObject.SetActive(false);
                _pool._instances.Add(this);

    #if UNITY_EDITOR
                if (_pool._groupInHierarchy)
                {
                    _pool.AddInHierarchy(this);
                    _pool.UpdatePoolStateName();
                }
    #endif

                return true;
            }

            protected abstract void Init();
            protected abstract void OnActivate();
            protected abstract void OnDeactivate();
        }
    }
}
