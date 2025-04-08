using UnityEngine;
using TMPro;
using System.Collections;
using YuGiOhTowerDefense.Core;

namespace YuGiOhTowerDefense.UI
{
    public class DuelPointsUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI duelPointsText;
        [SerializeField] private TextMeshProUGUI duelPointsChangeText;
        [SerializeField] private GameObject insufficientPointsWarning;
        
        [Header("Animation Settings")]
        [SerializeField] private float pointsChangeDuration = 1f;
        [SerializeField] private float warningDuration = 2f;
        [SerializeField] private float warningPulseSpeed = 2f;
        [SerializeField] private float warningPulseMin = 0.5f;
        [SerializeField] private float warningPulseMax = 1f;
        
        [Header("Colors")]
        [SerializeField] private Color gainColor = Color.green;
        [SerializeField] private Color lossColor = Color.red;
        [SerializeField] private Color warningColor = Color.red;
        
        private CardManager cardManager;
        private Coroutine pointsChangeCoroutine;
        private Coroutine warningCoroutine;
        
        private void Awake()
        {
            cardManager = FindObjectOfType<CardManager>();
            if (cardManager == null)
            {
                Debug.LogError("DuelPointsUI: CardManager not found!");
            }
            
            if (insufficientPointsWarning != null)
            {
                insufficientPointsWarning.SetActive(false);
            }
        }
        
        private void Start()
        {
            UpdateDuelPointsDisplay(cardManager.GetCurrentDuelPoints());
        }
        
        private void Update()
        {
            // Update duel points display
            UpdateDuelPointsDisplay(cardManager.GetCurrentDuelPoints());
        }
        
        public void UpdateDuelPointsDisplay(int points)
        {
            if (duelPointsText != null)
            {
                duelPointsText.text = points.ToString();
            }
        }
        
        public void ShowPointsChange(int change)
        {
            if (pointsChangeCoroutine != null)
            {
                StopCoroutine(pointsChangeCoroutine);
            }
            
            pointsChangeCoroutine = StartCoroutine(PointsChangeAnimation(change));
        }
        
        public void ShowInsufficientPointsWarning()
        {
            if (warningCoroutine != null)
            {
                StopCoroutine(warningCoroutine);
            }
            
            warningCoroutine = StartCoroutine(InsufficientPointsWarningAnimation());
        }
        
        private IEnumerator PointsChangeAnimation(int change)
        {
            if (duelPointsChangeText == null)
            {
                yield break;
            }
            
            // Set up the change text
            duelPointsChangeText.text = (change >= 0 ? "+" : "") + change.ToString();
            duelPointsChangeText.color = change >= 0 ? gainColor : lossColor;
            
            // Show the text
            duelPointsChangeText.gameObject.SetActive(true);
            
            // Animate the text
            float elapsedTime = 0f;
            Vector3 startPosition = duelPointsChangeText.transform.position;
            Vector3 endPosition = startPosition + Vector3.up * 50f;
            
            while (elapsedTime < pointsChangeDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / pointsChangeDuration;
                
                // Move up and fade out
                duelPointsChangeText.transform.position = Vector3.Lerp(startPosition, endPosition, t);
                duelPointsChangeText.alpha = Mathf.Lerp(1f, 0f, t);
                
                yield return null;
            }
            
            // Hide the text
            duelPointsChangeText.gameObject.SetActive(false);
            duelPointsChangeText.alpha = 1f;
        }
        
        private IEnumerator InsufficientPointsWarningAnimation()
        {
            if (insufficientPointsWarning == null)
            {
                yield break;
            }
            
            // Show the warning
            insufficientPointsWarning.SetActive(true);
            
            // Pulse the warning
            float elapsedTime = 0f;
            CanvasGroup warningCanvasGroup = insufficientPointsWarning.GetComponent<CanvasGroup>();
            
            while (elapsedTime < warningDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = (Mathf.Sin(elapsedTime * warningPulseSpeed) + 1f) * 0.5f;
                
                if (warningCanvasGroup != null)
                {
                    warningCanvasGroup.alpha = Mathf.Lerp(warningPulseMin, warningPulseMax, t);
                }
                
                yield return null;
            }
            
            // Hide the warning
            insufficientPointsWarning.SetActive(false);
            if (warningCanvasGroup != null)
            {
                warningCanvasGroup.alpha = 1f;
            }
        }
        
        public void UpdateCardCostDisplay(YuGiOhCard card)
        {
            if (card == null)
            {
                return;
            }
            
            // Update the cost display based on whether the card can be played
            bool canPlay = cardManager.CanPlayCard(card);
            
            if (duelPointsChangeText != null)
            {
                duelPointsChangeText.text = card.cost.ToString();
                duelPointsChangeText.color = canPlay ? gainColor : lossColor;
            }
        }
    }
} 