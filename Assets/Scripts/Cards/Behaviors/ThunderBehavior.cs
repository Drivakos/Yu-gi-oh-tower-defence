using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace YuGiOh.Cards
{
    public class ThunderBehavior : MonsterTypeBehavior
    {
        [Header("Thunder Special Ability")]
        [SerializeField] private float lightningCost = 30f;
        [SerializeField] private float lightningDamage = 40f;
        [SerializeField] private float chainRange = 5f;
        [SerializeField] private int maxChainTargets = 3;
        [SerializeField] private float chainDamageReduction = 0.7f;
        [SerializeField] private GameObject lightningEffectPrefab;
        [SerializeField] private AudioClip lightningSound;
        
        [Header("Thunder Charge System")]
        [SerializeField] private float chargePool = 100f;
        [SerializeField] private float maxChargePool = 100f;
        [SerializeField] private float chargeRegenRate = 5f;
        [SerializeField] private float chargeConsumptionRate = 2f;
        [SerializeField] private GameObject chargeEffectPrefab;
        
        [Header("Thunder Overcharge")]
        [SerializeField] private float overchargeThreshold = 80f;
        [SerializeField] private float overchargeDamageBonus = 1.5f;
        [SerializeField] private float overchargeSpeedBonus = 1.3f;
        [SerializeField] private GameObject overchargeEffectPrefab;
        [SerializeField] private AudioClip overchargeSound;
        
        private bool isOverchargeActive = false;
        private float originalSpeed;
        private float originalDamage;
        private List<GameObject> chainedEnemies = new List<GameObject>();
        
        protected override void Start()
        {
            base.Start();
            StartCoroutine(RegenerateCharge());
            originalSpeed = monsterCard.GetMoveSpeed();
            originalDamage = monsterCard.GetAttackDamage();
        }
        
        protected override void ApplySpecialAbilityEffects()
        {
            // Thunder's special ability: Lightning Chain
            if (chargePool >= lightningCost)
            {
                CastLightningChain();
                chargePool -= lightningCost;
            }
        }
        
        protected override void RemoveSpecialAbilityEffects()
        {
            // No cleanup needed for lightning chain
        }
        
        private void CastLightningChain()
        {
            // Get initial target
            Transform target = monsterCard.GetCurrentTarget();
            if (target == null)
            {
                return;
            }
            
            // Clear previous chain
            chainedEnemies.Clear();
            
            // Apply lightning to initial target
            MonsterCard initialEnemy = target.GetComponent<MonsterCard>();
            if (initialEnemy != null && initialEnemy.IsEnemy())
            {
                ApplyLightning(initialEnemy, 1f);
                
                // Chain to nearby enemies
                ChainLightning(initialEnemy, maxChainTargets - 1, chainDamageReduction);
            }
        }
        
        private void ChainLightning(MonsterCard source, int remainingChains, float damageMultiplier)
        {
            if (remainingChains <= 0)
            {
                return;
            }
            
            // Find nearest enemy in range
            MonsterCard nearestEnemy = FindNearestEnemy(source.transform.position, chainRange);
            
            if (nearestEnemy != null)
            {
                // Apply lightning to chained enemy
                ApplyLightning(nearestEnemy, damageMultiplier);
                
                // Continue chain
                ChainLightning(nearestEnemy, remainingChains - 1, damageMultiplier * chainDamageReduction);
            }
        }
        
        private MonsterCard FindNearestEnemy(Vector3 position, float range)
        {
            MonsterCard nearest = null;
            float nearestDistance = float.MaxValue;
            
            // Find all enemies in range
            Collider[] colliders = Physics.OverlapSphere(position, range);
            
            foreach (Collider collider in colliders)
            {
                MonsterCard enemy = collider.GetComponent<MonsterCard>();
                if (enemy != null && enemy.IsEnemy() && !chainedEnemies.Contains(enemy.gameObject))
                {
                    float distance = Vector3.Distance(position, enemy.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearest = enemy;
                        nearestDistance = distance;
                    }
                }
            }
            
            return nearest;
        }
        
        private void ApplyLightning(MonsterCard enemy, float damageMultiplier)
        {
            // Add to chained enemies
            chainedEnemies.Add(enemy.gameObject);
            
            // Apply damage
            enemy.TakeDamage(Mathf.RoundToInt(lightningDamage * damageMultiplier));
            
            // Spawn lightning effect
            if (lightningEffectPrefab != null)
            {
                GameObject effect = Instantiate(lightningEffectPrefab, enemy.transform.position, Quaternion.identity, enemy.transform);
                Destroy(effect, 1f);
            }
            
            // Play sound
            if (lightningSound != null)
            {
                AudioSource.PlayClipAtPoint(lightningSound, enemy.transform.position);
            }
        }
        
        private IEnumerator RegenerateCharge()
        {
            while (true)
            {
                // Consume charge if in overcharge
                if (isOverchargeActive)
                {
                    chargePool = Mathf.Max(0, chargePool - chargeConsumptionRate * Time.deltaTime);
                    
                    // Check if overcharge should be deactivated
                    if (chargePool < overchargeThreshold)
                    {
                        DeactivateOvercharge();
                    }
                }
                // Regenerate charge if not in overcharge
                else if (chargePool < maxChargePool)
                {
                    chargePool = Mathf.Min(maxChargePool, chargePool + chargeRegenRate * Time.deltaTime);
                    
                    // Check if overcharge should be activated
                    if (chargePool >= overchargeThreshold)
                    {
                        ActivateOvercharge();
                    }
                }
                
                yield return null;
            }
        }
        
        private void ActivateOvercharge()
        {
            isOverchargeActive = true;
            
            // Apply overcharge bonuses
            monsterCard.SetMoveSpeed(originalSpeed * overchargeSpeedBonus);
            monsterCard.SetAttackDamage(originalDamage * overchargeDamageBonus);
            
            // Spawn overcharge effect
            if (overchargeEffectPrefab != null)
            {
                GameObject effect = Instantiate(overchargeEffectPrefab, transform.position, Quaternion.identity, transform);
                Destroy(effect, 2f);
            }
            
            // Play sound
            if (overchargeSound != null)
            {
                AudioSource.PlayClipAtPoint(overchargeSound, transform.position);
            }
        }
        
        private void DeactivateOvercharge()
        {
            isOverchargeActive = false;
            
            // Remove overcharge bonuses
            monsterCard.SetMoveSpeed(originalSpeed);
            monsterCard.SetAttackDamage(originalDamage);
        }
        
        public override void OnMonsterSpawned()
        {
            base.OnMonsterSpawned();
            
            // Spawn charge effect
            if (chargeEffectPrefab != null)
            {
                GameObject effect = Instantiate(chargeEffectPrefab, transform.position, Quaternion.identity, transform);
                Destroy(effect, 2f);
            }
        }
        
        public override void OnMonsterDefeated()
        {
            base.OnMonsterDefeated();
            
            // Deactivate overcharge if active
            if (isOverchargeActive)
            {
                DeactivateOvercharge();
            }
            
            // Clear chained enemies
            chainedEnemies.Clear();
        }
    }
} 