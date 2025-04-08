using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using YuGiOhTowerDefense.Core;

namespace YuGiOhTowerDefense.UI
{
    public class CardPackUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image packIcon;
        [SerializeField] private TextMeshProUGUI packNameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Button purchaseButton;
        [SerializeField] private Button previewButton;
        [SerializeField] private GameObject rarityDistributionPanel;
        
        [Header("Rarity Display")]
        [SerializeField] private Image commonBar;
        [SerializeField] private Image rareBar;
        [SerializeField] private Image superRareBar;
        [SerializeField] private Image ultraRareBar;
        [SerializeField] private TextMeshProUGUI commonText;
        [SerializeField] private TextMeshProUGUI rareText;
        [SerializeField] private TextMeshProUGUI superRareText;
        [SerializeField] private TextMeshProUGUI ultraRareText;

        [Header("Animation Settings")]
        [SerializeField] private float hoverScaleMultiplier = 1.1f;
        [SerializeField] private float hoverDuration = 0.2f;
        
        private CardPack cardPack;
        private CardPackManager packManager;
        private Vector3 originalScale;
        private Coroutine hoverAnimation;

        private void Awake()
        {
            originalScale = transform.localScale;
            packManager = FindObjectOfType<CardPackManager>();
            
            purchaseButton.onClick.AddListener(OnPurchaseClicked);
            previewButton.onClick.AddListener(OnPreviewClicked);
        }

        public void SetCardPack(CardPack pack)
        {
            cardPack = pack;
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (cardPack == null) return;

            packIcon.sprite = cardPack.PackIcon;
            packNameText.text = cardPack.PackName;
            descriptionText.text = cardPack.Description;
            costText.text = $"{cardPack.Cost} G";

            UpdateRarityBars();
        }

        private void UpdateRarityBars()
        {
            float commonChance = cardPack.GetRarityChance(CardRarity.Common);
            float rareChance = cardPack.GetRarityChance(CardRarity.Rare);
            float superRareChance = cardPack.GetRarityChance(CardRarity.SuperRare);
            float ultraRareChance = cardPack.GetRarityChance(CardRarity.UltraRare);

            commonBar.fillAmount = commonChance / 100f;
            rareBar.fillAmount = rareChance / 100f;
            superRareBar.fillAmount = superRareChance / 100f;
            ultraRareBar.fillAmount = ultraRareChance / 100f;

            commonText.text = $"{commonChance}%";
            rareText.text = $"{rareChance}%";
            superRareText.text = $"{superRareChance}%";
            ultraRareText.text = $"{ultraRareChance}%";
        }

        private void OnPurchaseClicked()
        {
            if (packManager != null && cardPack != null)
            {
                var cards = packManager.OpenPack(cardPack);
                // Trigger card reveal animation through CardRevealUI
                FindObjectOfType<CardRevealUI>()?.ShowReveal(cards, cardPack.PackName);
            }
        }

        private void OnPreviewClicked()
        {
            rarityDistributionPanel.SetActive(!rarityDistributionPanel.activeSelf);
        }

        public void OnPointerEnter()
        {
            if (hoverAnimation != null)
            {
                StopCoroutine(hoverAnimation);
            }
            hoverAnimation = StartCoroutine(AnimateHover(true));
        }

        public void OnPointerExit()
        {
            if (hoverAnimation != null)
            {
                StopCoroutine(hoverAnimation);
            }
            hoverAnimation = StartCoroutine(AnimateHover(false));
        }

        private IEnumerator AnimateHover(bool isEntering)
        {
            Vector3 targetScale = isEntering ? originalScale * hoverScaleMultiplier : originalScale;
            Vector3 startScale = transform.localScale;
            float elapsedTime = 0f;

            while (elapsedTime < hoverDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / hoverDuration;
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }

            transform.localScale = targetScale;
            hoverAnimation = null;
        }

        private void OnDestroy()
        {
            purchaseButton.onClick.RemoveListener(OnPurchaseClicked);
            previewButton.onClick.RemoveListener(OnPreviewClicked);
        }
    }
} 