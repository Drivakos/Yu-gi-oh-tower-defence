using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace YuGiOhTowerDefense.UI
{
    /// <summary>
    /// Handles the game over menu UI and its animations.
    /// </summary>
    public class GameOverMenuPrefab : MonoBehaviour
    {
        [Header("Background")]
        [SerializeField] private Image background;
        [SerializeField, Range(1f, 10f)] private float backgroundFadeSpeed = 5f;

        [Header("Main Panel")]
        [SerializeField] private Image mainPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI waveText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Animations")]
        [SerializeField, Range(1f, 2f)] private float buttonHoverScale = 1.1f;
        [SerializeField, Range(0.1f, 1f)] private float buttonClickScale = 0.9f;
        [SerializeField, Range(1f, 20f)] private float panelSlideSpeed = 10f;

        private const float BACKGROUND_ALPHA = 0.8f;
        private const float ANIMATION_THRESHOLD = 0.01f;
        private const float BUTTON_ANIMATION_DURATION = 0.1f;

        private void Awake()
        {
            ValidateComponents();
        }

        private void OnEnable()
        {
            InitializeButtons();
        }

        private void OnDisable()
        {
            UnsubscribeButtons();
        }

        /// <summary>
        /// Validates all required components are assigned.
        /// </summary>
        private void ValidateComponents()
        {
            if (background == null)
                throw new MissingComponentException($"{nameof(background)} is not assigned in {nameof(GameOverMenuPrefab)}");
            if (mainPanel == null)
                throw new MissingComponentException($"{nameof(mainPanel)} is not assigned in {nameof(GameOverMenuPrefab)}");
            if (titleText == null)
                throw new MissingComponentException($"{nameof(titleText)} is not assigned in {nameof(GameOverMenuPrefab)}");
            if (waveText == null)
                throw new MissingComponentException($"{nameof(waveText)} is not assigned in {nameof(GameOverMenuPrefab)}");
            if (scoreText == null)
                throw new MissingComponentException($"{nameof(scoreText)} is not assigned in {nameof(GameOverMenuPrefab)}");
            if (restartButton == null)
                throw new MissingComponentException($"{nameof(restartButton)} is not assigned in {nameof(GameOverMenuPrefab)}");
            if (mainMenuButton == null)
                throw new MissingComponentException($"{nameof(mainMenuButton)} is not assigned in {nameof(GameOverMenuPrefab)}");
        }

        /// <summary>
        /// Initializes button click effects.
        /// </summary>
        private void InitializeButtons()
        {
            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartButtonClick);
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenuButtonClick);
        }

        /// <summary>
        /// Unsubscribes from button events.
        /// </summary>
        private void UnsubscribeButtons()
        {
            if (restartButton != null)
                restartButton.onClick.RemoveListener(OnRestartButtonClick);
            if (mainMenuButton != null)
                mainMenuButton.onClick.RemoveListener(OnMainMenuButtonClick);
        }

        private void OnRestartButtonClick()
        {
            StartCoroutine(AnimateButtonClick(restartButton));
            // TODO: Implement restart logic
        }

        private void OnMainMenuButtonClick()
        {
            StartCoroutine(AnimateButtonClick(mainMenuButton));
            // TODO: Implement main menu logic
        }

        /// <summary>
        /// Animates button click effect.
        /// </summary>
        private System.Collections.IEnumerator AnimateButtonClick(Button button)
        {
            if (button == null) yield break;

            RectTransform rectTransform = button.GetComponent<RectTransform>();
            if (rectTransform == null) yield break;

            Vector3 originalScale = rectTransform.localScale;
            rectTransform.localScale = originalScale * buttonClickScale;
            
            yield return new WaitForSeconds(BUTTON_ANIMATION_DURATION);
            
            if (rectTransform != null)
                rectTransform.localScale = originalScale;
        }

        /// <summary>
        /// Shows the main panel with animation.
        /// </summary>
        public void ShowMainPanel()
        {
            if (mainPanel != null)
                StartCoroutine(AnimatePanelTransition(mainPanel, true));
        }

        /// <summary>
        /// Hides the main panel with animation.
        /// </summary>
        public void HideMainPanel()
        {
            if (mainPanel != null)
                StartCoroutine(AnimatePanelTransition(mainPanel, false));
        }

        /// <summary>
        /// Animates panel transition.
        /// </summary>
        private System.Collections.IEnumerator AnimatePanelTransition(Image panel, bool show)
        {
            if (panel == null) yield break;

            Vector3 targetScale = show ? Vector3.one : Vector3.zero;
            Vector3 currentScale = panel.transform.localScale;

            while (Vector3.Distance(currentScale, targetScale) > ANIMATION_THRESHOLD)
            {
                if (panel == null) yield break;
                
                currentScale = Vector3.Lerp(currentScale, targetScale, Time.deltaTime * panelSlideSpeed);
                panel.transform.localScale = currentScale;
                yield return null;
            }

            if (panel != null)
            {
                panel.transform.localScale = targetScale;
                panel.gameObject.SetActive(show);
            }
        }

        /// <summary>
        /// Updates game over information display.
        /// </summary>
        /// <param name="wave">The wave number reached</param>
        /// <param name="score">The final score achieved</param>
        public void UpdateGameOverInfo(int wave, int score)
        {
            if (waveText != null)
                waveText.text = $"Wave Reached: {Math.Max(0, wave)}";
            if (scoreText != null)
                scoreText.text = $"Final Score: {Math.Max(0, score)}";
        }

        /// <summary>
        /// Fades the background in or out.
        /// </summary>
        /// <param name="fadeIn">Whether to fade in or out</param>
        public void FadeBackground(bool fadeIn)
        {
            if (background != null)
                StartCoroutine(AnimateBackgroundFade(fadeIn));
        }

        /// <summary>
        /// Animates background fade.
        /// </summary>
        private System.Collections.IEnumerator AnimateBackgroundFade(bool fadeIn)
        {
            if (background == null) yield break;

            float targetAlpha = fadeIn ? BACKGROUND_ALPHA : 0f;
            Color currentColor = background.color;
            Color targetColor = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);

            while (Mathf.Abs(currentColor.a - targetAlpha) > ANIMATION_THRESHOLD)
            {
                if (background == null) yield break;
                
                currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * backgroundFadeSpeed);
                background.color = currentColor;
                yield return null;
            }

            if (background != null)
                background.color = targetColor;
        }
    }
} 