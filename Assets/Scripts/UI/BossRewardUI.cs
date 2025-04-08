using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using YuGiOhTowerDefense.Core;

namespace YuGiOhTowerDefense.UI
{
    public class BossRewardUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private GameObject rewardPanel;
        [SerializeField] private Transform cardContainer;
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Button claimButton;
        
        [Header("Animation Settings")]
        [SerializeField] private float revealDelay = 0.5f;
        [SerializeField] private float cardRevealInterval = 0.3f;
        [SerializeField] private AnimationCurve revealCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        private CardPoolManager cardPoolManager;
        private List<YuGiOhCard> rewardCards = new List<YuGiOhCard>();
        private bool isRevealing;
        
        private void Awake()
        {
            cardPoolManager = GetComponent<CardPoolManager>();
            if (cardPoolManager == null)
            {
                Debug.LogError("CardPoolManager not found!");
            }
            
            if (claimButton != null)
            {
                claimButton.onClick.AddListener(OnClaimButtonClicked);
            }
            
            // Hide panel initially
            if (rewardPanel != null)
            {
                rewardPanel.SetActive(false);
            }
        }
        
        public void ShowBossRewards(string bossId)
        {
            if (cardPoolManager == null)
            {
                return;
            }
            
            // Generate reward cards
            rewardCards = cardPoolManager.GenerateBossRewards(bossId);
            
            // Update UI
            if (titleText != null)
            {
                titleText.text = "Boss Defeated!";
            }
            
            if (descriptionText != null)
            {
                descriptionText.text = $"You've earned {rewardCards.Count} cards!";
            }
            
            // Show panel
            if (rewardPanel != null)
            {
                rewardPanel.SetActive(true);
            }
            
            // Start reveal animation
            StartCoroutine(RevealCards());
        }
        
        private IEnumerator RevealCards()
        {
            isRevealing = true;
            
            // Clear existing cards
            foreach (Transform child in cardContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Wait for initial delay
            yield return new WaitForSeconds(revealDelay);
            
            // Reveal each card
            foreach (var card in rewardCards)
            {
                GameObject cardObj = Instantiate(cardPrefab, cardContainer);
                CardRevealUI cardUI = cardObj.GetComponent<CardRevealUI>();
                
                if (cardUI != null)
                {
                    cardUI.SetCard(card);
                    cardUI.SetRevealDelay(0f);
                }
                
                yield return new WaitForSeconds(cardRevealInterval);
            }
            
            isRevealing = false;
            
            // Enable claim button
            if (claimButton != null)
            {
                claimButton.interactable = true;
            }
        }
        
        private void OnClaimButtonClicked()
        {
            if (isRevealing)
            {
                return;
            }
            
            // Add cards to player's collection
            foreach (var card in rewardCards)
            {
                cardPoolManager.AddCardToPlayerPool(card.id);
            }
            
            // Hide panel
            if (rewardPanel != null)
            {
                rewardPanel.SetActive(false);
            }
            
            // Clear reward cards
            rewardCards.Clear();
        }
    }
} 