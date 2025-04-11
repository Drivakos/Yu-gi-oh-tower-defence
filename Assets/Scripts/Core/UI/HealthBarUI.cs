using UnityEngine;
using UnityEngine.UI;

namespace YuGiOhTowerDefense.UI
{
    public class HealthBarUI : MonoBehaviour
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private float updateSpeed = 5f;
        [SerializeField] private Vector3 offset = new Vector3(0, 2f, 0);

        private Transform target;
        private Camera mainCamera;
        private float currentFill;
        private float targetFill;

        private void Start()
        {
            mainCamera = Camera.main;
            currentFill = 1f;
            targetFill = 1f;
            UpdatePosition();
        }

        private void LateUpdate()
        {
            UpdatePosition();
            UpdateFill();
        }

        private void UpdatePosition()
        {
            if (target == null) return;

            // Convert world position to screen position
            Vector3 targetPosition = target.position + offset;
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(targetPosition);

            // Check if behind camera
            if (screenPosition.z < 0)
            {
                screenPosition *= -1;
            }

            transform.position = screenPosition;
        }

        private void UpdateFill()
        {
            if (Mathf.Approximately(currentFill, targetFill)) return;

            currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * updateSpeed);
            fillImage.fillAmount = currentFill;
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        public void UpdateHealth(float currentHealth, float maxHealth)
        {
            targetFill = currentHealth / maxHealth;
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
} 