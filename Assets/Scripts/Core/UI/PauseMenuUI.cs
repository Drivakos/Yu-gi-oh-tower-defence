using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YuGiOhTowerDefense.Managers;

namespace YuGiOhTowerDefense.UI
{
    public class PauseMenuUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button settingsButton;

        [Header("Settings Panel")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Button backButton;

        private void Start()
        {
            InitializeButtons();
            InitializeSettings();
        }

        private void InitializeButtons()
        {
            resumeButton.onClick.AddListener(OnResumeClicked);
            restartButton.onClick.AddListener(OnRestartClicked);
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            settingsButton.onClick.AddListener(OnSettingsClicked);
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
        private void OnResumeClicked()
        {
            UIManager.Instance.TogglePauseMenu();
        }

        private void OnRestartClicked()
        {
            // TODO: Implement game restart
            Debug.Log("Restart game");
        }

        private void OnMainMenuClicked()
        {
            UIManager.Instance.ShowMainMenu();
        }

        private void OnSettingsClicked()
        {
            settingsPanel.SetActive(true);
        }

        private void OnBackClicked()
        {
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