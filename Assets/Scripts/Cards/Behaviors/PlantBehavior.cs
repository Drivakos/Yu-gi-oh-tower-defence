using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace YuGiOh.Cards
{
    public class PlantBehavior : MonsterTypeBehavior
    {
        [Header("Plant Special Ability")]
        [SerializeField] private float healingCost = 30f;
        [SerializeField] private float healingAmount = 40f;
        [SerializeField] private float healingRadius = 8f;
        [SerializeField] private GameObject healingEffectPrefab;
        [SerializeField] private AudioClip healingSound;
        
        [Header("Plant Growth")]
        [SerializeField] private float growthInterval = 5f;
        [SerializeField] private float growthHealthBonus = 20f;
        [SerializeField] private float growthDamageBonus = 10f;
        [SerializeField] private int maxGrowthStages = 3;
        [SerializeField] private GameObject growthEffectPrefab;
        [SerializeField] private AudioClip growthSound;
        
        [Header("Plant Spores")]
        [SerializeField] private float sporeInterval = 3f;
        [SerializeField] private float sporeHealAmount = 5f;
        [SerializeField] private float sporeRadius = 5f;
        [SerializeField] private GameObject sporeEffectPrefab;
        [SerializeField] private AudioClip sporeSound;
        
        private int currentGrowthStage = 0;
        private float nextGrowthTime;
        private float nextSporeTime;
        private List<GameObject> healedAllies = new List<GameObject>();
        
        protected override void Start()
        {
            base.Start();
            nextGrowthTime = Time.time + growthInterval;
            nextSporeTime = Time.time + sporeInterval;
        }
        
        protected override void ApplySpecialAbilityEffects()
        {
            // Plant's special ability: Healing Aura
            CastHealingAura();
        }
        
        protected override void RemoveSpecialAbilityEffects()
        {
            // No cleanup needed for healing aura
        }
        
        private void CastHealingAura()
        {
            // Find all friendly monsters in range
            Collider[] colliders = Physics.OverlapSphere(transform.position, healingRadius);
            
            foreach (Collider collider in colliders)
            {
                MonsterCard allyMonster = collider.GetComponent<MonsterCard>();
                if (allyMonster != null && !allyMonster.IsEnemy())
                {
                    // Heal ally
                    allyMonster.Heal(healingAmount);
                    
                    // Add to healed list
                    healedAllies.Add(allyMonster.gameObject);
                    
                    // Spawn healing effect
                    if (healingEffectPrefab != null)
                    {
                        GameObject effect = Instantiate(healingEffectPrefab, allyMonster.transform.position, Quaternion.identity, allyMonster.transform);
                        Destroy(effect, 2f);
                    }
                }
            }
            
            // Play sound
            if (healingSound != null)
            {
                AudioSource.PlayClipAtPoint(healingSound, transform.position);
            }
        }
        
        public override void OnMonsterSpawned()
        {
            base.OnMonsterSpawned();
            
            // Start growth and spore coroutines
            StartCoroutine(GrowthCycle());
            StartCoroutine(SporeCycle());
        }
        
        public override void OnMonsterDefeated()
        {
            base.OnMonsterDefeated();
            
            // No special cleanup needed for plants
        }
        
        private IEnumerator GrowthCycle()
        {
            while (true)
            {
                if (Time.time >= nextGrowthTime && currentGrowthStage < maxGrowthStages)
                {
                    // Apply growth
                    ApplyGrowth();
                    
                    nextGrowthTime = Time.time + growthInterval;
                }
                
                yield return null;
            }
        }
        
        private void ApplyGrowth()
        {
            // Increase growth stage
            currentGrowthStage++;
            
            // Apply growth bonuses
            monsterCard.AddHealthBonus(growthHealthBonus);
            monsterCard.AddDamageBonus(growthDamageBonus);
            
            // Spawn growth effect
            if (growthEffectPrefab != null)
            {
                GameObject effect = Instantiate(growthEffectPrefab, transform.position, Quaternion.identity, transform);
                Destroy(effect, 2f);
            }
            
            // Play sound
            if (growthSound != null)
            {
                AudioSource.PlayClipAtPoint(growthSound, transform.position);
            }
        }
        
        private IEnumerator SporeCycle()
        {
            while (true)
            {
                if (Time.time >= nextSporeTime)
                {
                    // Release spores
                    ReleaseSpores();
                    
                    nextSporeTime = Time.time + sporeInterval;
                }
                
                yield return null;
            }
        }
        
        private void ReleaseSpores()
        {
            // Find all friendly monsters in range
            Collider[] colliders = Physics.OverlapSphere(transform.position, sporeRadius);
            
            foreach (Collider collider in colliders)
            {
                MonsterCard allyMonster = collider.GetComponent<MonsterCard>();
                if (allyMonster != null && !allyMonster.IsEnemy())
                {
                    // Heal ally with spores
                    allyMonster.Heal(sporeHealAmount);
                    
                    // Spawn spore effect
                    if (sporeEffectPrefab != null)
                    {
                        GameObject effect = Instantiate(sporeEffectPrefab, allyMonster.transform.position, Quaternion.identity, allyMonster.transform);
                        Destroy(effect, 1f);
                    }
                }
            }
            
            // Play sound
            if (sporeSound != null)
            {
                AudioSource.PlayClipAtPoint(sporeSound, transform.position);
            }
        }
    }
} 