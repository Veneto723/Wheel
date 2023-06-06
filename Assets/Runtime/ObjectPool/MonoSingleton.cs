using UnityEngine;

namespace Runtime.ObjectPool {
    public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T> {
        public static T Instance { get; private set; }
        
        
        protected virtual void Awake() {
            Instance = (T) this;
        }
    }
}