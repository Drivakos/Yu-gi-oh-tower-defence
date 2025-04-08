using UnityEngine;
using System.Collections;
using YuGiOhTowerDefense.Core;

namespace YuGiOhTowerDefense.Enemies
{
    public class BlueEyesUltimateDragon : Enemy
    {
        [Header("Blue-Eyes Ultimate Settings")]
        [SerializeField] private float attackRange = 5f;
        [SerializeField] private float attackCooldown = 3f;
        [SerializeField] private float attackDamage = 500f;
        [SerializeField] private GameObject attackEffectPrefab;
        [SerializeField] private float specialAttackCooldown = 15f;
        [SerializeField] private float specialAttackRadius = 8f;
        [SerializeField] private float specialAttackDamage = 1000f;
        [SerializeField] private GameObject specialAttackEffectPrefab;
        [SerializeField] private int maxHeads = 3;
        
        private float currentAttackCooldown;
        private float currentSpecialAttackCooldown;
        private int remainingHeads;
        private bool isAttacking;
        private Transform attackTarget;
        
        protected override void Start()
        {
            base.Start();
            stats.type = EnemyType.Boss;
            remainingHeads = maxHeads;
            currentAttackCooldown = 0f;
            currentSpecialAttackCooldown = 0f;
            isAttacking = false;
        }
        
        protected override void Update()
        {
            base.Update();
            
            if (!isActive || isStunned) return;
            
            // Update cooldowns
            if (currentAttackCooldown > 0)
            {
                currentAttackCooldown -= Time.deltaTime;
            }
            
            if (currentSpecialAttackCooldown > 0)
            {
                currentSpecialAttackCooldown -= Time.deltaTime;
            }
            
            // Find nearest monster to attack
            if (!isAttacking && currentAttackCooldown <= 0)
            {
                FindAttackTarget();
            }
            
            // Perform attack if target is in range
            if (attackTarget != null && !isAttacking)
            {
                float distanceToTarget = Vector3.Distance(transform.position, attackTarget.position);
                if (distanceToTarget <= attackRange)
                {
                    StartCoroutine(PerformAttack());
                }
            }
            
            // Use special attack if cooldown is ready
            if (currentSpecialAttackCooldown <= 0)
            {
                UseSpecialAttack();
            }
        }
        
        private void FindAttackTarget()
        {
            // Find all monsters in range
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);
            float closestDistance = float.MaxValue;
            Transform closestTarget = null;
            
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Monster"))
                {
                    float distance = Vector3.Distance(transform.position, hitCollider.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestTarget = hitCollider.transform;
                    }
                }
            }
            
            attackTarget = closestTarget;
        }
        
        private IEnumerator PerformAttack()
        {
            isAttacking = true;
            currentAttackCooldown = attackCooldown;
            
            // Face the target
            Vector3 direction = (attackTarget.position - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = targetRotation;
            
            // Spawn attack effect
            if (attackEffectPrefab != null)
            {
                Instantiate(attackEffectPrefab, transform.position + transform.forward * 2f, transform.rotation);
            }
            
            // Deal damage to target
            Monster monster = attackTarget.GetComponent<Monster>();
            if (monster != null)
            {
                monster.TakeDamage(attackDamage);
            }
            
            // Wait for attack animation
            yield return new WaitForSeconds(1f);
            
            isAttacking = false;
        }
        
        private void UseSpecialAttack()
        {
            currentSpecialAttackCooldown = specialAttackCooldown;
            
            // Spawn special attack effect
            if (specialAttackEffectPrefab != null)
            {
                Instantiate(specialAttackEffectPrefab, transform.position, Quaternion.identity);
            }
            
            // Deal damage to all monsters in range
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, specialAttackRadius);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Monster"))
                {
                    Monster monster = hitCollider.GetComponent<Monster>();
                    if (monster != null)
                    {
                        monster.TakeDamage(specialAttackDamage);
                    }
                }
            }
        }
        
        protected override void Die()
        {
            // Lose a head instead of dying immediately
            remainingHeads--;
            
            if (remainingHeads <= 0)
            {
                base.Die();
            }
            else
            {
                // Heal partially and continue
                currentHealth = stats.health * 0.5f;
                
                // Visual feedback for losing a head
                // This would be handled by animation in a real implementation
                transform.localScale *= 0.9f;
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            // Visualize attack range in editor
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, specialAttackRadius);
        }
    }
} 