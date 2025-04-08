using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using YuGiOhTowerDefense.Core;

namespace YuGiOhTowerDefense.UI
{
    public class LevelRatingUI : MonoBehaviour
    {
        [Header("Rating Panel")]
        [SerializeField] private GameObject ratingPanel;
        [SerializeField] private CanvasGroup ratingCanvasGroup;
        [SerializeField] private RectTransform ratingPanelRect;
        
        [Header("Rating Content")]
        [SerializeField] private TextMeshProUGUI levelNameText;
        [SerializeField] private TextMeshProUGUI completionTimeText;
        [SerializeField] private TextMeshProUGUI monstersUsedText;
        [SerializeField] private TextMeshProUGUI cardsUsedText;
        [SerializeField] private TextMeshProUGUI duelPointsText;
        [SerializeField] private TextMeshProUGUI rewardText;
        [SerializeField] private TextMeshProUGUI newRecordText;
        
        [Header("Stars")]
        [SerializeField] private Image[] starImages;
        [SerializeField] private Sprite emptyStarSprite;
        [SerializeField] private Sprite filledStarSprite;
        [SerializeField] private float starAnimationDelay = 0.2f;
        
        [Header("Animation")]
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float fadeOutDuration = 0.3f;
        [SerializeField] private float slideInDistance = 100f;
        [SerializeField] private AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private float starPopScale = 1.2f;
        [SerializeField] private float starPopDuration = 0.3f;
        
        [Header("Buttons")]
        [SerializeField] private Button continueButton;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button levelSelectButton;
        
        private LevelManager levelManager;
        private Coroutine fadeCoroutine;
        private Coroutine starAnimationCoroutine;
        
        private void Start()
        {
            levelManager = FindObjectOfType<LevelManager>();
            
            if (levelManager == null)
            {
                Debug.LogError("LevelManager not found!");
            }
            
            // Set up button listeners
            continueButton.onClick.AddListener(OnContinueButtonClicked);
            retryButton.onClick.AddListener(OnRetryButtonClicked);
            levelSelectButton.onClick.AddListener(OnLevelSelectButtonClicked);
            
            // Initialize UI state
            ratingPanel.SetActive(false);
            
            if (ratingCanvasGroup != null)
            {
                ratingCanvasGroup.alpha = 0f;
            }
            
            // Initialize stars
            foreach (var star in starImages)
            {
                star.sprite = emptyStarSprite;
            }
        }
        
        public void ShowRatingResults(string levelId, string levelName, int stars, int reward, bool isNewRecord, 
                                     float completionTime, int monstersUsed, int cardsUsed, int duelPointsEarned)
        {
            // Update content
            levelNameText.text = levelName;
            completionTimeText.text = FormatTime(completionTime);
            monstersUsedText.text = monstersUsed.ToString();
            cardsUsedText.text = cardsUsed.ToString();
            duelPointsText.text = duelPointsEarned.ToString();
            rewardText.text = "+" + reward.ToString();
            
            newRecordText.gameObject.SetActive(isNewRecord);
            
            // Show panel with animation
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            
            fadeCoroutine = StartCoroutine(ShowPanelAnimation());
            
            // Animate stars
            if (starAnimationCoroutine != null)
            {
                StopCoroutine(starAnimationCoroutine);
            }
            
            starAnimationCoroutine = StartCoroutine(AnimateStars(stars));
        }
        
        private string FormatTime(float seconds)
        {
            int minutes = Mathf.FloorToInt(seconds / 60f);
            int remainingSeconds = Mathf.FloorToInt(seconds % 60f);
            return string.Format("{0:00}:{1:00}", minutes, remainingSeconds);
        }
        
        private IEnumerator ShowPanelAnimation()
        {
            ratingPanel.SetActive(true);
            
            float elapsedTime = 0f;
            Vector2 startPosition = ratingPanelRect.anchoredPosition + Vector2.up * slideInDistance;
            Vector2 targetPosition = ratingPanelRect.anchoredPosition;
            
            ratingPanelRect.anchoredPosition = startPosition;
            
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / fadeInDuration;
                float curveValue = slideCurve.Evaluate(t);
                
                if (ratingCanvasGroup != null)
                {
                    ratingCanvasGroup.alpha = curveValue;
                }
                
                ratingPanelRect.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, curveValue);
                
                yield return null;
            }
            
            if (ratingCanvasGroup != null)
            {
                ratingCanvasGroup.alpha = 1f;
            }
            
            ratingPanelRect.anchoredPosition = targetPosition;
        }
        
        private IEnumerator AnimateStars(int stars)
        {
            // Reset stars
            foreach (var star in starImages)
            {
                star.sprite = emptyStarSprite;
                star.transform.localScale = Vector3.one;
            }
            
            // Animate each star
            for (int i = 0; i < starImages.Length; i++)
            {
                if (i < stars)
                {
                    yield return new WaitForSeconds(starAnimationDelay);
                    
                    // Pop animation
                    float elapsedTime = 0f;
                    Vector3 startScale = Vector3.one;
                    Vector3 targetScale = Vector3.one * starPopScale;
                    
                    while (elapsedTime < starPopDuration / 2f)
                    {
                        elapsedTime += Time.deltaTime;
                        float t = elapsedTime / (starPopDuration / 2f);
                        
                        starImages[i].transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                        
                        yield return null;
                    }
                    
                    // Fill star
                    starImages[i].sprite = filledStarSprite;
                    
                    // Return to normal scale
                    elapsedTime = 0f;
                    startScale = Vector3.one * starPopScale;
                    targetScale = Vector3.one;
                    
                    while (elapsedTime < starPopDuration / 2f)
                    {
                        elapsedTime += Time.deltaTime;
                        float t = elapsedTime / (starPopDuration / 2f);
                        
                        starImages[i].transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                        
                        yield return null;
                    }
                    
                    starImages[i].transform.localScale = Vector3.one;
                }
            }
        }
        
        public void HideRatingResults()
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            
            fadeCoroutine = StartCoroutine(HidePanelAnimation());
        }
        
        private IEnumerator HidePanelAnimation()
        {
            float elapsedTime = 0f;
            Vector2 startPosition = ratingPanelRect.anchoredPosition;
            Vector2 targetPosition = startPosition + Vector2.up * slideInDistance;
            
            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / fadeOutDuration;
                float curveValue = slideCurve.Evaluate(t);
                
                if (ratingCanvasGroup != null)
                {
                    ratingCanvasGroup.alpha = 1f - curveValue;
                }
                
                ratingPanelRect.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, curveValue);
                
                yield return null;
            }
            
            if (ratingCanvasGroup != null)
            {
                ratingCanvasGroup.alpha = 0f;
            }
            
            ratingPanel.SetActive(false);
        }
        
        private void OnContinueButtonClicked()
        {
            if (levelManager != null)
            {
                levelManager.LoadNextLevel();
            }
        }
        
        private void OnRetryButtonClicked()
        {
            if (levelManager != null)
            {
                levelManager.RetryCurrentLevel();
            }
        }
        
        private void OnLevelSelectButtonClicked()
        {
            if (levelManager != null)
            {
                levelManager.ReturnToLevelSelect();
            }
        }
        
        private void OnDestroy()
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            
            if (starAnimationCoroutine != null)
            {
                StopCoroutine(starAnimationCoroutine);
            }
        }
    }
} 