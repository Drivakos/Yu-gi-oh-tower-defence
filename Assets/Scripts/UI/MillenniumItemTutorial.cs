using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using YuGiOhTowerDefense.Core;

namespace YuGiOhTowerDefense.UI
{
    public class MillenniumItemTutorial : MonoBehaviour
    {
        [Header("Tutorial UI")]
        [SerializeField] private GameObject tutorialPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Image itemIcon;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button skipButton;
        [SerializeField] private GameObject highlightFrame;
        
        [Header("Tutorial Steps")]
        [SerializeField] private List<TutorialStep> tutorialSteps = new List<TutorialStep>();
        
        [Header("Animation Settings")]
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float fadeOutDuration = 0.3f;
        [SerializeField] private AnimationCurve fadeCurve;
        
        private MillenniumItemManager itemManager;
        private MillenniumItemUI itemUI;
        private int currentStepIndex = -1;
        private Coroutine fadeCoroutine;
        
        [System.Serializable]
        public class TutorialStep
        {
            public string title;
            [TextArea(3, 5)]
            public string description;
            public Sprite itemIcon;
            public MillenniumItemType itemType;
            public bool requiresItemCollection;
            public bool requiresItemActivation;
            public string highlightTarget;
        }
        
        private void Awake()
        {
            itemManager = FindObjectOfType<MillenniumItemManager>();
            itemUI = FindObjectOfType<MillenniumItemUI>();
            
            nextButton.onClick.AddListener(ShowNextStep);
            skipButton.onClick.AddListener(SkipTutorial);
            
            // Hide tutorial initially
            tutorialPanel.SetActive(false);
            if (highlightFrame != null)
                highlightFrame.SetActive(false);
        }
        
        public void StartTutorial()
        {
            currentStepIndex = -1;
            ShowNextStep();
        }
        
        private void ShowNextStep()
        {
            currentStepIndex++;
            
            // Check if tutorial is complete
            if (currentStepIndex >= tutorialSteps.Count)
            {
                EndTutorial();
                return;
            }
            
            TutorialStep step = tutorialSteps[currentStepIndex];
            
            // Check if this step requires a specific item
            if (step.requiresItemCollection && !itemManager.HasItem(step.itemType))
            {
                // Skip this step if the required item isn't collected yet
                ShowNextStep();
                return;
            }
            
            // Update UI
            titleText.text = step.title;
            descriptionText.text = step.description;
            itemIcon.sprite = step.itemIcon;
            
            // Show tutorial panel with fade
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeIn());
            
            // Highlight relevant UI element if specified
            if (!string.IsNullOrEmpty(step.highlightTarget))
            {
                HighlightUIElement(step.highlightTarget);
            }
            else
            {
                if (highlightFrame != null)
                    highlightFrame.SetActive(false);
            }
            
            // Update next button text for last step
            if (currentStepIndex == tutorialSteps.Count - 1)
            {
                nextButton.GetComponentInChildren<TextMeshProUGUI>().text = "Finish";
            }
        }
        
        private void HighlightUIElement(string targetName)
        {
            if (highlightFrame == null) return;
            
            // Find the target UI element
            GameObject target = GameObject.Find(targetName);
            if (target != null)
            {
                highlightFrame.SetActive(true);
                highlightFrame.transform.position = target.transform.position;
                highlightFrame.transform.SetParent(target.transform, false);
                highlightFrame.transform.SetAsLastSibling();
            }
            else
            {
                highlightFrame.SetActive(false);
            }
        }
        
        private void SkipTutorial()
        {
            EndTutorial();
        }
        
        private void EndTutorial()
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeOut());
        }
        
        private IEnumerator FadeIn()
        {
            tutorialPanel.SetActive(true);
            CanvasGroup canvasGroup = tutorialPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = tutorialPanel.AddComponent<CanvasGroup>();
                
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeInDuration;
                canvasGroup.alpha = fadeCurve.Evaluate(t);
                yield return null;
            }
            
            canvasGroup.alpha = 1f;
        }
        
        private IEnumerator FadeOut()
        {
            CanvasGroup canvasGroup = tutorialPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = tutorialPanel.AddComponent<CanvasGroup>();
                
            float elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeOutDuration;
                canvasGroup.alpha = 1f - fadeCurve.Evaluate(t);
                yield return null;
            }
            
            canvasGroup.alpha = 0f;
            tutorialPanel.SetActive(false);
            
            if (highlightFrame != null)
                highlightFrame.SetActive(false);
        }
        
        private void OnDestroy()
        {
            nextButton.onClick.RemoveListener(ShowNextStep);
            skipButton.onClick.RemoveListener(SkipTutorial);
        }
    }
} 