using System;
using System.Collections.Generic;
using UnityEngine;

namespace YuGiOhTowerDefense.Core
{
    [Serializable]
    public class Achievement
    {
        public string id;
        public string title;
        public string description;
        public Sprite icon;
        public int progressTarget;
        public AchievementCategory category;
        public AchievementReward reward;
        public bool isSecret;
        public bool isRepeatable;
        public int repeatCount; // How many times this achievement can be earned
        
        [NonSerialized]
        public bool isUnlocked;
        
        [NonSerialized]
        public int currentProgress;
        
        [NonSerialized]
        public int timesCompleted;
    }
    
    [Serializable]
    public class AchievementReward
    {
        public RewardType type;
        public int amount;
        public string itemId; // For card rewards
        
        public AchievementReward(RewardType type, int amount, string itemId = "")
        {
            this.type = type;
            this.amount = amount;
            this.itemId = itemId;
        }
    }
    
    public enum AchievementCategory
    {
        Collection,
        Battle,
        Progression,
        Special
    }
    
    public enum RewardType
    {
        None,
        Coins,
        Gems,
        DuelPoints,
        Card,
        CardPack
    }
    
    public class AchievementManager : MonoBehaviour
    {
        [Header("Achievements")]
        [SerializeField] private List<Achievement> achievements = new List<Achievement>();
        
        [Header("Notifications")]
        [SerializeField] private GameObject achievementNotificationPrefab;
        [SerializeField] private Transform notificationContainer;
        [SerializeField] private float notificationDuration = 3f;
        [SerializeField] private float notificationDelay = 0.5f;
        
        [Header("References")]
        [SerializeField] private CurrencyManager currencyManager;
        [SerializeField] private PlayerCollection playerCollection;
        [SerializeField] private CardPackManager cardPackManager;
        [SerializeField] private AudioManager audioManager;
        
        private Dictionary<string, Achievement> achievementLookup = new Dictionary<string, Achievement>();
        private Queue<Achievement> achievementNotificationQueue = new Queue<Achievement>();
        private bool isShowingNotification = false;
        
        // Events
        public event Action<Achievement> OnAchievementUnlocked;
        public event Action<Achievement, int, int> OnAchievementProgressUpdated;
        
        public static AchievementManager Instance { get; private set; }
        
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
            
            // Find references if not assigned
            if (currencyManager == null)
                currencyManager = CurrencyManager.Instance;
                
            if (playerCollection == null)
                playerCollection = PlayerCollection.Instance;
                
            if (cardPackManager == null)
                cardPackManager = FindObjectOfType<CardPackManager>();
                
            if (audioManager == null)
                audioManager = AudioManager.Instance;
            
            // Build lookup dictionary
            foreach (var achievement in achievements)
            {
                if (!string.IsNullOrEmpty(achievement.id) && !achievementLookup.ContainsKey(achievement.id))
                {
                    achievementLookup.Add(achievement.id, achievement);
                }
                else if (string.IsNullOrEmpty(achievement.id))
                {
                    Debug.LogError("Achievement has no ID!");
                }
                else
                {
                    Debug.LogError($"Duplicate achievement ID: {achievement.id}");
                }
            }
            
            // Load achievement progress
            LoadAchievements();
        }
        
        private void Update()
        {
            // Process achievement notification queue
            if (!isShowingNotification && achievementNotificationQueue.Count > 0)
            {
                ShowNextAchievementNotification();
            }
        }
        
        private void LoadAchievements()
        {
            foreach (var achievement in achievements)
            {
                // Load unlocked status
                achievement.isUnlocked = PlayerPrefs.GetInt($"Achievement_{achievement.id}_Unlocked", 0) == 1;
                
                // Load progress
                achievement.currentProgress = PlayerPrefs.GetInt($"Achievement_{achievement.id}_Progress", 0);
                
                // Load times completed
                achievement.timesCompleted = PlayerPrefs.GetInt($"Achievement_{achievement.id}_TimesCompleted", 0);
            }
        }
        
        private void SaveAchievements()
        {
            foreach (var achievement in achievements)
            {
                PlayerPrefs.SetInt($"Achievement_{achievement.id}_Unlocked", achievement.isUnlocked ? 1 : 0);
                PlayerPrefs.SetInt($"Achievement_{achievement.id}_Progress", achievement.currentProgress);
                PlayerPrefs.SetInt($"Achievement_{achievement.id}_TimesCompleted", achievement.timesCompleted);
            }
            
            PlayerPrefs.Save();
        }
        
        public void ResetAllAchievements()
        {
            foreach (var achievement in achievements)
            {
                achievement.isUnlocked = false;
                achievement.currentProgress = 0;
                achievement.timesCompleted = 0;
            }
            
            SaveAchievements();
            Debug.Log("All achievements have been reset");
        }
        
        public List<Achievement> GetAllAchievements()
        {
            return new List<Achievement>(achievements);
        }
        
        public List<Achievement> GetAchievementsByCategory(AchievementCategory category)
        {
            return achievements.FindAll(a => a.category == category);
        }
        
        public List<Achievement> GetUnlockedAchievements()
        {
            return achievements.FindAll(a => a.isUnlocked);
        }
        
        public Achievement GetAchievement(string id)
        {
            return achievementLookup.TryGetValue(id, out Achievement achievement) ? achievement : null;
        }
        
        public void UpdateProgress(string achievementId, int progressIncrement = 1)
        {
            if (achievementLookup.TryGetValue(achievementId, out Achievement achievement))
            {
                // If already unlocked and not repeatable, skip
                if (achievement.isUnlocked && !achievement.isRepeatable)
                {
                    return;
                }
                
                // If reached max repeat count, skip
                if (achievement.isRepeatable && achievement.repeatCount > 0 && 
                    achievement.timesCompleted >= achievement.repeatCount)
                {
                    return;
                }
                
                int previousProgress = achievement.currentProgress;
                achievement.currentProgress += progressIncrement;
                
                // Cap progress at target
                if (achievement.currentProgress > achievement.progressTarget)
                {
                    achievement.currentProgress = achievement.progressTarget;
                }
                
                // Check if achievement is completed
                if (achievement.currentProgress >= achievement.progressTarget && !achievement.isUnlocked)
                {
                    UnlockAchievement(achievementId);
                }
                else if (achievement.currentProgress >= achievement.progressTarget && achievement.isRepeatable)
                {
                    // Handle repeatable achievement
                    achievement.timesCompleted++;
                    achievement.currentProgress = 0; // Reset progress for next completion
                    
                    // Grant rewards again
                    GrantReward(achievement);
                    
                    // Add to notification queue
                    achievementNotificationQueue.Enqueue(achievement);
                    
                    // Save progress
                    SaveAchievements();
                }
                else
                {
                    // Just progress update, not unlocked yet
                    OnAchievementProgressUpdated?.Invoke(achievement, previousProgress, achievement.currentProgress);
                    SaveAchievements();
                }
            }
            else
            {
                Debug.LogWarning($"Achievement with ID {achievementId} not found!");
            }
        }
        
        public void SetProgress(string achievementId, int newProgress)
        {
            if (achievementLookup.TryGetValue(achievementId, out Achievement achievement))
            {
                if (achievement.isUnlocked && !achievement.isRepeatable)
                {
                    return;
                }
                
                int previousProgress = achievement.currentProgress;
                achievement.currentProgress = Mathf.Clamp(newProgress, 0, achievement.progressTarget);
                
                if (achievement.currentProgress >= achievement.progressTarget && !achievement.isUnlocked)
                {
                    UnlockAchievement(achievementId);
                }
                else
                {
                    OnAchievementProgressUpdated?.Invoke(achievement, previousProgress, achievement.currentProgress);
                    SaveAchievements();
                }
            }
        }
        
        public void UnlockAchievement(string achievementId)
        {
            if (achievementLookup.TryGetValue(achievementId, out Achievement achievement))
            {
                if (achievement.isUnlocked)
                {
                    return;
                }
                
                achievement.isUnlocked = true;
                achievement.currentProgress = achievement.progressTarget;
                achievement.timesCompleted++;
                
                // Grant reward
                GrantReward(achievement);
                
                // Add to notification queue
                achievementNotificationQueue.Enqueue(achievement);
                
                // Invoke event
                OnAchievementUnlocked?.Invoke(achievement);
                
                // Save progress
                SaveAchievements();
                
                Debug.Log($"Achievement unlocked: {achievement.title}");
            }
        }
        
        private void GrantReward(Achievement achievement)
        {
            AchievementReward reward = achievement.reward;
            
            if (reward == null || reward.type == RewardType.None || reward.amount <= 0)
            {
                return;
            }
            
            switch (reward.type)
            {
                case RewardType.Coins:
                    if (currencyManager != null)
                    {
                        currencyManager.AddCurrency(CurrencyType.Coins, reward.amount, $"Achievement: {achievement.title}");
                    }
                    break;
                    
                case RewardType.Gems:
                    if (currencyManager != null)
                    {
                        currencyManager.AddCurrency(CurrencyType.Gems, reward.amount, $"Achievement: {achievement.title}");
                    }
                    break;
                    
                case RewardType.DuelPoints:
                    if (currencyManager != null)
                    {
                        currencyManager.AddCurrency(CurrencyType.DuelPoints, reward.amount, $"Achievement: {achievement.title}");
                    }
                    break;
                    
                case RewardType.Card:
                    if (playerCollection != null && !string.IsNullOrEmpty(reward.itemId))
                    {
                        playerCollection.AddCard(reward.itemId, reward.amount);
                    }
                    break;
                    
                case RewardType.CardPack:
                    if (cardPackManager != null)
                    {
                        // Find pack by name/id and open it
                        CardPack pack = cardPackManager.GetPackByName(reward.itemId);
                        if (pack != null)
                        {
                            for (int i = 0; i < reward.amount; i++)
                            {
                                cardPackManager.OpenPack(pack);
                            }
                        }
                    }
                    break;
            }
        }
        
        private void ShowNextAchievementNotification()
        {
            if (achievementNotificationQueue.Count == 0 || isShowingNotification)
            {
                return;
            }
            
            isShowingNotification = true;
            Achievement achievement = achievementNotificationQueue.Dequeue();
            
            // Play sound
            if (audioManager != null)
            {
                audioManager.PlayOneShot("AchievementUnlocked");
            }
            
            // Create notification UI
            if (achievementNotificationPrefab != null && notificationContainer != null)
            {
                GameObject notificationObj = Instantiate(achievementNotificationPrefab, notificationContainer);
                AchievementNotificationUI notificationUI = notificationObj.GetComponent<AchievementNotificationUI>();
                
                if (notificationUI != null)
                {
                    notificationUI.Initialize(achievement, notificationDuration);
                    notificationUI.OnNotificationComplete += HandleNotificationComplete;
                }
                else
                {
                    // If UI component not found, clean up after delay
                    Destroy(notificationObj, notificationDuration);
                    Invoke(nameof(HandleNotificationComplete), notificationDuration);
                }
            }
            else
            {
                // If prefab or container missing, just wait then continue
                Invoke(nameof(HandleNotificationComplete), notificationDuration);
            }
        }
        
        private void HandleNotificationComplete()
        {
            isShowingNotification = false;
            // Wait a bit before showing the next one
            if (achievementNotificationQueue.Count > 0)
            {
                Invoke(nameof(ShowNextAchievementNotification), notificationDelay);
            }
        }
        
        public void CheckCollectionAchievements()
        {
            if (playerCollection == null)
                return;
                
            int totalCards = playerCollection.TotalCards;
            int uniqueCards = playerCollection.UniqueCards;
            
            // Examples of collection achievements to check
            UpdateProgress("collect_10_cards", 0); // Set progress directly
            UpdateProgress("collect_50_cards", 0);
            UpdateProgress("collect_100_cards", 0);
            
            SetProgress("collect_10_cards", uniqueCards);
            SetProgress("collect_50_cards", uniqueCards);
            SetProgress("collect_100_cards", uniqueCards);
        }
        
        public void CheckBattleAchievements(int enemiesDefeated, int wavesCompleted, int perfectWaves)
        {
            // Examples of battle achievements to update
            UpdateProgress("defeat_100_enemies", enemiesDefeated);
            UpdateProgress("complete_10_waves", wavesCompleted);
            UpdateProgress("perfect_wave", perfectWaves);
        }
        
        public void CheckLevelCompletionAchievement(int levelId, int stars)
        {
            // Examples of level completion achievements
            UpdateProgress("complete_all_levels", 1);
            
            if (stars == 3)
            {
                UpdateProgress("three_star_level", 1);
            }
        }
    }
    
    // Helper class for UI notifications
    public class AchievementNotificationUI : MonoBehaviour
    {
        public event Action OnNotificationComplete;
        
        [SerializeField] private UnityEngine.UI.Image iconImage;
        [SerializeField] private TMPro.TextMeshProUGUI titleText;
        [SerializeField] private TMPro.TextMeshProUGUI descriptionText;
        [SerializeField] private UnityEngine.UI.Image rewardIconImage;
        [SerializeField] private TMPro.TextMeshProUGUI rewardText;
        
        [SerializeField] private Animation showAnimation;
        [SerializeField] private Animation hideAnimation;
        
        [SerializeField] private string showAnimationName = "ShowAchievement";
        [SerializeField] private string hideAnimationName = "HideAchievement";
        
        private float duration;
        
        public void Initialize(Achievement achievement, float displayDuration)
        {
            duration = displayDuration;
            
            // Set UI elements
            if (iconImage != null && achievement.icon != null)
                iconImage.sprite = achievement.icon;
                
            if (titleText != null)
                titleText.text = achievement.title;
                
            if (descriptionText != null)
                descriptionText.text = achievement.description;
                
            // Set reward info
            if (rewardText != null && achievement.reward != null)
            {
                switch (achievement.reward.type)
                {
                    case RewardType.Coins:
                        rewardText.text = $"{achievement.reward.amount} Coins";
                        break;
                    case RewardType.Gems:
                        rewardText.text = $"{achievement.reward.amount} Gems";
                        break;
                    case RewardType.DuelPoints:
                        rewardText.text = $"{achievement.reward.amount} Duel Points";
                        break;
                    case RewardType.Card:
                        rewardText.text = $"{achievement.reward.amount}x Card";
                        break;
                    case RewardType.CardPack:
                        rewardText.text = $"{achievement.reward.amount}x Card Pack";
                        break;
                    default:
                        rewardText.text = "";
                        break;
                }
            }
            
            // Play show animation
            if (showAnimation != null)
            {
                showAnimation.Play(showAnimationName);
            }
            
            // Schedule hide animation
            Invoke(nameof(HideNotification), duration);
        }
        
        private void HideNotification()
        {
            if (hideAnimation != null)
            {
                hideAnimation.Play(hideAnimationName);
                
                // Get animation length and destroy after it's done
                float animLength = 1f;
                AnimationClip clip = null;
                
                foreach (AnimationState state in hideAnimation)
                {
                    if (state.name == hideAnimationName)
                    {
                        animLength = state.length;
                        break;
                    }
                }
                
                Invoke(nameof(CompleteNotification), animLength);
            }
            else
            {
                CompleteNotification();
            }
        }
        
        private void CompleteNotification()
        {
            OnNotificationComplete?.Invoke();
            Destroy(gameObject);
        }
    }
} 