using UnityEngine;
using YuGiOhTowerDefense.Core;

namespace YuGiOhTowerDefense.Monsters
{
    public class BlueEyesWhiteDragon : Monster
    {
        [Header("Blue-Eyes Specific Settings")]
        [SerializeField] private float areaOfEffectRadius = 3f;
        [SerializeField] private GameObject areaEffectPrefab;
        [SerializeField] private float specialAttackCooldown = 10f;
        [SerializeField] private float specialAttackDamageMultiplier = 2f;
        
        private float specialAttackTimer;
        private bool canUseSpecialAttack = true;

        protected override void Start()
        {
            base.Start();
            specialAttackTimer = 0f;
            
            // Set Blue-Eyes specific attributes
            type = MonsterType.Dragon;
            attribute = Attribute.Light;
        }

        protected override void Update()
        {
            base.Update();

            if (!canUseSpecialAttack)
            {
                specialAttackTimer -= Time.deltaTime;
                if (specialAttackTimer <= 0)
                {
                    canUseSpecialAttack = true;
                }
            }
        }

        protected override void Attack()
        {
            if (target == null) return;

            base.Attack();

            // Check for enemies in area of effect
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, areaOfEffectRadius);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.TryGetComponent<Enemy>(out var enemy) && enemy.gameObject != target.gameObject)
                {
                    // Deal reduced damage to secondary targets
                    enemy.TakeDamage(stats.attack * 0.5f);
                }
            }
        }

        public void UseSpecialAttack()
        {
            if (!canUseSpecialAttack || target == null) return;

            // Spawn area effect
            if (areaEffectPrefab != null)
            {
                Instantiate(areaEffectPrefab, target.position, Quaternion.identity);
            }

            // Deal massive damage in an area
            Collider[] hitColliders = Physics.OverlapSphere(target.position, areaOfEffectRadius * 2f);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.TryGetComponent<Enemy>(out var enemy))
                {
                    enemy.TakeDamage(stats.attack * specialAttackDamageMultiplier);
                }
            }

            // Apply cooldown
            canUseSpecialAttack = false;
            specialAttackTimer = specialAttackCooldown;
        }

        public override void LevelUp()
        {
            base.LevelUp();
            
            // Blue-Eyes specific level up bonuses
            areaOfEffectRadius *= 1.1f;
            specialAttackDamageMultiplier *= 1.1f;
        }

        private void OnDrawGizmosSelected()
        {
            // Visualize attack range in editor
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, areaOfEffectRadius);
        }
    }
} 