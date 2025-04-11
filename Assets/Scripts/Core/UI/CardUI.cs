using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using YuGiOhTowerDefense.Cards;

namespace YuGiOhTowerDefense.UI
{
    public class CardUI : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Card Elements")]
        [SerializeField] private Image cardImage;
        [SerializeField] private Image cardFrame;
        [SerializeField] private TextMeshProUGUI cardNameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI attackText;
        [SerializeField] private TextMeshProUGUI defenseText;
        [SerializeField] private GameObject monsterStatsPanel;
        [SerializeField] private GameObject spellTrapPanel;

        [Header("Visual Effects")]
        [SerializeField] private GameObject selectionHighlight;
        [SerializeField] private float hoverScale = 1.2f;
        [SerializeField] private float dragScale = 0.8f;
        [SerializeField] private float animationSpeed = 10f;

        private YuGiOhCard cardData;
        private RectTransform rectTransform;
        private Canvas canvas;
        private Vector2 originalPosition;
        private Vector3 originalScale;
        private bool isDragging;
        private bool isSelected;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();
            originalScale = transform.localScale;
        }

        public void Initialize(YuGiOhCard card)
        {
            cardData = card;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            cardNameText.text = cardData.CardName;
            descriptionText.text = cardData.Description;
            cardImage.sprite = cardData.CardImage;

            // Update frame and stats based on card type
            if (cardData is MonsterCard monsterCard)
            {
                monsterStatsPanel.SetActive(true);
                spellTrapPanel.SetActive(false);
                attackText.text = monsterCard.AttackPoints.ToString();
                defenseText.text = monsterCard.DefensePoints.ToString();
                // TODO: Set appropriate frame for monster type/attribute
            }
            else
            {
                monsterStatsPanel.SetActive(false);
                spellTrapPanel.SetActive(true);
                // TODO: Set appropriate frame for spell/trap type
            }
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            selectionHighlight.SetActive(selected);
        }

        #region Drag and Drop
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!isDragging)
            {
                UIManager.Instance.SelectCard(this);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!isSelected) return;

            isDragging = true;
            originalPosition = rectTransform.anchoredPosition;
            transform.localScale = originalScale * dragScale;

            // Bring card to front
            transform.SetAsLastSibling();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging) return;

            // Convert screen point to local anchored position
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)canvas.transform,
                eventData.position,
                canvas.worldCamera,
                out Vector2 localPoint
            );

            rectTransform.anchoredPosition = localPoint;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging) return;

            isDragging = false;
            transform.localScale = originalScale;

            // Check if card was dropped on a valid tile
            Ray ray = Camera.main.ScreenPointToRay(eventData.position);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Tile tile = hit.collider.GetComponent<Tile>();
                if (tile != null && !tile.IsOccupied)
                {
                    // Attempt to play card on tile
                    GameManager.Instance.PlayCard(cardData, tile.GridPosition);
                    return;
                }
            }

            // If not placed on valid tile, return to hand
            rectTransform.anchoredPosition = originalPosition;
        }
        #endregion

        private void OnDestroy()
        {
            if (isSelected)
            {
                UIManager.Instance.ClearSelectedCard();
            }
        }
    }
} 