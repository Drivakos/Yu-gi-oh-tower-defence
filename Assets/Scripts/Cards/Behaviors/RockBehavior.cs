using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace YuGiOh.Cards
{
    public class RockBehavior : MonsterTypeBehavior
    {
        [Header("Rock Special Ability")]
        [SerializeField] private float earthquakeCost = 35f;
        [SerializeField] private float earthquakeDamage = 40f;
        [SerializeField] private float earthquakeRadius = 8f;
        [SerializeField] private float earthquakeStunDuration = 2f;
        [SerializeField] private GameObject earthquakeEffectPrefab;
        [SerializeField] private AudioClip earthquakeSound;
        
        [Header("Rock Defense")]
        [SerializeField] private float defenseModeHealthBonus = 100f;
        [SerializeField] private float defenseModeSpeedPenalty = 0.5f;
        [SerializeField] private GameObject defenseEffectPrefab;
        [SerializeField] private AudioClip defenseSound;
        
        [Header("Rock Earth Manipulation")]
        [SerializeField] private float earthArmorAmount = 50f;
        [SerializeField] private float earthArmorDuration = 5f;
        [SerializeField] private GameObject earthArmorEffectPrefab;
        [SerializeField] private AudioClip earthArmorSound;
        
        private bool isDefenseModeActive = false;
        private bool isEarthArmorActive = false;
        private float originalSpeed;
        private float originalHealth;
        private List<GameObject> stunnedEnemies = new List<GameObject>();
        
        protected override void Start()
        {
            base.Start();
            originalSpeed = monsterCard.GetMoveSpeed();
            originalHealth = monsterCard.GetMaxHealth();
        }
        
        protected override void ApplySpecialAbilityEffects()
        {
            // Rock's special ability: Earthquake
            CastEarthquake();
        }
        
        protected override void RemoveSpecialAbilityEffects()
        {
            // No cleanup needed for earthquake
        }
        
        private void CastEarthquake()
        {
            // Get target position
            Transform target = monsterCard.GetCurrentTarget();
            if (target == null)
            {
                return;
            }
            
            // Find all enemies in radius
            Collider[] colliders = Physics.OverlapSphere(target.position, earthquakeRadius);
            
            foreach (Collider collider in colliders)
            {
                MonsterCard enemyMonster = collider.GetComponent<MonsterCard>();
                if (enemyMonster != null && enemyMonster.IsEnemy())
                {
                    // Apply damage
                    enemyMonster.TakeDamage(Mathf.RoundToInt(earthquakeDamage));
                    
                    // Stun enemy
                    StunEnemy(enemyMonster);
                    
                    // Spawn earthquake effect
                    if (earthquakeEffectPrefab != null)
                    {
                        GameObject effect = Instantiate(earthquakeEffectPrefab, enemyMonster.transform.position, Quaternion.identity, enemyMonster.transform);
                        Destroy(effect, 2f);
                    }
                }
            }
            
            // Play sound
            if (earthquakeSound != null)
            {
                AudioSource.PlayClipAtPoint(earthquakeSound, target.position);
            }
        }
        
        private void StunEnemy(MonsterCard enemy)
        {
            // Add to stunned list
            stunnedEnemies.Add(enemy.gameObject);
            
            // Disable enemy movement
            enemy.SetMoveSpeed(0);
            
            // Start stun duration
            StartCoroutine(StunDuration(enemy));
        }
        
        private IEnumerator StunDuration(MonsterCard enemy)
        {
            yield return new WaitForSeconds(earthquakeStunDuration);
            
            // Check if enemy is still stunned
            if (stunnedEnemies.Contains(enemy.gameObject))
            {
                // Remove from stunned list
                stunnedEnemies.Remove(enemy.gameObject);
                
                // Restore enemy movement
                enemy.SetMoveSpeed(enemy.GetOriginalMoveSpeed());
            }
        }
        
        public override void OnMonsterSpawned()
        {
            base.OnMonsterSpawned();
            
            // Activate defense mode
            ActivateDefenseMode();
        }
        
        public override void OnMonsterDefeated()
        {
            base.OnMonsterDefeated();
            
            // Deactivate defense mode if active
            if (isDefenseModeActive)
            {
                DeactivateDefenseMode();
            }
            
            // Deactivate earth armor if active
            if (isEarthArmorActive)
            {
                DeactivateEarthArmor();
            }
            
            // Release all stunned enemies
            foreach (GameObject stunnedEnemy in stunnedEnemies)
            {
                if (stunnedEnemy != null)
                {
                    MonsterCard enemyMonster = stunnedEnemy.GetComponent<MonsterCard>();
                    if (enemyMonster != null)
                    {
                        enemyMonster.SetMoveSpeed(enemyMonster.GetOriginalMoveSpeed());
                    }
                }
            }
            
            stunnedEnemies.Clear();
        }
        
        private void ActivateDefenseMode()
        {
            isDefenseModeActive = true;
            
            // Apply defense mode bonuses
            monsterCard.AddHealthBonus(defenseModeHealthBonus);
            monsterCard.SetMoveSpeed(originalSpeed * defenseModeSpeedPenalty);
            
            // Spawn defense effect
            if (defenseEffectPrefab != null)
            {
                GameObject effect = Instantiate(defenseEffectPrefab, transform.position, Quaternion.identity, transform);
                Destroy(effect, 2f);
            }
            
            // Play sound
            if (defenseSound != null)
            {
                AudioSource.PlayClipAtPoint(defenseSound, transform.position);
            }
            
            // Activate earth armor
            ActivateEarthArmor();
        }
        
        private void DeactivateDefenseMode()
        {
            isDefenseModeActive = false;
            
            // Remove defense mode bonuses
            monsterCard.RemoveHealthBonus(defenseModeHealthBonus);
            monsterCard.SetMoveSpeed(originalSpeed);
        }
        
        private void ActivateEarthArmor()
        {
            isEarthArmorActive = true;
            
            // Apply earth armor
            monsterCard.AddHealthBonus(earthArmorAmount);
            
            // Spawn earth armor effect
            if (earthArmorEffectPrefab != null)
            {
                GameObject effect = Instantiate(earthArmorEffectPrefab, transform.position, Quaternion.identity, transform);
                Destroy(effect, 2f);
            }
            
            // Play sound
            if (earthArmorSound != null)
            {
                AudioSource.PlayClipAtPoint(earthArmorSound, transform.position);
            }
            
            // Deactivate earth armor after duration
            StartCoroutine(EarthArmorDuration());
        }
        
        private IEnumerator EarthArmorDuration()
        {
            yield return new WaitForSeconds(earthArmorDuration);
            
            if (isEarthArmorActive)
            {
                DeactivateEarthArmor();
            }
        }
        
        private void DeactivateEarthArmor()
        {
            isEarthArmorActive = false;
            
            // Remove earth armor
            monsterCard.RemoveHealthBonus(earthArmorAmount);
        }
    }
} 