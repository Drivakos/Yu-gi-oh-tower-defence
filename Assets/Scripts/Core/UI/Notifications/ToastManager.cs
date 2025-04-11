using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace YuGiOhTowerDefense.Core.UI.Notifications
{
    public enum ToastType
    {
        Success,
        Error,
        Warning,
        Info
    }
    
    public class ToastManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float defaultDuration = 3f;
        [SerializeField] private float fadeDuration = 0.5f;
        [SerializeField] private int maxToasts = 3;
        
        [Header("Prefabs")]
        [SerializeField] private GameObject toastPrefab;
        
        [Header("Colors")]
        [SerializeField] private Color successColor = Color.green;
        [SerializeField] private Color errorColor = Color.red;
        [SerializeField] private Color warningColor = Color.yellow;
        [SerializeField] private Color infoColor = Color.blue;
        
        private Queue<Toast> toastQueue = new Queue<Toast>();
        private List<Toast> activeToasts = new List<Toast>();
        
        private static ToastManager instance;
        public static ToastManager Instance => instance;
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        public void ShowToast(string message, ToastType type = ToastType.Info, float duration = -1)
        {
            if (duration < 0) duration = defaultDuration;
            
            Toast toast = new Toast
            {
                Message = message,
                Type = type,
                Duration = duration
            };
            
            toastQueue.Enqueue(toast);
            ProcessQueue();
        }
        
        private void ProcessQueue()
        {
            if (activeToasts.Count >= maxToasts || toastQueue.Count == 0) return;
            
            Toast toast = toastQueue.Dequeue();
            StartCoroutine(ShowToastCoroutine(toast));
        }
        
        private IEnumerator ShowToastCoroutine(Toast toast)
        {
            GameObject toastObj = Instantiate(toastPrefab, transform);
            ToastUI toastUI = toastObj.GetComponent<ToastUI>();
            
            if (toastUI != null)
            {
                toastUI.Initialize(toast.Message, GetToastColor(toast.Type));
                activeToasts.Add(toastUI);
                
                // Fade in
                yield return StartCoroutine(toastUI.FadeIn(fadeDuration));
                
                // Wait for duration
                yield return new WaitForSeconds(toast.Duration);
                
                // Fade out
                yield return StartCoroutine(toastUI.FadeOut(fadeDuration));
                
                activeToasts.Remove(toastUI);
                Destroy(toastObj);
                
                ProcessQueue();
            }
        }
        
        private Color GetToastColor(ToastType type)
        {
            return type switch
            {
                ToastType.Success => successColor,
                ToastType.Error => errorColor,
                ToastType.Warning => warningColor,
                ToastType.Info => infoColor,
                _ => infoColor
            };
        }
        
        private class Toast
        {
            public string Message { get; set; }
            public ToastType Type { get; set; }
            public float Duration { get; set; }
        }
    }
    
    public class ToastUI : MonoBehaviour
    {
        [SerializeField] private Image background;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private CanvasGroup canvasGroup;
        
        public void Initialize(string message, Color color)
        {
            messageText.text = message;
            background.color = color;
            canvasGroup.alpha = 0f;
        }
        
        public IEnumerator FadeIn(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }
        
        public IEnumerator FadeOut(float duration)
        {
            float elapsed = 0f;
            float startAlpha = canvasGroup.alpha;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
                yield return null;
            }
            canvasGroup.alpha = 0f;
        }
    }
} 