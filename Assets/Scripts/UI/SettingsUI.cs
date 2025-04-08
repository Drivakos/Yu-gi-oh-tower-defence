using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YuGiOhTowerDefense.Core;

namespace YuGiOhTowerDefense.UI
{
    public class SettingsUI : MonoBehaviour
    {
        [Header("Panel Settings")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button applyButton;
        [SerializeField] private Button resetButton;
        
        [Header("Audio Settings")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Toggle muteToggle;
        
        [Header("Graphics Settings")]
        [SerializeField] private TMP_Dropdown qualityDropdown;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private TMP_Dropdown resolutionDropdown;
        [SerializeField] private Slider brightnessSlider;
        
        [Header("Gameplay Settings")]
        [SerializeField] private Toggle tutorialToggle;
        [SerializeField] private Toggle vibrationToggle;
        [SerializeField] private Slider animationSpeedSlider;
        
        [Header("Language Settings")]
        [SerializeField] private TMP_Dropdown languageDropdown;
        
        // References
        private AudioManager audioManager;
        private SaveManager saveManager;
        
        // Cached Settings
        private float masterVolume;
        private float musicVolume;
        private float sfxVolume;
        private bool isMuted;
        private int graphicsQuality;
        private bool isFullscreen;
        private int resolutionIndex;
        private float brightness;
        private bool tutorialEnabled;
        private bool vibrationEnabled;
        private float animationSpeed;
        private string languageCode;
        
        private Resolution[] availableResolutions;
        
        private void Awake()
        {
            audioManager = AudioManager.Instance;
            saveManager = SaveManager.Instance;
            
            // Setup UI elements
            SetupUI();
            
            // Hide panel by default
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
        }
        
        private void OnEnable()
        {
            LoadSettings();
            UpdateUI();
        }
        
        private void SetupUI()
        {
            // Button listeners
            if (closeButton != null)
                closeButton.onClick.AddListener(CloseSettings);
            
            if (applyButton != null)
                applyButton.onClick.AddListener(ApplySettings);
            
            if (resetButton != null)
                resetButton.onClick.AddListener(ResetSettings);
            
            // Audio settings listeners
            if (masterVolumeSlider != null)
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            
            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            
            if (muteToggle != null)
                muteToggle.onValueChanged.AddListener(OnMuteToggled);
            
            // Graphics settings listeners
            if (qualityDropdown != null)
            {
                qualityDropdown.ClearOptions();
                qualityDropdown.AddOptions(new List<string>(QualitySettings.names));
                qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
            }
            
            if (fullscreenToggle != null)
                fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggled);
            
            if (resolutionDropdown != null)
            {
                SetupResolutions();
                resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
            }
            
            if (brightnessSlider != null)
                brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
            
            // Gameplay settings listeners
            if (tutorialToggle != null)
                tutorialToggle.onValueChanged.AddListener(OnTutorialToggled);
            
            if (vibrationToggle != null)
                vibrationToggle.onValueChanged.AddListener(OnVibrationToggled);
            
            if (animationSpeedSlider != null)
                animationSpeedSlider.onValueChanged.AddListener(OnAnimationSpeedChanged);
            
            // Language settings
            if (languageDropdown != null)
            {
                languageDropdown.ClearOptions();
                languageDropdown.AddOptions(new List<string> { "English", "Spanish", "French", "German", "Japanese", "Chinese" });
                languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
            }
        }
        
        private void SetupResolutions()
        {
            if (resolutionDropdown == null)
                return;
            
            availableResolutions = Screen.resolutions;
            resolutionDropdown.ClearOptions();
            
            List<string> resolutionOptions = new List<string>();
            int currentResolutionIndex = 0;
            
            for (int i = 0; i < availableResolutions.Length; i++)
            {
                string option = availableResolutions[i].width + " x " + availableResolutions[i].height;
                resolutionOptions.Add(option);
                
                if (availableResolutions[i].width == Screen.currentResolution.width &&
                    availableResolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = i;
                }
            }
            
            resolutionDropdown.AddOptions(resolutionOptions);
            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.RefreshShownValue();
            
            resolutionIndex = currentResolutionIndex;
        }
        
        private void LoadSettings()
        {
            // Load audio settings
            masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            isMuted = PlayerPrefs.GetInt("AudioMuted", 0) == 1;
            
            // Load graphics settings
            graphicsQuality = PlayerPrefs.GetInt("GraphicsQuality", QualitySettings.GetQualityLevel());
            isFullscreen = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;
            resolutionIndex = PlayerPrefs.GetInt("Resolution", 0);
            brightness = PlayerPrefs.GetFloat("Brightness", 1f);
            
            // Load gameplay settings
            tutorialEnabled = PlayerPrefs.GetInt("TutorialEnabled", 1) == 1;
            vibrationEnabled = PlayerPrefs.GetInt("VibrationEnabled", 1) == 1;
            animationSpeed = PlayerPrefs.GetFloat("AnimationSpeed", 1f);
            
            // Load language settings
            languageCode = PlayerPrefs.GetString("LanguageCode", "en");
        }
        
        private void UpdateUI()
        {
            // Update audio UI
            if (masterVolumeSlider != null)
                masterVolumeSlider.value = masterVolume;
            
            if (musicVolumeSlider != null)
                musicVolumeSlider.value = musicVolume;
            
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.value = sfxVolume;
            
            if (muteToggle != null)
                muteToggle.isOn = isMuted;
            
            // Update graphics UI
            if (qualityDropdown != null)
                qualityDropdown.value = graphicsQuality;
            
            if (fullscreenToggle != null)
                fullscreenToggle.isOn = isFullscreen;
            
            if (resolutionDropdown != null && resolutionIndex < resolutionDropdown.options.Count)
                resolutionDropdown.value = resolutionIndex;
            
            if (brightnessSlider != null)
                brightnessSlider.value = brightness;
            
            // Update gameplay UI
            if (tutorialToggle != null)
                tutorialToggle.isOn = tutorialEnabled;
            
            if (vibrationToggle != null)
                vibrationToggle.isOn = vibrationEnabled;
            
            if (animationSpeedSlider != null)
                animationSpeedSlider.value = animationSpeed;
            
            // Update language UI
            if (languageDropdown != null)
            {
                int langIndex = 0;
                switch (languageCode)
                {
                    case "en": langIndex = 0; break;
                    case "es": langIndex = 1; break;
                    case "fr": langIndex = 2; break;
                    case "de": langIndex = 3; break;
                    case "ja": langIndex = 4; break;
                    case "zh": langIndex = 5; break;
                }
                languageDropdown.value = langIndex;
            }
        }
        
        public void ShowSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
            }
            
            LoadSettings();
            UpdateUI();
        }
        
        public void CloseSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
        }
        
        public void ApplySettings()
        {
            // Apply audio settings
            if (audioManager != null)
            {
                audioManager.SetCategoryVolume(AudioCategory.Master, isMuted ? 0f : masterVolume);
                audioManager.SetCategoryVolume(AudioCategory.Music, isMuted ? 0f : musicVolume);
                audioManager.SetCategoryVolume(AudioCategory.SFX, isMuted ? 0f : sfxVolume);
                audioManager.SaveVolumeSettings();
            }
            
            // Apply graphics settings
            QualitySettings.SetQualityLevel(graphicsQuality);
            Screen.fullScreen = isFullscreen;
            
            if (resolutionIndex < availableResolutions.Length)
            {
                Resolution resolution = availableResolutions[resolutionIndex];
                Screen.SetResolution(resolution.width, resolution.height, isFullscreen);
            }
            
            // TODO: Apply brightness
            // Would normally use post-processing or a global brightness material
            
            // Save settings to PlayerPrefs
            PlayerPrefs.SetFloat("MasterVolume", masterVolume);
            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
            PlayerPrefs.SetInt("AudioMuted", isMuted ? 1 : 0);
            
            PlayerPrefs.SetInt("GraphicsQuality", graphicsQuality);
            PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
            PlayerPrefs.SetInt("Resolution", resolutionIndex);
            PlayerPrefs.SetFloat("Brightness", brightness);
            
            PlayerPrefs.SetInt("TutorialEnabled", tutorialEnabled ? 1 : 0);
            PlayerPrefs.SetInt("VibrationEnabled", vibrationEnabled ? 1 : 0);
            PlayerPrefs.SetFloat("AnimationSpeed", animationSpeed);
            
            PlayerPrefs.SetString("LanguageCode", languageCode);
            
            PlayerPrefs.Save();
            
            // Save to game save if available
            if (saveManager != null)
            {
                saveManager.SaveGame();
            }
            
            // Play sound effect
            if (audioManager != null)
            {
                audioManager.PlayButtonClickSound();
            }
            
            // Close settings panel
            CloseSettings();
        }
        
        public void ResetSettings()
        {
            // Reset to default values
            masterVolume = 1f;
            musicVolume = 0.8f;
            sfxVolume = 1f;
            isMuted = false;
            
            graphicsQuality = 1; // Medium
            isFullscreen = true;
            resolutionIndex = availableResolutions.Length - 1; // Highest available
            brightness = 1f;
            
            tutorialEnabled = true;
            vibrationEnabled = true;
            animationSpeed = 1f;
            
            languageCode = "en";
            
            // Update UI
            UpdateUI();
            
            // Apply default settings right away
            ApplySettings();
            
            // Play sound effect
            if (audioManager != null)
            {
                audioManager.PlayButtonClickSound();
            }
        }
        
        private void OnMasterVolumeChanged(float value)
        {
            masterVolume = value;
            if (audioManager != null)
            {
                audioManager.SetCategoryVolume(AudioCategory.Master, isMuted ? 0f : value);
            }
        }
        
        private void OnMusicVolumeChanged(float value)
        {
            musicVolume = value;
            if (audioManager != null)
            {
                audioManager.SetCategoryVolume(AudioCategory.Music, isMuted ? 0f : value);
            }
        }
        
        private void OnSFXVolumeChanged(float value)
        {
            sfxVolume = value;
            if (audioManager != null)
            {
                audioManager.SetCategoryVolume(AudioCategory.SFX, isMuted ? 0f : value);
                
                // Play a test sound to hear the new volume
                audioManager.PlayButtonClickSound();
            }
        }
        
        private void OnMuteToggled(bool value)
        {
            isMuted = value;
            if (audioManager != null)
            {
                audioManager.SetCategoryVolume(AudioCategory.Master, isMuted ? 0f : masterVolume);
                audioManager.SetCategoryVolume(AudioCategory.Music, isMuted ? 0f : musicVolume);
                audioManager.SetCategoryVolume(AudioCategory.SFX, isMuted ? 0f : sfxVolume);
            }
        }
        
        private void OnQualityChanged(int index)
        {
            graphicsQuality = index;
        }
        
        private void OnFullscreenToggled(bool value)
        {
            isFullscreen = value;
        }
        
        private void OnResolutionChanged(int index)
        {
            resolutionIndex = index;
        }
        
        private void OnBrightnessChanged(float value)
        {
            brightness = value;
        }
        
        private void OnTutorialToggled(bool value)
        {
            tutorialEnabled = value;
        }
        
        private void OnVibrationToggled(bool value)
        {
            vibrationEnabled = value;
        }
        
        private void OnAnimationSpeedChanged(float value)
        {
            animationSpeed = value;
        }
        
        private void OnLanguageChanged(int index)
        {
            switch (index)
            {
                case 0: languageCode = "en"; break;
                case 1: languageCode = "es"; break;
                case 2: languageCode = "fr"; break;
                case 3: languageCode = "de"; break;
                case 4: languageCode = "ja"; break;
                case 5: languageCode = "zh"; break;
                default: languageCode = "en"; break;
            }
        }
        
        public void PlayButtonClickSound()
        {
            if (audioManager != null)
            {
                audioManager.PlayButtonClickSound();
            }
        }
    }
} 