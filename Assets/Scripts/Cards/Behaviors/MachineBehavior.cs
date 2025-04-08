using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace YuGiOh.Cards
{
    public class MachineBehavior : MonsterTypeBehavior
    {
        [Header("Machine Special Ability")]
        [SerializeField] private float upgradeCost = 40f;
        [SerializeField] private float upgradeDamageBonus = 30f;
        [SerializeField] private float upgradeHealthBonus = 50f;
        [SerializeField] private int maxUpgrades = 3;
        [SerializeField] private GameObject upgradeEffectPrefab;
        [SerializeField] private AudioClip upgradeSound;
        
        [Header("Machine Energy System")]
        [SerializeField] private float energyPool = 100f;
        [SerializeField] private float maxEnergyPool = 100f;
        [SerializeField] private float energyRegenRate = 5f;
        [SerializeField] private float energyConsumptionRate = 2f;
        [SerializeField] private GameObject energyEffectPrefab;
        
        [Header("Machine Overdrive")]
        [SerializeField] private float overdriveThreshold = 80f;
        [SerializeField] private float overdriveSpeedBonus = 1.5f;
        [SerializeField] private float overdriveDamageBonus = 1.5f;
        [SerializeField] private GameObject overdriveEffectPrefab;
        [SerializeField] private AudioClip overdriveSound;
        
        private int currentUpgradeLevel = 0;
        private bool isOverdriveActive = false;
        private float originalSpeed;
        private float originalDamage;
        
        protected override void Start()
        {
            base.Start();
            StartCoroutine(RegenerateEnergy());
            originalSpeed = monsterCard.GetMoveSpeed();
            originalDamage = monsterCard.GetAttackDamage();
        }
        
        protected override void ApplySpecialAbilityEffects()
        {
            // Machine's special ability: Self-Upgrade
            if (energyPool >= upgradeCost && currentUpgradeLevel < maxUpgrades)
            {
                ApplyUpgrade();
                energyPool -= upgradeCost;
            }
        }
        
        protected override void RemoveSpecialAbilityEffects()
        {
            // No cleanup needed for upgrades
        }
        
        private void ApplyUpgrade()
        {
            // Increase upgrade level
            currentUpgradeLevel++;
            
            // Apply stat bonuses
            monsterCard.AddDamageBonus(upgradeDamageBonus);
            monsterCard.AddHealthBonus(upgradeHealthBonus);
            
            // Spawn upgrade effect
            if (upgradeEffectPrefab != null)
            {
                GameObject effect = Instantiate(upgradeEffectPrefab, transform.position, Quaternion.identity, transform);
                Destroy(effect, 2f);
            }
            
            // Play sound
            if (upgradeSound != null)
            {
                AudioSource.PlayClipAtPoint(upgradeSound, transform.position);
            }
            
            // Check for overdrive activation
            CheckOverdrive();
        }
        
        private void CheckOverdrive()
        {
            // Activate overdrive if energy is above threshold and not already active
            if (energyPool >= overdriveThreshold && !isOverdriveActive)
            {
                ActivateOverdrive();
            }
            // Deactivate overdrive if energy drops below threshold and is active
            else if (energyPool < overdriveThreshold && isOverdriveActive)
            {
                DeactivateOverdrive();
            }
        }
        
        private void ActivateOverdrive()
        {
            isOverdriveActive = true;
            
            // Apply overdrive bonuses
            monsterCard.SetMoveSpeed(originalSpeed * overdriveSpeedBonus);
            monsterCard.SetAttackDamage(originalDamage * overdriveDamageBonus);
            
            // Spawn overdrive effect
            if (overdriveEffectPrefab != null)
            {
                GameObject effect = Instantiate(overdriveEffectPrefab, transform.position, Quaternion.identity, transform);
                Destroy(effect, 2f);
            }
            
            // Play sound
            if (overdriveSound != null)
            {
                AudioSource.PlayClipAtPoint(overdriveSound, transform.position);
            }
        }
        
        private void DeactivateOverdrive()
        {
            isOverdriveActive = false;
            
            // Remove overdrive bonuses
            monsterCard.SetMoveSpeed(originalSpeed);
            monsterCard.SetAttackDamage(originalDamage);
        }
        
        private IEnumerator RegenerateEnergy()
        {
            while (true)
            {
                // Consume energy if in overdrive
                if (isOverdriveActive)
                {
                    energyPool = Mathf.Max(0, energyPool - energyConsumptionRate * Time.deltaTime);
                    
                    // Check if overdrive should be deactivated
                    if (energyPool < overdriveThreshold)
                    {
                        DeactivateOverdrive();
                    }
                }
                // Regenerate energy if not in overdrive
                else if (energyPool < maxEnergyPool)
                {
                    energyPool = Mathf.Min(maxEnergyPool, energyPool + energyRegenRate * Time.deltaTime);
                    
                    // Check if overdrive should be activated
                    if (energyPool >= overdriveThreshold)
                    {
                        ActivateOverdrive();
                    }
                }
                
                yield return null;
            }
        }
        
        public override void OnMonsterSpawned()
        {
            base.OnMonsterSpawned();
            
            // Spawn energy effect
            if (energyEffectPrefab != null)
            {
                GameObject effect = Instantiate(energyEffectPrefab, transform.position, Quaternion.identity, transform);
                Destroy(effect, 2f);
            }
        }
        
        public override void OnMonsterDefeated()
        {
            base.OnMonsterDefeated();
            
            // Deactivate overdrive if active
            if (isOverdriveActive)
            {
                DeactivateOverdrive();
            }
        }
    }
} 