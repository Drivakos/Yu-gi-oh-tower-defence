using UnityEngine;
using System;

namespace YuGiOhTowerDefense.Core.Player
{
    public class Player : MonoBehaviour
    {
        [Header("Player Stats")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int currentHealth;
        
        public event Action<int> OnHealthChanged;
        public event Action OnPlayerDeath;
        
        private void Awake()
        {
            currentHealth = maxHealth;
        }
        
        public void TakeDamage(int damage)
        {
            currentHealth = Mathf.Max(0, currentHealth - damage);
            OnHealthChanged?.Invoke(currentHealth);
            
            if (currentHealth <= 0)
            {
                Die();
            }
        }
        
        public void Heal(int amount)
        {
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            OnHealthChanged?.Invoke(currentHealth);
        }
        
        private void Die()
        {
            OnPlayerDeath?.Invoke();
            // Handle player death (e.g., game over, respawn, etc.)
            Debug.Log("Player died!");
        }
        
        public float GetHealthPercentage()
        {
            return (float)currentHealth / maxHealth;
        }
    }
} 