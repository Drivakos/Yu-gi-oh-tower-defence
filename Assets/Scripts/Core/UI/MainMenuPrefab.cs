using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace YuGiOhTowerDefense.UI
{
    public class MainMenuPrefab : MonoBehaviour
    {
        [Header("Background")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private GameObject titleText;

        [Header("Main Buttons")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button deckBuilderButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("Deck Selection Panel")]
        [SerializeField] private GameObject deckSelectionPanel;
        [SerializeField] private TextMeshProUGUI deckSelectionTitle;
        [SerializeField] private Transform deckListContainer;
        [SerializeField] private Button createNewDeckButton;
        [SerializeField] private Button backButton;

        [Header("Settings Panel")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private TextMeshProUGUI settingsTitle;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private TextMeshProUGUI musicVolumeText;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private TextMeshProUGUI sfxVolumeText;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Button settingsBackButton;

        [Header("Animations")]
        [SerializeField] private float buttonHoverScale = 1.1f;
        [SerializeField] private float buttonClickScale = 0.9f;
        [SerializeField] private float animationSpeed = 10f;

        private void Start()
        {
            InitializeButtons();
            InitializeSettings();
        }

        private void InitializeButtons()
        {
            // Add hover and click effects to all buttons
            Button[] buttons = GetComponentsInChildren<Button>();
            foreach (Button button in buttons)
            {
                AddButtonEffects(button);
            }
        }

        private void AddButtonEffects(Button button)
        {
            // Add hover effect
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
            // Update volume text when sliders change
            musicVolumeSlider.onValueChanged.AddListener(value => {
                musicVolumeText.text = $"Music Volume: {Mathf.RoundToInt(value * 100)}%";
            });

            sfxVolumeSlider.onValueChanged.AddListener(value => {
                sfxVolumeText.text = $"SFX Volume: {Mathf.RoundToInt(value * 100)}%";
            });
        }

        public void ShowDeckSelection()
        {
            deckSelectionPanel.SetActive(true);
            settingsPanel.SetActive(false);
        }

        public void ShowSettings()
        {
            settingsPanel.SetActive(true);
            deckSelectionPanel.SetActive(false);
        }

        public void HidePanels()
        {
            deckSelectionPanel.SetActive(false);
            settingsPanel.SetActive(false);
        }
    }
} 