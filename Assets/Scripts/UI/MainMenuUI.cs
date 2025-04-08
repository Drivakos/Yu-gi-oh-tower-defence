using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YuGiOhTowerDefense.Core;
using YuGiOhTowerDefense.UI;

namespace YuGiOhTowerDefense.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Menu Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject optionsPanel;
        [SerializeField] private GameObject creditsPanel;
        
        [Header("Main Menu Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button quitButton;
        
        [Header("Options Panel")]
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Button optionsBackButton;
        
        [Header("Credits Panel")]
        [SerializeField] private Button creditsBackButton;
        
        private LevelSelectionUI levelSelectionUI;
        private SaveLoadUI saveLoadUI;
        
        private void Start()
        {
            levelSelectionUI = FindObjectOfType<LevelSelectionUI>();
            saveLoadUI = FindObjectOfType<SaveLoadUI>();
            
            // Set up button listeners
            playButton.onClick.AddListener(OnPlayButtonClicked);
            optionsButton.onClick.AddListener(OnOptionsButtonClicked);
            creditsButton.onClick.AddListener(OnCreditsButtonClicked);
            quitButton.onClick.AddListener(OnQuitButtonClicked);
            
            optionsBackButton.onClick.AddListener(OnOptionsBackButtonClicked);
            creditsBackButton.onClick.AddListener(OnCreditsBackButtonClicked);
            
            // Set up options
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggled);
            
            // Load saved settings
            LoadSettings();
            
            // Show main menu
            ShowMainMenu();
        }
        
        private void OnPlayButtonClicked()
        {
            mainMenuPanel.SetActive(false);
            levelSelectionUI.ShowLevelSelection();
        }
        
        private void OnOptionsButtonClicked()
        {
            mainMenuPanel.SetActive(false);
            optionsPanel.SetActive(true);
        }
        
        private void OnCreditsButtonClicked()
        {
            mainMenuPanel.SetActive(false);
            creditsPanel.SetActive(true);
        }
        
        private void OnQuitButtonClicked()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
        
        private void OnOptionsBackButtonClicked()
        {
            optionsPanel.SetActive(false);
            mainMenuPanel.SetActive(true);
        }
        
        private void OnCreditsBackButtonClicked()
        {
            creditsPanel.SetActive(false);
            mainMenuPanel.SetActive(true);
        }
        
        private void OnMusicVolumeChanged(float value)
        {
            // Set music volume
            PlayerPrefs.SetFloat("MusicVolume", value);
            PlayerPrefs.Save();
        }
        
        private void OnSFXVolumeChanged(float value)
        {
            // Set SFX volume
            PlayerPrefs.SetFloat("SFXVolume", value);
            PlayerPrefs.Save();
        }
        
        private void OnFullscreenToggled(bool isFullscreen)
        {
            // Set fullscreen mode
            Screen.fullScreen = isFullscreen;
            PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
            PlayerPrefs.Save();
        }
        
        private void LoadSettings()
        {
            // Load music volume
            float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
            musicVolumeSlider.value = musicVolume;
            
            // Load SFX volume
            float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1.0f);
            sfxVolumeSlider.value = sfxVolume;
            
            // Load fullscreen setting
            bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
            fullscreenToggle.isOn = isFullscreen;
            Screen.fullScreen = isFullscreen;
        }
        
        public void ShowMainMenu()
        {
            mainMenuPanel.SetActive(true);
            optionsPanel.SetActive(false);
            creditsPanel.SetActive(false);
            
            if (levelSelectionUI != null)
            {
                // Hide level selection if it's visible
                levelSelectionUI.gameObject.SetActive(false);
            }
            
            if (saveLoadUI != null)
            {
                // Hide save/load UI if it's visible
                saveLoadUI.gameObject.SetActive(false);
            }
        }
    }
} 