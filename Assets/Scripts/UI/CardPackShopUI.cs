using UnityEngine;
using System.Collections.Generic;
using YuGiOhTowerDefense.Core;

namespace YuGiOhTowerDefense.UI
{
    public class CardPackShopUI : MonoBehaviour
    {
        [Header("Shop Settings")]
        [SerializeField] private Transform packContainer;
        [SerializeField] private GameObject cardPackPrefab;
        [SerializeField] private float packSpacing = 20f;
        [SerializeField] private int packsPerRow = 3;

        [Header("Shop UI")]
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private GameObject noPacksMessage;
        [SerializeField] private GameObject insufficientFundsMessage;

        private CardPackManager packManager;
        private List<CardPackUI> displayedPacks = new List<CardPackUI>();

        private void Start()
        {
            packManager = FindObjectOfType<CardPackManager>();
            if (packManager == null)
            {
                Debug.LogError("CardPackManager not found in scene!");
                return;
            }

            RefreshPackDisplay();
        }

        public void ShowShop()
        {
            shopPanel.SetActive(true);
            RefreshPackDisplay();
        }

        public void HideShop()
        {
            shopPanel.SetActive(false);
        }

        private void RefreshPackDisplay()
        {
            // Clear existing packs
            foreach (var pack in displayedPacks)
            {
                Destroy(pack.gameObject);
            }
            displayedPacks.Clear();

            var availablePacks = packManager.GetAvailablePacks();
            if (availablePacks.Count == 0)
            {
                noPacksMessage.SetActive(true);
                return;
            }

            noPacksMessage.SetActive(false);
            
            // Calculate layout
            float rowWidth = packContainer.GetComponent<RectTransform>().rect.width;
            float packWidth = (rowWidth - (packSpacing * (packsPerRow - 1))) / packsPerRow;

            // Create pack displays
            for (int i = 0; i < availablePacks.Count; i++)
            {
                var packObject = Instantiate(cardPackPrefab, packContainer);
                var packUI = packObject.GetComponent<CardPackUI>();
                
                if (packUI != null)
                {
                    packUI.SetCardPack(availablePacks[i]);
                    displayedPacks.Add(packUI);

                    // Position the pack
                    RectTransform rectTransform = packObject.GetComponent<RectTransform>();
                    int row = i / packsPerRow;
                    int col = i % packsPerRow;
                    
                    float xPos = col * (packWidth + packSpacing);
                    float yPos = -row * (packWidth + packSpacing);
                    
                    rectTransform.anchoredPosition = new Vector2(xPos, yPos);
                    rectTransform.sizeDelta = new Vector2(packWidth, packWidth);
                }
            }
        }

        public void ShowInsufficientFundsMessage()
        {
            insufficientFundsMessage.SetActive(true);
            Invoke(nameof(HideInsufficientFundsMessage), 2f);
        }

        private void HideInsufficientFundsMessage()
        {
            insufficientFundsMessage.SetActive(false);
        }

        private void OnEnable()
        {
            RefreshPackDisplay();
        }

        private void OnDisable()
        {
            HideInsufficientFundsMessage();
        }
    }
} 