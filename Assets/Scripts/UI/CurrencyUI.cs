using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using YuGiOhTowerDefense.Core;

namespace YuGiOhTowerDefense.UI
{
    public class CurrencyUI : MonoBehaviour
    {
        [System.Serializable]
        public class CurrencyDisplay
        {
            public CurrencyType type;
            public GameObject container;
            public TextMeshProUGUI amountText;
            public Image iconImage;
            public Animation addAnimation;
            public Animation subtractAnimation;
        }
        
        [Header("Currency Displays")]
        [SerializeField] private List<CurrencyDisplay> currencyDisplays = new List<CurrencyDisplay>();
        
        [Header("Animation Settings")]
        [SerializeField] private float animationDuration = 0.5f;
        [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("References")]
        [SerializeField] private CurrencyManager currencyManager;
        
        private Dictionary<CurrencyType, CurrencyDisplay> displayLookup = new Dictionary<CurrencyType, CurrencyDisplay>();
        private Dictionary<CurrencyType, int> previousValues = new Dictionary<CurrencyType, int>();
        private Dictionary<CurrencyType, Coroutine> runningAnimations = new Dictionary<CurrencyType, Coroutine>();
        
        private void Awake()
        {
            if (currencyManager == null)
            {
                currencyManager = CurrencyManager.Instance;
                if (currencyManager == null)
                {
                    Debug.LogError("CurrencyManager not found!");
                    gameObject.SetActive(false);
                    return;
                }
            }
            
            // Build lookup dictionary for faster access
            displayLookup.Clear();
            foreach (var display in currencyDisplays)
            {
                if (display.container != null && display.amountText != null)
                {
                    displayLookup[display.type] = display;
                    previousValues[display.type] = currencyManager.GetCurrency(display.type);
                }
            }
        }
        
        private void OnEnable()
        {
            if (currencyManager != null)
            {
                currencyManager.OnCurrencyChanged += HandleCurrencyChanged;
                UpdateAllDisplays();
            }
        }
        
        private void OnDisable()
        {
            if (currencyManager != null)
            {
                currencyManager.OnCurrencyChanged -= HandleCurrencyChanged;
            }
            
            // Stop all running animations
            foreach (var anim in runningAnimations.Values)
            {
                if (anim != null)
                {
                    StopCoroutine(anim);
                }
            }
            runningAnimations.Clear();
        }
        
        private void HandleCurrencyChanged(CurrencyType type, int newAmount)
        {
            if (!displayLookup.ContainsKey(type))
            {
                return;
            }
            
            int oldAmount = previousValues.ContainsKey(type) ? previousValues[type] : 0;
            previousValues[type] = newAmount;
            
            CurrencyDisplay display = displayLookup[type];
            
            // Stop any running animation for this currency
            if (runningAnimations.TryGetValue(type, out Coroutine runningAnimation) && runningAnimation != null)
            {
                StopCoroutine(runningAnimation);
                runningAnimations.Remove(type);
            }
            
            // Start a new animation
            runningAnimations[type] = StartCoroutine(AnimateCurrencyChange(display, oldAmount, newAmount));
            
            // Play appropriate animation if available
            if (newAmount > oldAmount && display.addAnimation != null)
            {
                display.addAnimation.Play();
            }
            else if (newAmount < oldAmount && display.subtractAnimation != null)
            {
                display.subtractAnimation.Play();
            }
        }
        
        private System.Collections.IEnumerator AnimateCurrencyChange(CurrencyDisplay display, int fromValue, int toValue)
        {
            float time = 0;
            
            while (time < animationDuration)
            {
                time += Time.deltaTime;
                float progress = animationCurve.Evaluate(time / animationDuration);
                int currentValue = (int)Mathf.Lerp(fromValue, toValue, progress);
                
                display.amountText.text = FormatCurrencyAmount(currentValue);
                
                yield return null;
            }
            
            // Ensure we end on the exact target value
            display.amountText.text = FormatCurrencyAmount(toValue);
            
            runningAnimations.Remove(display.type);
        }
        
        private void UpdateAllDisplays()
        {
            foreach (var display in currencyDisplays)
            {
                int amount = currencyManager.GetCurrency(display.type);
                display.amountText.text = FormatCurrencyAmount(amount);
                previousValues[display.type] = amount;
            }
        }
        
        private string FormatCurrencyAmount(int amount)
        {
            if (amount >= 1000000)
            {
                return $"{amount / 1000000f:0.#}M";
            }
            else if (amount >= 1000)
            {
                return $"{amount / 1000f:0.#}K";
            }
            else
            {
                return amount.ToString();
            }
        }
        
        // For manual updates if needed
        public void RefreshDisplays()
        {
            UpdateAllDisplays();
        }
    }
} 