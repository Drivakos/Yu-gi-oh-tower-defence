using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using YuGiOhTowerDefense.Core;

namespace YuGiOhTowerDefense.UI
{
    public class RecruitmentUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject recruitmentPanel;
        [SerializeField] private Image cardImage;
        [SerializeField] private TextMeshProUGUI cardNameText;
        [SerializeField] private TextMeshProUGUI cardDescriptionText;
        [SerializeField] private TextMeshProUGUI recruitmentChanceText;
        [SerializeField] private TextMeshProUGUI attemptsRemainingText;
        [SerializeField] private Button recruitButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private GameObject recruitmentSuccessPanel;
        [SerializeField] private GameObject recruitmentFailedPanel;

        [Header("Animation Settings")]
        [SerializeField] private float recruitmentAnimationDuration = 2f;
        [SerializeField] private AnimationCurve recruitmentCurve;
        [SerializeField] private ParticleSystem successParticles;
        [SerializeField] private ParticleSystem failParticles;

        private GameStory gameStory;
        private YuGiOhCard currentCard;
        private Coroutine recruitmentAnimation;

        private void Awake()
        {
            gameStory = FindObjectOfType<GameStory>();
            
            recruitButton.onClick.AddListener(OnRecruitClicked);
            cancelButton.onClick.AddListener(HideRecruitmentPanel);

            // Hide panels initially
            recruitmentPanel.SetActive(false);
            recruitmentSuccessPanel.SetActive(false);
            recruitmentFailedPanel.SetActive(false);
        }

        public void ShowRecruitment(YuGiOhCard card)
        {
            if (card == null || !gameStory.IsCardRecruitable(card))
            {
                Debug.LogWarning("Card is not recruitable!");
                return;
            }

            currentCard = card;
            UpdateUI();
            recruitmentPanel.SetActive(true);
        }

        private void UpdateUI()
        {
            if (currentCard == null) return;

            cardImage.sprite = currentCard.CardImage;
            cardNameText.text = currentCard.Name;
            cardDescriptionText.text = currentCard.Description;

            float recruitChance = gameStory.GetRecruitmentChance(currentCard);
            recruitmentChanceText.text = $"Recruitment Chance: {recruitChance * 100:F1}%";

            int remainingAttempts = 3; // Get from GameStory
            attemptsRemainingText.text = $"Attempts Remaining: {remainingAttempts}";

            recruitButton.interactable = remainingAttempts > 0;
        }

        private void OnRecruitClicked()
        {
            if (currentCard == null) return;

            recruitButton.interactable = false;
            if (recruitmentAnimation != null)
            {
                StopCoroutine(recruitmentAnimation);
            }
            recruitmentAnimation = StartCoroutine(PlayRecruitmentAnimation());
        }

        private IEnumerator PlayRecruitmentAnimation()
        {
            float elapsedTime = 0f;
            Vector3 originalScale = cardImage.transform.localScale;
            Vector3 targetScale = originalScale * 1.2f;

            while (elapsedTime < recruitmentAnimationDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / recruitmentAnimationDuration;
                float curveValue = recruitmentCurve.Evaluate(t);

                cardImage.transform.localScale = Vector3.Lerp(originalScale, targetScale, curveValue);
                cardImage.transform.Rotate(Vector3.forward, 360f * Time.deltaTime);

                yield return null;
            }

            // Try recruitment
            bool success = gameStory.TryRecruitCard(currentCard);

            // Show result
            if (success)
            {
                ShowRecruitmentSuccess();
            }
            else
            {
                ShowRecruitmentFailed();
            }

            // Reset card transform
            cardImage.transform.localScale = originalScale;
            cardImage.transform.rotation = Quaternion.identity;

            // Update UI
            UpdateUI();
            recruitButton.interactable = true;
            recruitmentAnimation = null;
        }

        private void ShowRecruitmentSuccess()
        {
            recruitmentSuccessPanel.SetActive(true);
            if (successParticles != null)
            {
                successParticles.Play();
            }
            StartCoroutine(HideResultPanel(recruitmentSuccessPanel));
        }

        private void ShowRecruitmentFailed()
        {
            recruitmentFailedPanel.SetActive(true);
            if (failParticles != null)
            {
                failParticles.Play();
            }
            StartCoroutine(HideResultPanel(recruitmentFailedPanel));
        }

        private IEnumerator HideResultPanel(GameObject panel)
        {
            yield return new WaitForSeconds(2f);
            panel.SetActive(false);
        }

        public void HideRecruitmentPanel()
        {
            recruitmentPanel.SetActive(false);
            currentCard = null;
        }

        private void OnDestroy()
        {
            recruitButton.onClick.RemoveListener(OnRecruitClicked);
            cancelButton.onClick.RemoveListener(HideRecruitmentPanel);
        }
    }
} 