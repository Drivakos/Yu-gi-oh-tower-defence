using UnityEngine;
using System.Collections.Generic;
using System;

namespace YuGiOhTowerDefense.Core
{
    public class LevelRatingSystem : MonoBehaviour
    {
        [System.Serializable]
        public class LevelRating
        {
            public string levelId;
            public int stars;
            public float completionTime;
            public int monstersUsed;
            public int cardsUsed;
            public int duelPointsEarned;
            public bool isCompleted;
            public DateTime completionDate;
        }
        
        [System.Serializable]
        public class RatingCriteria
        {
            public int stars;
            public float maxTime;
            public int maxMonsters;
            public int maxCards;
            public int minDuelPoints;
        }
        
        [Header("Rating Criteria")]
        [SerializeField] private List<RatingCriteria> ratingCriteria = new List<RatingCriteria>
        {
            new RatingCriteria { stars = 3, maxTime = 180f, maxMonsters = 5, maxCards = 3, minDuelPoints = 1000 },
            new RatingCriteria { stars = 2, maxTime = 240f, maxMonsters = 7, maxCards = 5, minDuelPoints = 750 },
            new RatingCriteria { stars = 1, maxTime = 300f, maxMonsters = 10, maxCards = 8, minDuelPoints = 500 }
        };
        
        [Header("Rewards")]
        [SerializeField] private int baseReward = 100;
        [SerializeField] private int starMultiplier = 50;
        [SerializeField] private int perfectBonus = 200;
        
        private Dictionary<string, LevelRating> levelRatings = new Dictionary<string, LevelRating>();
        private GameManager gameManager;
        private LevelManager levelManager;
        
        private void Awake()
        {
            gameManager = FindObjectOfType<GameManager>();
            levelManager = FindObjectOfType<LevelManager>();
            
            if (gameManager == null)
            {
                Debug.LogError("GameManager not found!");
            }
            
            if (levelManager == null)
            {
                Debug.LogError("LevelManager not found!");
            }
            
            LoadLevelRatings();
        }
        
        private void Start()
        {
            if (gameManager != null)
            {
                gameManager.OnLevelCompleted += OnLevelCompleted;
            }
        }
        
        public void OnLevelCompleted(string levelId, float completionTime, int monstersUsed, int cardsUsed, int duelPointsEarned)
        {
            // Calculate stars based on performance
            int stars = CalculateStars(completionTime, monstersUsed, cardsUsed, duelPointsEarned);
            
            // Check if this is a new record
            bool isNewRecord = false;
            if (!levelRatings.ContainsKey(levelId) || stars > levelRatings[levelId].stars)
            {
                isNewRecord = true;
            }
            
            // Create or update level rating
            LevelRating rating = new LevelRating
            {
                levelId = levelId,
                stars = stars,
                completionTime = completionTime,
                monstersUsed = monstersUsed,
                cardsUsed = cardsUsed,
                duelPointsEarned = duelPointsEarned,
                isCompleted = true,
                completionDate = DateTime.Now
            };
            
            levelRatings[levelId] = rating;
            
            // Calculate and award rewards
            int reward = CalculateReward(stars, isNewRecord);
            
            // Save ratings and update player resources
            SaveLevelRatings();
            
            if (gameManager != null)
            {
                gameManager.AddDuelPoints(reward);
            }
            
            // Notify UI to show rating results
            if (levelManager != null)
            {
                levelManager.ShowLevelRatingResults(levelId, stars, reward, isNewRecord);
            }
        }
        
        private int CalculateStars(float completionTime, int monstersUsed, int cardsUsed, int duelPointsEarned)
        {
            foreach (var criteria in ratingCriteria)
            {
                if (completionTime <= criteria.maxTime &&
                    monstersUsed <= criteria.maxMonsters &&
                    cardsUsed <= criteria.maxCards &&
                    duelPointsEarned >= criteria.minDuelPoints)
                {
                    return criteria.stars;
                }
            }
            
            return 0; // No stars if criteria not met
        }
        
        private int CalculateReward(int stars, bool isNewRecord)
        {
            int reward = baseReward + (stars * starMultiplier);
            
            if (isNewRecord)
            {
                reward += perfectBonus;
            }
            
            return reward;
        }
        
        public LevelRating GetLevelRating(string levelId)
        {
            if (levelRatings.ContainsKey(levelId))
            {
                return levelRatings[levelId];
            }
            
            return null;
        }
        
        public int GetTotalStars()
        {
            int total = 0;
            foreach (var rating in levelRatings.Values)
            {
                total += rating.stars;
            }
            return total;
        }
        
        public int GetCompletedLevels()
        {
            int count = 0;
            foreach (var rating in levelRatings.Values)
            {
                if (rating.isCompleted)
                {
                    count++;
                }
            }
            return count;
        }
        
        private void LoadLevelRatings()
        {
            string ratingsJson = PlayerPrefs.GetString("LevelRatings", "{}");
            Dictionary<string, LevelRating> loadedRatings = JsonUtility.FromJson<Dictionary<string, LevelRating>>(ratingsJson);
            
            if (loadedRatings != null)
            {
                levelRatings = loadedRatings;
            }
        }
        
        private void SaveLevelRatings()
        {
            string ratingsJson = JsonUtility.ToJson(levelRatings);
            PlayerPrefs.SetString("LevelRatings", ratingsJson);
            PlayerPrefs.Save();
        }
        
        private void OnDestroy()
        {
            if (gameManager != null)
            {
                gameManager.OnLevelCompleted -= OnLevelCompleted;
            }
        }
    }
} 