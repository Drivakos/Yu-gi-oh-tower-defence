using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using YuGiOhTowerDefense.Monsters;
using YuGiOhTowerDefense.Cards;
using System;

namespace YuGiOhTowerDefense.Core
{
    [Serializable]
    public class TutorialStep
    {
        public string id;
        public string title;
        [TextArea(3, 10)]
        public string description;
        public GameObject highlightTarget;
        public RectTransform pointerTarget;
        public Vector2 pointerOffset = Vector2.zero;
        public bool requireInteraction = true;
        public float autoAdvanceDelay = 0f;
        public string triggerEventName = "";
        public AudioClip voiceoverClip;
        public GameObject visualAid;
    }

    public class TutorialManager : MonoBehaviour
    {
        [Header("Tutorial Steps")]
        [SerializeField] private List<TutorialStep> tutorialSteps = new List<TutorialStep>();
        
        [Header("UI Elements")]
        [SerializeField] private GameObject tutorialPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button skipButton;
        [SerializeField] private RectTransform pointer;
        [SerializeField] private GameObject highlightOverlay;
        [SerializeField] private Image progressBar;
        [SerializeField] private TextMeshProUGUI progressText;
        
        [Header("Animation")]
        [SerializeField] private Animation panelAnimation;
        [SerializeField] private string showAnimationName = "ShowTutorialPanel";
        [SerializeField] private string hideAnimationName = "HideTutorialPanel";
        [SerializeField] private float animationDuration = 0.5f;
        [SerializeField] private AnimationCurve highlightPulseCurve;
        [SerializeField] private float highlightPulseDuration = 1.5f;
        
        [Header("Settings")]
        [SerializeField] private bool showTutorialOnStart = true;
        [SerializeField] private bool dismissOnClickOutside = false;
        [SerializeField] private float highlightPadding = 20f;
        
        [Header("References")]
        [SerializeField] private SaveManager saveManager;
        [SerializeField] private AudioManager audioManager;
        
        private Dictionary<string, TutorialStep> tutorialStepLookup = new Dictionary<string, TutorialStep>();
        private Dictionary<string, bool> completedSteps = new Dictionary<string, bool>();
        private int currentStepIndex = -1;
        private TutorialStep currentStep;
        private bool isTutorialActive = false;
        private Coroutine highlightCoroutine;
        private Coroutine autoAdvanceCoroutine;
        private Camera mainCamera;
        
        // Events
        public event Action<TutorialStep> OnTutorialStepStarted;
        public event Action<TutorialStep> OnTutorialStepCompleted;
        public event Action OnTutorialCompleted;
        
        public static TutorialManager Instance { get; private set; }
        public bool IsTutorialActive => isTutorialActive;
        
        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            // Find references if not assigned
            if (saveManager == null)
                saveManager = SaveManager.Instance;
                
            if (audioManager == null)
                audioManager = AudioManager.Instance;
                
            // Build lookup dictionary
            foreach (var step in tutorialSteps)
            {
                if (!string.IsNullOrEmpty(step.id) && !tutorialStepLookup.ContainsKey(step.id))
                {
                    tutorialStepLookup.Add(step.id, step);
                }
                else if (string.IsNullOrEmpty(step.id))
                {
                    Debug.LogError("Tutorial step has no ID!");
                }
                else
                {
                    Debug.LogError($"Duplicate tutorial step ID: {step.id}");
                }
            }
            
            // Initialize UI
            mainCamera = Camera.main;
            InitializeUI();
            
            // Hide tutorial panel initially
            if (tutorialPanel != null)
            {
                tutorialPanel.SetActive(false);
            }
            
            if (highlightOverlay != null)
            {
                highlightOverlay.SetActive(false);
            }
            
            if (pointer != null)
            {
                pointer.gameObject.SetActive(false);
            }
        }
        
        private void Start()
        {
            // Load saved tutorial progress
            LoadTutorialProgress();
            
            // Check if we should show tutorial on start
            if (showTutorialOnStart && IsTutorialEnabled() && !HasCompletedAllSteps())
            {
                StartTutorial();
            }
        }
        
        private void InitializeUI()
        {
            if (nextButton != null)
                nextButton.onClick.AddListener(AdvanceToNextStep);
            
            if (skipButton != null)
                skipButton.onClick.AddListener(SkipTutorial);
        }
        
        private void LoadTutorialProgress()
        {
            completedSteps.Clear();
            
            foreach (var step in tutorialSteps)
            {
                bool completed = PlayerPrefs.GetInt($"Tutorial_{step.id}_Completed", 0) == 1;
                completedSteps[step.id] = completed;
            }
        }
        
        private void SaveTutorialProgress()
        {
            foreach (var step in completedSteps)
            {
                PlayerPrefs.SetInt($"Tutorial_{step.Key}_Completed", step.Value ? 1 : 0);
            }
            
            PlayerPrefs.Save();
        }
        
        public bool IsTutorialEnabled()
        {
            return PlayerPrefs.GetInt("TutorialEnabled", 1) == 1;
        }
        
        public void SetTutorialEnabled(bool enabled)
        {
            PlayerPrefs.SetInt("TutorialEnabled", enabled ? 1 : 0);
            PlayerPrefs.Save();
        }
        
        public bool HasCompletedStep(string stepId)
        {
            return completedSteps.TryGetValue(stepId, out bool completed) && completed;
        }
        
        public bool HasCompletedAllSteps()
        {
            foreach (var step in tutorialSteps)
            {
                if (!HasCompletedStep(step.id))
                {
                    return false;
                }
            }
            
            return true;
        }
        
        public void StartTutorial()
        {
            if (!IsTutorialEnabled())
                return;
                
            currentStepIndex = -1;
            isTutorialActive = true;
            
            AdvanceToNextStep();
        }
        
        public void StartTutorialFromStep(string stepId)
        {
            if (!IsTutorialEnabled())
                return;
                
            int stepIndex = tutorialSteps.FindIndex(s => s.id == stepId);
            
            if (stepIndex >= 0)
            {
                currentStepIndex = stepIndex - 1;
                isTutorialActive = true;
                
                AdvanceToNextStep();
            }
            else
            {
                Debug.LogWarning($"Tutorial step with ID {stepId} not found!");
            }
        }
        
        public void AdvanceToNextStep()
        {
            // Stop any running coroutines
            if (autoAdvanceCoroutine != null)
            {
                StopCoroutine(autoAdvanceCoroutine);
                autoAdvanceCoroutine = null;
            }
            
            // Mark current step as completed
            if (currentStep != null)
            {
                CompleteCurrentStep();
            }
            
            // Move to next step
            currentStepIndex++;
            
            // Check if we've reached the end
            if (currentStepIndex >= tutorialSteps.Count)
            {
                CompleteTutorial();
                return;
            }
            
            // Show the next step
            currentStep = tutorialSteps[currentStepIndex];
            ShowTutorialStep(currentStep);
            
            // Check if step should auto-advance
            if (currentStep.autoAdvanceDelay > 0 && !currentStep.requireInteraction)
            {
                autoAdvanceCoroutine = StartCoroutine(AutoAdvanceAfterDelay(currentStep.autoAdvanceDelay));
            }
            
            // Invoke event
            OnTutorialStepStarted?.Invoke(currentStep);
        }
        
        private void ShowTutorialStep(TutorialStep step)
        {
            // Ensure panel is visible
            if (tutorialPanel != null && !tutorialPanel.activeSelf)
            {
                tutorialPanel.SetActive(true);
                
                if (panelAnimation != null)
                {
                    panelAnimation.Play(showAnimationName);
                }
            }
            
            // Update UI elements
            if (titleText != null)
                titleText.text = step.title;
                
            if (descriptionText != null)
                descriptionText.text = step.description;
                
            // Update progress indicator
            if (progressBar != null)
                progressBar.fillAmount = (float)(currentStepIndex + 1) / tutorialSteps.Count;
                
            if (progressText != null)
                progressText.text = $"{currentStepIndex + 1}/{tutorialSteps.Count}";
                
            // Show/hide next button based on interaction requirement
            if (nextButton != null)
                nextButton.gameObject.SetActive(!step.requireInteraction || string.IsNullOrEmpty(step.triggerEventName));
                
            // Update highlight and pointer
            UpdateHighlight(step.highlightTarget);
            UpdatePointer(step.pointerTarget, step.pointerOffset);
            
            // Show visual aid if available
            if (step.visualAid != null)
            {
                step.visualAid.SetActive(true);
            }
            
            // Play voiceover if available
            if (step.voiceoverClip != null && audioManager != null)
            {
                audioManager.PlayOneShot("TutorialVoiceover");
            }
        }
        
        private void UpdateHighlight(GameObject target)
        {
            // Stop any existing highlight coroutine
            if (highlightCoroutine != null)
            {
                StopCoroutine(highlightCoroutine);
                highlightCoroutine = null;
            }
            
            // Hide highlight if no target
            if (target == null || highlightOverlay == null)
            {
                if (highlightOverlay != null)
                    highlightOverlay.SetActive(false);
                return;
            }
            
            // Show highlight
            highlightOverlay.SetActive(true);
            
            // Start highlight animation
            highlightCoroutine = StartCoroutine(AnimateHighlight(target));
        }
        
        private void UpdatePointer(RectTransform target, Vector2 offset)
        {
            if (pointer == null)
                return;
                
            if (target == null)
            {
                pointer.gameObject.SetActive(false);
                return;
            }
            
            pointer.gameObject.SetActive(true);
            
            // Position pointer near target
            pointer.position = target.position + (Vector3)offset;
            
            // Point towards the target
            if (offset != Vector2.zero)
            {
                float angle = Mathf.Atan2(-offset.y, -offset.x) * Mathf.Rad2Deg;
                pointer.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
        
        private IEnumerator AnimateHighlight(GameObject target)
        {
            RectTransform highlightRect = highlightOverlay.GetComponent<RectTransform>();
            Image highlightImage = highlightOverlay.GetComponent<Image>();
            
            if (highlightRect == null || highlightImage == null)
                yield break;
                
            // Get target rect in screen space
            RectTransform targetRect = target.GetComponent<RectTransform>();
            Vector3[] targetCorners = new Vector3[4];
            
            if (targetRect != null)
            {
                // UI element
                targetRect.GetWorldCorners(targetCorners);
            }
            else
            {
                // 3D object - get screen space bounds
                Renderer renderer = target.GetComponent<Renderer>();
                if (renderer != null && mainCamera != null)
                {
                    Bounds bounds = renderer.bounds;
                    
                    // Convert 8 corners of the bounds to screen space
                    Vector3 center = bounds.center;
                    Vector3 extents = bounds.extents;
                    
                    Vector3[] worldCorners = new Vector3[8];
                    worldCorners[0] = center + new Vector3(-extents.x, -extents.y, -extents.z); // Bottom left back
                    worldCorners[1] = center + new Vector3(extents.x, -extents.y, -extents.z); // Bottom right back
                    worldCorners[2] = center + new Vector3(-extents.x, extents.y, -extents.z); // Top left back
                    worldCorners[3] = center + new Vector3(extents.x, extents.y, -extents.z); // Top right back
                    worldCorners[4] = center + new Vector3(-extents.x, -extents.y, extents.z); // Bottom left front
                    worldCorners[5] = center + new Vector3(extents.x, -extents.y, extents.z); // Bottom right front
                    worldCorners[6] = center + new Vector3(-extents.x, extents.y, extents.z); // Top left front
                    worldCorners[7] = center + new Vector3(extents.x, extents.y, extents.z); // Top right front
                    
                    // Find min/max screen coordinates
                    Vector2 minScreen = new Vector2(float.MaxValue, float.MaxValue);
                    Vector2 maxScreen = new Vector2(float.MinValue, float.MinValue);
                    
                    foreach (Vector3 worldPos in worldCorners)
                    {
                        Vector2 screenPos = mainCamera.WorldToScreenPoint(worldPos);
                        
                        minScreen.x = Mathf.Min(minScreen.x, screenPos.x);
                        minScreen.y = Mathf.Min(minScreen.y, screenPos.y);
                        maxScreen.x = Mathf.Max(maxScreen.x, screenPos.x);
                        maxScreen.y = Mathf.Max(maxScreen.y, screenPos.y);
                    }
                    
                    // Create virtual corners from min/max
                    targetCorners[0] = new Vector3(minScreen.x, minScreen.y); // Bottom left
                    targetCorners[1] = new Vector3(maxScreen.x, minScreen.y); // Bottom right
                    targetCorners[2] = new Vector3(maxScreen.x, maxScreen.y); // Top right
                    targetCorners[3] = new Vector3(minScreen.x, maxScreen.y); // Top left
                }
                else
                {
                    // Fallback to screen center
                    for (int i = 0; i < 4; i++)
                    {
                        targetCorners[i] = new Vector3(Screen.width / 2, Screen.height / 2);
                    }
                }
            }
            
            // Add padding to the target rect
            Vector2 min = new Vector2(
                Mathf.Min(targetCorners[0].x, targetCorners[1].x, targetCorners[2].x, targetCorners[3].x) - highlightPadding,
                Mathf.Min(targetCorners[0].y, targetCorners[1].y, targetCorners[2].y, targetCorners[3].y) - highlightPadding
            );
            
            Vector2 max = new Vector2(
                Mathf.Max(targetCorners[0].x, targetCorners[1].x, targetCorners[2].x, targetCorners[3].x) + highlightPadding,
                Mathf.Max(targetCorners[0].y, targetCorners[1].y, targetCorners[2].y, targetCorners[3].y) + highlightPadding
            );
            
            // Calculate size and position
            Vector2 size = max - min;
            Vector2 center = (min + max) / 2;
            
            // Convert to canvas space if needed
            Canvas canvas = highlightOverlay.GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.GetComponent<RectTransform>(),
                    center,
                    canvas.worldCamera,
                    out Vector2 localPoint
                );
                center = localPoint;
                
                // Scale size based on canvas scale
                size /= canvas.scaleFactor;
            }
            
            // Apply position and size
            highlightRect.position = center;
            highlightRect.sizeDelta = size;
            
            // Animate the highlight
            float time = 0;
            Color baseColor = highlightImage.color;
            
            while (true)
            {
                time += Time.deltaTime;
                float t = (time % highlightPulseDuration) / highlightPulseDuration;
                float alpha = highlightPulseCurve.Evaluate(t);
                
                highlightImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
                
                yield return null;
            }
        }
        
        private IEnumerator AutoAdvanceAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            AdvanceToNextStep();
        }
        
        private void CompleteCurrentStep()
        {
            if (currentStep == null)
                return;
                
            // Mark as completed
            completedSteps[currentStep.id] = true;
            SaveTutorialProgress();
            
            // Hide visual aid if there is one
            if (currentStep.visualAid != null)
            {
                currentStep.visualAid.SetActive(false);
            }
            
            // Invoke event
            OnTutorialStepCompleted?.Invoke(currentStep);
        }
        
        private void CompleteTutorial()
        {
            // Stop any running coroutines
            if (highlightCoroutine != null)
            {
                StopCoroutine(highlightCoroutine);
                highlightCoroutine = null;
            }
            
            if (autoAdvanceCoroutine != null)
            {
                StopCoroutine(autoAdvanceCoroutine);
                autoAdvanceCoroutine = null;
            }
            
            // Hide UI elements
            if (tutorialPanel != null)
            {
                if (panelAnimation != null)
                {
                    panelAnimation.Play(hideAnimationName);
                    Invoke(nameof(HideTutorialUI), animationDuration);
                }
                else
                {
                    HideTutorialUI();
                }
            }
            
            if (highlightOverlay != null)
            {
                highlightOverlay.SetActive(false);
            }
            
            if (pointer != null)
            {
                pointer.gameObject.SetActive(false);
            }
            
            // Reset state
            currentStep = null;
            isTutorialActive = false;
            
            // Invoke event
            OnTutorialCompleted?.Invoke();
        }
        
        private void HideTutorialUI()
        {
            if (tutorialPanel != null)
            {
                tutorialPanel.SetActive(false);
            }
        }
        
        public void SkipTutorial()
        {
            // Mark all steps as completed
            foreach (var step in tutorialSteps)
            {
                completedSteps[step.id] = true;
            }
            
            SaveTutorialProgress();
            CompleteTutorial();
        }
        
        public void ResetTutorial()
        {
            // Mark all steps as not completed
            foreach (var step in tutorialSteps)
            {
                completedSteps[step.id] = false;
            }
            
            SaveTutorialProgress();
        }
        
        public void TriggerEvent(string eventName)
        {
            if (!isTutorialActive || currentStep == null)
                return;
                
            if (currentStep.requireInteraction && currentStep.triggerEventName == eventName)
            {
                // Event matches the current step's trigger - advance
                AdvanceToNextStep();
            }
        }
        
        public void OnButtonClick(string buttonName)
        {
            TriggerEvent("button_" + buttonName);
        }
        
        public void OnCardPlaced(string cardId)
        {
            TriggerEvent("card_placed");
        }
        
        public void OnEnemyDefeated()
        {
            TriggerEvent("enemy_defeated");
        }
        
        public void OnScreenTouched()
        {
            TriggerEvent("screen_touched");
        }
    }
} 