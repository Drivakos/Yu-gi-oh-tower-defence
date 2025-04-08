using System;
using UnityEngine;
using System.Collections.Generic;

namespace YuGiOhTowerDefense.Core
{
    public enum CurrencyType
    {
        Coins,
        Gems,
        DuelPoints
    }
    
    [Serializable]
    public class CurrencyAmount
    {
        public CurrencyType type;
        public int amount;
        
        public CurrencyAmount(CurrencyType type, int amount)
        {
            this.type = type;
            this.amount = amount;
        }
    }
    
    [Serializable]
    public class CurrencyTransaction
    {
        public CurrencyType type;
        public int amount;
        public string source;
        public DateTime timestamp;
        
        public CurrencyTransaction(CurrencyType type, int amount, string source)
        {
            this.type = type;
            this.amount = amount;
            this.source = source;
            this.timestamp = DateTime.Now;
        }
    }
    
    public class CurrencyManager : MonoBehaviour
    {
        [Header("Starting Currency")]
        [SerializeField] private int startingCoins = 500;
        [SerializeField] private int startingGems = 50;
        [SerializeField] private int startingDuelPoints = 0;
        
        [Header("Settings")]
        [SerializeField] private bool resetOnStart = false;
        [SerializeField] private int maxTransactionHistory = 100;
        
        private Dictionary<CurrencyType, int> currencyAmounts = new Dictionary<CurrencyType, int>();
        private List<CurrencyTransaction> transactionHistory = new List<CurrencyTransaction>();
        
        public event Action<CurrencyType, int> OnCurrencyChanged;
        public event Action<CurrencyTransaction> OnTransactionCompleted;
        
        public static CurrencyManager Instance { get; private set; }
        
        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            // Initialize currency values
            InitializeCurrency();
        }
        
        private void InitializeCurrency()
        {
            if (resetOnStart)
            {
                ResetCurrency();
            }
            else
            {
                LoadCurrency();
            }
        }
        
        private void LoadCurrency()
        {
            // Initialize with default values
            currencyAmounts[CurrencyType.Coins] = startingCoins;
            currencyAmounts[CurrencyType.Gems] = startingGems;
            currencyAmounts[CurrencyType.DuelPoints] = startingDuelPoints;
            
            // Load from PlayerPrefs if available
            if (PlayerPrefs.HasKey("Currency_Coins"))
            {
                currencyAmounts[CurrencyType.Coins] = PlayerPrefs.GetInt("Currency_Coins");
            }
            
            if (PlayerPrefs.HasKey("Currency_Gems"))
            {
                currencyAmounts[CurrencyType.Gems] = PlayerPrefs.GetInt("Currency_Gems");
            }
            
            if (PlayerPrefs.HasKey("Currency_DuelPoints"))
            {
                currencyAmounts[CurrencyType.DuelPoints] = PlayerPrefs.GetInt("Currency_DuelPoints");
            }
            
            // Load transaction history if available
            if (PlayerPrefs.HasKey("CurrencyTransactions"))
            {
                string json = PlayerPrefs.GetString("CurrencyTransactions");
                TransactionData data = JsonUtility.FromJson<TransactionData>(json);
                
                if (data != null && data.transactions != null)
                {
                    transactionHistory = data.transactions;
                    
                    // Limit history size
                    if (transactionHistory.Count > maxTransactionHistory)
                    {
                        transactionHistory = transactionHistory.GetRange(
                            transactionHistory.Count - maxTransactionHistory, 
                            maxTransactionHistory);
                    }
                }
            }
            
            Debug.Log("Currency loaded from storage");
        }
        
        private void SaveCurrency()
        {
            // Save currency amounts
            foreach (var currency in currencyAmounts)
            {
                PlayerPrefs.SetInt($"Currency_{currency.Key}", currency.Value);
            }
            
            // Save transaction history
            TransactionData data = new TransactionData
            {
                transactions = transactionHistory
            };
            
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString("CurrencyTransactions", json);
            
            PlayerPrefs.Save();
            Debug.Log("Currency saved to storage");
        }
        
        public void ResetCurrency()
        {
            currencyAmounts.Clear();
            transactionHistory.Clear();
            
            currencyAmounts[CurrencyType.Coins] = startingCoins;
            currencyAmounts[CurrencyType.Gems] = startingGems;
            currencyAmounts[CurrencyType.DuelPoints] = startingDuelPoints;
            
            SaveCurrency();
            
            // Notify all currencies changed
            foreach (var currency in currencyAmounts)
            {
                OnCurrencyChanged?.Invoke(currency.Key, currency.Value);
            }
            
            Debug.Log("Currency reset to starting values");
        }
        
        public int GetCurrency(CurrencyType type)
        {
            if (currencyAmounts.TryGetValue(type, out int amount))
            {
                return amount;
            }
            
            return 0;
        }
        
        public bool HasEnough(CurrencyType type, int amount)
        {
            return GetCurrency(type) >= amount;
        }
        
        public bool HasEnough(List<CurrencyAmount> costs)
        {
            foreach (var cost in costs)
            {
                if (!HasEnough(cost.type, cost.amount))
                {
                    return false;
                }
            }
            
            return true;
        }
        
        public bool AddCurrency(CurrencyType type, int amount, string source)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"Attempted to add invalid amount ({amount}) of {type}");
                return false;
            }
            
            // Get current amount or initialize if not exists
            if (!currencyAmounts.TryGetValue(type, out int currentAmount))
            {
                currentAmount = 0;
                currencyAmounts[type] = currentAmount;
            }
            
            // Add currency
            currencyAmounts[type] = currentAmount + amount;
            
            // Record transaction
            CurrencyTransaction transaction = new CurrencyTransaction(type, amount, source);
            AddTransaction(transaction);
            
            // Save changes
            SaveCurrency();
            
            // Notify listeners
            OnCurrencyChanged?.Invoke(type, currencyAmounts[type]);
            
            Debug.Log($"Added {amount} {type} from {source}. New balance: {currencyAmounts[type]}");
            return true;
        }
        
        public bool SpendCurrency(CurrencyType type, int amount, string reason)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"Attempted to spend invalid amount ({amount}) of {type}");
                return false;
            }
            
            // Check if we have enough
            if (!HasEnough(type, amount))
            {
                Debug.LogWarning($"Not enough {type} to spend {amount}. Current: {GetCurrency(type)}");
                return false;
            }
            
            // Spend currency
            currencyAmounts[type] -= amount;
            
            // Record transaction
            CurrencyTransaction transaction = new CurrencyTransaction(type, -amount, reason);
            AddTransaction(transaction);
            
            // Save changes
            SaveCurrency();
            
            // Notify listeners
            OnCurrencyChanged?.Invoke(type, currencyAmounts[type]);
            
            Debug.Log($"Spent {amount} {type} for {reason}. New balance: {currencyAmounts[type]}");
            return true;
        }
        
        public bool SpendCurrency(List<CurrencyAmount> costs, string reason)
        {
            // Check if we have enough of all currencies
            if (!HasEnough(costs))
            {
                return false;
            }
            
            // Spend all currencies
            foreach (var cost in costs)
            {
                if (!SpendCurrency(cost.type, cost.amount, reason))
                {
                    Debug.LogError($"Failed to spend {cost.amount} {cost.type} for {reason}");
                    return false;
                }
            }
            
            return true;
        }
        
        private void AddTransaction(CurrencyTransaction transaction)
        {
            transactionHistory.Add(transaction);
            
            // Trim history if it gets too large
            if (transactionHistory.Count > maxTransactionHistory)
            {
                transactionHistory.RemoveAt(0);
            }
            
            // Notify listeners
            OnTransactionCompleted?.Invoke(transaction);
        }
        
        public List<CurrencyTransaction> GetTransactionHistory(int count = 0)
        {
            if (count <= 0 || count >= transactionHistory.Count)
            {
                return new List<CurrencyTransaction>(transactionHistory);
            }
            
            // Return the most recent transactions
            return transactionHistory.GetRange(
                transactionHistory.Count - count,
                count);
        }
        
        [Serializable]
        private class TransactionData
        {
            public List<CurrencyTransaction> transactions;
        }
    }
} 