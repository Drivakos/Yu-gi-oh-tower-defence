using UnityEngine;
using System.Collections.Generic;

namespace YuGiOhTowerDefense.Utils
{
    public class ObjectPool : MonoBehaviour
    {
        private GameObject prefab;
        private Transform poolContainer;
        private Queue<GameObject> pool = new Queue<GameObject>();
        private int initialSize = 10;
        private bool expandable = true;
        
        public void Initialize(GameObject prefab, int initialSize = 10, bool expandable = true)
        {
            this.prefab = prefab;
            this.initialSize = initialSize;
            this.expandable = expandable;
            
            // Create pool container
            poolContainer = new GameObject($"Pool_{prefab.name}").transform;
            poolContainer.SetParent(transform);
            
            // Pre-instantiate objects
            for (int i = 0; i < initialSize; i++)
            {
                CreateNewInstance();
            }
        }
        
        private GameObject CreateNewInstance()
        {
            GameObject obj = Instantiate(prefab, poolContainer);
            obj.SetActive(false);
            pool.Enqueue(obj);
            return obj;
        }
        
        public GameObject Get()
        {
            if (pool.Count == 0)
            {
                if (expandable)
                {
                    return CreateNewInstance();
                }
                else
                {
                    Debug.LogWarning($"Object pool for {prefab.name} is empty and not expandable!");
                    return null;
                }
            }
            
            GameObject obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        
        public void Return(GameObject obj)
        {
            if (obj == null) return;
            
            obj.SetActive(false);
            obj.transform.SetParent(poolContainer);
            pool.Enqueue(obj);
        }
        
        public void Clear()
        {
            while (pool.Count > 0)
            {
                GameObject obj = pool.Dequeue();
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
            
            if (poolContainer != null)
            {
                Destroy(poolContainer.gameObject);
            }
        }
    }
} 