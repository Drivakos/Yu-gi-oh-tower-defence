using UnityEngine;
using System.Collections.Generic;
using YuGiOhTowerDefense.Cards;
using YuGiOhTowerDefense.Core.Enemy;
using YuGiOhTowerDefense.Core.Player;

namespace YuGiOhTowerDefense.Core.CardEffects
{
    public class CardEffectManager : MonoBehaviour
    {
        [Header("Effect Settings")]
        [SerializeField] private float effectDuration = 2f;
        [SerializeField] private GameObject effectPrefab;
        
        private Dictionary<CardType, System.Action<YuGiOhCard>> effectHandlers;
        private GameManager gameManager;
        
        private void Awake()
        {
            gameManager = FindObjectOfType<GameManager>();
            if (gameManager == null)
            {
                Debug.LogError("GameManager not found!");
                return;
            }
            
            InitializeEffectHandlers();
        }
        
        private void InitializeEffectHandlers()
        {
            effectHandlers = new Dictionary<CardType, System.Action<YuGiOhCard>>()
            {
                { CardType.Monster, HandleMonsterEffect },
                { CardType.Spell, HandleSpellEffect },
                { CardType.Trap, HandleTrapEffect }
            };
        }
        
        public void HandleCardEffect(YuGiOhCard card)
        {
            if (effectHandlers.TryGetValue(card.CardType, out System.Action<YuGiOhCard> handler))
            {
                handler(card);
            }
            else
            {
                Debug.LogWarning($"No effect handler found for card type: {card.CardType}");
            }
        }
        
        private void HandleMonsterEffect(YuGiOhCard card)
        {
            // Spawn monster at target position
            Vector3 spawnPosition = GetTargetPosition();
            GameObject monster = Instantiate(card.Prefab, spawnPosition, Quaternion.identity);
            
            // Initialize monster with card data
            Monster monsterComponent = monster.GetComponent<Monster>();
            if (monsterComponent != null)
            {
                monsterComponent.Initialize(card);
            }
            
            // Play spawn effect
            PlayEffect(spawnPosition);
        }
        
        private void HandleSpellEffect(YuGiOhCard card)
        {
            switch (card.SpellType)
            {
                case SpellType.Damage:
                    HandleDamageSpell(card);
                    break;
                case SpellType.Heal:
                    HandleHealSpell(card);
                    break;
                case SpellType.Buff:
                    HandleBuffSpell(card);
                    break;
                default:
                    Debug.LogWarning($"Unknown spell type: {card.SpellType}");
                    break;
            }
        }
        
        private void HandleTrapEffect(YuGiOhCard card)
        {
            // Set up trap at target position
            Vector3 trapPosition = GetTargetPosition();
            GameObject trap = Instantiate(card.Prefab, trapPosition, Quaternion.identity);
            
            // Initialize trap with card data
            Trap trapComponent = trap.GetComponent<Trap>();
            if (trapComponent != null)
            {
                trapComponent.Initialize(card);
            }
            
            // Play trap set effect
            PlayEffect(trapPosition);
        }
        
        private void HandleDamageSpell(YuGiOhCard card)
        {
            // Find all enemies in range
            Collider[] hitColliders = Physics.OverlapSphere(GetTargetPosition(), card.EffectRange);
            foreach (var hitCollider in hitColliders)
            {
                Enemy enemy = hitCollider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    // Apply damage to enemy
                    enemy.TakeDamage(card.EffectValue);
                    
                    // Play hit effect
                    PlayEffect(enemy.transform.position);
                }
            }
        }
        
        private void HandleHealSpell(YuGiOhCard card)
        {
            // Heal player
            Player player = FindObjectOfType<Player>();
            if (player != null)
            {
                player.Heal(card.EffectValue);
                
                // Play heal effect
                PlayEffect(player.transform.position);
            }
        }
        
        private void HandleBuffSpell(YuGiOhCard card)
        {
            // Find all monsters in range
            Collider[] hitColliders = Physics.OverlapSphere(GetTargetPosition(), card.EffectRange);
            foreach (var hitCollider in hitColliders)
            {
                Monster monster = hitCollider.GetComponent<Monster>();
                if (monster != null)
                {
                    // Apply buff to monster
                    monster.ApplyBuff(card.EffectValue, effectDuration);
                    
                    // Play buff effect
                    PlayEffect(monster.transform.position);
                }
            }
        }
        
        private Vector3 GetTargetPosition()
        {
            // TODO: Implement target position selection
            // For now, return a default position
            return Vector3.zero;
        }
        
        private void PlayEffect(Vector3 position)
        {
            if (effectPrefab != null)
            {
                GameObject effect = Instantiate(effectPrefab, position, Quaternion.identity);
                Destroy(effect, effectDuration);
            }
        }
    }
} 