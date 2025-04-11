using UnityEngine;
using UnityEngine.UI;

namespace YuGiOhTowerDefence.Core.UI
{
    /// <summary>
    /// Manages the color blind filter settings and applies them to the camera
    /// </summary>
    public class ColorBlindFilter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Material colorBlindMaterial;
        [SerializeField] private Camera mainCamera;
        
        [Header("Settings")]
        [SerializeField, Range(0, 2)] private int filterType = 0;
        [SerializeField] private bool isEnabled = false;

        private const string FilterTypeKey = "ColorBlindFilterType";
        private const string FilterEnabledKey = "ColorBlindFilterEnabled";

        private void Awake()
        {
            if (colorBlindMaterial == null)
            {
                Debug.LogError("Color blind material is not assigned!");
                enabled = false;
                return;
            }

            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null)
                {
                    Debug.LogError("Main camera not found!");
                    enabled = false;
                    return;
                }
            }

            LoadSettings();
            ApplyFilter();
        }

        private void OnDestroy()
        {
            SaveSettings();
        }

        /// <summary>
        /// Sets the type of color blindness to simulate
        /// </summary>
        /// <param name="type">0: None, 1: Protanopia, 2: Deuteranopia, 3: Tritanopia</param>
        public void SetFilterType(int type)
        {
            filterType = Mathf.Clamp(type, 0, 3);
            colorBlindMaterial.SetInt("_FilterType", filterType);
            SaveSettings();
        }

        /// <summary>
        /// Toggles the color blind filter on/off
        /// </summary>
        /// <param name="enabled">Whether the filter should be enabled</param>
        public void SetFilterEnabled(bool enabled)
        {
            isEnabled = enabled;
            ApplyFilter();
            SaveSettings();
        }

        private void ApplyFilter()
        {
            if (isEnabled)
            {
                mainCamera.SetReplacementShader(colorBlindMaterial.shader, "RenderType");
            }
            else
            {
                mainCamera.ResetReplacementShader();
            }
        }

        private void LoadSettings()
        {
            filterType = PlayerPrefs.GetInt(FilterTypeKey, 0);
            isEnabled = PlayerPrefs.GetInt(FilterEnabledKey, 0) == 1;
            
            colorBlindMaterial.SetInt("_FilterType", filterType);
        }

        private void SaveSettings()
        {
            PlayerPrefs.SetInt(FilterTypeKey, filterType);
            PlayerPrefs.SetInt(FilterEnabledKey, isEnabled ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
} 