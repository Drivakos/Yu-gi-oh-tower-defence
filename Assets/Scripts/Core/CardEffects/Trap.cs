using UnityEngine;
using YuGiOhTowerDefense.Cards;
using YuGiOhTowerDefense.Core.Enemy;

namespace YuGiOhTowerDefense.Core.CardEffects
{
    public class Trap : MonoBehaviour
    {
        [Header("Trap Settings")]
        [SerializeField] private float triggerRadius = 2f;
        [SerializeField] private float lifetime = 10f;
        [SerializeField] private GameObject triggerEffect;
        
        private YuGiOhCard cardData;
        private bool isTriggered;
        private float currentLifetime;
        
        public void Initialize(YuGiOhCard card)
        {
            cardData = card;
            currentLifetime = lifetime;
            isTriggered = false;
        }
        
        private void Update()
        {
            if (isTriggered) return;
            
            // Check for enemies in range
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, triggerRadius);
            foreach (var hitCollider in hitColliders)
            {
                Enemy enemy = hitCollider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    TriggerTrap(enemy);
                    break;
                }
            }
            
            // Update lifetime
            currentLifetime -= Time.deltaTime;
            if (currentLifetime <= 0)
            {
                Destroy(gameObject);
            }
        }
        
        private void TriggerTrap(Enemy enemy)
        {
            isTriggered = true;
            
            // Play trigger effect
            if (triggerEffect != null)
            {
                Instantiate(triggerEffect, transform.position, Quaternion.identity);
            }
            
            // Apply trap effect based on card data
            switch (cardData.TrapType)
            {
                case TrapType.Damage:
                    enemy.TakeDamage(cardData.EffectValue);
                    break;
                case TrapType.Stun:
                    enemy.ApplyStun(cardData.EffectDuration);
                    break;
                case TrapType.Slow:
                    enemy.ApplySlow(cardData.EffectValue, cardData.EffectDuration);
                    break;
                default:
                    Debug.LogWarning($"Unknown trap type: {cardData.TrapType}");
                    break;
            }
            
            // Destroy trap after triggering
            Destroy(gameObject);
        }
        
        private void OnDrawGizmos()
        {
            // Draw trigger radius in editor
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, triggerRadius);
        }
    }
} 