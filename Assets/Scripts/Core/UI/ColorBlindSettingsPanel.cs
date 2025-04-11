using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YuGiOhTowerDefense.Core.Accessibility;

namespace YuGiOhTowerDefense.Core.UI
{
    public class ColorBlindSettingsPanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Toggle enableToggle;
        [SerializeField] private TMP_Dropdown filterTypeDropdown;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button applyButton;
        [SerializeField] private Button resetButton;
        
        [Header("Preview")]
        [SerializeField] private Image previewImage;
        [SerializeField] private Sprite[] previewSprites;
        
        private ColorBlindFilter colorBlindFilter;
        private AccessibilityManager accessibilityManager;
        
        private void Awake()
        {
            colorBlindFilter = FindObjectOfType<ColorBlindFilter>();
            accessibilityManager = AccessibilityManager.Instance;
            
            if (colorBlindFilter == null)
            {
                Debug.LogError("ColorBlindFilter component not found in scene!");
                enabled = false;
                return;
            }
            
            InitializeUI();
        }
        
        private void InitializeUI()
        {
            // Initialize dropdown options
            filterTypeDropdown.ClearOptions();
            filterTypeDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "None",
                "Protanopia",
                "Deuteranopia",
                "Tritanopia"
            });
            
            // Load current settings
            LoadCurrentSettings();
            
            // Add listeners
            enableToggle.onValueChanged.AddListener(OnEnableToggleChanged);
            filterTypeDropdown.onValueChanged.AddListener(OnFilterTypeChanged);
            closeButton.onClick.AddListener(OnCloseClicked);
            applyButton.onClick.AddListener(OnApplyClicked);
            resetButton.onClick.AddListener(OnResetClicked);
            
            // Update preview
            UpdatePreview();
        }
        
        private void LoadCurrentSettings()
        {
            enableToggle.isOn = accessibilityManager.CurrentSettings.colorBlindMode;
            filterTypeDropdown.value = (int)accessibilityManager.CurrentSettings.colorBlindType;
        }
        
        private void OnEnableToggleChanged(bool isEnabled)
        {
            colorBlindFilter.SetFilterEnabled(isEnabled);
            UpdatePreview();
        }
        
        private void OnFilterTypeChanged(int type)
        {
            colorBlindFilter.SetFilterType(type);
            UpdatePreview();
        }
        
        private void OnCloseClicked()
        {
            // Revert to saved settings
            LoadCurrentSettings();
            gameObject.SetActive(false);
        }
        
        private void OnApplyClicked()
        {
            // Save current settings
            accessibilityManager.CurrentSettings.colorBlindMode = enableToggle.isOn;
            accessibilityManager.CurrentSettings.colorBlindType = (ColorBlindMode)filterTypeDropdown.value;
            accessibilityManager.SaveSettings();
            
            ToastManager.Instance.ShowToast("Color blind settings saved", ToastType.Success);
            gameObject.SetActive(false);
        }
        
        private void OnResetClicked()
        {
            // Reset to default settings
            enableToggle.isOn = accessibilityManager.DefaultSettings.colorBlindMode;
            filterTypeDropdown.value = (int)accessibilityManager.DefaultSettings.colorBlindType;
            
            colorBlindFilter.SetFilterEnabled(enableToggle.isOn);
            colorBlindFilter.SetFilterType(filterTypeDropdown.value);
            
            UpdatePreview();
        }
        
        private void UpdatePreview()
        {
            if (previewImage != null && previewSprites != null && previewSprites.Length > 0)
            {
                int previewIndex = Mathf.Clamp(filterTypeDropdown.value, 0, previewSprites.Length - 1);
                previewImage.sprite = previewSprites[previewIndex];
            }
        }
        
        private void OnDestroy()
        {
            // Remove listeners
            if (enableToggle != null)
                enableToggle.onValueChanged.RemoveListener(OnEnableToggleChanged);
            if (filterTypeDropdown != null)
                filterTypeDropdown.onValueChanged.RemoveListener(OnFilterTypeChanged);
            if (closeButton != null)
                closeButton.onClick.RemoveListener(OnCloseClicked);
            if (applyButton != null)
                applyButton.onClick.RemoveListener(OnApplyClicked);
            if (resetButton != null)
                resetButton.onClick.RemoveListener(OnResetClicked);
        }
    }
} 