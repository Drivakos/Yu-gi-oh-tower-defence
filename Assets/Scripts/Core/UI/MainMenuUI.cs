using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YuGiOhTowerDefense.Managers;

namespace YuGiOhTowerDefense.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Menu Buttons")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button deckBuilderButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("Deck Selection")]
        [SerializeField] private GameObject deckSelectionPanel;
        [SerializeField] private Transform deckListContainer;
        [SerializeField] private GameObject deckButtonPrefab;
        [SerializeField] private Button backButton;

        [Header("Settings")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Toggle fullscreenToggle;

        private void Start()
        {
            InitializeButtons();
            InitializeSettings();
        }

        private void InitializeButtons()
        {
            startButton.onClick.AddListener(OnStartClicked);
            deckBuilderButton.onClick.AddListener(OnDeckBuilderClicked);
            settingsButton.onClick.AddListener(OnSettingsClicked);
            quitButton.onClick.AddListener(OnQuitClicked);
            backButton.onClick.AddListener(OnBackClicked);
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

        #region Button Handlers
        private void OnStartClicked()
        {
            // TODO: Load selected deck
            UIManager.Instance.ShowGameplay();
        }

        private void OnDeckBuilderClicked()
        {
            deckSelectionPanel.SetActive(true);
            // TODO: Populate deck list
        }

        private void OnSettingsClicked()
        {
            settingsPanel.SetActive(true);
        }

        private void OnQuitClicked()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        private void OnBackClicked()
        {
            deckSelectionPanel.SetActive(false);
            settingsPanel.SetActive(false);
        }
        #endregion

        #region Settings Handlers
        private void OnMusicVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat("MusicVolume", value);
            // TODO: Update music volume
        }

        private void OnSFXVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat("SFXVolume", value);
            // TODO: Update SFX volume
        }

        private void OnFullscreenChanged(bool value)
        {
            PlayerPrefs.SetInt("Fullscreen", value ? 1 : 0);
            Screen.fullScreen = value;
        }
        #endregion

        private void OnDestroy()
        {
            // Save settings
            PlayerPrefs.Save();
        }
    }
} 