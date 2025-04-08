using UnityEngine;
using System.Collections.Generic;
using YuGiOhTowerDefense.Enemies;

namespace YuGiOhTowerDefense.Core
{
    public class LevelDefinitions : MonoBehaviour
    {
        [SerializeField] private LevelManager levelManager;
        
        [Header("Enemy Prefabs")]
        [SerializeField] private GameObject kuribohPrefab;
        [SerializeField] private GameObject blueEyesUltimateDragonPrefab;
        
        [Header("Monster IDs")]
        [SerializeField] private string[] basicMonsterIds = { "monster_001", "monster_002" };
        [SerializeField] private string[] advancedMonsterIds = { "monster_003", "monster_004" };
        
        [Header("Card IDs")]
        [SerializeField] private string[] basicCardIds = { "card_001", "card_002" };
        [SerializeField] private string[] advancedCardIds = { "card_003", "card_004" };
        
        private void Awake()
        {
            if (levelManager == null)
            {
                levelManager = FindObjectOfType<LevelManager>();
            }
            
            DefineLevels();
        }
        
        private void DefineLevels()
        {
            // Level 1: Tutorial
            LevelManager.LevelData level1 = new LevelManager.LevelData
            {
                levelName = "Tutorial",
                description = "Learn the basics of Yu-Gi-Oh! Tower Defense. Place monsters and use cards to defend against Kuribohs.",
                startingDuelPoints = 1000,
                startingLifePoints = 8000,
                availableMonsterIds = new List<string>(basicMonsterIds),
                availableCardIds = new List<string>(basicCardIds),
                isUnlocked = true,
                waves = new List<WaveManager.Wave>
                {
                    CreateTutorialWave()
                }
            };
            
            // Level 2: First Challenge
            LevelManager.LevelData level2 = new LevelManager.LevelData
            {
                levelName = "First Challenge",
                description = "Face your first real challenge with more Kuribohs and a mini-boss.",
                startingDuelPoints = 1500,
                startingLifePoints = 8000,
                availableMonsterIds = new List<string>(basicMonsterIds),
                availableCardIds = new List<string>(basicCardIds),
                isUnlocked = false,
                waves = new List<WaveManager.Wave>
                {
                    CreateFirstChallengeWave1(),
                    CreateFirstChallengeWave2()
                }
            };
            
            // Level 3: Blue-Eyes Invasion
            LevelManager.LevelData level3 = new LevelManager.LevelData
            {
                levelName = "Blue-Eyes Invasion",
                description = "Defend against the powerful Blue-Eyes Ultimate Dragon!",
                startingDuelPoints = 2000,
                startingLifePoints = 8000,
                availableMonsterIds = new List<string>(advancedMonsterIds),
                availableCardIds = new List<string>(advancedCardIds),
                isUnlocked = false,
                waves = new List<WaveManager.Wave>
                {
                    CreateBlueEyesWave1(),
                    CreateBlueEyesWave2(),
                    CreateBlueEyesBossWave()
                }
            };
            
            // Add levels to the level manager
            // Note: This would typically be done in the Unity Inspector,
            // but we're doing it programmatically for demonstration
            var levels = new List<LevelManager.LevelData> { level1, level2, level3 };
            
            // Use reflection to set the private levels field
            var field = typeof(LevelManager).GetField("levels", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(levelManager, levels);
            }
            else
            {
                Debug.LogError("Could not find 'levels' field in LevelManager");
            }
        }
        
        private WaveManager.Wave CreateTutorialWave()
        {
            return new WaveManager.Wave
            {
                waveName = "Tutorial Wave",
                timeBetweenSpawns = 3f,
                timeBetweenWaves = 10f,
                isBossWave = false,
                enemies = new List<WaveManager.EnemySpawnInfo>
                {
                    new WaveManager.EnemySpawnInfo
                    {
                        enemyPrefab = kuribohPrefab,
                        count = 5,
                        healthMultiplier = 0.5f,
                        speedMultiplier = 0.5f,
                        damageMultiplier = 0.5f
                    }
                }
            };
        }
        
        private WaveManager.Wave CreateFirstChallengeWave1()
        {
            return new WaveManager.Wave
            {
                waveName = "Kuriboh Swarm",
                timeBetweenSpawns = 2f,
                timeBetweenWaves = 10f,
                isBossWave = false,
                enemies = new List<WaveManager.EnemySpawnInfo>
                {
                    new WaveManager.EnemySpawnInfo
                    {
                        enemyPrefab = kuribohPrefab,
                        count = 8,
                        healthMultiplier = 0.7f,
                        speedMultiplier = 0.7f,
                        damageMultiplier = 0.7f
                    }
                }
            };
        }
        
        private WaveManager.Wave CreateFirstChallengeWave2()
        {
            return new WaveManager.Wave
            {
                waveName = "Mini-Boss",
                timeBetweenSpawns = 5f,
                timeBetweenWaves = 15f,
                isBossWave = true,
                enemies = new List<WaveManager.EnemySpawnInfo>
                {
                    new WaveManager.EnemySpawnInfo
                    {
                        enemyPrefab = kuribohPrefab,
                        count = 1,
                        healthMultiplier = 2.0f,
                        speedMultiplier = 1.5f,
                        damageMultiplier = 1.5f
                    }
                }
            };
        }
        
        private WaveManager.Wave CreateBlueEyesWave1()
        {
            return new WaveManager.Wave
            {
                waveName = "Kuriboh Army",
                timeBetweenSpawns = 1.5f,
                timeBetweenWaves = 10f,
                isBossWave = false,
                enemies = new List<WaveManager.EnemySpawnInfo>
                {
                    new WaveManager.EnemySpawnInfo
                    {
                        enemyPrefab = kuribohPrefab,
                        count = 12,
                        healthMultiplier = 1.0f,
                        speedMultiplier = 1.0f,
                        damageMultiplier = 1.0f
                    }
                }
            };
        }
        
        private WaveManager.Wave CreateBlueEyesWave2()
        {
            return new WaveManager.Wave
            {
                waveName = "Elite Kuribohs",
                timeBetweenSpawns = 3f,
                timeBetweenWaves = 15f,
                isBossWave = false,
                enemies = new List<WaveManager.EnemySpawnInfo>
                {
                    new WaveManager.EnemySpawnInfo
                    {
                        enemyPrefab = kuribohPrefab,
                        count = 6,
                        healthMultiplier = 1.5f,
                        speedMultiplier = 1.2f,
                        damageMultiplier = 1.3f
                    }
                }
            };
        }
        
        private WaveManager.Wave CreateBlueEyesBossWave()
        {
            return new WaveManager.Wave
            {
                waveName = "Blue-Eyes Ultimate Dragon",
                timeBetweenSpawns = 10f,
                timeBetweenWaves = 20f,
                isBossWave = true,
                enemies = new List<WaveManager.EnemySpawnInfo>
                {
                    new WaveManager.EnemySpawnInfo
                    {
                        enemyPrefab = blueEyesUltimateDragonPrefab,
                        count = 1,
                        healthMultiplier = 3.0f,
                        speedMultiplier = 1.5f,
                        damageMultiplier = 2.0f
                    }
                }
            };
        }
    }
} 