using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace YuGiOh.MillenniumItems
{
    public class MillenniumItemManager : MonoBehaviour
    {
        [Header("Item Prefabs")]
        [SerializeField] private GameObject ringPrefab;
        [SerializeField] private GameObject puzzlePrefab;
        [SerializeField] private GameObject eyePrefab;
        [SerializeField] private GameObject rodPrefab;
        [SerializeField] private GameObject keyPrefab;
        [SerializeField] private GameObject scalePrefab;
        [SerializeField] private GameObject necklacePrefab;
        
        [Header("Item Data")]
        [SerializeField] private TextAsset itemDataFile;
        
        [Header("Spawn Settings")]
        [SerializeField] private Transform itemContainer;
        [SerializeField] private float spawnRadius = 5f;
        [SerializeField] private float spawnHeight = 1f;
        
        private Dictionary<MillenniumItemType, MillenniumItem> activeItems = new Dictionary<MillenniumItemType, MillenniumItem>();
        private Dictionary<MillenniumItemType, ItemData> itemData = new Dictionary<MillenniumItemType, ItemData>();
        
        private void Awake()
        {
            LoadItemData();
        }
        
        private void LoadItemData()
        {
            if (itemDataFile != null)
            {
                // Parse JSON data if available
                // For now, we'll use hardcoded data
                InitializeDefaultItemData();
            }
            else
            {
                InitializeDefaultItemData();
            }
        }
        
        private void InitializeDefaultItemData()
        {
            // Ring
            itemData[MillenniumItemType.Ring] = new ItemData
            {
                Name = "Millennium Ring",
                Description = "An ancient Egyptian artifact that grants its wielder the ability to sense the presence of other Millennium Items and protect against dark magic.",
                PowerLevel = 0.7f,
                UnlockRequirement = "Complete the tutorial"
            };
            
            // Puzzle
            itemData[MillenniumItemType.Puzzle] = new ItemData
            {
                Name = "Millennium Puzzle",
                Description = "The most powerful of the Millennium Items, containing the soul of an ancient Egyptian pharaoh. Grants the ability to summon powerful monsters.",
                PowerLevel = 1.0f,
                UnlockRequirement = "Collect all other Millennium Items"
            };
            
            // Eye
            itemData[MillenniumItemType.Eye] = new ItemData
            {
                Name = "Millennium Eye",
                Description = "Allows the wielder to see into the future and read the minds of others. Grants a significant advantage in duels.",
                PowerLevel = 0.9f,
                UnlockRequirement = "Win 10 duels"
            };
            
            // Rod
            itemData[MillenniumItemType.Rod] = new ItemData
            {
                Name = "Millennium Rod",
                Description = "Grants the ability to control the minds of others and manipulate their actions. A powerful tool for those with strong willpower.",
                PowerLevel = 0.8f,
                UnlockRequirement = "Defeat 5 opponents"
            };
            
            // Key
            itemData[MillenniumItemType.Key] = new ItemData
            {
                Name = "Millennium Key",
                Description = "Opens the door to the wielder's heart and allows them to enter the Shadow Realm. Provides protection against dark forces.",
                PowerLevel = 0.6f,
                UnlockRequirement = "Complete the first story arc"
            };
            
            // Scale
            itemData[MillenniumItemType.Scale] = new ItemData
            {
                Name = "Millennium Scale",
                Description = "Measures the balance between light and darkness in a person's heart. Grants the ability to purify corrupted souls.",
                PowerLevel = 0.5f,
                UnlockRequirement = "Collect 50 cards"
            };
            
            // Necklace
            itemData[MillenniumItemType.Necklace] = new ItemData
            {
                Name = "Millennium Necklace",
                Description = "Allows the wielder to see into the future and make decisions based on what they see. A powerful tool for strategic planning.",
                PowerLevel = 0.75f,
                UnlockRequirement = "Win 20 duels"
            };
        }
        
        public void SpawnItem(MillenniumItemType itemType, Vector3 position)
        {
            if (activeItems.ContainsKey(itemType))
            {
                Debug.LogWarning($"Item of type {itemType} already exists in the scene.");
                return;
            }
            
            GameObject prefab = GetPrefabForItemType(itemType);
            if (prefab == null)
            {
                Debug.LogError($"No prefab found for item type {itemType}");
                return;
            }
            
            Vector3 spawnPosition = position;
            if (spawnPosition == Vector3.zero)
            {
                // Generate a random position within the spawn radius
                float randomAngle = Random.Range(0f, 360f);
                float randomDistance = Random.Range(0f, spawnRadius);
                spawnPosition = itemContainer.position + new Vector3(
                    Mathf.Cos(randomAngle * Mathf.Deg2Rad) * randomDistance,
                    spawnHeight,
                    Mathf.Sin(randomAngle * Mathf.Deg2Rad) * randomDistance
                );
            }
            
            GameObject itemObject = Instantiate(prefab, spawnPosition, Quaternion.identity, itemContainer);
            MillenniumItem item = itemObject.GetComponent<MillenniumItem>();
            
            if (item != null)
            {
                activeItems[itemType] = item;
                Debug.Log($"Spawned {itemType} at {spawnPosition}");
            }
            else
            {
                Debug.LogError($"Prefab for {itemType} does not have a MillenniumItem component");
                Destroy(itemObject);
            }
        }
        
        public void SpawnAllItems()
        {
            foreach (MillenniumItemType itemType in System.Enum.GetValues(typeof(MillenniumItemType)))
            {
                SpawnItem(itemType, Vector3.zero);
            }
        }
        
        public void ActivateItem(MillenniumItemType itemType)
        {
            if (activeItems.TryGetValue(itemType, out MillenniumItem item))
            {
                item.Activate();
            }
            else
            {
                Debug.LogWarning($"Cannot activate {itemType} as it is not spawned in the scene.");
            }
        }
        
        public void DeactivateItem(MillenniumItemType itemType)
        {
            if (activeItems.TryGetValue(itemType, out MillenniumItem item))
            {
                item.Deactivate();
            }
            else
            {
                Debug.LogWarning($"Cannot deactivate {itemType} as it is not spawned in the scene.");
            }
        }
        
        public void ActivateAllItems()
        {
            foreach (var item in activeItems.Values)
            {
                item.Activate();
            }
        }
        
        public void DeactivateAllItems()
        {
            foreach (var item in activeItems.Values)
            {
                item.Deactivate();
            }
        }
        
        public bool IsItemActive(MillenniumItemType itemType)
        {
            if (activeItems.TryGetValue(itemType, out MillenniumItem item))
            {
                return item.IsActive;
            }
            return false;
        }
        
        public ItemData GetItemData(MillenniumItemType itemType)
        {
            if (itemData.TryGetValue(itemType, out ItemData data))
            {
                return data;
            }
            return null;
        }
        
        public List<MillenniumItemType> GetUnlockedItems()
        {
            // In a real game, this would check against player progress
            // For now, we'll return all items
            return activeItems.Keys.ToList();
        }
        
        private GameObject GetPrefabForItemType(MillenniumItemType itemType)
        {
            return itemType switch
            {
                MillenniumItemType.Ring => ringPrefab,
                MillenniumItemType.Puzzle => puzzlePrefab,
                MillenniumItemType.Eye => eyePrefab,
                MillenniumItemType.Rod => rodPrefab,
                MillenniumItemType.Key => keyPrefab,
                MillenniumItemType.Scale => scalePrefab,
                MillenniumItemType.Necklace => necklacePrefab,
                _ => null
            };
        }
    }
    
    [System.Serializable]
    public class ItemData
    {
        public string Name;
        public string Description;
        public float PowerLevel;
        public string UnlockRequirement;
    }
} 