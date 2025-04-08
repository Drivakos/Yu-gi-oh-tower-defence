using UnityEngine;
using System.Collections.Generic;

namespace YuGiOhTowerDefense.Core
{
    public class ObjectPool
    {
        private GameObject prefab;
        private Transform poolContainer;
        private List<GameObject> activeObjects = new List<GameObject>();
        private Queue<GameObject> inactiveObjects = new Queue<GameObject>();
        private int poolSize;
        
        public ObjectPool(GameObject prefab, int poolSize)
        {
            this.prefab = prefab;
            this.poolSize = poolSize;
            
            // Create a container for pooled objects
            poolContainer = new GameObject($"Pool_{prefab.name}").transform;
            poolContainer.SetParent(GameObject.Find("ObjectPools")?.transform);
            
            // Pre-instantiate objects
            for (int i = 0; i < poolSize; i++)
            {
                CreateNewObject();
            }
        }
        
        private void CreateNewObject()
        {
            GameObject obj = GameObject.Instantiate(prefab, poolContainer);
            obj.SetActive(false);
            inactiveObjects.Enqueue(obj);
        }
        
        public GameObject GetObject()
        {
            GameObject obj;
            
            if (inactiveObjects.Count > 0)
            {
                obj = inactiveObjects.Dequeue();
            }
            else
            {
                // If we run out of objects, create a new one
                CreateNewObject();
                obj = inactiveObjects.Dequeue();
            }
            
            obj.SetActive(true);
            activeObjects.Add(obj);
            
            return obj;
        }
        
        public void ReturnObject(GameObject obj)
        {
            if (obj == null)
            {
                return;
            }
            
            obj.SetActive(false);
            obj.transform.SetParent(poolContainer);
            
            if (activeObjects.Contains(obj))
            {
                activeObjects.Remove(obj);
            }
            
            if (!inactiveObjects.Contains(obj))
            {
                inactiveObjects.Enqueue(obj);
            }
        }
        
        public void ReturnAllObjects()
        {
            foreach (GameObject obj in activeObjects.ToArray())
            {
                ReturnObject(obj);
            }
        }
        
        public int GetActiveObjectCount()
        {
            return activeObjects.Count;
        }
        
        public int GetInactiveObjectCount()
        {
            return inactiveObjects.Count;
        }
    }
} 