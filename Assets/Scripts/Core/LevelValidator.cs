using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using YuGiOhTowerDefense.Core;
using YuGiOhTowerDefense.Monsters;
using YuGiOhTowerDefense.Cards;

namespace YuGiOhTowerDefense.Core
{
    public class LevelValidator : MonoBehaviour
    {
        [System.Serializable]
        public class ValidationResult
        {
            public bool isValid;
            public List<string> errors = new List<string>();
            public List<string> warnings = new List<string>();
            public Dictionary<string, float> metrics = new Dictionary<string, float>();
            
            public ValidationResult()
            {
                isValid = true;
                errors = new List<string>();
                warnings = new List<string>();
                metrics = new Dictionary<string, float>();
            }
        }
        
        [Header("Validation Settings")]
        [SerializeField] private int minGridSize = 5;
        [SerializeField] private int maxGridSize = 20;
        [SerializeField] private int minWaves = 1;
        [SerializeField] private int maxWaves = 10;
        [SerializeField] private int minStartingLifePoints = 1000;
        [SerializeField] private int minStartingDuelPoints = 500;
        [SerializeField] private float minWaveDelay = 5f;
        [SerializeField] private float minSpawnDelay = 1f;
        
        [Header("Difficulty Settings")]
        [SerializeField] private int minMonstersPerWave = 3;
        [SerializeField] private int maxMonstersPerWave = 20;
        [SerializeField] private float minDifficultyScore = 0.5f;
        [SerializeField] private float maxDifficultyScore = 2.0f;
        
        private MonsterDatabase monsterDatabase;
        private CardDatabase cardDatabase;
        
        private void Awake()
        {
            monsterDatabase = FindObjectOfType<MonsterDatabase>();
            if (monsterDatabase == null)
            {
                Debug.LogError("LevelValidator: MonsterDatabase not found!");
            }
            
            cardDatabase = FindObjectOfType<CardDatabase>();
            if (cardDatabase == null)
            {
                Debug.LogError("LevelValidator: CardDatabase not found!");
            }
        }
        
        public ValidationResult ValidateLevel(LevelData level)
        {
            ValidationResult result = new ValidationResult();
            
            // Validate basic requirements
            ValidateBasicRequirements(level, result);
            
            // Validate grid dimensions
            ValidateGridDimensions(level, result);
            
            // Validate waves
            ValidateWaves(level, result);
            
            // Validate available monsters and cards
            ValidateAvailableUnits(level, result);
            
            // Calculate difficulty metrics
            CalculateDifficultyMetrics(level, result);
            
            // Set overall validity
            result.isValid = result.errors.Count == 0;
            
            return result;
        }
        
        private void ValidateBasicRequirements(LevelData level, ValidationResult result)
        {
            if (string.IsNullOrEmpty(level.name))
            {
                result.errors.Add("Level name cannot be empty");
            }
            
            if (string.IsNullOrEmpty(level.description))
            {
                result.warnings.Add("Level description is empty");
            }
            
            if (level.startingLifePoints < minStartingLifePoints)
            {
                result.errors.Add($"Starting life points must be at least {minStartingLifePoints}");
            }
            
            if (level.startingDuelPoints < minStartingDuelPoints)
            {
                result.errors.Add($"Starting duel points must be at least {minStartingDuelPoints}");
            }
        }
        
        private void ValidateGridDimensions(LevelData level, ValidationResult result)
        {
            if (level.gridWidth < minGridSize || level.gridWidth > maxGridSize)
            {
                result.errors.Add($"Grid width must be between {minGridSize} and {maxGridSize}");
            }
            
            if (level.gridHeight < minGridSize || level.gridHeight > maxGridSize)
            {
                result.errors.Add($"Grid height must be between {minGridSize} and {maxGridSize}");
            }
        }
        
        private void ValidateWaves(LevelData level, ValidationResult result)
        {
            if (level.waves.Count < minWaves)
            {
                result.errors.Add($"Level must have at least {minWaves} wave");
            }
            
            if (level.waves.Count > maxWaves)
            {
                result.errors.Add($"Level cannot have more than {maxWaves} waves");
            }
            
            for (int i = 0; i < level.waves.Count; i++)
            {
                WaveData wave = level.waves[i];
                
                if (string.IsNullOrEmpty(wave.name))
                {
                    result.warnings.Add($"Wave {i + 1} has no name");
                }
                
                if (wave.spawnDelay < minSpawnDelay)
                {
                    result.errors.Add($"Wave {i + 1} spawn delay must be at least {minSpawnDelay} seconds");
                }
                
                if (wave.waveDelay < minWaveDelay)
                {
                    result.errors.Add($"Wave {i + 1} delay must be at least {minWaveDelay} seconds");
                }
                
                ValidateWaveEnemies(wave, i, result);
            }
        }
        
        private void ValidateWaveEnemies(WaveData wave, int waveIndex, ValidationResult result)
        {
            if (wave.enemies.Count == 0)
            {
                result.errors.Add($"Wave {waveIndex + 1} has no enemies");
                return;
            }
            
            int totalEnemies = 0;
            foreach (EnemySpawnData enemy in wave.enemies)
            {
                if (string.IsNullOrEmpty(enemy.enemyId))
                {
                    result.errors.Add($"Wave {waveIndex + 1} has an enemy with no ID");
                    continue;
                }
                
                if (enemy.count <= 0)
                {
                    result.errors.Add($"Wave {waveIndex + 1} has an invalid enemy count");
                    continue;
                }
                
                if (enemy.spawnInterval < 0.1f)
                {
                    result.errors.Add($"Wave {waveIndex + 1} has an invalid spawn interval");
                    continue;
                }
                
                totalEnemies += enemy.count;
            }
            
            if (totalEnemies < minMonstersPerWave)
            {
                result.warnings.Add($"Wave {waveIndex + 1} has fewer than {minMonstersPerWave} total enemies");
            }
            
            if (totalEnemies > maxMonstersPerWave)
            {
                result.warnings.Add($"Wave {waveIndex + 1} has more than {maxMonstersPerWave} total enemies");
            }
        }
        
        private void ValidateAvailableUnits(LevelData level, ValidationResult result)
        {
            if (level.availableMonsterIds.Count == 0)
            {
                result.errors.Add("No monsters available in the level");
            }
            
            if (level.availableCardIds.Count == 0)
            {
                result.warnings.Add("No cards available in the level");
            }
            
            foreach (string monsterId in level.availableMonsterIds)
            {
                if (!monsterDatabase.IsValidMonsterId(monsterId))
                {
                    result.errors.Add($"Invalid monster ID: {monsterId}");
                }
            }
            
            foreach (string cardId in level.availableCardIds)
            {
                if (!cardDatabase.IsValidCardId(cardId))
                {
                    result.errors.Add($"Invalid card ID: {cardId}");
                }
            }
        }
        
        private void CalculateDifficultyMetrics(LevelData level, ValidationResult result)
        {
            float totalEnemyPower = 0f;
            float totalWaveCount = level.waves.Count;
            float averageEnemiesPerWave = 0f;
            
            foreach (WaveData wave in level.waves)
            {
                int waveEnemyCount = 0;
                foreach (EnemySpawnData enemy in wave.enemies)
                {
                    MonsterData monsterData = monsterDatabase.GetMonsterData(enemy.enemyId);
                    if (monsterData != null)
                    {
                        totalEnemyPower += monsterData.power * enemy.count;
                        waveEnemyCount += enemy.count;
                    }
                }
                averageEnemiesPerWave += waveEnemyCount;
            }
            
            averageEnemiesPerWave /= totalWaveCount;
            
            // Calculate difficulty score (0.5 to 2.0)
            float difficultyScore = Mathf.Clamp(
                (totalEnemyPower / (level.startingLifePoints * totalWaveCount)) * 
                (averageEnemiesPerWave / minMonstersPerWave),
                minDifficultyScore,
                maxDifficultyScore
            );
            
            result.metrics["TotalEnemyPower"] = totalEnemyPower;
            result.metrics["AverageEnemiesPerWave"] = averageEnemiesPerWave;
            result.metrics["DifficultyScore"] = difficultyScore;
        }
        
        public bool IsLevelValid(LevelData level)
        {
            ValidationResult result = ValidateLevel(level);
            return result.isValid;
        }
        
        public ValidationResult GetValidationDetails(LevelData level)
        {
            return ValidateLevel(level);
        }
    }
} 