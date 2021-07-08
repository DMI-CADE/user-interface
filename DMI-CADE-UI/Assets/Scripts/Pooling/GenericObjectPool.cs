using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pooling
{
    public abstract class GenericObjectPool<T> : MonoBehaviour where T : Component
    {
        [SerializeField] private T prefab;

        private Queue<T> _pool = new Queue<T>();

        // Simple Singleton
        public static GenericObjectPool<T> Instance { get; private set; }
        
        private void Awake()
        {
            Instance = this;
        }

        public T Get()
        {
            if (_pool.Count == 0)
            {
                AddObject(1);
            }
            return _pool.Dequeue();
        }

        public void ReturnToPool(T objectToReturn)
        {
            objectToReturn.gameObject.SetActive(false);
            _pool.Enqueue(objectToReturn);
        }

        public void AddObject(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var newObject = GameObject.Instantiate(prefab, transform);
                newObject.gameObject.SetActive(false);
                _pool.Enqueue(newObject);
            }
        }
    }
}
