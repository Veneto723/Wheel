using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.ObjectPool {
    public class PoolManager : MonoSingleton<PoolManager> {
        private Dictionary<GameObject, ObjectPoolContainer<GameObject>> _prefabLookup;
        private Dictionary<GameObject, ObjectPoolContainer<GameObject>> _instanceLookup;
        private readonly Queue<GameObject> _queue = new Queue<GameObject>();

        protected override void Awake() {
            base.Awake();
            _prefabLookup = new Dictionary<GameObject, ObjectPoolContainer<GameObject>>();
            _instanceLookup = new Dictionary<GameObject, ObjectPoolContainer<GameObject>>();
        }

        private void Update() {
            while (_queue.Count > 0) {
                ReleaseObject(_queue.Dequeue());
            }
        }

        private void Enqueue(GameObject action) {
            _queue.Enqueue(action);
        }

        private void Warm(GameObject prefab, int size, Transform parent = null) {
            if (_prefabLookup.ContainsKey(prefab)) {
                throw new Exception("Pool for prefab " + prefab.name + " has already been created");
            }

            _prefabLookup[prefab] = new ObjectPoolContainer<GameObject>(() => InstantiatePrefab(prefab, parent), size);
        }

        private GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation) {
            if (!_prefabLookup.ContainsKey(prefab)) {
                WarmPool(prefab, 1);
            }

            var pool = _prefabLookup[prefab];

            var clone = pool.GetItem();
            clone.transform.position = position;
            clone.transform.rotation = rotation;
            clone.SetActive(true);

            _instanceLookup.Add(clone, pool);
            return clone;
        }

        private void Release(GameObject clone) {
            clone.SetActive(false);
            if (_instanceLookup.ContainsKey(clone)) {
                _instanceLookup[clone].ReleaseItem(clone);
                _instanceLookup.Remove(clone);
                Destroy(clone);
            } else {
                Debug.Log($"No pool contains the object: {clone.name}");
            }
        }


        private static GameObject InstantiatePrefab(GameObject prefab, Transform parent = null) {
            return Instantiate(prefab, parent);
        }


        #region Static API

        /// <summary>
        /// Pre-construct a pool
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="size"></param>
        /// <param name="parent"></param>
        private static void WarmPool(GameObject prefab, int size, Transform parent = null) {
            Instance.Warm(prefab, size, parent);
        }

        /// <summary>
        /// Instantiate a prefab from the pool
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public static GameObject SpawnObject(GameObject prefab, Vector3 position = default,
            Quaternion rotation = default) {
            return Instance.Spawn(prefab, position, rotation);
        }

        /// <summary>
        /// Instantiate a prefab from the pool and call the init func of this prefab
        /// 
        /// [Main Thread blocked is possible]
        /// 如果调用的Action运行时间过长，会造成主线程阻塞。如果要改成异步调用的话，将无法修改在action中修改GameObject的属性(e.g. active, transform)，
        /// 因为这些操作无法在非主线程中执行。并且这些操作是在调用方那里写的，暂时没有什么特别好的方法来处理。
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="action">call the init func</param>
        /// <param name="position">default = Vector3.zero</param>
        /// <param name="rotation">default = Quaternion.identity</param>
        /// <typeparam name="T">component of this prefab with init func in it</typeparam>
        /// <returns></returns>
        public static GameObject SpawnObject<T>(GameObject prefab, Action<T> action, Vector3 position = default,
            Quaternion rotation = default) {
            var go = SpawnObject(prefab, position, rotation);
            action.Invoke(go.GetComponent<T>());
            return go;
        }


        /// <summary>
        /// Release a instantiated clone from the pool
        /// </summary>
        /// <param name="clone"></param>
        public static void ReleaseObject(GameObject clone) {
            Instance.Release(clone);
        }

        /// <summary>
        /// Release a instantiated clone from the pool AFTER calling the release func of this GameObject
        /// </summary>
        /// <param name="clone"></param>
        /// <param name="action"></param>
        /// <typeparam name="T"></typeparam>
        public static void ReleaseObject<T>(GameObject clone, Action<T> action) {
            action.BeginInvoke(clone.GetComponent<T>(), ar => {
                action.EndInvoke(ar);
                Instance.Release(clone);
                print("release object");
            }, null);
        }

        #endregion
    }
}