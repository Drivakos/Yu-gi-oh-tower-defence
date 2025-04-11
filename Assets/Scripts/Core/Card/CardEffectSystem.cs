using UnityEngine;
using System;
using System.Collections.Generic;
using YuGiOhTowerDefense.Core.Enemy;
using YuGiOhTowerDefense.Core.Grid;

namespace YuGiOhTowerDefense.Core.Card
{
    /// <summary>
    /// Handles card effects and their interactions
    /// </summary>
    public class CardEffectSystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GridManager gridManager;
        [SerializeField] private EnemyManager enemyManager;
        
        [Header("Effect Settings")]
        [SerializeField] private LayerMask enemyLayerMask;
        [SerializeField] private float effectRadius = 5f;
        
        private List<CardController> activeCards = new List<CardController>();
        
        public event Action<CardController, EnemyController> OnCardEffectApplied;
        public event Action<CardController> OnCardEffectFailed;
        
        private void OnEnable()
        {
            // Subscribe to card events
            CardController.OnCardSelected += HandleCardSelected;
            CardController.OnCardDeselected += HandleCardDeselected;
        }
        
        private void OnDisable()
        {
            // Unsubscribe from card events
            CardController.OnCardSelected -= HandleCardSelected;
            CardController.OnCardDeselected -= HandleCardDeselected;
        }
        
        private void HandleCardSelected(CardController card)
        {
            if (!activeCards.Contains(card))
            {
                activeCards.Add(card);
            }
        }
        
        private void HandleCardDeselected(CardController card)
        {
            activeCards.Remove(card);
        }
        
        public void ApplyCardEffect(CardController card, Vector3 targetPosition)
        {
            if (card == null)
            {
                Debug.LogError("Cannot apply effect from null card");
                return;
            }
            
            switch (card.CardData)
            {
                case MonsterCardData monsterCard:
                    ApplyMonsterEffect(monsterCard, card, targetPosition);
                    break;
                    
                case SpellCardData spellCard:
                    ApplySpellEffect(spellCard, card, targetPosition);
                    break;
                    
                case TrapCardData trapCard:
                    ApplyTrapEffect(trapCard, card, targetPosition);
                    break;
                    
                default:
                    Debug.LogError($"Unknown card type: {card.CardData.GetType()}");
                    break;
            }
        }
        
        private void ApplyMonsterEffect(MonsterCardData monsterCard, CardController card, Vector3 targetPosition)
        {
            // Find enemies in range
            Collider[] hitColliders = Physics.OverlapSphere(targetPosition, effectRadius, enemyLayerMask);
            
            if (hitColliders.Length == 0)
            {
                OnCardEffectFailed?.Invoke(card);
                return;
            }
            
            // Apply effect to each enemy in range
            foreach (Collider hitCollider in hitColliders)
            {
                EnemyController enemy = hitCollider.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    // Apply damage based on monster's attack
                    enemy.TakeDamage(monsterCard.Attack);
                    OnCardEffectApplied?.Invoke(card, enemy);
                }
            }
        }
        
        private void ApplySpellEffect(SpellCardData spellCard, CardController card, Vector3 targetPosition)
        {
            switch (spellCard.SpellType)
            {
                case SpellType.Normal:
                    ApplyNormalSpellEffect(spellCard, card, targetPosition);
                    break;
                    
                case SpellType.Continuous:
                    ApplyContinuousSpellEffect(spellCard, card, targetPosition);
                    break;
                    
                case SpellType.QuickPlay:
                    ApplyQuickPlaySpellEffect(spellCard, card, targetPosition);
                    break;
                    
                case SpellType.Field:
                    ApplyFieldSpellEffect(spellCard, card, targetPosition);
                    break;
                    
                case SpellType.Equip:
                    ApplyEquipSpellEffect(spellCard, card, targetPosition);
                    break;
                    
                case SpellType.Ritual:
                    ApplyRitualSpellEffect(spellCard, card, targetPosition);
                    break;
                    
                default:
                    Debug.LogError($"Unknown spell type: {spellCard.SpellType}");
                    break;
            }
        }
        
        private void ApplyTrapEffect(TrapCardData trapCard, CardController card, Vector3 targetPosition)
        {
            switch (trapCard.TrapType)
            {
                case TrapType.Normal:
                    ApplyNormalTrapEffect(trapCard, card, targetPosition);
                    break;
                    
                case TrapType.Continuous:
                    ApplyContinuousTrapEffect(trapCard, card, targetPosition);
                    break;
                    
                case TrapType.Counter:
                    ApplyCounterTrapEffect(trapCard, card, targetPosition);
                    break;
                    
                default:
                    Debug.LogError($"Unknown trap type: {trapCard.TrapType}");
                    break;
            }
        }
        
        private void ApplyNormalSpellEffect(SpellCardData spellCard, CardController card, Vector3 targetPosition)
        {
            // Find enemies in range
            Collider[] hitColliders = Physics.OverlapSphere(targetPosition, effectRadius, enemyLayerMask);
            
            if (hitColliders.Length == 0)
            {
                OnCardEffectFailed?.Invoke(card);
                return;
            }
            
            // Apply effect to each enemy in range
            foreach (Collider hitCollider in hitColliders)
            {
                EnemyController enemy = hitCollider.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    // Apply spell effect based on spell type
                    switch (spellCard.SpellIcon)
                    {
                        case SpellIcon.Destroy:
                            enemy.TakeDamage(enemy.CurrentHealth); // Instant kill
                            break;
                            
                        case SpellIcon.Damage:
                            enemy.TakeDamage(spellCard.EffectValue);
                            break;
                            
                        case SpellIcon.Modify:
                            // Apply stat modification
                            // This will be implemented when we have stat modification system
                            break;
                            
                        default:
                            Debug.LogWarning($"Unhandled spell icon: {spellCard.SpellIcon}");
                            break;
                    }
                    
                    OnCardEffectApplied?.Invoke(card, enemy);
                }
            }
        }
        
        private void ApplyContinuousSpellEffect(SpellCardData spellCard, CardController card, Vector3 targetPosition)
        {
            // Create a continuous effect zone
            // This will be implemented when we have continuous effect system
        }
        
        private void ApplyQuickPlaySpellEffect(SpellCardData spellCard, CardController card, Vector3 targetPosition)
        {
            // Similar to normal spell but with different timing rules
            ApplyNormalSpellEffect(spellCard, card, targetPosition);
        }
        
        private void ApplyFieldSpellEffect(SpellCardData spellCard, CardController card, Vector3 targetPosition)
        {
            // Create a field effect zone
            // This will be implemented when we have field effect system
        }
        
        private void ApplyEquipSpellEffect(SpellCardData spellCard, CardController card, Vector3 targetPosition)
        {
            // Find monster to equip
            // This will be implemented when we have monster targeting system
        }
        
        private void ApplyRitualSpellEffect(SpellCardData spellCard, CardController card, Vector3 targetPosition)
        {
            // Handle ritual summoning
            // This will be implemented when we have ritual summoning system
        }
        
        private void ApplyNormalTrapEffect(TrapCardData trapCard, CardController card, Vector3 targetPosition)
        {
            // Similar to normal spell but with different timing rules
            ApplyNormalSpellEffect(new SpellCardData
            {
                SpellType = SpellType.Normal,
                SpellIcon = (SpellIcon)trapCard.TrapIcon,
                EffectValue = trapCard.EffectValue
            }, card, targetPosition);
        }
        
        private void ApplyContinuousTrapEffect(TrapCardData trapCard, CardController card, Vector3 targetPosition)
        {
            // Create a continuous trap zone
            // This will be implemented when we have continuous trap system
        }
        
        private void ApplyCounterTrapEffect(TrapCardData trapCard, CardController card, Vector3 targetPosition)
        {
            // Handle counter trap effects
            // This will be implemented when we have counter trap system
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, effectRadius);
        }
    }
} 