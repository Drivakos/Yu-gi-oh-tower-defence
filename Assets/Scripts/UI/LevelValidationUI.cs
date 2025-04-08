using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using YuGiOhTowerDefense.Core;

namespace YuGiOhTowerDefense.UI
{
    public class LevelValidationUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject validationPanel;
        [SerializeField] private CanvasGroup validationCanvasGroup;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Transform errorsContainer;
        [SerializeField] private Transform warningsContainer;
        [SerializeField] private Transform metricsContainer;
        
        [Header("Prefabs")]
        [SerializeField] private GameObject errorItemPrefab;
        [SerializeField] private GameObject warningItemPrefab;
        [SerializeField] private GameObject metricItemPrefab;
        
        [Header("Animation Settings")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.2f;
        [SerializeField] private float itemAppearDelay = 0.1f;
        
        private LevelValidator levelValidator;
        private LevelEditor levelEditor;
        
        private void Awake()
        {
            levelValidator = FindObjectOfType<LevelValidator>();
            if (levelValidator == null)
            {
                Debug.LogError("LevelValidationUI: LevelValidator not found!");
            }
            
            levelEditor = FindObjectOfType<LevelEditor>();
            if (levelEditor == null)
            {
                Debug.LogError("LevelValidationUI: LevelEditor not found!");
            }
            
            // Hide panel initially
            if (validationPanel != null)
            {
                validationPanel.SetActive(false);
            }
        }
        
        public void ShowValidationResults(LevelData level)
        {
            if (levelValidator == null || level == null)
            {
                return;
            }
            
            LevelValidator.ValidationResult result = levelValidator.ValidateLevel(level);
            
            // Update UI
            UpdateValidationUI(result);
            
            // Show panel with animation
            ShowPanel();
        }
        
        private void UpdateValidationUI(LevelValidator.ValidationResult result)
        {
            // Clear previous content
            ClearContainers();
            
            // Update status
            UpdateStatus(result.isValid);
            
            // Add errors
            foreach (string error in result.errors)
            {
                AddErrorItem(error);
            }
            
            // Add warnings
            foreach (string warning in result.warnings)
            {
                AddWarningItem(warning);
            }
            
            // Add metrics
            foreach (var metric in result.metrics)
            {
                AddMetricItem(metric.Key, metric.Value);
            }
        }
        
        private void UpdateStatus(bool isValid)
        {
            if (statusText != null)
            {
                statusText.text = isValid ? "Level is Valid" : "Level has Errors";
                statusText.color = isValid ? Color.green : Color.red;
            }
        }
        
        private void AddErrorItem(string error)
        {
            if (errorItemPrefab == null || errorsContainer == null)
            {
                return;
            }
            
            GameObject errorItem = Instantiate(errorItemPrefab, errorsContainer);
            TextMeshProUGUI errorText = errorItem.GetComponentInChildren<TextMeshProUGUI>();
            if (errorText != null)
            {
                errorText.text = error;
            }
        }
        
        private void AddWarningItem(string warning)
        {
            if (warningItemPrefab == null || warningsContainer == null)
            {
                return;
            }
            
            GameObject warningItem = Instantiate(warningItemPrefab, warningsContainer);
            TextMeshProUGUI warningText = warningItem.GetComponentInChildren<TextMeshProUGUI>();
            if (warningText != null)
            {
                warningText.text = warning;
            }
        }
        
        private void AddMetricItem(string name, float value)
        {
            if (metricItemPrefab == null || metricsContainer == null)
            {
                return;
            }
            
            GameObject metricItem = Instantiate(metricItemPrefab, metricsContainer);
            TextMeshProUGUI[] texts = metricItem.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 2)
            {
                texts[0].text = name;
                texts[1].text = FormatMetricValue(value);
            }
        }
        
        private string FormatMetricValue(float value)
        {
            if (value >= 1000)
            {
                return $"{value / 1000:F1}K";
            }
            return value.ToString("F1");
        }
        
        private void ClearContainers()
        {
            if (errorsContainer != null)
            {
                foreach (Transform child in errorsContainer)
                {
                    Destroy(child.gameObject);
                }
            }
            
            if (warningsContainer != null)
            {
                foreach (Transform child in warningsContainer)
                {
                    Destroy(child.gameObject);
                }
            }
            
            if (metricsContainer != null)
            {
                foreach (Transform child in metricsContainer)
                {
                    Destroy(child.gameObject);
                }
            }
        }
        
        private void ShowPanel()
        {
            if (validationPanel == null || validationCanvasGroup == null)
            {
                return;
            }
            
            validationPanel.SetActive(true);
            StartCoroutine(ShowPanelAnimation());
        }
        
        private System.Collections.IEnumerator ShowPanelAnimation()
        {
            float elapsedTime = 0f;
            validationCanvasGroup.alpha = 0f;
            
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                validationCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
                yield return null;
            }
            
            validationCanvasGroup.alpha = 1f;
        }
        
        public void HidePanel()
        {
            if (validationPanel == null || validationCanvasGroup == null)
            {
                return;
            }
            
            StartCoroutine(HidePanelAnimation());
        }
        
        private System.Collections.IEnumerator HidePanelAnimation()
        {
            float elapsedTime = 0f;
            validationCanvasGroup.alpha = 1f;
            
            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                validationCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutDuration);
                yield return null;
            }
            
            validationCanvasGroup.alpha = 0f;
            validationPanel.SetActive(false);
        }
        
        public void OnCloseButtonClicked()
        {
            HidePanel();
        }
        
        public void OnValidateButtonClicked()
        {
            if (levelEditor != null)
            {
                ShowValidationResults(levelEditor.GetCurrentLevel());
            }
        }
    }
} 