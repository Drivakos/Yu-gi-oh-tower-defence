using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace YuGiOhTowerDefense.UI
{
    public class MonsterButton : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Button button;
        [SerializeField] private GameObject selectedIndicator;
        
        private int cost;
        private Action onClick;

        public void Initialize(string monsterName, int monsterCost, Sprite icon, Action callback)
        {
            nameText.text = monsterName;
            costText.text = monsterCost.ToString();
            iconImage.sprite = icon;
            cost = monsterCost;
            onClick = callback;

            button.onClick.AddListener(OnButtonClicked);
            UpdateButtonState();
        }

        private void OnButtonClicked()
        {
            if (GameManager.Instance.CanAffordMonster(cost))
            {
                onClick?.Invoke();
            }
        }

        private void UpdateButtonState()
        {
            bool canAfford = GameManager.Instance.CanAffordMonster(cost);
            button.interactable = canAfford;
            
            // Update visual feedback
            Color textColor = canAfford ? Color.white : Color.gray;
            nameText.color = textColor;
            costText.color = textColor;
            iconImage.color = canAfford ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
        }

        private void Update()
        {
            UpdateButtonState();
        }

        public void SetSelected(bool selected)
        {
            selectedIndicator.SetActive(selected);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(OnButtonClicked);
        }
    }
} 