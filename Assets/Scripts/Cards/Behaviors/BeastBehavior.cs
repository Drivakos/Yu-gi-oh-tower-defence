using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace YuGiOh.Cards
{
    public class BeastBehavior : MonsterTypeBehavior
    {
        [Header("Beast Special Ability")]
        [SerializeField] private float howlCost = 25f;
        [SerializeField] private float howlDamageBonus = 20f;
        [SerializeField] private float howlSpeedBonus = 1.3f;
        [SerializeField] private float howlDuration = 8f;
        [SerializeField] private GameObject howlEffectPrefab;
        [SerializeField] private AudioClip howlSound;
        
        [Header("Beast Pack Tactics")]
        [SerializeField] private float packRange = 10f;
        [SerializeField] private float packDamageBonus = 0.2f;
        [SerializeField] private float packSpeedBonus = 0.1f;
        [SerializeField] private GameObject packEffectPrefab;
        
        [Header("Beast Primal Instinct")]
        [SerializeField] private float lowHealthThreshold = 0.3f;
        [SerializeField] private float primalDamageBonus = 1.5f;
        [SerializeField] private float primalSpeedBonus = 1.3f;
        [SerializeField] private GameObject primalEffectPrefab;
        [SerializeField] private AudioClip primalSound;
        
        private bool isHowlActive = false;
        private bool isPrimalActive = false;
        private float originalSpeed;
        private float originalDamage;
        private List<GameObject> packMembers = new List<GameObject>();
        
        protected override void Start()
        {
            base.Start();
            originalSpeed = monsterCard.GetMoveSpeed();
            originalDamage = monsterCard.GetAttackDamage();
        }
        
        protected override void ApplySpecialAbilityEffects()
        {
            // Beast's special ability: Howl
            if (!isHowlActive)
            {
                ActivateHowl();
            }
        }
        
        protected override void RemoveSpecialAbilityEffects()
        {
            // Deactivate howl if active
            if (isHowlActive)
            {
                DeactivateHowl();
            }
        }
        
        private void ActivateHowl()
        {
            isHowlActive = true;
            
            // Apply howl bonuses
            monsterCard.AddDamageBonus(howlDamageBonus);
            monsterCard.SetMoveSpeed(originalSpeed * howlSpeedBonus);
            
            // Find all friendly beasts in range
            Collider[] colliders = Physics.OverlapSphere(transform.position, packRange);
            
            foreach (Collider collider in colliders)
            {
                MonsterCard otherMonster = collider.GetComponent<MonsterCard>();
                if (otherMonster != null && otherMonster != monsterCard && !otherMonster.IsEnemy())
                {
                    // Apply howl bonuses to other beasts
                    BeastBehavior otherBeast = otherMonster.GetComponent<BeastBehavior>();
                    if (otherBeast != null)
                    {
                        otherBeast.ReceiveHowlBonus(howlDamageBonus, howlSpeedBonus);
                    }
                }
            }
            
            // Spawn howl effect
            if (howlEffectPrefab != null)
            {
                GameObject effect = Instantiate(howlEffectPrefab, transform.position, Quaternion.identity, transform);
                Destroy(effect, 2f);
            }
            
            // Play sound
            if (howlSound != null)
            {
                AudioSource.PlayClipAtPoint(howlSound, transform.position);
            }
            
            // Deactivate howl after duration
            StartCoroutine(HowlDuration());
        }
        
        private IEnumerator HowlDuration()
        {
            yield return new WaitForSeconds(howlDuration);
            
            if (isHowlActive)
            {
                DeactivateHowl();
            }
        }
        
        private void DeactivateHowl()
        {
            isHowlActive = false;
            
            // Remove howl bonuses
            monsterCard.RemoveDamageBonus(howlDamageBonus);
            monsterCard.SetMoveSpeed(originalSpeed);
            
            // Remove howl bonuses from other beasts
            foreach (GameObject packMember in packMembers)
            {
                if (packMember != null)
                {
                    BeastBehavior otherBeast = packMember.GetComponent<BeastBehavior>();
                    if (otherBeast != null)
                    {
                        otherBeast.RemoveHowlBonus(howlDamageBonus, howlSpeedBonus);
                    }
                }
            }
        }
        
        public void ReceiveHowlBonus(float damageBonus, float speedBonus)
        {
            monsterCard.AddDamageBonus(damageBonus);
            monsterCard.SetMoveSpeed(originalSpeed * speedBonus);
        }
        
        public void RemoveHowlBonus(float damageBonus, float speedBonus)
        {
            monsterCard.RemoveDamageBonus(damageBonus);
            monsterCard.SetMoveSpeed(originalSpeed);
        }
        
        public override void OnMonsterSpawned()
        {
            base.OnMonsterSpawned();
            
            // Update pack members
            UpdatePackMembers();
            
            // Check for primal instinct
            CheckPrimalInstinct();
        }
        
        public override void OnMonsterDefeated()
        {
            base.OnMonsterDefeated();
            
            // Deactivate howl if active
            if (isHowlActive)
            {
                DeactivateHowl();
            }
            
            // Deactivate primal instinct if active
            if (isPrimalActive)
            {
                DeactivatePrimalInstinct();
            }
            
            // Remove from pack
            packMembers.Remove(gameObject);
        }
        
        private void UpdatePackMembers()
        {
            // Clear old pack members
            packMembers.Clear();
            
            // Find all friendly beasts in range
            Collider[] colliders = Physics.OverlapSphere(transform.position, packRange);
            
            foreach (Collider collider in colliders)
            {
                MonsterCard otherMonster = collider.GetComponent<MonsterCard>();
                if (otherMonster != null && otherMonster != monsterCard && !otherMonster.IsEnemy())
                {
                    BeastBehavior otherBeast = otherMonster.GetComponent<BeastBehavior>();
                    if (otherBeast != null)
                    {
                        // Add to pack members
                        packMembers.Add(otherMonster.gameObject);
                        
                        // Apply pack bonuses
                        float totalDamageBonus = packDamageBonus * packMembers.Count;
                        float totalSpeedBonus = 1f + (packSpeedBonus * packMembers.Count);
                        
                        monsterCard.AddDamageBonus(totalDamageBonus);
                        monsterCard.SetMoveSpeed(originalSpeed * totalSpeedBonus);
                        
                        // Spawn pack effect
                        if (packEffectPrefab != null)
                        {
                            GameObject effect = Instantiate(packEffectPrefab, transform.position, Quaternion.identity, transform);
                            Destroy(effect, 2f);
                        }
                    }
                }
            }
        }
        
        private void CheckPrimalInstinct()
        {
            // Check if health is below threshold
            float healthPercent = monsterCard.GetCurrentHealth() / monsterCard.GetMaxHealth();
            
            if (healthPercent <= lowHealthThreshold && !isPrimalActive)
            {
                ActivatePrimalInstinct();
            }
            else if (healthPercent > lowHealthThreshold && isPrimalActive)
            {
                DeactivatePrimalInstinct();
            }
        }
        
        private void ActivatePrimalInstinct()
        {
            isPrimalActive = true;
            
            // Apply primal bonuses
            monsterCard.SetAttackDamage(originalDamage * primalDamageBonus);
            monsterCard.SetMoveSpeed(originalSpeed * primalSpeedBonus);
            
            // Spawn primal effect
            if (primalEffectPrefab != null)
            {
                GameObject effect = Instantiate(primalEffectPrefab, transform.position, Quaternion.identity, transform);
                Destroy(effect, 2f);
            }
            
            // Play sound
            if (primalSound != null)
            {
                AudioSource.PlayClipAtPoint(primalSound, transform.position);
            }
        }
        
        private void DeactivatePrimalInstinct()
        {
            isPrimalActive = false;
            
            // Remove primal bonuses
            monsterCard.SetAttackDamage(originalDamage);
            monsterCard.SetMoveSpeed(originalSpeed);
        }
        
        private void Update()
        {
            // Check for primal instinct
            CheckPrimalInstinct();
            
            // Update pack members
            UpdatePackMembers();
        }
    }
} 