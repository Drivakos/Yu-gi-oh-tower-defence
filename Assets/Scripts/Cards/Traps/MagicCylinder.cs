using UnityEngine;
using YuGiOhTowerDefense.Core;
using YuGiOhTowerDefense.Core.CardPacks;

namespace YuGiOhTowerDefense.Cards.Traps
{
    public class MagicCylinder : Card
    {
        [Header("Magic Cylinder Settings")]
        [SerializeField] private float redirectRadius = 4f;
        [SerializeField] private float damageMultiplier = 2f;
        [SerializeField] private float effectDuration = 3f;
        [SerializeField] private GameObject cylinderEffectPrefab;
        [SerializeField] private GameObject redirectEffectPrefab;
        [SerializeField] private LayerMask enemyLayer;
        
        private bool isActive = false;
        private float activeTimer;
        private GameObject activeCylinder;

        protected override void Start()
        {
            base.Start();
            cardType = CardType.Trap;
            rarity = CardRarity.SuperRare;
            cardName = "Magic Cylinder";
            cardDescription = "Redirects enemy attacks to damage other enemies in range.";
            availableInPacks = new CardPack[] { Resources.Load<TrapPack>("CardPacks/TrapPack") };
        }

        protected override void Update()
        {
            base.Update();

            if (isActive)
            {
                activeTimer -= Time.deltaTime;
                if (activeTimer <= 0)
                {
                    DeactivateCylinder();
                }
            }
        }

        protected override void ApplyEffect()
        {
            if (isActive) return;

            isActive = true;
            activeTimer = effectDuration;

            // Spawn cylinder effect
            if (cylinderEffectPrefab != null)
            {
                activeCylinder = Instantiate(cylinderEffectPrefab, transform.position, Quaternion.identity);
                activeCylinder.transform.localScale = Vector3.one * redirectRadius;
            }
        }

        private void DeactivateCylinder()
        {
            isActive = false;
            if (activeCylinder != null)
            {
                Destroy(activeCylinder);
            }
            Destroy();
        }

        public void OnEnemyAttack(Enemy attacker, float damage)
        {
            if (!isActive) return;

            // Find all enemies in range
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, redirectRadius, enemyLayer);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.TryGetComponent<Enemy>(out var enemy) && enemy != attacker)
                {
                    // Apply redirected damage
                    float redirectedDamage = damage * damageMultiplier;
                    enemy.TakeDamage(redirectedDamage);

                    // Spawn redirect effect
                    if (redirectEffectPrefab != null)
                    {
                        GameObject redirectEffect = Instantiate(redirectEffectPrefab, 
                            transform.position, 
                            Quaternion.LookRotation(enemy.transform.position - transform.position));
                        Destroy(redirectEffect, 1f);
                    }
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Visualize redirect range in editor
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, redirectRadius);
        }
    }
} 