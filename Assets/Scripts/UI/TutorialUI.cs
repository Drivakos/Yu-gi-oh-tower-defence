using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using YuGiOhTowerDefense.Core;

namespace YuGiOhTowerDefense.UI
{
    public class TutorialUI : MonoBehaviour
    {
        [Header("Tutorial Panel")]
        [SerializeField] private GameObject tutorialPanel;
        [SerializeField] private CanvasGroup tutorialCanvasGroup;
        [SerializeField] private RectTransform tutorialPanelRect;
        
        [Header("Tutorial Content")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        
        [Header("Buttons")]
        [SerializeField] private Button nextButton;
        [SerializeField] private Button skipButton;
        [SerializeField] private TextMeshProUGUI nextButtonText;
        [SerializeField] private TextMeshProUGUI skipButtonText;
        
        [Header("Highlight")]
        [SerializeField] private GameObject highlightOverlay;
        [SerializeField] private Image highlightImage;
        [SerializeField] private float highlightPulseSpeed = 1f;
        [SerializeField] private float highlightPulseMinAlpha = 0.3f;
        [SerializeField] private float highlightPulseMaxAlpha = 0.7f;
        
        [Header("Animation")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.2f;
        [SerializeField] private float slideInDistance = 50f;
        [SerializeField] private AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        private TutorialManager tutorialManager;
        private Coroutine highlightPulseCoroutine;
        private Coroutine fadeCoroutine;
        
        private void Start()
        {
            tutorialManager = FindObjectOfType<TutorialManager>();
            
            if (tutorialManager == null)
            {
                Debug.LogError("TutorialManager not found!");
                return;
            }
            
            // Set up button listeners
            nextButton.onClick.AddListener(OnNextButtonClicked);
            skipButton.onClick.AddListener(OnSkipButtonClicked);
            
            // Initialize UI state
            tutorialPanel.SetActive(false);
            highlightOverlay.SetActive(false);
            
            if (tutorialCanvasGroup != null)
            {
                tutorialCanvasGroup.alpha = 0f;
            }
        }
        
        public void ShowTutorialStep(string title, string description, Sprite icon)
        {
            // Update content
            titleText.text = title;
            descriptionText.text = description;
            
            if (icon != null)
            {
                iconImage.sprite = icon;
                iconImage.gameObject.SetActive(true);
            }
            else
            {
                iconImage.gameObject.SetActive(false);
            }
            
            // Show panel with animation
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            
            fadeCoroutine = StartCoroutine(ShowPanelAnimation());
        }
        
        public void HideTutorialStep()
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            
            fadeCoroutine = StartCoroutine(HidePanelAnimation());
        }
        
        public void HighlightUIElement(RectTransform element)
        {
            if (element == null)
            {
                highlightOverlay.SetActive(false);
                return;
            }
            
            // Position highlight overlay over the element
            highlightOverlay.SetActive(true);
            highlightImage.rectTransform.position = element.position;
            highlightImage.rectTransform.sizeDelta = element.sizeDelta;
            
            // Start pulse animation
            if (highlightPulseCoroutine != null)
            {
                StopCoroutine(highlightPulseCoroutine);
            }
            
            highlightPulseCoroutine = StartCoroutine(PulseHighlight());
        }
        
        public void SetNextButtonState(bool interactable, string text = "Next")
        {
            nextButton.interactable = interactable;
            nextButtonText.text = text;
        }
        
        public void SetSkipButtonState(bool interactable, string text = "Skip Tutorial")
        {
            skipButton.interactable = interactable;
            skipButtonText.text = text;
        }
        
        private IEnumerator ShowPanelAnimation()
        {
            tutorialPanel.SetActive(true);
            
            float elapsedTime = 0f;
            Vector2 startPosition = tutorialPanelRect.anchoredPosition + Vector2.right * slideInDistance;
            Vector2 targetPosition = tutorialPanelRect.anchoredPosition;
            
            tutorialPanelRect.anchoredPosition = startPosition;
            
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / fadeInDuration;
                float curveValue = slideCurve.Evaluate(t);
                
                if (tutorialCanvasGroup != null)
                {
                    tutorialCanvasGroup.alpha = curveValue;
                }
                
                tutorialPanelRect.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, curveValue);
                
                yield return null;
            }
            
            if (tutorialCanvasGroup != null)
            {
                tutorialCanvasGroup.alpha = 1f;
            }
            
            tutorialPanelRect.anchoredPosition = targetPosition;
        }
        
        private IEnumerator HidePanelAnimation()
        {
            float elapsedTime = 0f;
            Vector2 startPosition = tutorialPanelRect.anchoredPosition;
            Vector2 targetPosition = startPosition + Vector2.right * slideInDistance;
            
            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / fadeOutDuration;
                float curveValue = slideCurve.Evaluate(t);
                
                if (tutorialCanvasGroup != null)
                {
                    tutorialCanvasGroup.alpha = 1f - curveValue;
                }
                
                tutorialPanelRect.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, curveValue);
                
                yield return null;
            }
            
            if (tutorialCanvasGroup != null)
            {
                tutorialCanvasGroup.alpha = 0f;
            }
            
            tutorialPanel.SetActive(false);
        }
        
        private IEnumerator PulseHighlight()
        {
            Color highlightColor = highlightImage.color;
            
            while (true)
            {
                float pulse = Mathf.PingPong(Time.time * highlightPulseSpeed, 1f);
                float alpha = Mathf.Lerp(highlightPulseMinAlpha, highlightPulseMaxAlpha, pulse);
                
                highlightColor.a = alpha;
                highlightImage.color = highlightColor;
                
                yield return null;
            }
        }
        
        private void OnNextButtonClicked()
        {
            if (tutorialManager != null)
            {
                tutorialManager.OnNextButtonClicked();
            }
        }
        
        private void OnSkipButtonClicked()
        {
            if (tutorialManager != null)
            {
                tutorialManager.OnSkipButtonClicked();
            }
        }
        
        private void OnDestroy()
        {
            if (highlightPulseCoroutine != null)
            {
                StopCoroutine(highlightPulseCoroutine);
            }
            
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
        }
    }
} 