using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using YuGiOhTowerDefense.Core;

namespace YuGiOhTowerDefense.UI
{
    public class MillenniumItemUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject itemPanel;
        [SerializeField] private Transform itemContainer;
        [SerializeField] private GameObject itemSlotPrefab;
        [SerializeField] private GameObject itemDetailsPanel;
        [SerializeField] private Image itemIcon;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI itemDescriptionText;
        [SerializeField] private TextMeshProUGUI cooldownText;
        [SerializeField] private Button activateButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Slider cooldownSlider;

        [Header("Animation Settings")]
        [SerializeField] private float itemHoverScale = 1.1f;
        [SerializeField] private float itemHoverDuration = 0.2f;
        [SerializeField] private AnimationCurve hoverCurve;
        [SerializeField] private ParticleSystem activationParticles;

        private MillenniumItemManager itemManager;
        private Dictionary<MillenniumItemType, GameObject> itemSlots = new Dictionary<MillenniumItemType, GameObject>();
        private MillenniumItem selectedItem;
        private Coroutine cooldownCoroutine;

        private void Awake()
        {
            itemManager = FindObjectOfType<MillenniumItemManager>();
            
            activateButton.onClick.AddListener(OnActivateClicked);
            closeButton.onClick.AddListener(HideItemDetails);

            // Hide panels initially
            itemPanel.SetActive(false);
            itemDetailsPanel.SetActive(false);
        }

        private void Start()
        {
            RefreshItemDisplay();
        }

        public void ShowItemPanel()
        {
            itemPanel.SetActive(true);
            RefreshItemDisplay();
        }

        public void HideItemPanel()
        {
            itemPanel.SetActive(false);
            HideItemDetails();
        }

        private void RefreshItemDisplay()
        {
            // Clear existing slots
            foreach (var slot in itemSlots.Values)
            {
                Destroy(slot);
            }
            itemSlots.Clear();

            // Create slots for collected items
            var collectedItems = itemManager.GetCollectedItems();
            foreach (var item in collectedItems)
            {
                CreateItemSlot(item);
            }
        }

        private void CreateItemSlot(MillenniumItem item)
        {
            var slotObject = Instantiate(itemSlotPrefab, itemContainer);
            var slotUI = slotObject.GetComponent<MillenniumItemSlotUI>();
            
            if (slotUI != null)
            {
                slotUI.SetItem(item);
                slotUI.OnItemSelected += ShowItemDetails;
                itemSlots[item.ItemType] = slotObject;
            }
        }

        private void ShowItemDetails(MillenniumItem item)
        {
            selectedItem = item;
            
            itemIcon.sprite = item.ItemIcon;
            itemNameText.text = item.ItemName;
            itemDescriptionText.text = item.Description;
            
            UpdateCooldownUI();
            
            itemDetailsPanel.SetActive(true);
        }

        private void HideItemDetails()
        {
            itemDetailsPanel.SetActive(false);
            selectedItem = null;
            
            if (cooldownCoroutine != null)
            {
                StopCoroutine(cooldownCoroutine);
                cooldownCoroutine = null;
            }
        }

        private void OnActivateClicked()
        {
            if (selectedItem == null) return;

            bool success = itemManager.ActivateItem(selectedItem.ItemType);
            if (success)
            {
                // Play activation effect
                if (activationParticles != null)
                {
                    activationParticles.Play();
                }
                
                // Update UI
                UpdateCooldownUI();
                activateButton.interactable = false;
                
                // Start cooldown coroutine
                if (cooldownCoroutine != null)
                {
                    StopCoroutine(cooldownCoroutine);
                }
                cooldownCoroutine = StartCoroutine(UpdateCooldown());
            }
        }

        private void UpdateCooldownUI()
        {
            if (selectedItem == null) return;

            bool canActivate = selectedItem.CanActivate();
            activateButton.interactable = canActivate;
            
            if (canActivate)
            {
                cooldownText.text = "Ready";
                cooldownSlider.value = 1f;
            }
            else
            {
                float remainingCooldown = selectedItem.RemainingCooldown;
                cooldownText.text = $"Cooldown: {remainingCooldown:F1}s";
                cooldownSlider.value = 1f - (remainingCooldown / selectedItem.Cooldown);
            }
        }

        private IEnumerator UpdateCooldown()
        {
            while (selectedItem != null && !selectedItem.CanActivate())
            {
                UpdateCooldownUI();
                yield return new WaitForSeconds(0.1f);
            }
            
            if (selectedItem != null)
            {
                UpdateCooldownUI();
            }
            
            cooldownCoroutine = null;
        }

        private void OnDestroy()
        {
            activateButton.onClick.RemoveListener(OnActivateClicked);
            closeButton.onClick.RemoveListener(HideItemDetails);
            
            foreach (var slot in itemSlots.Values)
            {
                var slotUI = slot.GetComponent<MillenniumItemSlotUI>();
                if (slotUI != null)
                {
                    slotUI.OnItemSelected -= ShowItemDetails;
                }
            }
        }
    }

    public class MillenniumItemSlotUI : MonoBehaviour
    {
        [SerializeField] private Image itemIcon;
        [SerializeField] private GameObject activeIndicator;
        [SerializeField] private Button slotButton;
        
        private MillenniumItem item;
        
        public System.Action<MillenniumItem> OnItemSelected;
        
        private void Awake()
        {
            slotButton.onClick.AddListener(OnSlotClicked);
        }
        
        public void SetItem(MillenniumItem newItem)
        {
            item = newItem;
            itemIcon.sprite = item.ItemIcon;
            UpdateActiveIndicator();
        }
        
        private void UpdateActiveIndicator()
        {
            if (activeIndicator != null)
            {
                activeIndicator.SetActive(item.IsActive);
            }
        }
        
        private void OnSlotClicked()
        {
            OnItemSelected?.Invoke(item);
        }
        
        private void OnDestroy()
        {
            slotButton.onClick.RemoveListener(OnSlotClicked);
        }
    }
} 