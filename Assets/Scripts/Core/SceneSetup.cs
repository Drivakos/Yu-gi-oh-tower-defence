using UnityEngine;
using YuGiOh.Managers;
using YuGiOh.Cards;

namespace YuGiOh.Core
{
    public class SceneSetup : MonoBehaviour
    {
        [Header("Manager References")]
        [SerializeField] private GameObject gameManagerPrefab;
        [SerializeField] private GameObject waveManagerPrefab;
        
        [Header("Container Settings")]
        [SerializeField] private Vector3 monsterContainerPosition = Vector3.zero;
        [SerializeField] private Vector3 enemyContainerPosition = Vector3.zero;
        [SerializeField] private Vector3 spawnPointPosition = new Vector3(0, 0, 20);
        [SerializeField] private Vector3 objectivePointPosition = new Vector3(0, 0, -20);
        
        [Header("Enemy Prefabs")]
        [SerializeField] private GameObject[] enemyPrefabs;
        
        private GameManager gameManager;
        private WaveManager waveManager;
        
        private void Start()
        {
            SetupManagers();
            SetupContainers();
            ConnectManagers();
            SetupEnemyPrefabs();
        }
        
        private void SetupManagers()
        {
            if (gameManagerPrefab == null)
            {
                Debug.LogError("SceneSetup: GameManager prefab is not assigned!");
                return;
            }
            
            if (waveManagerPrefab == null)
            {
                Debug.LogError("SceneSetup: WaveManager prefab is not assigned!");
                return;
            }
            
            // Get or create GameManager
            gameManager = FindObjectOfType<GameManager>();
            if (gameManager == null)
            {
                GameObject gameManagerObj = Instantiate(gameManagerPrefab);
                gameManager = gameManagerObj.GetComponent<GameManager>();
            }
            
            // Get or create WaveManager
            waveManager = FindObjectOfType<WaveManager>();
            if (waveManager == null)
            {
                GameObject waveManagerObj = Instantiate(waveManagerPrefab);
                waveManager = waveManagerObj.GetComponent<WaveManager>();
            }
        }
        
        private void SetupContainers()
        {
            // Create monster container
            GameObject monsterContainer = new GameObject("MonsterContainer");
            monsterContainer.transform.position = monsterContainerPosition;
            
            // Create enemy container
            GameObject enemyContainer = new GameObject("EnemyContainer");
            enemyContainer.transform.position = enemyContainerPosition;
            
            // Create spawn point
            GameObject spawnPoint = new GameObject("SpawnPoint");
            spawnPoint.transform.position = spawnPointPosition;
            
            // Create objective point
            GameObject objectivePoint = new GameObject("ObjectivePoint");
            objectivePoint.transform.position = objectivePointPosition;
        }
        
        private void ConnectManagers()
        {
            if (gameManager == null || waveManager == null)
            {
                Debug.LogError("SceneSetup: Managers are not properly initialized!");
                return;
            }
            
            // Assign containers
            gameManager.SetMonsterContainer(GameObject.Find("MonsterContainer").transform);
            gameManager.SetEnemyContainer(GameObject.Find("EnemyContainer").transform);
            
            waveManager.SetMonsterContainer(GameObject.Find("MonsterContainer").transform);
            waveManager.SetEnemyContainer(GameObject.Find("EnemyContainer").transform);
            
            // Assign points
            gameManager.SetSpawnPoint(GameObject.Find("SpawnPoint").transform);
            gameManager.SetObjectivePoint(GameObject.Find("ObjectivePoint").transform);
        }
        
        private void SetupEnemyPrefabs()
        {
            if (enemyPrefabs == null || enemyPrefabs.Length == 0)
            {
                Debug.LogWarning("SceneSetup: No enemy prefabs assigned!");
                return;
            }
            
            foreach (GameObject prefab in enemyPrefabs)
            {
                if (prefab == null)
                {
                    Debug.LogError("SceneSetup: Enemy prefab is null!");
                    continue;
                }
                
                MonsterCard monsterCard = prefab.GetComponent<MonsterCard>();
                if (monsterCard == null)
                {
                    Debug.LogError($"SceneSetup: Enemy prefab '{prefab.name}' does not have MonsterCard component!");
                    continue;
                }
                
                waveManager.AddEnemyPrefab(prefab);
            }
        }
    }
} 