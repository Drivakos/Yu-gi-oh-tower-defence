using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace YuGiOhTowerDefense.UI
{
    public class PauseMenuPrefab : MonoBehaviour
    {
        [Header("Background")]
        [SerializeField] private Image background;
        [SerializeField] private float backgroundFadeSpeed = 5f;

        [Header("Main Panel")]
        [SerializeField] private Image mainPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button settingsButton;

        [Header("Settings Panel")]
        [SerializeField] private Image settingsPanel;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Button backButton;

        [Header("Animations")]
        [SerializeField] private float buttonHoverScale = 1.1f;
        [SerializeField] private float buttonClickScale = 0.9f;
        [SerializeField] private float panelSlideSpeed = 10f;

        private void Start()
        {
            InitializeButtons();
            InitializeSettings();
            ShowMainPanel();
        }

        private void InitializeButtons()
        {
            // Add hover and click effects to all buttons
            AddButtonEffects(resumeButton);
            AddButtonEffects(restartButton);
            AddButtonEffects(mainMenuButton);
            AddButtonEffects(settingsButton);
            AddButtonEffects(backButton);
        }

        private void AddButtonEffects(Button button)
        {
            button.onClick.AddListener(() => {
                StartCoroutine(AnimateButtonClick(button));
            });
        }

        private System.Collections.IEnumerator AnimateButtonClick(Button button)
        {
            RectTransform rectTransform = button.GetComponent<RectTransform>();
            Vector3 originalScale = rectTransform.localScale;

            // Click animation
            rectTransform.localScale = originalScale * buttonClickScale;
            yield return new WaitForSeconds(0.1f);
            rectTransform.localScale = originalScale;
        }

        private void InitializeSettings()
        {
            // Load saved settings
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
            fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;

            // Add listeners
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        }

        private void OnMusicVolumeChanged(float value)
        {
            // Update music volume
            PlayerPrefs.SetFloat("MusicVolume", value);
        }

        private void OnSFXVolumeChanged(float value)
        {
            // Update SFX volume
            PlayerPrefs.SetFloat("SFXVolume", value);
        }

        private void OnFullscreenChanged(bool value)
        {
            Screen.fullScreen = value;
            PlayerPrefs.SetInt("Fullscreen", value ? 1 : 0);
        }

        public void ShowMainPanel()
        {
            StartCoroutine(AnimatePanelTransition(mainPanel, true));
            StartCoroutine(AnimatePanelTransition(settingsPanel, false));
        }

        public void ShowSettingsPanel()
        {
            StartCoroutine(AnimatePanelTransition(mainPanel, false));
            StartCoroutine(AnimatePanelTransition(settingsPanel, true));
        }

        private System.Collections.IEnumerator AnimatePanelTransition(Image panel, bool show)
        {
            Vector3 targetScale = show ? Vector3.one : Vector3.zero;
            Vector3 currentScale = panel.transform.localScale;

            while (Vector3.Distance(currentScale, targetScale) > 0.01f)
            {
                currentScale = Vector3.Lerp(currentScale, targetScale, Time.deltaTime * panelSlideSpeed);
                panel.transform.localScale = currentScale;
                yield return null;
            }

            panel.transform.localScale = targetScale;
            panel.gameObject.SetActive(show);
        }

        public void FadeBackground(bool fadeIn)
        {
            StartCoroutine(AnimateBackgroundFade(fadeIn));
        }

        private System.Collections.IEnumerator AnimateBackgroundFade(bool fadeIn)
        {
            float targetAlpha = fadeIn ? 0.8f : 0f;
            Color currentColor = background.color;
            Color targetColor = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);

            while (Mathf.Abs(currentColor.a - targetAlpha) > 0.01f)
            {
                currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * backgroundFadeSpeed);
                background.color = currentColor;
                yield return null;
            }

            background.color = targetColor;
        }
    }
} 