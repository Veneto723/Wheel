using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Runtime.ObjectPoolManager {
    public class ObjectPoolManager : MonoBehaviour {
        private static readonly List<PooledObjectInfo> ObjectPools = new();
        private GameObject _objectPoolEmptyHolder;
        private static GameObject _particleSystemsEmpty;
        private static GameObject _gameObjectEmpty;

        public enum PoolType {
            ParticleSystem,
            GameObject,
            None
        }

        private void Awake() {
            SetupEmpties();
        }

        private void SetupEmpties() {
            _objectPoolEmptyHolder = new GameObject("Pooled Objects");
            
            _particleSystemsEmpty = new GameObject("Particle Effects");
            _particleSystemsEmpty.transform.SetParent(_objectPoolEmptyHolder.transform);
            _gameObjectEmpty = new GameObject("GameObjects");
            _gameObjectEmpty.transform.SetParent(_objectPoolEmptyHolder.transform);
        }

        public static GameObject SpawnObject(GameObject objectToSpawn, Vector3 spawnPosition,
            Quaternion spawnRotation, PoolType poolType = PoolType.None) {
            var pool = ObjectPools.Find(p => p.LookupString == objectToSpawn.name);
            if (pool == null) { // if not found, create a new pool
                pool = new PooledObjectInfo() { LookupString = objectToSpawn.name };
                ObjectPools.Add(pool);
            }

            // check if there are any inactive objects in the pool
            var spawnableObject = pool.InactiveObjects.FirstOrDefault(obj => obj != null);
            if (spawnableObject == null) {
                var parentObject = SetParentObject(poolType);
                spawnableObject = Instantiate(objectToSpawn, spawnPosition, spawnRotation);
                if (parentObject != null) spawnableObject.transform.SetParent(parentObject.transform);
            } else { // if we found one, assign the position and rotation to it and reactive it.
                spawnableObject.transform.position = spawnPosition;
                spawnableObject.transform.rotation = spawnRotation;
                pool.InactiveObjects.Remove(spawnableObject);
                spawnableObject.SetActive(true);
            }
            return spawnableObject;
        }
        
        public static GameObject SpawnObject(GameObject objectToSpawn, Transform parentTransform) {
            var pool = ObjectPools.Find(p => p.LookupString == objectToSpawn.name);
            if (pool == null) { // if not found, create a new pool
                pool = new PooledObjectInfo() { LookupString = objectToSpawn.name };
                ObjectPools.Add(pool);
            }

            // check if there are any inactive objects in the pool
            var spawnableObject = pool.InactiveObjects.FirstOrDefault(obj => obj != null);
            if (spawnableObject == null) {
                spawnableObject = Instantiate(objectToSpawn, parentTransform);
            } else { // if we found one, reactive it.
                pool.InactiveObjects.Remove(spawnableObject);
                spawnableObject.SetActive(true);
            }
            return spawnableObject;
        }
        
        public static void ReturnObjectToPool(GameObject obj) {
            var goName = obj.name[..^7]; 
            // by taking off the last 7 characters from its name, we are removing the (Clone) from the name of the passed in obj
            var pool = ObjectPools.Find(p => p.LookupString == goName);
            if (pool == null) {
                Debug.LogWarning($"Trying to release an object {goName} that has not been previously pooled");
            } else {
                obj.SetActive(false);
                pool.InactiveObjects.Add(obj);
            }
        }

        private static GameObject SetParentObject(PoolType poolType) {
            return poolType switch {
                PoolType.ParticleSystem => _particleSystemsEmpty,
                PoolType.GameObject => _gameObjectEmpty,
                PoolType.None => null,
                _ => null
            };
        }
    }

    public class PooledObjectInfo {
        public string LookupString;
        public readonly List<GameObject> InactiveObjects = new ();
    }
}