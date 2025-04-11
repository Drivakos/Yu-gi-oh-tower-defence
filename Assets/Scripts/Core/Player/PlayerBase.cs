using UnityEngine;
using YuGiOhTowerDefense.Core.UI;

namespace YuGiOhTowerDefense.Core.Player
{
    /// <summary>
    /// Controls the player's base and life points
    /// </summary>
    public class PlayerBase : MonoBehaviour
    {
        [Header("Base Stats")]
        [SerializeField] private int maxLifePoints = 8000;
        [SerializeField] private GameObject damageEffect;
        [SerializeField] private GameObject defeatEffect;
        
        private int currentLifePoints;
        private bool isDefeated;
        
        public static event System.Action<int> OnLifePointsChanged;
        public static event System.Action OnPlayerDefeated;
        
        public int CurrentLifePoints => currentLifePoints;
        public int MaxLifePoints => maxLifePoints;
        public bool IsDefeated => isDefeated;
        
        private void Awake()
        {
            currentLifePoints = maxLifePoints;
            isDefeated = false;
        }
        
        public void TakeDamage(int damage)
        {
            if (isDefeated) return;
            
            currentLifePoints = Mathf.Max(0, currentLifePoints - damage);
            OnLifePointsChanged?.Invoke(currentLifePoints);
            
            // Spawn damage effect
            if (damageEffect != null)
            {
                Instantiate(damageEffect, transform.position, Quaternion.identity);
            }
            
            if (currentLifePoints <= 0)
            {
                HandleDefeat();
            }
        }
        
        public void Heal(int amount)
        {
            if (isDefeated) return;
            
            currentLifePoints = Mathf.Min(maxLifePoints, currentLifePoints + amount);
            OnLifePointsChanged?.Invoke(currentLifePoints);
        }
        
        private void HandleDefeat()
        {
            isDefeated = true;
            
            // Spawn defeat effect
            if (defeatEffect != null)
            {
                Instantiate(defeatEffect, transform.position, Quaternion.identity);
            }
            
            OnPlayerDefeated?.Invoke();
        }
        
        public void ResetBase()
        {
            currentLifePoints = maxLifePoints;
            isDefeated = false;
            OnLifePointsChanged?.Invoke(currentLifePoints);
        }
    }
} 