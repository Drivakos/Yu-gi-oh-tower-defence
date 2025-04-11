using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using YuGiOhTowerDefense.Cards;

namespace YuGiOhTowerDefense.Managers
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("UI Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject gameplayPanel;
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private GameObject gameOverPanel;

        [Header("Gameplay UI")]
        [SerializeField] private Transform handContainer;
        [SerializeField] private TextMeshProUGUI waveText;
        [SerializeField] private TextMeshProUGUI lifePointsText;
        [SerializeField] private TextMeshProUGUI deckCountText;
        [SerializeField] private Slider waveProgressBar;
        [SerializeField] private GameObject cardPrefab;

        [Header("Game Over UI")]
        [SerializeField] private TextMeshProUGUI gameOverText;
        [SerializeField] private TextMeshProUGUI finalScoreText;

        private List<CardUI> handCards = new List<CardUI>();
        private CardUI selectedCard;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            ShowMainMenu();
        }

        #region Panel Management
        public void ShowMainMenu()
        {
            mainMenuPanel.SetActive(true);
            gameplayPanel.SetActive(false);
            pauseMenuPanel.SetActive(false);
            gameOverPanel.SetActive(false);
        }

        public void ShowGameplay()
        {
            mainMenuPanel.SetActive(false);
            gameplayPanel.SetActive(true);
            pauseMenuPanel.SetActive(false);
            gameOverPanel.SetActive(false);
        }

        public void TogglePauseMenu()
        {
            pauseMenuPanel.SetActive(!pauseMenuPanel.activeSelf);
            Time.timeScale = pauseMenuPanel.activeSelf ? 0f : 1f;
        }

        public void ShowGameOver(bool victory)
        {
            gameOverPanel.SetActive(true);
            gameOverText.text = victory ? "Victory!" : "Game Over";
            // TODO: Calculate and display final score
        }
        #endregion

        #region Gameplay UI Updates
        public void UpdateWaveInfo(int currentWave, int totalWaves)
        {
            waveText.text = $"Wave {currentWave}/{totalWaves}";
            waveProgressBar.value = (float)currentWave / totalWaves;
        }

        public void UpdateLifePoints(int lifePoints)
        {
            lifePointsText.text = $"LP: {lifePoints}";
        }

        public void UpdateDeckCount(int remainingCards)
        {
            deckCountText.text = $"Deck: {remainingCards}";
        }

        public void AddCardToHand(YuGiOhCard card)
        {
            GameObject cardObj = Instantiate(cardPrefab, handContainer);
            CardUI cardUI = cardObj.GetComponent<CardUI>();
            if (cardUI != null)
            {
                cardUI.Initialize(card);
                handCards.Add(cardUI);
                UpdateHandLayout();
            }
        }

        public void RemoveCardFromHand(CardUI cardUI)
        {
            handCards.Remove(cardUI);
            Destroy(cardUI.gameObject);
            UpdateHandLayout();
        }

        private void UpdateHandLayout()
        {
            // Update the position and rotation of cards in hand
            float spacing = 100f; // Adjust based on your card size
            float startX = -(handCards.Count - 1) * spacing / 2f;

            for (int i = 0; i < handCards.Count; i++)
            {
                CardUI card = handCards[i];
                Vector3 targetPosition = new Vector3(startX + i * spacing, 0, 0);
                card.transform.localPosition = targetPosition;
            }
        }
        #endregion

        #region Card Selection
        public void SelectCard(CardUI card)
        {
            if (selectedCard != null)
            {
                selectedCard.SetSelected(false);
            }

            selectedCard = card;
            if (selectedCard != null)
            {
                selectedCard.SetSelected(true);
            }
        }

        public CardUI GetSelectedCard()
        {
            return selectedCard;
        }

        public void ClearSelectedCard()
        {
            if (selectedCard != null)
            {
                selectedCard.SetSelected(false);
                selectedCard = null;
            }
        }
        #endregion
    }
} 