using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace YuGiOh.MillenniumItems
{
    public class MillenniumItemUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject itemPanelPrefab;
        [SerializeField] private Transform itemContainer;
        [SerializeField] private GameObject itemDetailPanel;
        [SerializeField] private Image itemIconImage;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI itemDescriptionText;
        [SerializeField] private TextMeshProUGUI unlockRequirementText;
        [SerializeField] private Slider powerLevelSlider;
        [SerializeField] private Button activateButton;
        [SerializeField] private Button deactivateButton;
        
        [Header("Animation")]
        [SerializeField] private float panelAnimationDuration = 0.3f;
        [SerializeField] private AnimationCurve panelAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        private MillenniumItemManager itemManager;
        private Dictionary<MillenniumItemType, GameObject> itemPanels = new Dictionary<MillenniumItemType, GameObject>();
        private MillenniumItemType selectedItemType;
        
        private void Awake()
        {
            itemManager = FindObjectOfType<MillenniumItemManager>();
            if (itemManager == null)
            {
                Debug.LogError("MillenniumItemManager not found in the scene!");
            }
            
            if (itemDetailPanel != null)
            {
                itemDetailPanel.SetActive(false);
            }
            
            if (activateButton != null)
            {
                activateButton.onClick.AddListener(OnActivateButtonClicked);
            }
            
            if (deactivateButton != null)
            {
                deactivateButton.onClick.AddListener(OnDeactivateButtonClicked);
            }
        }
        
        private void Start()
        {
            RefreshItemList();
        }
        
        public void RefreshItemList()
        {
            // Clear existing panels
            foreach (var panel in itemPanels.Values)
            {
                Destroy(panel);
            }
            itemPanels.Clear();
            
            // Create panels for each item
            foreach (MillenniumItemType itemType in System.Enum.GetValues(typeof(MillenniumItemType)))
            {
                CreateItemPanel(itemType);
            }
        }
        
        private void CreateItemPanel(MillenniumItemType itemType)
        {
            if (itemPanelPrefab == null || itemContainer == null)
            {
                Debug.LogError("Item panel prefab or container is not assigned!");
                return;
            }
            
            GameObject panel = Instantiate(itemPanelPrefab, itemContainer);
            itemPanels[itemType] = panel;
            
            // Set up panel components
            ItemPanelUI panelUI = panel.GetComponent<ItemPanelUI>();
            if (panelUI != null)
            {
                ItemData itemData = itemManager.GetItemData(itemType);
                if (itemData != null)
                {
                    panelUI.SetupPanel(itemType, itemData, OnItemPanelClicked);
                }
            }
        }
        
        private void OnItemPanelClicked(MillenniumItemType itemType)
        {
            selectedItemType = itemType;
            ShowItemDetails(itemType);
        }
        
        private void ShowItemDetails(MillenniumItemType itemType)
        {
            if (itemDetailPanel == null)
            {
                Debug.LogError("Item detail panel is not assigned!");
                return;
            }
            
            ItemData itemData = itemManager.GetItemData(itemType);
            if (itemData == null)
            {
                Debug.LogError($"No data found for item type {itemType}");
                return;
            }
            
            // Update UI elements
            if (itemNameText != null)
            {
                itemNameText.text = itemData.Name;
            }
            
            if (itemDescriptionText != null)
            {
                itemDescriptionText.text = itemData.Description;
            }
            
            if (unlockRequirementText != null)
            {
                unlockRequirementText.text = $"Unlock: {itemData.UnlockRequirement}";
            }
            
            if (powerLevelSlider != null)
            {
                powerLevelSlider.value = itemData.PowerLevel;
            }
            
            // Update button states
            bool isActive = itemManager.IsItemActive(itemType);
            if (activateButton != null)
            {
                activateButton.gameObject.SetActive(!isActive);
            }
            
            if (deactivateButton != null)
            {
                deactivateButton.gameObject.SetActive(isActive);
            }
            
            // Show the panel with animation
            itemDetailPanel.SetActive(true);
            AnimatePanel(itemDetailPanel.GetComponent<RectTransform>(), true);
        }
        
        private void OnActivateButtonClicked()
        {
            if (itemManager != null)
            {
                itemManager.ActivateItem(selectedItemType);
                ShowItemDetails(selectedItemType); // Refresh the UI
            }
        }
        
        private void OnDeactivateButtonClicked()
        {
            if (itemManager != null)
            {
                itemManager.DeactivateItem(selectedItemType);
                ShowItemDetails(selectedItemType); // Refresh the UI
            }
        }
        
        private void AnimatePanel(RectTransform panel, bool show)
        {
            if (panel == null)
            {
                return;
            }
            
            // Set initial state
            if (show)
            {
                panel.gameObject.SetActive(true);
                panel.localScale = Vector3.zero;
            }
            
            // Animate
            StartCoroutine(AnimatePanelCoroutine(panel, show));
        }
        
        private System.Collections.IEnumerator AnimatePanelCoroutine(RectTransform panel, bool show)
        {
            float elapsedTime = 0f;
            Vector3 startScale = show ? Vector3.zero : panel.localScale;
            Vector3 targetScale = show ? Vector3.one : Vector3.zero;
            
            while (elapsedTime < panelAnimationDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = panelAnimationCurve.Evaluate(elapsedTime / panelAnimationDuration);
                
                panel.localScale = Vector3.Lerp(startScale, targetScale, t);
                
                yield return null;
            }
            
            // Ensure final state
            panel.localScale = targetScale;
            
            if (!show)
            {
                panel.gameObject.SetActive(false);
            }
        }
    }
    
    public class ItemPanelUI : MonoBehaviour
    {
        [SerializeField] private Image itemIcon;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private GameObject activeIndicator;
        
        private MillenniumItemType itemType;
        private System.Action<MillenniumItemType> onClickCallback;
        
        public void SetupPanel(MillenniumItemType type, ItemData data, System.Action<MillenniumItemType> callback)
        {
            itemType = type;
            onClickCallback = callback;
            
            if (itemNameText != null)
            {
                itemNameText.text = data.Name;
            }
            
            // Set active indicator based on item state
            MillenniumItemManager manager = FindObjectOfType<MillenniumItemManager>();
            if (manager != null && activeIndicator != null)
            {
                activeIndicator.SetActive(manager.IsItemActive(type));
            }
            
            // Add click listener
            Button button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(OnPanelClicked);
            }
        }
        
        private void OnPanelClicked()
        {
            onClickCallback?.Invoke(itemType);
        }
        
        public void UpdateActiveState(bool isActive)
        {
            if (activeIndicator != null)
            {
                activeIndicator.SetActive(isActive);
            }
        }
    }
} 