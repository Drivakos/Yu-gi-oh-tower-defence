using UnityEngine;
using YuGiOhTowerDefense.Core;
using YuGiOhTowerDefense.Core.CardPacks;

namespace YuGiOhTowerDefense.Cards.Traps
{
    public class SakuretsuArmor : Card
    {
        [Header("Sakuretsu Armor Settings")]
        [SerializeField] private float barrierRadius = 2f;
        [SerializeField] private float barrierHealth = 200f;
        [SerializeField] private float damageReflectionMultiplier = 1.5f;
        [SerializeField] private float effectDuration = 5f;
        [SerializeField] private GameObject barrierEffectPrefab;
        [SerializeField] private GameObject reflectionEffectPrefab;
        
        private bool isActive = false;
        private float activeTimer;
        private GameObject activeBarrier;
        private float currentBarrierHealth;

        protected override void Start()
        {
            base.Start();
            cardType = CardType.Trap;
            rarity = CardRarity.Rare;
            cardName = "Sakuretsu Armor";
            cardDescription = "Creates a protective barrier that reflects damage back to attacking enemies.";
            availableInPacks = new CardPack[] { Resources.Load<TrapPack>("CardPacks/TrapPack") };
        }

        protected override void Update()
        {
            base.Update();

            if (isActive)
            {
                activeTimer -= Time.deltaTime;
                if (activeTimer <= 0 || currentBarrierHealth <= 0)
                {
                    DeactivateBarrier();
                }
            }
        }

        protected override void ApplyEffect()
        {
            if (isActive) return;

            isActive = true;
            activeTimer = effectDuration;
            currentBarrierHealth = barrierHealth;

            // Spawn barrier effect
            if (barrierEffectPrefab != null)
            {
                activeBarrier = Instantiate(barrierEffectPrefab, transform.position, Quaternion.identity);
                activeBarrier.transform.localScale = Vector3.one * barrierRadius;
            }
        }

        private void DeactivateBarrier()
        {
            isActive = false;
            if (activeBarrier != null)
            {
                Destroy(activeBarrier);
            }
            Destroy();
        }

        public void OnBarrierDamaged(float damage, Enemy attacker)
        {
            if (!isActive) return;

            currentBarrierHealth -= damage;

            // Reflect damage back to the attacker
            float reflectedDamage = damage * damageReflectionMultiplier;
            attacker.TakeDamage(reflectedDamage);

            // Spawn reflection effect
            if (reflectionEffectPrefab != null)
            {
                GameObject reflectionEffect = Instantiate(reflectionEffectPrefab, transform.position, Quaternion.identity);
                Destroy(reflectionEffect, 1f);
            }

            // Update barrier visual state based on health
            if (activeBarrier != null)
            {
                float healthRatio = currentBarrierHealth / barrierHealth;
                activeBarrier.transform.localScale = Vector3.one * barrierRadius * healthRatio;
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Visualize barrier range in editor
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, barrierRadius);
        }
    }
} 