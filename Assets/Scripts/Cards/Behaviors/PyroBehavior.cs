using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace YuGiOh.Cards
{
    public class PyroBehavior : MonsterTypeBehavior
    {
        [Header("Pyro Special Ability")]
        [SerializeField] private float infernoCost = 35f;
        [SerializeField] private float infernoDamage = 50f;
        [SerializeField] private float infernoRadius = 6f;
        [SerializeField] private float burnDuration = 5f;
        [SerializeField] private float burnDamagePerSecond = 10f;
        [SerializeField] private GameObject infernoEffectPrefab;
        [SerializeField] private AudioClip infernoSound;
        
        [Header("Pyro Heat System")]
        [SerializeField] private float heatPool = 100f;
        [SerializeField] private float maxHeatPool = 100f;
        [SerializeField] private float heatRegenRate = 5f;
        [SerializeField] private float heatConsumptionRate = 2f;
        [SerializeField] private GameObject heatEffectPrefab;
        
        [Header("Pyro Combustion")]
        [SerializeField] private float combustionThreshold = 80f;
        [SerializeField] private float combustionDamageBonus = 1.5f;
        [SerializeField] private float combustionSpeedBonus = 1.3f;
        [SerializeField] private GameObject combustionEffectPrefab;
        [SerializeField] private AudioClip combustionSound;
        
        private bool isCombustionActive = false;
        private float originalSpeed;
        private float originalDamage;
        private Dictionary<GameObject, float> burningEnemies = new Dictionary<GameObject, float>();
        
        protected override void Start()
        {
            base.Start();
            StartCoroutine(RegenerateHeat());
            originalSpeed = monsterCard.GetMoveSpeed();
            originalDamage = monsterCard.GetAttackDamage();
        }
        
        protected override void ApplySpecialAbilityEffects()
        {
            // Pyro's special ability: Inferno
            if (heatPool >= infernoCost)
            {
                CastInferno();
                heatPool -= infernoCost;
            }
        }
        
        protected override void RemoveSpecialAbilityEffects()
        {
            // No cleanup needed for inferno
        }
        
        private void CastInferno()
        {
            // Get target position
            Transform target = monsterCard.GetCurrentTarget();
            if (target == null)
            {
                return;
            }
            
            // Find all enemies in radius
            Collider[] colliders = Physics.OverlapSphere(target.position, infernoRadius);
            
            foreach (Collider collider in colliders)
            {
                MonsterCard enemyMonster = collider.GetComponent<MonsterCard>();
                if (enemyMonster != null && enemyMonster.IsEnemy())
                {
                    // Apply initial damage
                    enemyMonster.TakeDamage(Mathf.RoundToInt(infernoDamage));
                    
                    // Apply burn effect
                    ApplyBurnEffect(enemyMonster);
                    
                    // Spawn inferno effect
                    if (infernoEffectPrefab != null)
                    {
                        GameObject effect = Instantiate(infernoEffectPrefab, enemyMonster.transform.position, Quaternion.identity, enemyMonster.transform);
                        Destroy(effect, 2f);
                    }
                }
            }
            
            // Play sound
            if (infernoSound != null)
            {
                AudioSource.PlayClipAtPoint(infernoSound, target.position);
            }
        }
        
        private void ApplyBurnEffect(MonsterCard enemy)
        {
            // Add to burning enemies
            burningEnemies[enemy.gameObject] = Time.time + burnDuration;
            
            // Start burn damage coroutine
            StartCoroutine(BurnDamage(enemy));
        }
        
        private IEnumerator BurnDamage(MonsterCard enemy)
        {
            while (burningEnemies.ContainsKey(enemy.gameObject) && Time.time < burningEnemies[enemy.gameObject])
            {
                // Apply burn damage
                enemy.TakeDamage(Mathf.RoundToInt(burnDamagePerSecond * Time.deltaTime));
                
                yield return null;
            }
            
            // Remove from burning enemies
            burningEnemies.Remove(enemy.gameObject);
        }
        
        private IEnumerator RegenerateHeat()
        {
            while (true)
            {
                // Consume heat if in combustion
                if (isCombustionActive)
                {
                    heatPool = Mathf.Max(0, heatPool - heatConsumptionRate * Time.deltaTime);
                    
                    // Check if combustion should be deactivated
                    if (heatPool < combustionThreshold)
                    {
                        DeactivateCombustion();
                    }
                }
                // Regenerate heat if not in combustion
                else if (heatPool < maxHeatPool)
                {
                    heatPool = Mathf.Min(maxHeatPool, heatPool + heatRegenRate * Time.deltaTime);
                    
                    // Check if combustion should be activated
                    if (heatPool >= combustionThreshold)
                    {
                        ActivateCombustion();
                    }
                }
                
                yield return null;
            }
        }
        
        private void ActivateCombustion()
        {
            isCombustionActive = true;
            
            // Apply combustion bonuses
            monsterCard.SetMoveSpeed(originalSpeed * combustionSpeedBonus);
            monsterCard.SetAttackDamage(originalDamage * combustionDamageBonus);
            
            // Spawn combustion effect
            if (combustionEffectPrefab != null)
            {
                GameObject effect = Instantiate(combustionEffectPrefab, transform.position, Quaternion.identity, transform);
                Destroy(effect, 2f);
            }
            
            // Play sound
            if (combustionSound != null)
            {
                AudioSource.PlayClipAtPoint(combustionSound, transform.position);
            }
        }
        
        private void DeactivateCombustion()
        {
            isCombustionActive = false;
            
            // Remove combustion bonuses
            monsterCard.SetMoveSpeed(originalSpeed);
            monsterCard.SetAttackDamage(originalDamage);
        }
        
        public override void OnMonsterSpawned()
        {
            base.OnMonsterSpawned();
            
            // Spawn heat effect
            if (heatEffectPrefab != null)
            {
                GameObject effect = Instantiate(heatEffectPrefab, transform.position, Quaternion.identity, transform);
                Destroy(effect, 2f);
            }
        }
        
        public override void OnMonsterDefeated()
        {
            base.OnMonsterDefeated();
            
            // Deactivate combustion if active
            if (isCombustionActive)
            {
                DeactivateCombustion();
            }
            
            // Clear burning enemies
            burningEnemies.Clear();
        }
    }
} 