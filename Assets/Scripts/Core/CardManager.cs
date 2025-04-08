using UnityEngine;
using System.Collections.Generic;
using YuGiOhTowerDefense.UI;
using System;

namespace YuGiOhTowerDefense.Core
{
    public class CardManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CardGenerator cardGenerator;
        [SerializeField] private MobileHandUI handUI;
        [SerializeField] private CardDetailsUI cardDetailsUI;
        [SerializeField] private DuelPointsUI duelPointsUI;
        [SerializeField] private CardFactory cardFactory;
        
        [Header("Game Settings")]
        [SerializeField] private int startingDuelPoints = 1000;
        [SerializeField] private int maxDuelPoints = 2000;
        [SerializeField] private float duelPointsPerSecond = 10f;
        
        [Header("Card Settings")]
        [SerializeField] private float cardPlayCooldown = 1f;
        [SerializeField] private float cardPlacementRadius = 2f;
        [SerializeField] private LayerMask placementLayer;
        
        private int currentDuelPoints;
        private float duelPointsTimer;
        private Dictionary<string, float> cardCooldowns = new Dictionary<string, float>();
        private List<YuGiOhCard> activeCards = new List<YuGiOhCard>();
        private YuGiOhCard selectedCard;
        private CardManagerState currentState;
        private Dictionary<CardManagerState, ICardManagerState> states;
        
        public event Action<YuGiOhCard> OnCardPlayed;
        public event Action<int> OnDuelPointsChanged;
        
        private void Awake()
        {
            InitializeComponents();
            InitializeStateMachine();
        }
        
        private void InitializeComponents()
        {
            if (cardGenerator == null)
            {
                cardGenerator = GetComponent<CardGenerator>();
            }
            
            if (handUI == null)
            {
                handUI = FindObjectOfType<MobileHandUI>();
            }
            
            if (cardDetailsUI == null)
            {
                cardDetailsUI = FindObjectOfType<CardDetailsUI>();
            }
            
            if (duelPointsUI == null)
            {
                duelPointsUI = FindObjectOfType<DuelPointsUI>();
            }
            
            if (cardFactory == null)
            {
                cardFactory = FindObjectOfType<CardFactory>();
                if (cardFactory == null)
                {
                    Debug.LogError("CardManager: CardFactory not found!");
                }
            }
            
            currentDuelPoints = startingDuelPoints;
        }
        
        private void InitializeStateMachine()
        {
            states = new Dictionary<CardManagerState, ICardManagerState>
            {
                { CardManagerState.Idle, new CardManagerIdleState(this) },
                { CardManagerState.PlacingCard, new CardManagerPlacingState(this) }
            };
            
            SetState(CardManagerState.Idle);
        }
        
        private void Start()
        {
            if (handUI != null)
            {
                handUI.OnCardSelected += HandleCardSelected;
            }
            
            if (cardDetailsUI != null)
            {
                cardDetailsUI.OnCardPlayed += HandleCardPlayed;
            }
            
            UpdateDuelPointsUI();
        }
        
        private void OnDestroy()
        {
            if (handUI != null)
            {
                handUI.OnCardSelected -= HandleCardSelected;
            }
            
            if (cardDetailsUI != null)
            {
                cardDetailsUI.OnCardPlayed -= HandleCardPlayed;
            }
        }
        
        private void Update()
        {
            if (currentState == null)
            {
                return;
            }
            
            currentState.Update();
            UpdateDuelPoints();
            UpdateCardCooldowns();
        }
        
        public void SetState(CardManagerState newState)
        {
            if (currentState != null)
            {
                currentState.Exit();
            }
            
            currentState = states[newState];
            currentState.Enter();
        }
        
        private void UpdateDuelPoints()
        {
            duelPointsTimer += Time.deltaTime;
            
            if (duelPointsTimer >= 1f)
            {
                duelPointsTimer = 0f;
                int previousPoints = currentDuelPoints;
                currentDuelPoints = Mathf.Min(currentDuelPoints + Mathf.RoundToInt(duelPointsPerSecond), maxDuelPoints);
                
                if (currentDuelPoints != previousPoints)
                {
                    OnDuelPointsChanged?.Invoke(currentDuelPoints - previousPoints);
                    UpdateDuelPointsUI();
                }
            }
        }
        
        private void UpdateCardCooldowns()
        {
            List<string> finishedCooldowns = new List<string>();
            
            foreach (var cooldown in cardCooldowns)
            {
                cardCooldowns[cooldown.Key] -= Time.deltaTime;
                
                if (cardCooldowns[cooldown.Key] <= 0f)
                {
                    finishedCooldowns.Add(cooldown.Key);
                }
            }
            
            foreach (var cardId in finishedCooldowns)
            {
                cardCooldowns.Remove(cardId);
            }
        }
        
        private void HandleCardSelected(YuGiOhCard card)
        {
            if (card == null)
            {
                return;
            }
            
            selectedCard = card;
            
            if (cardDetailsUI != null)
            {
                cardDetailsUI.ShowCardDetails(card);
            }
        }
        
        private void HandleCardPlayed(YuGiOhCard card)
        {
            if (card == null)
            {
                return;
            }
            
            try
            {
                ICardCommand command = CreateCardCommand(card);
                if (command != null)
                {
                    command.Execute();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error executing card command: {e.Message}");
            }
        }
        
        private ICardCommand CreateCardCommand(YuGiOhCard card)
        {
            if (cardCooldowns.ContainsKey(card.id))
            {
                Debug.Log($"Card {card.name} is on cooldown!");
                return null;
            }
            
            if (currentDuelPoints < card.cost)
            {
                Debug.Log($"Not enough duel points to play {card.name}!");
                
                if (duelPointsUI != null)
                {
                    duelPointsUI.ShowInsufficientPointsWarning();
                }
                
                return null;
            }
            
            if (card.IsMonster())
            {
                return new PlayMonsterCommand(this, card);
            }
            else if (card.IsSpell())
            {
                return new PlaySpellCommand(this, card);
            }
            else if (card.IsTrap())
            {
                return new PlayTrapCommand(this, card);
            }
            
            return null;
        }
        
        public void PlaceCard(Vector3 position)
        {
            if (selectedCard == null || cardFactory == null)
            {
                return;
            }
            
            if (!IsValidPlacementPosition(position))
            {
                Debug.Log("Invalid placement position!");
                return;
            }
            
            GameObject cardObject = cardFactory.CreateCard(selectedCard, position);
            
            if (cardObject != null)
            {
                activeCards.Add(selectedCard);
                cardCooldowns[selectedCard.id] = cardPlayCooldown;
                currentDuelPoints -= selectedCard.cost;
                
                OnCardPlayed?.Invoke(selectedCard);
                UpdateDuelPointsUI();
                
                SetState(CardManagerState.Idle);
            }
        }
        
        private bool IsValidPlacementPosition(Vector3 position)
        {
            if (Vector3.Distance(position, Vector3.zero) > cardPlacementRadius)
            {
                return false;
            }
            
            Collider[] colliders = Physics.OverlapSphere(position, 1f);
            
            foreach (Collider collider in colliders)
            {
                if (collider.GetComponent<MonsterCard>() != null)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        private void UpdateDuelPointsUI()
        {
            if (duelPointsUI != null)
            {
                duelPointsUI.UpdateDuelPointsDisplay(currentDuelPoints);
            }
        }
        
        public bool CanPlayCard(YuGiOhCard card)
        {
            if (card == null)
            {
                return false;
            }
            
            return !cardCooldowns.ContainsKey(card.id) && currentDuelPoints >= card.cost;
        }
        
        public void RemoveCard(YuGiOhCard card)
        {
            if (card == null)
            {
                return;
            }
            
            activeCards.Remove(card);
            
            // Find the card object in the scene and return it to the pool
            MonsterCard[] monsterCards = FindObjectsOfType<MonsterCard>();
            foreach (MonsterCard monsterCard in monsterCards)
            {
                if (monsterCard.GetCardData() == card)
                {
                    if (cardFactory != null)
                    {
                        cardFactory.ReturnCardToPool(monsterCard.gameObject);
                    }
                    else
                    {
                        Destroy(monsterCard.gameObject);
                    }
                    break;
                }
            }
        }
        
        public List<YuGiOhCard> GetActiveCards()
        {
            return new List<YuGiOhCard>(activeCards);
        }
        
        public int GetCurrentDuelPoints()
        {
            return currentDuelPoints;
        }
        
        public void AddDuelPoints(int points)
        {
            int previousPoints = currentDuelPoints;
            currentDuelPoints = Mathf.Min(currentDuelPoints + points, maxDuelPoints);
            
            OnDuelPointsChanged?.Invoke(currentDuelPoints - previousPoints);
            UpdateDuelPointsUI();
            
            if (duelPointsUI != null)
            {
                duelPointsUI.ShowPointsChange(currentDuelPoints - previousPoints);
            }
        }
        
        public void RemoveDuelPoints(int points)
        {
            int previousPoints = currentDuelPoints;
            currentDuelPoints = Mathf.Max(currentDuelPoints - points, 0);
            
            OnDuelPointsChanged?.Invoke(currentDuelPoints - previousPoints);
            UpdateDuelPointsUI();
            
            if (duelPointsUI != null)
            {
                duelPointsUI.ShowPointsChange(currentDuelPoints - previousPoints);
            }
        }
        
        // Command pattern interfaces and implementations
        private interface ICardCommand
        {
            void Execute();
        }
        
        private class PlayMonsterCommand : ICardCommand
        {
            private readonly CardManager cardManager;
            private readonly YuGiOhCard card;
            
            public PlayMonsterCommand(CardManager manager, YuGiOhCard card)
            {
                this.cardManager = manager;
                this.card = card;
            }
            
            public void Execute()
            {
                cardManager.SetState(CardManagerState.PlacingCard);
            }
        }
        
        private class PlaySpellCommand : ICardCommand
        {
            private readonly CardManager cardManager;
            private readonly YuGiOhCard card;
            
            public PlaySpellCommand(CardManager manager, YuGiOhCard card)
            {
                this.cardManager = manager;
                this.card = card;
            }
            
            public void Execute()
            {
                // Implement spell effect logic
                Debug.Log($"Casting spell: {card.name}");
                
                cardManager.cardCooldowns[card.id] = cardManager.cardPlayCooldown;
                cardManager.currentDuelPoints -= card.cost;
                cardManager.OnCardPlayed?.Invoke(card);
                cardManager.UpdateDuelPointsUI();
            }
        }
        
        private class PlayTrapCommand : ICardCommand
        {
            private readonly CardManager cardManager;
            private readonly YuGiOhCard card;
            
            public PlayTrapCommand(CardManager manager, YuGiOhCard card)
            {
                this.cardManager = manager;
                this.card = card;
            }
            
            public void Execute()
            {
                // Implement trap placement logic
                Debug.Log($"Setting trap: {card.name}");
                
                cardManager.cardCooldowns[card.id] = cardManager.cardPlayCooldown;
                cardManager.currentDuelPoints -= card.cost;
                cardManager.OnCardPlayed?.Invoke(card);
                cardManager.UpdateDuelPointsUI();
            }
        }
        
        // State pattern interfaces and implementations
        private interface ICardManagerState
        {
            void Enter();
            void Update();
            void Exit();
        }
        
        private class CardManagerIdleState : ICardManagerState
        {
            private readonly CardManager cardManager;
            
            public CardManagerIdleState(CardManager manager)
            {
                this.cardManager = manager;
            }
            
            public void Enter()
            {
                // Nothing to do here
            }
            
            public void Update()
            {
                // Nothing to do here
            }
            
            public void Exit()
            {
                // Nothing to do here
            }
        }
        
        private class CardManagerPlacingState : ICardManagerState
        {
            private readonly CardManager cardManager;
            
            public CardManagerPlacingState(CardManager manager)
            {
                this.cardManager = manager;
            }
            
            public void Enter()
            {
                Debug.Log($"Ready to place monster: {cardManager.selectedCard.name}");
            }
            
            public void Update()
            {
                if (Input.touchCount > 0)
                {
                    Touch touch = Input.GetTouch(0);
                    
                    if (touch.phase == TouchPhase.Began)
                    {
                        Ray ray = Camera.main.ScreenPointToRay(touch.position);
                        RaycastHit hit;
                        
                        if (Physics.Raycast(ray, out hit, Mathf.Infinity, cardManager.placementLayer))
                        {
                            cardManager.PlaceCard(hit.point);
                        }
                    }
                }
                
                if (Input.GetMouseButtonDown(0))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, cardManager.placementLayer))
                    {
                        cardManager.PlaceCard(hit.point);
                    }
                }
            }
            
            public void Exit()
            {
                cardManager.selectedCard = null;
            }
        }
    }
    
    public enum CardManagerState
    {
        Idle,
        PlacingCard
    }
} 