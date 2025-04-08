using UnityEngine;

namespace YuGiOh.Managers
{
    public abstract class BaseManager : MonoBehaviour
    {
        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        public static BaseManager Instance { get; protected set; }
    }
} 