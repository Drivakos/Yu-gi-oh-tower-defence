using UnityEngine;
using YuGiOhTowerDefense.Cards;

namespace YuGiOhTowerDefense.Core.Monsters
{
    public class Monster : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private int attackPoints;
        [SerializeField] private int defensePoints;
        [SerializeField] private MonsterType monsterType;
        [SerializeField] private MonsterAttribute attribute;
        
        private int currentAttack;
        private int currentDefense;
        private List<TrapCard> equippedTraps = new List<TrapCard>();
        private List<SpellCard> activeSpells = new List<SpellCard>();
        private List<TrapCard> activeTraps = new List<TrapCard>();
        
        [Header("Battle")]
        [SerializeField] private bool canAttack = true;
        [SerializeField] private bool canBeAttacked = true;
        [SerializeField] private bool isInAttackPosition = true;
        
        public int AttackPoints => currentAttack;
        public int DefensePoints => currentDefense;
        public MonsterType Type => monsterType;
        public MonsterAttribute Attribute => attribute;
        
        public bool CanAttack => canAttack;
        public bool CanBeAttacked => canBeAttacked;
        public bool IsInAttackPosition => isInAttackPosition;
        
        public event System.Action<int, int> OnStatsChanged;
        public event System.Action<Monster> OnDestroyed;
        
        private void Awake()
        {
            if (attackPoints < 0) attackPoints = 0;
            if (defensePoints < 0) defensePoints = 0;
            
            currentAttack = attackPoints;
            currentDefense = defensePoints;
        }
        
        public void ApplyContinuousEffect(TrapCard trapCard)
        {
            if (trapCard == null) return;
            
            int previousAttack = currentAttack;
            int previousDefense = currentDefense;
            
            switch (trapCard.TrapIcon)
            {
                case TrapIcon.Continuous:
                    if (trapCard.EffectValue < 0)
                    {
                        Debug.LogWarning("Negative effect value for continuous trap");
                        return;
                    }
                    currentAttack += (int)trapCard.EffectValue;
                    currentDefense += (int)trapCard.EffectValue;
                    break;
                case TrapIcon.Equip:
                    equippedTraps.Add(trapCard);
                    break;
            }
            
            activeTraps.Add(trapCard);
            
            if (currentAttack != previousAttack || currentDefense != previousDefense)
            {
                OnStatsChanged?.Invoke(currentAttack, currentDefense);
            }
        }
        
        public void ApplyContinuousEffect(SpellCard spellCard)
        {
            if (spellCard == null) return;
            
            int previousAttack = currentAttack;
            int previousDefense = currentDefense;
            
            switch (spellCard.SpellIcon)
            {
                case SpellIcon.Continuous:
                    if (spellCard.EffectValue < 0)
                    {
                        Debug.LogWarning("Negative effect value for continuous spell");
                        return;
                    }
                    currentAttack += (int)spellCard.EffectValue;
                    currentDefense += (int)spellCard.EffectValue;
                    break;
                case SpellIcon.Equip:
                    activeSpells.Add(spellCard);
                    break;
            }
            
            if (currentAttack != previousAttack || currentDefense != previousDefense)
            {
                OnStatsChanged?.Invoke(currentAttack, currentDefense);
            }
        }
        
        public void UpdateContinuousEffect(TrapCard trapCard)
        {
            if (trapCard == null) return;
            
            int previousAttack = currentAttack;
            int previousDefense = currentDefense;
            
            switch (trapCard.TrapIcon)
            {
                case TrapIcon.Continuous:
                    if (trapCard.EffectValue < 0)
                    {
                        Debug.LogWarning("Negative effect value for continuous trap");
                        return;
                    }
                    currentAttack = attackPoints + (int)trapCard.EffectValue;
                    currentDefense = defensePoints + (int)trapCard.EffectValue;
                    break;
            }
            
            if (currentAttack != previousAttack || currentDefense != previousDefense)
            {
                OnStatsChanged?.Invoke(currentAttack, currentDefense);
            }
        }
        
        public void UpdateContinuousEffect(SpellCard spellCard)
        {
            if (spellCard == null) return;
            
            int previousAttack = currentAttack;
            int previousDefense = currentDefense;
            
            switch (spellCard.SpellIcon)
            {
                case SpellIcon.Continuous:
                    if (spellCard.EffectValue < 0)
                    {
                        Debug.LogWarning("Negative effect value for continuous spell");
                        return;
                    }
                    currentAttack = attackPoints + (int)spellCard.EffectValue;
                    currentDefense = defensePoints + (int)spellCard.EffectValue;
                    break;
            }
            
            if (currentAttack != previousAttack || currentDefense != previousDefense)
            {
                OnStatsChanged?.Invoke(currentAttack, currentDefense);
            }
        }
        
        public void EquipTrap(TrapCard trapCard)
        {
            if (trapCard == null) return;
            
            int previousAttack = currentAttack;
            int previousDefense = currentDefense;
            
            equippedTraps.Add(trapCard);
            
            switch (trapCard.TrapIcon)
            {
                case TrapIcon.Equip:
                    if (trapCard.EffectValue < 0)
                    {
                        Debug.LogWarning("Negative effect value for equip trap");
                        return;
                    }
                    currentAttack += (int)trapCard.EffectValue;
                    currentDefense += (int)trapCard.EffectValue;
                    break;
            }
            
            if (currentAttack != previousAttack || currentDefense != previousDefense)
            {
                OnStatsChanged?.Invoke(currentAttack, currentDefense);
            }
        }
        
        public void IncreaseStats(float value)
        {
            if (value < 0)
            {
                Debug.LogWarning("Negative value for stat increase");
                return;
            }
            
            int previousAttack = currentAttack;
            int previousDefense = currentDefense;
            
            currentAttack += (int)value;
            currentDefense += (int)value;
            
            if (currentAttack != previousAttack || currentDefense != previousDefense)
            {
                OnStatsChanged?.Invoke(currentAttack, currentDefense);
            }
        }
        
        public void DecreaseStats(float value)
        {
            if (value < 0)
            {
                Debug.LogWarning("Negative value for stat decrease");
                return;
            }
            
            int previousAttack = currentAttack;
            int previousDefense = currentDefense;
            
            currentAttack = Mathf.Max(0, currentAttack - (int)value);
            currentDefense = Mathf.Max(0, currentDefense - (int)value);
            
            if (currentAttack != previousAttack || currentDefense != previousDefense)
            {
                OnStatsChanged?.Invoke(currentAttack, currentDefense);
            }
        }
        
        public void TakeDamage(float damage)
        {
            if (damage < 0)
            {
                Debug.LogWarning("Negative damage value");
                return;
            }
            
            int previousAttack = currentAttack;
            int previousDefense = currentDefense;
            
            if (isInAttackPosition)
            {
                currentAttack = Mathf.Max(0, currentAttack - (int)damage);
            }
            else
            {
                currentDefense = Mathf.Max(0, currentDefense - (int)damage);
            }
            
            if (currentAttack != previousAttack || currentDefense != previousDefense)
            {
                OnStatsChanged?.Invoke(currentAttack, currentDefense);
            }
            
            if (currentAttack <= 0 && currentDefense <= 0)
            {
                OnDestroyed?.Invoke(this);
                Destroy(gameObject);
            }
        }
        
        public void RemoveContinuousEffects()
        {
            // Reset stats to base values
            currentAttack = attackPoints;
            currentDefense = defensePoints;
            
            // Clear all effects
            equippedTraps.Clear();
            activeSpells.Clear();
            activeTraps.Clear();
        }
        
        public void ResetBattleState()
        {
            canAttack = true;
            canBeAttacked = true;
        }
        
        public void SetAttackPosition(bool isAttack)
        {
            isInAttackPosition = isAttack;
            if (isAttack)
            {
                // Set to attack position
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            else
            {
                // Set to defense position
                transform.rotation = Quaternion.Euler(0, 0, 90);
            }
        }
        
        public void ApplyBattleEffect(Monster target)
        {
            if (target == null) return;
            
            // Apply battle effect based on monster type
            switch (monsterType)
            {
                case MonsterType.Effect:
                    // Apply effect monster battle effect
                    target.TakeDamage(currentAttack);
                    break;
                case MonsterType.Fusion:
                case MonsterType.Synchro:
                case MonsterType.Xyz:
                case MonsterType.Pendulum:
                case MonsterType.Link:
                    // Apply special summon monster battle effect
                    target.TakeDamage(currentAttack * 1.5f);
                    break;
                default:
                    // Normal monster battle effect
                    target.TakeDamage(currentAttack);
                    break;
            }
        }
    }
} 