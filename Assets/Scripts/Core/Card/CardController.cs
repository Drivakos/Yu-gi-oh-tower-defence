using UnityEngine;
using System;
using YuGiOhTowerDefense.Core.Grid;

namespace YuGiOhTowerDefense.Core.Card
{
    /// <summary>
    /// Controls card behavior and interactions
    /// </summary>
    public class CardController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject cardModel;
        [SerializeField] private GameObject attackRangeIndicator;
        [SerializeField] private CardEffectSystem effectSystem;
        
        [Header("Card Settings")]
        [SerializeField] private float rotationSpeed = 5f;
        [SerializeField] private float hoverHeight = 0.2f;
        [SerializeField] private float hoverSpeed = 2f;
        
        private CardData cardData;
        private Vector2Int gridPosition;
        private bool isSelected;
        private bool isHovering;
        private Vector3 originalPosition;
        private float hoverOffset;
        private float lastAttackTime;
        
        public CardData CardData => cardData;
        public Vector2Int GridPosition => gridPosition;
        public bool IsSelected => isSelected;
        
        public static event Action<CardController> OnCardSelected;
        public static event Action<CardController> OnCardDeselected;
        
        private void Awake()
        {
            if (attackRangeIndicator != null)
            {
                attackRangeIndicator.SetActive(false);
            }
        }
        
        public void Initialize(CardData data, Vector2Int position)
        {
            cardData = data;
            gridPosition = position;
            originalPosition = transform.position;
            lastAttackTime = Time.time;
            
            // Initialize card model
            if (cardModel != null)
            {
                // Set card model based on card data
                // This will be implemented when we have the card models
            }
            
            // Initialize attack range indicator
            if (attackRangeIndicator != null && cardData is MonsterCardData monsterData)
            {
                float scale = monsterData.AttackRange * 2f;
                attackRangeIndicator.transform.localScale = new Vector3(scale, 0.1f, scale);
            }
        }
        
        private void Update()
        {
            HandleHoverAnimation();
            
            if (isSelected && cardData is MonsterCardData monsterData)
            {
                HandleMonsterAttack(monsterData);
            }
        }
        
        private void HandleHoverAnimation()
        {
            if (isHovering)
            {
                hoverOffset = Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;
                transform.position = originalPosition + Vector3.up * hoverOffset;
                
                // Rotate card model
                if (cardModel != null)
                {
                    cardModel.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
                }
            }
        }
        
        private void HandleMonsterAttack(MonsterCardData monsterData)
        {
            if (Time.time - lastAttackTime >= monsterData.AttackCooldown)
            {
                // Find target position (mouse position or nearest enemy)
                Vector3 targetPosition = GetTargetPosition();
                
                // Apply effect
                effectSystem.ApplyCardEffect(this, targetPosition);
                
                lastAttackTime = Time.time;
            }
        }
        
        private Vector3 GetTargetPosition()
        {
            // For now, just return mouse position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit))
            {
                return hit.point;
            }
            
            return transform.position;
        }
        
        private void OnMouseEnter()
        {
            isHovering = true;
        }
        
        private void OnMouseExit()
        {
            isHovering = false;
            transform.position = originalPosition;
            
            if (cardModel != null)
            {
                cardModel.transform.rotation = Quaternion.identity;
            }
        }
        
        private void OnMouseDown()
        {
            if (!isSelected)
            {
                SelectCard();
            }
            else
            {
                DeselectCard();
            }
        }
        
        public void SelectCard()
        {
            isSelected = true;
            
            if (attackRangeIndicator != null)
            {
                attackRangeIndicator.SetActive(true);
            }
            
            OnCardSelected?.Invoke(this);
        }
        
        public void DeselectCard()
        {
            isSelected = false;
            
            if (attackRangeIndicator != null)
            {
                attackRangeIndicator.SetActive(false);
            }
            
            OnCardDeselected?.Invoke(this);
        }
        
        public void DestroyCard()
        {
            // Clean up before destroying
            if (attackRangeIndicator != null)
            {
                Destroy(attackRangeIndicator);
            }
            
            Destroy(gameObject);
        }
    }
} 