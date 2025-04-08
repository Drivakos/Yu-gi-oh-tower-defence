using UnityEngine;
using System.Collections;

namespace YuGiOh.Cards
{
    public abstract class MonsterTypeBehavior : MonoBehaviour
    {
        [Header("Type Settings")]
        [SerializeField] protected MonsterType monsterType;
        [SerializeField] protected float specialAbilityCooldown = 10f;
        [SerializeField] protected float specialAbilityDuration = 5f;
        
        protected MonsterCard monsterCard;
        protected bool isSpecialAbilityActive;
        protected float lastSpecialAbilityTime;
        
        protected virtual void Start()
        {
            monsterCard = GetComponent<MonsterCard>();
            if (monsterCard == null)
            {
                Debug.LogError($"MonsterTypeBehavior requires a MonsterCard component on {gameObject.name}");
                enabled = false;
                return;
            }
            
            // Verify monster type matches
            if (monsterCard.GetMonsterType() != monsterType)
            {
                Debug.LogError($"Monster type mismatch on {gameObject.name}. Expected {monsterType} but got {monsterCard.GetMonsterType()}");
                enabled = false;
                return;
            }
        }
        
        protected virtual void Update()
        {
            // Check for special ability activation
            if (Input.GetKeyDown(KeyCode.Q) && CanActivateSpecialAbility())
            {
                ActivateSpecialAbility();
            }
        }
        
        protected virtual bool CanActivateSpecialAbility()
        {
            return !isSpecialAbilityActive && Time.time >= lastSpecialAbilityTime + specialAbilityCooldown;
        }
        
        protected virtual void ActivateSpecialAbility()
        {
            if (!CanActivateSpecialAbility())
            {
                return;
            }
            
            lastSpecialAbilityTime = Time.time;
            isSpecialAbilityActive = true;
            
            StartCoroutine(SpecialAbilityCoroutine());
        }
        
        protected virtual IEnumerator SpecialAbilityCoroutine()
        {
            // Apply special ability effects
            ApplySpecialAbilityEffects();
            
            // Wait for duration
            yield return new WaitForSeconds(specialAbilityDuration);
            
            // Remove special ability effects
            RemoveSpecialAbilityEffects();
            
            isSpecialAbilityActive = false;
        }
        
        protected virtual void ApplySpecialAbilityEffects()
        {
            // Override in derived classes
        }
        
        protected virtual void RemoveSpecialAbilityEffects()
        {
            // Override in derived classes
        }
        
        public virtual void OnMonsterDefeated()
        {
            // Override in derived classes if needed
        }
        
        public virtual void OnMonsterSpawned()
        {
            // Override in derived classes if needed
        }
        
        public MonsterType GetMonsterType() => monsterType;
    }
} 