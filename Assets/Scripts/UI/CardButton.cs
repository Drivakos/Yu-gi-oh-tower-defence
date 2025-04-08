using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace YuGiOhTowerDefense.UI
{
    public class CardButton : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Button button;
        [SerializeField] private GameObject cooldownOverlay;
        [SerializeField] private Image cooldownFill;
        [SerializeField] private GameObject selectedIndicator;
        
        private int cost;
        private Action onClick;
        private float cooldown;
        private float currentCooldown;

        public void Initialize(string cardName, int cardCost, Sprite icon, Action callback)
        {
            nameText.text = cardName;
            costText.text = cardCost.ToString();
            iconImage.sprite = icon;
            cost = cardCost;
            onClick = callback;
            cooldown = 0f;
            currentCooldown = 0f;

            button.onClick.AddListener(OnButtonClicked);
            UpdateButtonState();
        }

        private void OnButtonClicked()
        {
            if (GameManager.Instance.CanAffordMonster(cost) && currentCooldown <= 0)
            {
                onClick?.Invoke();
                StartCooldown();
            }
        }

        private void StartCooldown()
        {
            currentCooldown = cooldown;
            UpdateCooldownVisual();
        }

        private void UpdateCooldownVisual()
        {
            if (currentCooldown > 0)
            {
                cooldownOverlay.SetActive(true);
                cooldownFill.fillAmount = currentCooldown / cooldown;
            }
            else
            {
                cooldownOverlay.SetActive(false);
            }
        }

        private void UpdateButtonState()
        {
            bool canAfford = GameManager.Instance.CanAffordMonster(cost);
            bool onCooldown = currentCooldown > 0;
            button.interactable = canAfford && !onCooldown;
            
            // Update visual feedback
            Color textColor = (canAfford && !onCooldown) ? Color.white : Color.gray;
            nameText.color = textColor;
            costText.color = textColor;
            iconImage.color = (canAfford && !onCooldown) ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
        }

        private void Update()
        {
            if (currentCooldown > 0)
            {
                currentCooldown -= Time.deltaTime;
                UpdateCooldownVisual();
            }
            
            UpdateButtonState();
        }

        public void SetSelected(bool selected)
        {
            selectedIndicator.SetActive(selected);
        }

        public void SetCooldown(float newCooldown)
        {
            cooldown = newCooldown;
            if (currentCooldown > cooldown)
            {
                currentCooldown = cooldown;
                UpdateCooldownVisual();
            }
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(OnButtonClicked);
        }
    }
} 