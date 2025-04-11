using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YuGiOhTowerDefense.Managers;

namespace YuGiOhTowerDefense.UI
{
    public class GameplayHUD : MonoBehaviour
    {
        [Header("Top Bar")]
        [SerializeField] private TextMeshProUGUI waveText;
        [SerializeField] private TextMeshProUGUI lifePointsText;
        [SerializeField] private TextMeshProUGUI deckCountText;
        [SerializeField] private Slider waveProgressBar;
        [SerializeField] private Button pauseButton;

        [Header("Card Hand")]
        [SerializeField] private Transform handContainer;
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private float cardSpacing = 100f;
        [SerializeField] private float cardRotation = 5f;

        [Header("Tile Info")]
        [SerializeField] private GameObject tileInfoPanel;
        [SerializeField] private TextMeshProUGUI tileInfoText;
        [SerializeField] private Image rangeIndicator;

        private void Start()
        {
            InitializeButtons();
            UpdateHandLayout();
        }

        private void InitializeButtons()
        {
            pauseButton.onClick.AddListener(OnPauseClicked);
        }

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

        public void ShowTileInfo(string info, float range)
        {
            tileInfoPanel.SetActive(true);
            tileInfoText.text = info;
            
            if (range > 0)
            {
                rangeIndicator.gameObject.SetActive(true);
                rangeIndicator.transform.localScale = new Vector3(range * 2, range * 2, 1);
            }
            else
            {
                rangeIndicator.gameObject.SetActive(false);
            }
        }

        public void HideTileInfo()
        {
            tileInfoPanel.SetActive(false);
        }

        private void UpdateHandLayout()
        {
            int cardCount = handContainer.childCount;
            float totalWidth = (cardCount - 1) * cardSpacing;
            float startX = -totalWidth / 2f;

            for (int i = 0; i < cardCount; i++)
            {
                Transform card = handContainer.GetChild(i);
                float xPos = startX + i * cardSpacing;
                float rotation = Mathf.Lerp(-cardRotation, cardRotation, (float)i / (cardCount - 1));
                
                card.localPosition = new Vector3(xPos, 0, 0);
                card.localRotation = Quaternion.Euler(0, 0, rotation);
            }
        }

        private void OnPauseClicked()
        {
            UIManager.Instance.TogglePauseMenu();
        }
    }
} 