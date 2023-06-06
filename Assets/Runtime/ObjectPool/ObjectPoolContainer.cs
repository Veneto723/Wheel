using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.ObjectPool {
    public class ObjectPoolContainer<T> {
        private readonly List<ObjectPool<T>> _list;
        private readonly Dictionary<T, ObjectPool<T>> _lookup;
        private readonly Func<T> _factoryFunc;
        private int _lastIndex;

        public int Count => _list.Count;
        public int CountUsedItems => _lookup.Count;

        public ObjectPoolContainer(Func<T> factoryFunc, int initialSize) {
            this._factoryFunc = factoryFunc;
            _list = new List<ObjectPool<T>>(initialSize);
            _lookup = new Dictionary<T, ObjectPool<T>>(initialSize);

            Warm(initialSize);
        }

        private void Warm(int capacity) {
            for (var i = 0; i < capacity; i++)
                CreateContainer();
        }

        private ObjectPool<T> CreateContainer() {
            var container = new ObjectPool<T> {Item = _factoryFunc()};
            _list.Add(container);
            return container;
        }

        public T GetItem() {
            ObjectPool<T> container = null;
            foreach (var _ in _list) {
                _lastIndex++;
                if (_lastIndex > _list.Count - 1) _lastIndex = 0;

                if (_list[_lastIndex].Used) continue;

                container = _list[_lastIndex];
                break;
            }

            container ??= CreateContainer();

            container.Consume();
            _lookup.Add(container.Item, container);
            return container.Item;
        }

        public void ReleaseItem(T item) {
            if (_lookup.ContainsKey(item)) {
                var container = _lookup[item];
                container.Release();
                _lookup.Remove(item);
            }
            else {
                Debug.Log("This object pool does not contain the item provided: " + item);
            }
        }
    }
}