using UnityEngine;
using System.Collections.Generic;
using YuGiOhTowerDefense.Cards;
using System;

namespace YuGiOhTowerDefense.Core.Duel
{
    public class DuelManager : MonoBehaviour
    {
        [Header("Duel Settings")]
        [SerializeField] private int startingLifePoints = 8000;
        [SerializeField] private int startingHandSize = 5;
        [SerializeField] private int maxDuelPoints = 9999;
        
        [Header("Zones")]
        [SerializeField] private Transform[] monsterZones;
        [SerializeField] private Transform[] spellTrapZones;
        [SerializeField] private Transform fieldZone;
        [SerializeField] private Transform graveyardZone;
        [SerializeField] private Transform deckZone;
        [SerializeField] private Transform extraDeckZone;
        
        private int currentLifePoints;
        private int opponentLifePoints;
        private int playerLifePoints;
        private int currentDuelPoints;
        private int currentTurn;
        private List<TrapCard> activeContinuousTraps = new List<TrapCard>();
        private List<SpellCard> activeContinuousSpells = new List<SpellCard>();
        private List<YuGiOhCard> mainDeck = new List<YuGiOhCard>();
        private List<YuGiOhCard> extraDeck = new List<YuGiOhCard>();
        private List<YuGiOhCard> graveyard = new List<YuGiOhCard>();
        private List<YuGiOhCard> hand = new List<YuGiOhCard>();
        private YuGiOhCard[] fieldMonsters = new YuGiOhCard[5];
        private YuGiOhCard[] fieldSpellTraps = new YuGiOhCard[5];
        private YuGiOhCard fieldSpell;
        
        private bool isPlayerTurn = true;
        private DuelPhase currentPhase = DuelPhase.Draw;
        
        private Action currentAction;
        
        public event System.Action<int> OnLifePointsChanged;
        public event System.Action<DuelPhase> OnPhaseChanged;
        public event System.Action<YuGiOhCard> OnCardDrawn;
        public event System.Action<YuGiOhCard> OnCardPlayed;
        public event System.Action<YuGiOhCard> OnCardActivated;
        public event System.Action<DuelManager> OnDuelEnded;
        public event System.Action<DuelManager> OnTurnEnded;
        public event System.Action<DuelManager> OnCardDestroyed;
        
        private void Start()
        {
            try
            {
                InitializeDuel();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to initialize duel: {e.Message}");
                enabled = false;
            }
        }
        
        private void InitializeDuel()
        {
            if (monsterZones == null || monsterZones.Length == 0)
            {
                throw new System.Exception("Monster zones not set up");
            }
            
            if (spellTrapZones == null || spellTrapZones.Length == 0)
            {
                throw new System.Exception("Spell/Trap zones not set up");
            }
            
            currentLifePoints = startingLifePoints;
            opponentLifePoints = startingLifePoints;
            playerLifePoints = startingLifePoints;
            currentDuelPoints = 0;
            currentTurn = 1;
            OnLifePointsChanged?.Invoke(currentLifePoints);
            
            // TODO: Initialize decks from player's collection
            ShuffleDeck();
            
            // Draw starting hand
            for (int i = 0; i < startingHandSize; i++)
            {
                DrawCard();
            }
            
            StartTurn();
        }
        
        private void StartTurn()
        {
            currentPhase = DuelPhase.Draw;
            OnPhaseChanged?.Invoke(currentPhase);
            
            // Draw Phase
            DrawCard();
            
            // Move to Main Phase 1
            currentPhase = DuelPhase.Main1;
            OnPhaseChanged?.Invoke(currentPhase);
        }
        
        public void EndTurn()
        {
            // End Phase
            currentPhase = DuelPhase.End;
            OnPhaseChanged?.Invoke(currentPhase);
            
            // Update continuous effects
            UpdateContinuousEffects();
            
            // Trigger event
            OnTurnEnded?.Invoke(this);
            
            // Switch turns
            isPlayerTurn = !isPlayerTurn;
            currentTurn++;
            StartTurn();
        }
        
        public void DrawCard()
        {
            if (mainDeck.Count > 0)
            {
                YuGiOhCard card = mainDeck[0];
                mainDeck.RemoveAt(0);
                hand.Add(card);
                OnCardDrawn?.Invoke(card);
            }
        }
        
        public void PlayCard(YuGiOhCard card, int zoneIndex)
        {
            if (card == null)
            {
                Debug.LogError("Cannot play null card");
                return;
            }
            
            if (!hand.Contains(card))
            {
                Debug.LogError("Card not in hand");
                return;
            }
            
            if (zoneIndex < 0 || zoneIndex >= monsterZones.Length)
            {
                Debug.LogError($"Invalid zone index: {zoneIndex}");
                return;
            }
            
            // Create action for the card play
            currentAction = new Action(Action.ActionType.Summon, card);
            
            try
            {
                switch (card.CardType)
                {
                    case CardType.Monster:
                        if (PlayMonsterCard(card, zoneIndex))
                        {
                            OnCardPlayed?.Invoke(card);
                        }
                        break;
                    case CardType.Spell:
                    case CardType.Trap:
                        if (PlaySpellTrapCard(card, zoneIndex))
                        {
                            OnCardPlayed?.Invoke(card);
                        }
                        break;
                    default:
                        Debug.LogError($"Invalid card type: {card.CardType}");
                        break;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to play card: {e.Message}");
            }
            finally
            {
                currentAction = null;
            }
        }
        
        private bool PlayMonsterCard(YuGiOhCard card, int zoneIndex)
        {
            if (zoneIndex < 0 || zoneIndex >= monsterZones.Length)
            {
                Debug.LogError($"Invalid zone index: {zoneIndex}");
                return false;
            }
            
            if (fieldMonsters[zoneIndex] != null)
            {
                Debug.LogError($"Zone {zoneIndex} is already occupied");
                return false;
            }
            
            hand.Remove(card);
            fieldMonsters[zoneIndex] = card;
            
            try
            {
                // Instantiate monster in the zone
                GameObject monster = Instantiate(card.Prefab, monsterZones[zoneIndex]);
                monster.transform.localPosition = Vector3.zero;
                
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to instantiate monster: {e.Message}");
                fieldMonsters[zoneIndex] = null;
                hand.Add(card);
                return false;
            }
        }
        
        private bool PlaySpellTrapCard(YuGiOhCard card, int zoneIndex)
        {
            if (zoneIndex < 0 || zoneIndex >= spellTrapZones.Length)
            {
                Debug.LogError($"Invalid zone index: {zoneIndex}");
                return false;
            }
            
            if (fieldSpellTraps[zoneIndex] != null)
            {
                Debug.LogError($"Zone {zoneIndex} is already occupied");
                return false;
            }
            
            hand.Remove(card);
            fieldSpellTraps[zoneIndex] = card;
            
            try
            {
                // Instantiate spell/trap in the zone
                GameObject spellTrap = Instantiate(card.Prefab, spellTrapZones[zoneIndex]);
                spellTrap.transform.localPosition = Vector3.zero;
                
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to instantiate spell/trap: {e.Message}");
                fieldSpellTraps[zoneIndex] = null;
                hand.Add(card);
                return false;
            }
        }
        
        public void ActivateSpell(int zoneIndex)
        {
            if (zoneIndex < 0 || zoneIndex >= spellTrapZones.Length)
            {
                Debug.LogError($"Invalid zone index: {zoneIndex}");
                return;
            }
            
            YuGiOhCard card = fieldSpellTraps[zoneIndex];
            if (card == null)
            {
                Debug.LogError("No card in specified zone");
                return;
            }
            
            if (card.CardType != CardType.Spell)
            {
                Debug.LogError("Card is not a spell");
                return;
            }
            
            SpellCard spellCard = card as SpellCard;
            if (spellCard == null)
            {
                Debug.LogError("Invalid spell card");
                return;
            }
            
            // Create action for the spell activation
            currentAction = new Action(Action.ActionType.SpellActivation, spellCard);
            
            try
            {
                // Apply spell effect
                ApplySpellEffect(spellCard);
                
                // Send to graveyard if not continuous
                if (spellCard.SpellType != SpellType.Continuous)
                {
                    fieldSpellTraps[zoneIndex] = null;
                    graveyard.Add(card);
                }
                
                // Trigger event
                OnCardActivated?.Invoke(card);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to activate spell: {e.Message}");
            }
            finally
            {
                currentAction = null;
            }
        }
        
        public void ActivateTrap(int zoneIndex)
        {
            if (zoneIndex < 0 || zoneIndex >= spellTrapZones.Length)
            {
                Debug.LogError($"Invalid zone index: {zoneIndex}");
                return;
            }
            
            YuGiOhCard card = fieldSpellTraps[zoneIndex];
            if (card == null)
            {
                Debug.LogError("No card in specified zone");
                return;
            }
            
            if (card.CardType != CardType.Trap)
            {
                Debug.LogError("Card is not a trap");
                return;
            }
            
            TrapCard trapCard = card as TrapCard;
            if (trapCard == null)
            {
                Debug.LogError("Invalid trap card");
                return;
            }
            
            // Check if trap can be activated
            if (!CanActivateTrap(trapCard))
            {
                Debug.LogWarning("Trap cannot be activated");
                return;
            }
            
            // Pay trap cost if any
            if (trapCard.Cost > 0)
            {
                if (!SpendDuelPoints(trapCard.Cost))
                {
                    Debug.LogWarning("Not enough duel points to activate trap");
                    return;
                }
            }
            
            // Create action for the trap activation
            currentAction = new Action(Action.ActionType.TrapActivation, trapCard);
            
            try
            {
                // Activate trap effect
                switch (trapCard.TrapType)
                {
                    case TrapType.Normal:
                        ActivateNormalTrap(trapCard);
                        break;
                    case TrapType.Continuous:
                        ActivateContinuousTrap(trapCard);
                        break;
                    case TrapType.Counter:
                        ActivateCounterTrap(trapCard);
                        break;
                    default:
                        Debug.LogError($"Invalid trap type: {trapCard.TrapType}");
                        break;
                }
                
                // Send to graveyard if not continuous
                if (trapCard.TrapType != TrapType.Continuous)
                {
                    fieldSpellTraps[zoneIndex] = null;
                    graveyard.Add(card);
                }
                
                // Trigger event
                OnCardActivated?.Invoke(card);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to activate trap: {e.Message}");
            }
            finally
            {
                currentAction = null;
            }
        }
        
        private bool CanActivateTrap(TrapCard trapCard)
        {
            // Check if it's the player's turn (unless it's a counter trap)
            if (!isPlayerTurn && trapCard.TrapType != TrapType.Counter) return false;
            
            // Check if trap has been set for at least one turn
            if (trapCard.TurnSet == currentTurn) return false;
            
            // Check if trap can be activated in current phase
            if (!CanActivateInCurrentPhase(trapCard)) return false;
            
            return true;
        }
        
        private bool CanActivateInCurrentPhase(TrapCard trapCard)
        {
            switch (currentPhase)
            {
                case DuelPhase.Draw:
                    return trapCard.CanActivateInDrawPhase;
                case DuelPhase.Standby:
                    return trapCard.CanActivateInStandbyPhase;
                case DuelPhase.Main1:
                    return trapCard.CanActivateInMainPhase;
                case DuelPhase.Battle:
                    return trapCard.CanActivateInBattlePhase;
                case DuelPhase.Main2:
                    return trapCard.CanActivateInMainPhase;
                case DuelPhase.End:
                    return trapCard.CanActivateInEndPhase;
                default:
                    return false;
            }
        }
        
        private void ActivateNormalTrap(TrapCard trapCard)
        {
            // Apply immediate effect
            switch (trapCard.TrapIcon)
            {
                case TrapIcon.Destroy:
                    // Destroy target monster(s)
                    // TODO: Implement monster targeting
                    break;
                case TrapIcon.Negate:
                    // Negate the effect of a card
                    // TODO: Implement effect negation
                    break;
                case TrapIcon.Protect:
                    // Protect target monster(s)
                    // TODO: Implement protection effect
                    break;
                case TrapIcon.Damage:
                    // Deal damage to opponent
                    if (isPlayerTurn)
                    {
                        opponentLifePoints -= (int)trapCard.EffectValue;
                    }
                    else
                    {
                        playerLifePoints -= (int)trapCard.EffectValue;
                    }
                    break;
            }
        }
        
        private void ActivateContinuousTrap(TrapCard trapCard)
        {
            // Add to active continuous traps
            activeContinuousTraps.Add(trapCard);
            
            // Apply continuous effect
            switch (trapCard.TrapIcon)
            {
                case TrapIcon.Continuous:
                    // Apply continuous effect to all monsters
                    foreach (var monster in fieldMonsters)
                    {
                        if (monster != null)
                        {
                            Monster monsterComponent = monster.Prefab.GetComponent<Monster>();
                            if (monsterComponent != null)
                            {
                                monsterComponent.ApplyContinuousEffect(trapCard);
                            }
                        }
                    }
                    break;
                case TrapIcon.Equip:
                    // Attach to target monster
                    Transform target = SelectTargetMonster();
                    if (target != null)
                    {
                        Monster monster = target.GetComponent<Monster>();
                        if (monster != null)
                        {
                            monster.EquipTrap(trapCard);
                        }
                    }
                    break;
            }
        }
        
        private void ActivateCounterTrap(TrapCard trapCard)
        {
            // Counter the current action
            switch (trapCard.TrapIcon)
            {
                case TrapIcon.Negate:
                    // Negate the current action
                    if (currentAction != null)
                    {
                        currentAction.Negate();
                        currentAction = null;
                    }
                    break;
                case TrapIcon.Destroy:
                    // Destroy the card being activated
                    if (currentAction != null)
                    {
                        currentAction.Destroy();
                        currentAction = null;
                    }
                    break;
            }
        }
        
        private void ShuffleDeck()
        {
            // Fisher-Yates shuffle algorithm
            for (int i = mainDeck.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                YuGiOhCard temp = mainDeck[i];
                mainDeck[i] = mainDeck[j];
                mainDeck[j] = temp;
            }
        }

        private Transform SelectTargetMonster()
        {
            // Find all monsters on the field
            List<Transform> monsters = new List<Transform>();
            foreach (var monster in fieldMonsters)
            {
                if (monster != null)
                {
                    monsters.Add(monster.Prefab.transform);
                }
            }
            
            // Select a random target
            if (monsters.Count > 0)
            {
                return monsters[Random.Range(0, monsters.Count)];
            }
            
            return null;
        }

        private void ApplySpellEffect(SpellCard spellCard)
        {
            // Apply spell effect based on spell icon
            switch (spellCard.SpellIcon)
            {
                case SpellIcon.Destroy:
                    // Destroy target monster(s)
                    Transform target = SelectTargetMonster();
                    if (target != null)
                    {
                        Destroy(target.gameObject);
                    }
                    break;
                case SpellIcon.Increase:
                    // Increase monster stats
                    target = SelectTargetMonster();
                    if (target != null)
                    {
                        Monster monster = target.GetComponent<Monster>();
                        if (monster != null)
                        {
                            monster.IncreaseStats(spellCard.EffectValue);
                        }
                    }
                    break;
                case SpellIcon.Decrease:
                    // Decrease monster stats
                    target = SelectTargetMonster();
                    if (target != null)
                    {
                        Monster monster = target.GetComponent<Monster>();
                        if (monster != null)
                        {
                            monster.DecreaseStats(spellCard.EffectValue);
                        }
                    }
                    break;
                case SpellIcon.SpecialSummon:
                    // Special summon monster from hand/deck
                    SpecialSummonMonster(spellCard);
                    break;
                case SpellIcon.Draw:
                    // Draw cards
                    for (int i = 0; i < spellCard.EffectValue; i++)
                    {
                        DrawCard();
                    }
                    break;
                case SpellIcon.LifePoints:
                    // Modify life points
                    currentLifePoints = Mathf.Max(0, currentLifePoints + (int)spellCard.EffectValue);
                    OnLifePointsChanged?.Invoke(currentLifePoints);
                    break;
            }
        }

        private void ApplyTrapEffect(TrapCard trapCard)
        {
            if (trapCard == null) return;
            
            switch (trapCard.TrapType)
            {
                case TrapType.Normal:
                    // Apply immediate effect
                    switch (trapCard.TrapIcon)
                    {
                        case TrapIcon.Destroy:
                            // Destroy target monster(s)
                            Transform target = SelectTargetMonster();
                            if (target != null)
                            {
                                Monster monster = target.GetComponent<Monster>();
                                if (monster != null)
                                {
                                    monster.TakeDamage(trapCard.EffectValue);
                                    if (monster.AttackPoints <= 0 && monster.DefensePoints <= 0)
                                    {
                                        MoveCardToGraveyard(fieldMonsters[Array.IndexOf(fieldMonsters, monster)]);
                                    }
                                }
                            }
                            break;
                        case TrapIcon.Negate:
                            // Negate the effect of a card
                            if (currentAction != null)
                            {
                                currentAction.Negate();
                                currentAction = null;
                            }
                            break;
                        case TrapIcon.Protect:
                            // Protect target monster(s)
                            target = SelectTargetMonster();
                            if (target != null)
                            {
                                Monster monster = target.GetComponent<Monster>();
                                if (monster != null)
                                {
                                    monster.IncreaseStats(trapCard.EffectValue);
                                }
                            }
                            break;
                        case TrapIcon.Damage:
                            // Deal damage to opponent
                            if (isPlayerTurn)
                            {
                                opponentLifePoints = Mathf.Max(0, opponentLifePoints - (int)trapCard.EffectValue);
                                CheckWinCondition();
                            }
                            else
                            {
                                playerLifePoints = Mathf.Max(0, playerLifePoints - (int)trapCard.EffectValue);
                                CheckWinCondition();
                            }
                            break;
                    }
                    break;
                case TrapType.Continuous:
                    // Apply continuous effect
                    activeContinuousTraps.Add(trapCard);
                    foreach (var monster in fieldMonsters)
                    {
                        if (monster != null)
                        {
                            Monster monsterComponent = monster.Prefab.GetComponent<Monster>();
                            if (monsterComponent != null)
                            {
                                monsterComponent.ApplyContinuousEffect(trapCard);
                            }
                        }
                    }
                    break;
                case TrapType.Counter:
                    // Counter opponent's action
                    if (currentAction != null)
                    {
                        switch (trapCard.TrapIcon)
                        {
                            case TrapIcon.Negate:
                                currentAction.Negate();
                                break;
                            case TrapIcon.Destroy:
                                currentAction.Destroy();
                                break;
                        }
                        currentAction = null;
                    }
                    break;
            }
        }

        private void SpecialSummonMonster(SpellCard spellCard)
        {
            // Check if we have space on the field
            for (int i = 0; i < fieldMonsters.Length; i++)
            {
                if (fieldMonsters[i] == null)
                {
                    // Summon monster from hand or deck
                    YuGiOhCard monsterCard = mainDeck.Find(card => card.CardType == CardType.Monster);
                    if (monsterCard != null)
                    {
                        fieldMonsters[i] = monsterCard;
                        mainDeck.Remove(monsterCard);
                        
                        // Instantiate monster in the zone
                        GameObject monster = Instantiate(monsterCard.Prefab, monsterZones[i]);
                        monster.transform.localPosition = Vector3.zero;
                        
                        OnCardPlayed?.Invoke(monsterCard);
                    }
                    break;
                }
            }
        }

        private bool SpendDuelPoints(int amount)
        {
            if (currentDuelPoints >= amount)
            {
                currentDuelPoints -= amount;
                return true;
            }
            return false;
        }

        private void UpdateContinuousEffects()
        {
            // Update continuous trap effects
            foreach (var trap in activeContinuousTraps)
            {
                if (trap != null)
                {
                    // Apply effect to all monsters
                    foreach (var monster in fieldMonsters)
                    {
                        if (monster != null)
                        {
                            Monster monsterComponent = monster.Prefab.GetComponent<Monster>();
                            if (monsterComponent != null)
                            {
                                monsterComponent.UpdateContinuousEffect(trap);
                            }
                        }
                    }
                }
            }
            
            // Update continuous spell effects
            foreach (var spell in activeContinuousSpells)
            {
                if (spell != null)
                {
                    // Apply effect to all monsters
                    foreach (var monster in fieldMonsters)
                    {
                        if (monster != null)
                        {
                            Monster monsterComponent = monster.Prefab.GetComponent<Monster>();
                            if (monsterComponent != null)
                            {
                                monsterComponent.UpdateContinuousEffect(spell);
                            }
                        }
                    }
                }
            }
        }

        private void EndDuel(bool isPlayerWin)
        {
            // Stop all game systems
            enabled = false;
            
            // Trigger event
            OnDuelEnded?.Invoke(this);
            
            // Save player progress if they won
            if (isPlayerWin)
            {
                SavePlayerProgress();
            }
        }

        private void CheckWinCondition()
        {
            if (currentLifePoints <= 0)
            {
                EndDuel(false);
            }
            else if (opponentLifePoints <= 0)
            {
                EndDuel(true);
            }
        }

        private void UpdateLifePoints(int amount)
        {
            currentLifePoints = Mathf.Max(0, currentLifePoints + amount);
            OnLifePointsChanged?.Invoke(currentLifePoints);
            CheckWinCondition();
        }

        private void StartBattlePhase()
        {
            currentPhase = DuelPhase.Battle;
            OnPhaseChanged?.Invoke(currentPhase);
            
            // Reset all monsters' battle state
            foreach (var monster in fieldMonsters)
            {
                if (monster != null)
                {
                    Monster monsterComponent = monster.Prefab.GetComponent<Monster>();
                    if (monsterComponent != null)
                    {
                        monsterComponent.ResetBattleState();
                    }
                }
            }
            
            // Start battle phase coroutine
            StartCoroutine(BattlePhaseCoroutine());
        }

        private System.Collections.IEnumerator BattlePhaseCoroutine()
        {
            // Allow player to declare attacks
            bool isAttacking = true;
            while (isAttacking)
            {
                // Wait for player input or AI decision
                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space) || !isPlayerTurn);
                
                if (!isPlayerTurn)
                {
                    // AI turn - select random monster to attack
                    List<int> availableMonsters = new List<int>();
                    for (int i = 0; i < fieldMonsters.Length; i++)
                    {
                        if (fieldMonsters[i] != null)
                        {
                            availableMonsters.Add(i);
                        }
                    }
                    
                    if (availableMonsters.Count > 0)
                    {
                        int attackerIndex = availableMonsters[Random.Range(0, availableMonsters.Count)];
                        PerformAttack(attackerIndex);
                    }
                }
                
                // Check if battle phase should end
                if (!CanContinueBattlePhase())
                {
                    isAttacking = false;
                }
                
                yield return null;
            }
            
            EndBattlePhase();
        }

        private bool CanContinueBattlePhase()
        {
            // Check if there are any monsters that can attack
            foreach (var monster in fieldMonsters)
            {
                if (monster != null)
                {
                    Monster monsterComponent = monster.Prefab.GetComponent<Monster>();
                    if (monsterComponent != null && monsterComponent.CanAttack)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void PerformAttack(int attackerIndex)
        {
            if (attackerIndex < 0 || attackerIndex >= fieldMonsters.Length) return;
            
            YuGiOhCard attacker = fieldMonsters[attackerIndex];
            if (attacker == null) return;
            
            Monster attackerComponent = attacker.Prefab.GetComponent<Monster>();
            if (attackerComponent == null || !attackerComponent.CanAttack) return;
            
            // Select target
            Transform target = SelectTargetMonster();
            if (target == null)
            {
                // Direct attack
                if (isPlayerTurn)
                {
                    opponentLifePoints = Mathf.Max(0, opponentLifePoints - attackerComponent.AttackPoints);
                }
                else
                {
                    playerLifePoints = Mathf.Max(0, playerLifePoints - attackerComponent.AttackPoints);
                }
                CheckWinCondition();
            }
            
            // Add to graveyard
            graveyard.Add(card);
            
            // Trigger event
            OnCardDestroyed?.Invoke(card);
        }

        private void ResolveCardEffect(YuGiOhCard card)
        {
            if (card == null) return;
            
            switch (card.CardType)
            {
                case CardType.Monster:
                    // TODO: Implement monster effect resolution
                    break;
                case CardType.Spell:
                    ApplySpellEffect(card as SpellCard);
                    break;
                case CardType.Trap:
                    ApplyTrapEffect(card as TrapCard);
                    break;
            }
        }
    }
    
    public enum DuelPhase
    {
        Draw,
        Standby,
        Main1,
        Battle,
        Main2,
        End
    }
} 