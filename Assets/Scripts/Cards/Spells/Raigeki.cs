using UnityEngine;
using YuGiOhTowerDefense.Core;

namespace YuGiOhTowerDefense.Cards.Spells
{
    public class Raigeki : Card
    {
        [Header("Raigeki Settings")]
        [SerializeField] private float damageRadius = 5f;
        [SerializeField] private float damageAmount = 1000f;
        [SerializeField] private GameObject lightningEffectPrefab;
        [SerializeField] private int maxTargets = 5;

        protected override void Start()
        {
            base.Start();
            cardType = CardType.Spell;
        }

        protected override void ApplyEffect()
        {
            // Find all enemies within range
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, damageRadius);
            int targetsHit = 0;

            foreach (var hitCollider in hitColliders)
            {
                if (targetsHit >= maxTargets) break;

                if (hitCollider.TryGetComponent<Enemy>(out var enemy))
                {
                    // Spawn lightning effect
                    if (lightningEffectPrefab != null)
                    {
                        Instantiate(lightningEffectPrefab, enemy.transform.position, Quaternion.identity);
                    }

                    // Deal damage
                    enemy.TakeDamage(damageAmount);
                    targetsHit++;
                }
            }

            // Destroy the card after use
            Destroy();
        }

        private void OnDrawGizmosSelected()
        {
            // Visualize effect range in editor
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, damageRadius);
        }
    }
} 