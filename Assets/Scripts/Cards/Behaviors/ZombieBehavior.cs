using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace YuGiOh.Cards
{
    public class ZombieBehavior : MonsterTypeBehavior
    {
        [Header("Zombie Special Ability")]
        [SerializeField] private float revivalCost = 30f;
        [SerializeField] private float revivalHealthPercent = 0.5f;
        [SerializeField] private float revivalDuration = 10f;
        [SerializeField] private GameObject revivalEffectPrefab;
        [SerializeField] private AudioClip revivalSound;
        
        [Header("Zombie Aura")]
        [SerializeField] private float healthDrainRate = 5f;
        [SerializeField] private float healthDrainRange = 8f;
        [SerializeField] private float healthDrainInterval = 1f;
        [SerializeField] private GameObject healthDrainEffectPrefab;
        [SerializeField] private AudioClip healthDrainSound;
        
        private float revivalEnergy = 100f;
        private float maxRevivalEnergy = 100f;
        private float revivalEnergyRegenRate = 5f;
        private List<GameObject> revivedMonsters = new List<GameObject>();
        private float nextHealthDrainTime;
        
        protected override void Start()
        {
            base.Start();
            StartCoroutine(RegenerateRevivalEnergy());
            nextHealthDrainTime = Time.time + healthDrainInterval;
        }
        
        protected override void ApplySpecialAbilityEffects()
        {
            // Zombie's special ability: Revive Fallen Allies
            if (revivalEnergy >= revivalCost)
            {
                ReviveFallenAllies();
                revivalEnergy -= revivalCost;
            }
        }
        
        protected override void RemoveSpecialAbilityEffects()
        {
            // No cleanup needed for revival
        }
        
        private void ReviveFallenAllies()
        {
            // Find all fallen friendly monsters in range
            Collider[] colliders = Physics.OverlapSphere(transform.position, healthDrainRange);
            
            foreach (Collider collider in colliders)
            {
                MonsterCard fallenMonster = collider.GetComponent<MonsterCard>();
                if (fallenMonster != null && !fallenMonster.IsEnemy() && fallenMonster.IsDefeated())
                {
                    // Revive the fallen monster
                    ReviveMonster(fallenMonster);
                    
                    // Add to revived list
                    revivedMonsters.Add(fallenMonster.gameObject);
                    
                    // Spawn revival effect
                    if (revivalEffectPrefab != null)
                    {
                        GameObject effect = Instantiate(revivalEffectPrefab, fallenMonster.transform.position, Quaternion.identity, fallenMonster.transform);
                        Destroy(effect, 2f);
                    }
                    
                    // Play sound
                    if (revivalSound != null)
                    {
                        AudioSource.PlayClipAtPoint(revivalSound, fallenMonster.transform.position);
                    }
                }
            }
        }
        
        private void ReviveMonster(MonsterCard fallenMonster)
        {
            // Calculate revival health
            float revivalHealth = fallenMonster.GetMaxHealth() * revivalHealthPercent;
            
            // Revive the monster with partial health
            fallenMonster.Revive(revivalHealth);
            
            // Set revival duration
            StartCoroutine(RevivalDuration(fallenMonster));
        }
        
        private IEnumerator RevivalDuration(MonsterCard revivedMonster)
        {
            yield return new WaitForSeconds(revivalDuration);
            
            // Check if monster is still alive
            if (revivedMonster != null && !revivedMonster.IsDefeated())
            {
                // Defeat the monster after revival duration
                revivedMonster.Defeat();
            }
        }
        
        private IEnumerator RegenerateRevivalEnergy()
        {
            while (true)
            {
                if (revivalEnergy < maxRevivalEnergy)
                {
                    revivalEnergy = Mathf.Min(revivalEnergy + revivalEnergyRegenRate * Time.deltaTime, maxRevivalEnergy);
                }
                yield return null;
            }
        }
        
        public override void OnMonsterSpawned()
        {
            base.OnMonsterSpawned();
            
            // Start health drain aura
            StartCoroutine(HealthDrainAura());
        }
        
        public override void OnMonsterDefeated()
        {
            base.OnMonsterDefeated();
            
            // No special cleanup needed for zombies
        }
        
        private IEnumerator HealthDrainAura()
        {
            while (true)
            {
                if (Time.time >= nextHealthDrainTime)
                {
                    // Find all enemies in range
                    Collider[] colliders = Physics.OverlapSphere(transform.position, healthDrainRange);
                    
                    foreach (Collider collider in colliders)
                    {
                        MonsterCard enemyMonster = collider.GetComponent<MonsterCard>();
                        if (enemyMonster != null && enemyMonster.IsEnemy())
                        {
                            // Drain health from enemy
                            enemyMonster.TakeDamage(Mathf.RoundToInt(healthDrainRate));
                            
                            // Heal zombie
                            monsterCard.Heal(healthDrainRate);
                            
                            // Spawn health drain effect
                            if (healthDrainEffectPrefab != null)
                            {
                                GameObject effect = Instantiate(healthDrainEffectPrefab, enemyMonster.transform.position, Quaternion.identity, enemyMonster.transform);
                                Destroy(effect, 1f);
                            }
                            
                            // Play sound
                            if (healthDrainSound != null)
                            {
                                AudioSource.PlayClipAtPoint(healthDrainSound, enemyMonster.transform.position);
                            }
                        }
                    }
                    
                    nextHealthDrainTime = Time.time + healthDrainInterval;
                }
                
                yield return null;
            }
        }
    }
} 