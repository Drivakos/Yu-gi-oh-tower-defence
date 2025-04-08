using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using YuGiOhTowerDefense.Core;

namespace YuGiOhTowerDefense.UI
{
    public class CardPlacementUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [Header("Placement UI")]
        [SerializeField] private RectTransform placementArea;
        [SerializeField] private Image placementPreview;
        [SerializeField] private CanvasGroup placementCanvasGroup;
        
        [Header("Animation Settings")]
        [SerializeField] private float previewFadeInDuration = 0.2f;
        [SerializeField] private float previewFadeOutDuration = 0.2f;
        [SerializeField] private float previewScaleMultiplier = 1.2f;
        [SerializeField] private AnimationCurve previewScaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("Touch Settings")]
        [SerializeField] private float touchThreshold = 0.1f;
        [SerializeField] private float longPressDuration = 0.5f;
        
        [Header("References")]
        [SerializeField] private CardPlacementManager placementManager;
        [SerializeField] private Canvas canvas;
        [SerializeField] private Camera mainCamera;
        
        private YuGiOhCard selectedCard;
        private bool isPlacing;
        private bool isLongPressing;
        private float pressStartTime;
        private Vector2 touchStartPosition;
        private Vector2 currentTouchPosition;
        private Coroutine previewCoroutine;
        
        private void Awake()
        {
            if (placementManager == null)
            {
                placementManager = GetComponent<CardPlacementManager>();
                if (placementManager == null)
                {
                    placementManager = FindObjectOfType<CardPlacementManager>();
                    if (placementManager == null)
                    {
                        Debug.LogError("CardPlacementManager not found!");
                    }
                }
            }
            
            if (canvas == null)
            {
                canvas = GetComponentInParent<Canvas>();
                if (canvas == null)
                {
                    Debug.LogError("Canvas not found!");
                }
            }
            
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null)
                {
                    Debug.LogError("Main camera not found!");
                }
            }
            
            // Initialize UI elements
            if (placementPreview != null)
            {
                placementPreview.gameObject.SetActive(false);
            }
            
            if (placementCanvasGroup == null)
            {
                placementCanvasGroup = GetComponent<CanvasGroup>();
                if (placementCanvasGroup == null)
                {
                    placementCanvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!isPlacing || selectedCard == null)
            {
                return;
            }
            
            pressStartTime = Time.time;
            touchStartPosition = eventData.position;
            currentTouchPosition = eventData.position;
            
            // Start long press check
            StartCoroutine(CheckLongPress());
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isPlacing || selectedCard == null)
            {
                return;
            }
            
            // Stop long press check
            StopAllCoroutines();
            
            // If not long pressing, try to place card
            if (!isLongPressing)
            {
                TryPlaceCard(eventData.position);
            }
            
            // Reset state
            isLongPressing = false;
            HidePlacementPreview();
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (!isPlacing || selectedCard == null)
            {
                return;
            }
            
            currentTouchPosition = eventData.position;
            
            // Update preview position
            UpdatePlacementPreview(currentTouchPosition);
        }
        
        private IEnumerator CheckLongPress()
        {
            yield return new WaitForSeconds(longPressDuration);
            
            if (Vector2.Distance(touchStartPosition, currentTouchPosition) < touchThreshold)
            {
                isLongPressing = true;
                ShowPlacementPreview(currentTouchPosition);
            }
        }
        
        private void TryPlaceCard(Vector2 screenPosition)
        {
            if (placementArea == null || !RectTransformUtility.RectangleContainsScreenPoint(placementArea, screenPosition, canvas.worldCamera))
            {
                return;
            }
            
            // Convert screen position to world position
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(placementArea, screenPosition, canvas.worldCamera, out localPoint);
            
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10f));
            worldPosition.y = 0f;
            
            // Try to place card
            placementManager.TryPlaceCard(worldPosition);
        }
        
        private void ShowPlacementPreview(Vector2 screenPosition)
        {
            if (placementPreview == null || selectedCard == null)
            {
                return;
            }
            
            // Stop any existing preview animation
            if (previewCoroutine != null)
            {
                StopCoroutine(previewCoroutine);
            }
            
            // Start new preview animation
            previewCoroutine = StartCoroutine(AnimatePreview(true));
            
            // Update preview position
            UpdatePlacementPreview(screenPosition);
        }
        
        private void HidePlacementPreview()
        {
            if (placementPreview == null)
            {
                return;
            }
            
            // Stop any existing preview animation
            if (previewCoroutine != null)
            {
                StopCoroutine(previewCoroutine);
            }
            
            // Start fade out animation
            previewCoroutine = StartCoroutine(AnimatePreview(false));
        }
        
        private void UpdatePlacementPreview(Vector2 screenPosition)
        {
            if (placementPreview == null || placementArea == null)
            {
                return;
            }
            
            // Convert screen position to local position within placement area
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(placementArea, screenPosition, canvas.worldCamera, out localPoint);
            
            // Update preview position
            placementPreview.rectTransform.anchoredPosition = localPoint;
        }
        
        private IEnumerator AnimatePreview(bool show)
        {
            if (placementPreview == null || placementCanvasGroup == null)
            {
                yield break;
            }
            
            float duration = show ? previewFadeInDuration : previewFadeOutDuration;
            float startAlpha = placementCanvasGroup.alpha;
            float targetAlpha = show ? 1f : 0f;
            float startScale = placementPreview.transform.localScale.x;
            float targetScale = show ? previewScaleMultiplier : 1f;
            float elapsedTime = 0f;
            
            placementPreview.gameObject.SetActive(true);
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                float curveValue = previewScaleCurve.Evaluate(t);
                
                // Update alpha
                placementCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, curveValue);
                
                // Update scale
                float currentScale = Mathf.Lerp(startScale, targetScale, curveValue);
                placementPreview.transform.localScale = new Vector3(currentScale, currentScale, 1f);
                
                yield return null;
            }
            
            // Ensure final values
            placementCanvasGroup.alpha = targetAlpha;
            placementPreview.transform.localScale = new Vector3(targetScale, targetScale, 1f);
            
            if (!show)
            {
                placementPreview.gameObject.SetActive(false);
            }
        }
        
        public void StartPlacement(YuGiOhCard card)
        {
            if (card == null || !card.IsMonster())
            {
                return;
            }
            
            selectedCard = card;
            isPlacing = true;
            
            // Update preview image
            if (placementPreview != null)
            {
                // TODO: Load card image
                // placementPreview.sprite = card.GetCardImage();
            }
        }
        
        public void CancelPlacement()
        {
            selectedCard = null;
            isPlacing = false;
            
            // Hide preview
            HidePlacementPreview();
        }
    }
} 