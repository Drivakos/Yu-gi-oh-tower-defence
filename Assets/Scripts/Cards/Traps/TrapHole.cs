using UnityEngine;
using YuGiOhTowerDefense.Core;
using YuGiOhTowerDefense.Core.CardPacks;

namespace YuGiOhTowerDefense.Cards.Traps
{
    public class TrapHole : Card
    {
        [Header("Trap Hole Settings")]
        [SerializeField] private float damageRadius = 3f;
        [SerializeField] private float damageAmount = 50f;
        [SerializeField] private float slowAmount = 0.5f;
        [SerializeField] private float slowDuration = 3f;
        [SerializeField] private GameObject trapEffectPrefab;
        [SerializeField] private float effectDuration = 2f;
        
        private bool isTriggered = false;
        private float triggerTimer;
        private GameObject activeEffect;

        protected override void Start()
        {
            base.Start();
            cardType = CardType.Trap;
            rarity = CardRarity.Common;
            cardName = "Trap Hole";
            cardDescription = "When an enemy enters this trap's range, it takes damage and is slowed.";
            availableInPacks = new CardPack[] { Resources.Load<TrapPack>("CardPacks/TrapPack") };
        }

        protected override void Update()
        {
            base.Update();

            if (isTriggered)
            {
                triggerTimer -= Time.deltaTime;
                if (triggerTimer <= 0)
                {
                    if (activeEffect != null)
                    {
                        Destroy(activeEffect);
                    }
                    Destroy();
                }
            }
        }

        protected override void ApplyEffect()
        {
            if (isTriggered) return;

            isTriggered = true;
            triggerTimer = effectDuration;

            // Spawn trap effect
            if (trapEffectPrefab != null)
            {
                activeEffect = Instantiate(trapEffectPrefab, transform.position, Quaternion.identity);
            }

            // Find all enemies in range
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, damageRadius);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.TryGetComponent<Enemy>(out var enemy))
                {
                    // Apply damage
                    enemy.TakeDamage(damageAmount);
                    
                    // Apply slow effect
                    enemy.ApplySlowEffect(slowAmount, slowDuration);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Visualize effect range in editor
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, damageRadius);
        }
    }
} 