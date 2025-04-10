using UnityEngine;

namespace YuGiOh.Managers
{
    public abstract class BaseManager : MonoBehaviour
    {
        protected virtual void Awake()
        {
            // Ensure only one instance exists
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            OnAwake();
        }
        
        protected virtual void OnAwake()
        {
            // Override this method in derived classes for additional initialization
        }
        
        protected virtual void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
        
        public static BaseManager Instance { get; protected set; }
    }
} 