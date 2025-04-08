using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using YuGiOhTowerDefense.Core;

namespace YuGiOhTowerDefense.Core
{
    public class BossEnemy : MonoBehaviour
    {
        [Header("Boss Settings")]
        [SerializeField] private string bossId;
        [SerializeField] private string bossName;
        [SerializeField] private float maxHealth = 1000f;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float attackRange = 5f;
        [SerializeField] private float attackCooldown = 2f;
        
        [Header("Card Usage")]
        [SerializeField] private int maxHandSize = 5;
        [SerializeField] private float cardUseInterval = 3f;
        [SerializeField] private float cardPowerThreshold = 500f;
        
        [Header("Special Abilities")]
        [SerializeField] private float specialAbilityCooldown = 10f;
        [SerializeField] private float specialAbilityRange = 8f;
        [SerializeField] private float specialAbilityDamage = 50f;
        
        private CardPoolManager cardPoolManager;
        private List<YuGiOhCard> currentHand = new List<YuGiOhCard>();
        private float currentHealth;
        private float lastAttackTime;
        private float lastCardUseTime;
        private float lastSpecialAbilityTime;
        private bool isDead;
        
        private void Awake()
        {
            cardPoolManager = GetComponent<CardPoolManager>();
            if (cardPoolManager == null)
            {
                Debug.LogError("CardPoolManager not found!");
            }
            
            currentHealth = maxHealth;
        }
        
        private void Start()
        {
            // Initialize boss hand
            DrawInitialHand();
            
            // Start card usage coroutine
            StartCoroutine(UseCardsRoutine());
        }
        
        private void DrawInitialHand()
        {
            if (cardPoolManager == null)
            {
                return;
            }
            
            // Generate boss cards
            var cards = cardPoolManager.GenerateBossCards(maxHandSize);
            currentHand.AddRange(cards);
        }
        
        private IEnumerator UseCardsRoutine()
        {
            while (!isDead)
            {
                // Check if it's time to use a card
                if (Time.time >= lastCardUseTime + cardUseInterval)
                {
                    UseRandomCard();
                    lastCardUseTime = Time.time;
                }
                
                yield return new WaitForSeconds(0.5f);
            }
        }
        
        private void UseRandomCard()
        {
            if (currentHand.Count == 0)
            {
                return;
            }
            
            // Select a random card from hand
            int randomIndex = Random.Range(0, currentHand.Count);
            YuGiOhCard selectedCard = currentHand[randomIndex];
            
            // Use the card based on its type
            if (selectedCard.IsMonster())
            {
                UseMonsterCard(selectedCard);
            }
            else if (selectedCard.IsSpell())
            {
                UseSpellCard(selectedCard);
            }
            else if (selectedCard.IsTrap())
            {
                UseTrapCard(selectedCard);
            }
            
            // Remove used card from hand
            currentHand.RemoveAt(randomIndex);
            
            // Draw a new card if hand is not full
            if (currentHand.Count < maxHandSize)
            {
                var newCards = cardPoolManager.GenerateBossCards(1);
                if (newCards.Count > 0)
                {
                    currentHand.Add(newCards[0]);
                }
            }
        }
        
        private void UseMonsterCard(YuGiOhCard card)
        {
            // Spawn monster and apply its stats
            GameObject monsterObj = new GameObject($"BossMonster_{card.name}");
            monsterObj.transform.position = transform.position;
            
            // Add monster component and set stats
            MonsterCard monsterCard = monsterObj.AddComponent<MonsterCard>();
            monsterCard.Initialize(card);
            
            // Apply boss-specific bonuses
            monsterCard.SetStats(
                card.attack * 1.5f,
                card.defense * 1.5f,
                card.attackSpeed * 0.8f,
                card.range * 1.2f
            );
        }
        
        private void UseSpellCard(YuGiOhCard card)
        {
            // Apply spell effects
            switch (card.name.ToLower())
            {
                case "raigeki":
                    // Destroy all player monsters
                    break;
                case "dark hole":
                    // Destroy all monsters
                    break;
                case "monster reborn":
                    // Revive a random monster from graveyard
                    break;
                default:
                    // Apply generic spell effect
                    break;
            }
        }
        
        private void UseTrapCard(YuGiOhCard card)
        {
            // Set up trap effect
            switch (card.name.ToLower())
            {
                case "mirror force":
                    // Reflect damage
                    break;
                case "magic cylinder":
                    // Absorb and redirect damage
                    break;
                default:
                    // Apply generic trap effect
                    break;
            }
        }
        
        public void TakeDamage(float damage)
        {
            if (isDead)
            {
                return;
            }
            
            currentHealth -= damage;
            
            // Check for death
            if (currentHealth <= 0)
            {
                Die();
            }
            // Check for special ability trigger
            else if (currentHealth <= maxHealth * 0.3f && Time.time >= lastSpecialAbilityTime + specialAbilityCooldown)
            {
                UseSpecialAbility();
            }
        }
        
        private void UseSpecialAbility()
        {
            lastSpecialAbilityTime = Time.time;
            
            // Find all player monsters in range
            Collider[] colliders = Physics.OverlapSphere(transform.position, specialAbilityRange);
            foreach (var collider in colliders)
            {
                MonsterCard monster = collider.GetComponent<MonsterCard>();
                if (monster != null && monster.IsPlayerMonster())
                {
                    monster.TakeDamage(specialAbilityDamage);
                }
            }
            
            // Visual effect
            // TODO: Add particle system or other visual feedback
        }
        
        private void Die()
        {
            isDead = true;
            
            // Stop all coroutines
            StopAllCoroutines();
            
            // Generate rewards
            if (cardPoolManager != null)
            {
                var rewards = cardPoolManager.GenerateBossRewards(bossId);
                // Rewards will be handled by BossRewardUI
            }
            
            // Destroy boss
            Destroy(gameObject);
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
            
            // Draw special ability range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, specialAbilityRange);
        }
    }
} 