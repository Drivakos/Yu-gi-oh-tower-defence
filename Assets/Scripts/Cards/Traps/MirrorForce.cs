using UnityEngine;
using YuGiOhTowerDefense.Core;

namespace YuGiOhTowerDefense.Cards.Traps
{
    public class MirrorForce : Card
    {
        [Header("Mirror Force Settings")]
        [SerializeField] private float damageRadius = 4f;
        [SerializeField] private float damageMultiplier = 1.5f;
        [SerializeField] private GameObject mirrorEffectPrefab;
        [SerializeField] private float effectDuration = 2f;
        
        private bool isTriggered = false;
        private float triggerTimer;

        protected override void Start()
        {
            base.Start();
            cardType = CardType.Trap;
        }

        protected override void Update()
        {
            base.Update();

            if (isTriggered)
            {
                triggerTimer -= Time.deltaTime;
                if (triggerTimer <= 0)
                {
                    Destroy();
                }
            }
        }

        protected override void ApplyEffect()
        {
            if (isTriggered) return;

            isTriggered = true;
            triggerTimer = effectDuration;

            // Spawn mirror effect
            if (mirrorEffectPrefab != null)
            {
                Instantiate(mirrorEffectPrefab, transform.position, Quaternion.identity);
            }

            // Find all enemies in range
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, damageRadius);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.TryGetComponent<Enemy>(out var enemy))
                {
                    // Calculate reflected damage based on enemy's damage
                    float reflectedDamage = enemy.GetStats().damage * damageMultiplier;
                    enemy.TakeDamage(reflectedDamage);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Visualize effect range in editor
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, damageRadius);
        }
    }
} 