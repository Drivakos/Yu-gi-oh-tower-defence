using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace YuGiOhTowerDefense.Core.Accessibility
{
    public class AccessibilityManager : MonoBehaviour
    {
        public static AccessibilityManager Instance { get; private set; }
        
        [System.Serializable]
        public class AccessibilitySettings
        {
            public float textSizeMultiplier = 1f;
            public float uiScaleMultiplier = 1f;
            public bool highContrastMode = false;
            public bool screenReaderEnabled = false;
            public bool colorBlindMode = false;
            public ColorBlindMode colorBlindType = ColorBlindMode.None;
            public bool subtitlesEnabled = true;
            public float subtitleSize = 1f;
            public bool reducedMotion = false;
        }
        
        public enum ColorBlindMode
        {
            None,
            Protanopia,
            Deuteranopia,
            Tritanopia
        }
        
        [SerializeField] private AccessibilitySettings defaultSettings;
        [SerializeField] private Color[] highContrastColors;
        [SerializeField] private ColorBlindFilter[] colorBlindFilters;
        
        private AccessibilitySettings currentSettings;
        private List<TextMeshProUGUI> textElements = new List<TextMeshProUGUI>();
        private List<Image> uiImages = new List<Image>();
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadSettings();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            FindAllUIElements();
            ApplySettings();
        }
        
        public void LoadSettings()
        {
            // TODO: Load settings from PlayerPrefs or save file
            currentSettings = new AccessibilitySettings
            {
                textSizeMultiplier = PlayerPrefs.GetFloat("TextSize", defaultSettings.textSizeMultiplier),
                uiScaleMultiplier = PlayerPrefs.GetFloat("UIScale", defaultSettings.uiScaleMultiplier),
                highContrastMode = PlayerPrefs.GetInt("HighContrast", defaultSettings.highContrastMode ? 1 : 0) == 1,
                screenReaderEnabled = PlayerPrefs.GetInt("ScreenReader", defaultSettings.screenReaderEnabled ? 1 : 0) == 1,
                colorBlindMode = PlayerPrefs.GetInt("ColorBlind", defaultSettings.colorBlindMode ? 1 : 0) == 1,
                colorBlindType = (ColorBlindMode)PlayerPrefs.GetInt("ColorBlindType", (int)defaultSettings.colorBlindType),
                subtitlesEnabled = PlayerPrefs.GetInt("Subtitles", defaultSettings.subtitlesEnabled ? 1 : 0) == 1,
                subtitleSize = PlayerPrefs.GetFloat("SubtitleSize", defaultSettings.subtitleSize),
                reducedMotion = PlayerPrefs.GetInt("ReducedMotion", defaultSettings.reducedMotion ? 1 : 0) == 1
            };
        }
        
        public void SaveSettings()
        {
            PlayerPrefs.SetFloat("TextSize", currentSettings.textSizeMultiplier);
            PlayerPrefs.SetFloat("UIScale", currentSettings.uiScaleMultiplier);
            PlayerPrefs.SetInt("HighContrast", currentSettings.highContrastMode ? 1 : 0);
            PlayerPrefs.SetInt("ScreenReader", currentSettings.screenReaderEnabled ? 1 : 0);
            PlayerPrefs.SetInt("ColorBlind", currentSettings.colorBlindMode ? 1 : 0);
            PlayerPrefs.SetInt("ColorBlindType", (int)currentSettings.colorBlindType);
            PlayerPrefs.SetInt("Subtitles", currentSettings.subtitlesEnabled ? 1 : 0);
            PlayerPrefs.SetFloat("SubtitleSize", currentSettings.subtitleSize);
            PlayerPrefs.SetInt("ReducedMotion", currentSettings.reducedMotion ? 1 : 0);
            PlayerPrefs.Save();
        }
        
        private void FindAllUIElements()
        {
            textElements.Clear();
            uiImages.Clear();
            
            textElements.AddRange(FindObjectsOfType<TextMeshProUGUI>());
            uiImages.AddRange(FindObjectsOfType<Image>());
        }
        
        public void ApplySettings()
        {
            ApplyTextSize();
            ApplyUIScale();
            ApplyHighContrast();
            ApplyColorBlindMode();
            ApplyReducedMotion();
        }
        
        private void ApplyTextSize()
        {
            foreach (var text in textElements)
            {
                if (text != null)
                {
                    text.fontSize *= currentSettings.textSizeMultiplier;
                }
            }
        }
        
        private void ApplyUIScale()
        {
            foreach (var image in uiImages)
            {
                if (image != null)
                {
                    image.rectTransform.localScale *= currentSettings.uiScaleMultiplier;
                }
            }
        }
        
        private void ApplyHighContrast()
        {
            if (!currentSettings.highContrastMode) return;
            
            foreach (var image in uiImages)
            {
                if (image != null)
                {
                    // Apply high contrast colors based on image type
                    // This is a simplified example - you'd need to categorize UI elements
                    image.color = highContrastColors[0]; // Default high contrast color
                }
            }
        }
        
        private void ApplyColorBlindMode()
        {
            if (!currentSettings.colorBlindMode) return;
            
            // Apply color blind filter based on selected type
            if (currentSettings.colorBlindType != ColorBlindMode.None)
            {
                int filterIndex = (int)currentSettings.colorBlindType - 1;
                if (filterIndex >= 0 && filterIndex < colorBlindFilters.Length)
                {
                    colorBlindFilters[filterIndex].enabled = true;
                }
            }
        }
        
        private void ApplyReducedMotion()
        {
            // Disable or reduce animation speeds
            var animators = FindObjectsOfType<Animator>();
            foreach (var animator in animators)
            {
                if (animator != null)
                {
                    animator.speed = currentSettings.reducedMotion ? 0.5f : 1f;
                }
            }
        }
        
        public void SpeakText(string text)
        {
            if (currentSettings.screenReaderEnabled)
            {
                // TODO: Implement text-to-speech functionality
                Debug.Log($"Screen reader: {text}");
            }
        }
        
        public void ShowSubtitle(string text, float duration)
        {
            if (currentSettings.subtitlesEnabled)
            {
                // TODO: Implement subtitle display system
                Debug.Log($"Subtitle: {text}");
            }
        }
        
        public void RegisterTextElement(TextMeshProUGUI text)
        {
            if (!textElements.Contains(text))
            {
                textElements.Add(text);
                text.fontSize *= currentSettings.textSizeMultiplier;
            }
        }
        
        public void RegisterUIImage(Image image)
        {
            if (!uiImages.Contains(image))
            {
                uiImages.Add(image);
                image.rectTransform.localScale *= currentSettings.uiScaleMultiplier;
                
                if (currentSettings.highContrastMode)
                {
                    image.color = highContrastColors[0];
                }
            }
        }
    }
} 