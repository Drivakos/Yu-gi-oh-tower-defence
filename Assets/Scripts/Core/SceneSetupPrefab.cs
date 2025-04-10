using UnityEngine;
using YuGiOh.Managers;

namespace YuGiOh.Core
{
    public class SceneSetupPrefab : MonoBehaviour
    {
        [Header("Manager Prefabs")]
        [SerializeField] private GameObject gameManagerPrefab;
        [SerializeField] private GameObject waveManagerPrefab;
        
        [Header("Container Settings")]
        [SerializeField] private Vector3 monsterContainerPosition = Vector3.zero;
        [SerializeField] private Vector3 enemyContainerPosition = Vector3.zero;
        [SerializeField] private Vector3 spawnPointPosition = new Vector3(0, 0, 20);
        [SerializeField] private Vector3 objectivePointPosition = new Vector3(0, 0, -20);
        
        private void OnValidate()
        {
            // Validate manager prefabs
            if (gameManagerPrefab == null)
            {
                Debug.LogWarning("SceneSetupPrefab: GameManager prefab is not assigned!");
            }
            else if (gameManagerPrefab.GetComponent<GameManager>() == null)
            {
                Debug.LogError("SceneSetupPrefab: GameManager prefab does not have GameManager component!");
            }
            
            if (waveManagerPrefab == null)
            {
                Debug.LogWarning("SceneSetupPrefab: WaveManager prefab is not assigned!");
            }
            else if (waveManagerPrefab.GetComponent<WaveManager>() == null)
            {
                Debug.LogError("SceneSetupPrefab: WaveManager prefab does not have WaveManager component!");
            }
            
            // Validate SceneSetup component
            SceneSetup sceneSetup = GetComponent<SceneSetup>();
            if (sceneSetup == null)
            {
                Debug.LogError("SceneSetupPrefab: SceneSetup component is missing!");
            }
        }
        
        private void Reset()
        {
            // Set default positions
            monsterContainerPosition = Vector3.zero;
            enemyContainerPosition = Vector3.zero;
            spawnPointPosition = new Vector3(0, 0, 20);
            objectivePointPosition = new Vector3(0, 0, -20);
            
            // Ensure SceneSetup component exists
            if (GetComponent<SceneSetup>() == null)
            {
                gameObject.AddComponent<SceneSetup>();
            }
        }
    }
} 