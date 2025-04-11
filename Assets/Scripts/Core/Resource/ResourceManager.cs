using UnityEngine;
using System;
using TMPro;

namespace YuGiOhTowerDefense.Core.Resource
{
    /// <summary>
    /// Manages player resources (DP and LP)
    /// </summary>
    public class ResourceManager : MonoBehaviour
    {
        [Header("Resource Settings")]
        [SerializeField] private int startingDP = 1000;
        [SerializeField] private int startingLP = 8000;
        [SerializeField] private int dpPerTurn = 1000;
        
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI dpText;
        [SerializeField] private TextMeshProUGUI lpText;
        
        private int currentDP;
        private int currentLP;
        private bool isGameOver;
        
        public int CurrentDP => currentDP;
        public int CurrentLP => currentLP;
        public bool IsGameOver => isGameOver;
        
        public event Action<int> OnDPChanged;
        public event Action<int> OnLPChanged;
        public event Action OnGameOver;
        
        private void Awake()
        {
            InitializeResources();
        }
        
        private void InitializeResources()
        {
            currentDP = startingDP;
            currentLP = startingLP;
            isGameOver = false;
            
            UpdateUI();
        }
        
        public bool CanAffordCard(int cost)
        {
            return currentDP >= cost;
        }
        
        public bool TrySpendDP(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning("Cannot spend negative or zero DP");
                return false;
            }
            
            if (!CanAffordCard(amount))
            {
                Debug.LogWarning("Not enough DP to spend");
                return false;
            }
            
            currentDP -= amount;
            OnDPChanged?.Invoke(currentDP);
            UpdateUI();
            return true;
        }
        
        public void AddDP(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning("Cannot add negative or zero DP");
                return;
            }
            
            currentDP += amount;
            OnDPChanged?.Invoke(currentDP);
            UpdateUI();
        }
        
        public void TakeDamage(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning("Cannot take negative or zero damage");
                return;
            }
            
            currentLP = Mathf.Max(0, currentLP - amount);
            OnLPChanged?.Invoke(currentLP);
            UpdateUI();
            
            if (currentLP <= 0)
            {
                HandleGameOver();
            }
        }
        
        public void HealLP(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning("Cannot heal negative or zero LP");
                return;
            }
            
            currentLP += amount;
            OnLPChanged?.Invoke(currentLP);
            UpdateUI();
        }
        
        public void StartNewTurn()
        {
            AddDP(dpPerTurn);
        }
        
        private void HandleGameOver()
        {
            isGameOver = true;
            OnGameOver?.Invoke();
        }
        
        private void UpdateUI()
        {
            if (dpText != null)
            {
                dpText.text = $"DP: {currentDP}";
            }
            
            if (lpText != null)
            {
                lpText.text = $"LP: {currentLP}";
            }
        }
        
        public void ResetResources()
        {
            InitializeResources();
        }
    }
} 