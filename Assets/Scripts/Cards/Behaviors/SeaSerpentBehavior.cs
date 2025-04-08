using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace YuGiOh.Cards.Behaviors
{
    public class SeaSerpentBehavior : MonsterTypeBehavior
    {
        [Header("Sea Serpent Special Ability")]
        [SerializeField] private float tidalWaveCost = 35f;
        [SerializeField] private float tidalWaveDamage = 45f;
        [SerializeField] private float tidalWaveWidth = 8f;
        [SerializeField] private float tidalWaveLength = 15f;
        [SerializeField] private float knockbackForce = 10f;
        [SerializeField] private GameObject tidalWaveEffectPrefab;
        [SerializeField] private AudioClip tidalWaveSound;
        
        [Header("Sea Serpent Water System")]
        [SerializeField] private float waterPool = 100f;
        [SerializeField] private float maxWaterPool = 100f;
        [SerializeField] private float waterRegenRate = 5f;
        [SerializeField] private float waterConsumptionRate = 2f;
        [SerializeField] private GameObject waterEffectPrefab;
        
        [Header("Sea Serpent Tsunami")]
        [SerializeField] private float tsunamiThreshold = 80f;
        [SerializeField] private float tsunamiDamageBonus = 1.5f;
        [SerializeField] private float tsunamiSpeedBonus = 1.3f;
        [SerializeField] private GameObject tsunamiEffectPrefab;
        [SerializeField] private AudioClip tsunamiSound;
        
        private bool isTsunamiActive = false;
        private float originalSpeed;
        private float originalDamage;
        private List<GameObject> affectedEnemies = new List<GameObject>();
        
        protected override void Start()
        {
            base.Start();
            StartCoroutine(RegenerateWater());
            originalSpeed = monsterCard.GetMoveSpeed();
            originalDamage = monsterCard.GetAttackDamage();
        }
        
        protected override void ApplySpecialAbilityEffects()
        {
            if (waterPool >= tidalWaveCost && IsTargetInRange(monsterCard.GetCurrentTarget()))
            {
                CastTidalWave();
                waterPool -= tidalWaveCost;
            }
        }
        
        protected override void RemoveSpecialAbilityEffects()
        {
            if (isTsunamiActive)
            {
                DeactivateTsunami();
            }
            affectedEnemies.Clear();
        }
        
        private void CastTidalWave()
        {
            Transform target = monsterCard.GetCurrentTarget();
            if (target == null) return;
            
            Vector3 direction = (target.position - transform.position).normalized;
            Vector3 waveCenter = transform.position + direction * (tidalWaveLength / 2f);
            
            Collider[] colliders = Physics.OverlapBox(
                waveCenter,
                new Vector3(tidalWaveWidth / 2f, 2f, tidalWaveLength / 2f),
                Quaternion.LookRotation(direction)
            );
            
            foreach (Collider collider in colliders)
            {
                MonsterCard enemyMonster = collider.GetComponent<MonsterCard>();
                if (enemyMonster != null && enemyMonster.IsEnemy())
                {
                    enemyMonster.TakeDamage(Mathf.RoundToInt(tidalWaveDamage));
                    
                    Rigidbody enemyRb = enemyMonster.GetComponent<Rigidbody>();
                    if (enemyRb != null)
                    {
                        enemyRb.AddForce(direction * knockbackForce, ForceMode.Impulse);
                    }
                    
                    affectedEnemies.Add(enemyMonster.gameObject);
                    PlayEffect(tidalWaveEffectPrefab, enemyMonster.transform.position, Quaternion.LookRotation(direction), enemyMonster.transform);
                }
            }
            
            PlaySound(tidalWaveSound, waveCenter);
        }
        
        private IEnumerator RegenerateWater()
        {
            while (true)
            {
                if (isTsunamiActive)
                {
                    waterPool = Mathf.Max(0, waterPool - waterConsumptionRate * Time.deltaTime);
                    if (waterPool < tsunamiThreshold)
                    {
                        DeactivateTsunami();
                    }
                }
                else if (waterPool < maxWaterPool)
                {
                    waterPool = Mathf.Min(maxWaterPool, waterPool + waterRegenRate * Time.deltaTime);
                    if (waterPool >= tsunamiThreshold)
                    {
                        ActivateTsunami();
                    }
                }
                yield return null;
            }
        }
        
        private void ActivateTsunami()
        {
            isTsunamiActive = true;
            monsterCard.SetMoveSpeed(originalSpeed * tsunamiSpeedBonus);
            monsterCard.SetAttackDamage(originalDamage * tsunamiDamageBonus);
            
            PlayEffect(tsunamiEffectPrefab, transform.position, Quaternion.identity, transform);
            PlaySound(tsunamiSound, transform.position);
        }
        
        private void DeactivateTsunami()
        {
            isTsunamiActive = false;
            monsterCard.SetMoveSpeed(originalSpeed);
            monsterCard.SetAttackDamage(originalDamage);
        }
        
        public override void OnMonsterSpawned()
        {
            base.OnMonsterSpawned();
            PlayEffect(waterEffectPrefab, transform.position, Quaternion.identity, transform);
        }
        
        public override void OnMonsterDefeated()
        {
            base.OnMonsterDefeated();
            if (isTsunamiActive)
            {
                DeactivateTsunami();
            }
            affectedEnemies.Clear();
        }
    }
} 