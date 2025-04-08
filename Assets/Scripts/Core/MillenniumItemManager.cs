using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace YuGiOhTowerDefense.Core
{
    public class MillenniumItemManager : MonoBehaviour
    {
        [Header("Millennium Items")]
        [SerializeField] private List<MillenniumItem> allMillenniumItems = new List<MillenniumItem>();
        [SerializeField] private Transform itemContainer;

        [Header("Item Effects")]
        [SerializeField] private GameObject itemActivationEffect;
        [SerializeField] private float effectScale = 1.5f;
        [SerializeField] private float effectDuration = 2f;

        private Dictionary<MillenniumItemType, MillenniumItem> collectedItems = new Dictionary<MillenniumItemType, MillenniumItem>();
        private List<MillenniumItem> activeItems = new List<MillenniumItem>();

        private void Awake()
        {
            InitializeItems();
        }

        private void InitializeItems()
        {
            // Load all Millennium Items from Resources
            var items = Resources.LoadAll<MillenniumItem>("MillenniumItems");
            allMillenniumItems = items.ToList();

            // Initialize collected items dictionary
            foreach (var item in allMillenniumItems)
            {
                if (item.IsCollected)
                {
                    collectedItems[item.ItemType] = item;
                }
            }
        }

        public void CollectItem(MillenniumItemType itemType)
        {
            var item = allMillenniumItems.FirstOrDefault(i => i.ItemType == itemType);
            if (item != null && !item.IsCollected)
            {
                item.Collect();
                collectedItems[itemType] = item;
                Debug.Log($"Collected {item.ItemName}!");
                
                // Notify GameStory about the collection
                FindObjectOfType<GameStory>()?.OnMillenniumItemCollected(item);
            }
        }

        public bool ActivateItem(MillenniumItemType itemType)
        {
            if (!collectedItems.ContainsKey(itemType))
            {
                Debug.LogWarning($"Item {itemType} not collected!");
                return false;
            }

            var item = collectedItems[itemType];
            if (!item.CanActivate())
            {
                Debug.LogWarning($"Item {item.ItemName} cannot be activated yet!");
                return false;
            }

            item.Activate();
            activeItems.Add(item);
            
            // Spawn activation effect
            if (itemActivationEffect != null)
            {
                var effect = Instantiate(itemActivationEffect, itemContainer);
                effect.transform.localScale = Vector3.one * effectScale;
                Destroy(effect, effectDuration);
            }

            return true;
        }

        public void DeactivateItem(MillenniumItemType itemType)
        {
            if (!collectedItems.ContainsKey(itemType))
            {
                return;
            }

            var item = collectedItems[itemType];
            item.Deactivate();
            activeItems.Remove(item);
        }

        public void DeactivateAllItems()
        {
            foreach (var item in activeItems.ToList())
            {
                item.Deactivate();
            }
            activeItems.Clear();
        }

        public bool HasItem(MillenniumItemType itemType)
        {
            return collectedItems.ContainsKey(itemType);
        }

        public bool IsItemActive(MillenniumItemType itemType)
        {
            if (!collectedItems.ContainsKey(itemType))
            {
                return false;
            }

            return collectedItems[itemType].IsActive;
        }

        public float GetDamageMultiplier()
        {
            float multiplier = 1f;
            foreach (var item in activeItems)
            {
                multiplier *= item.GetDamageMultiplier();
            }
            return multiplier;
        }

        public float GetDefenseMultiplier()
        {
            float multiplier = 1f;
            foreach (var item in activeItems)
            {
                multiplier *= item.GetDefenseMultiplier();
            }
            return multiplier;
        }

        public float GetRecruitmentBonus()
        {
            float bonus = 0f;
            foreach (var item in activeItems)
            {
                bonus += item.GetRecruitmentBonus();
            }
            return bonus;
        }

        public int GetExtraCardDraw()
        {
            int extraDraw = 0;
            foreach (var item in activeItems)
            {
                extraDraw += item.GetExtraCardDraw();
            }
            return extraDraw;
        }

        public bool CanSeeInvisible()
        {
            return activeItems.Any(item => item.CanSeeInvisible());
        }

        public bool CanControlMonsters()
        {
            return activeItems.Any(item => item.CanControlMonsters());
        }

        public bool CanManipulateTime()
        {
            return activeItems.Any(item => item.CanManipulateTime());
        }

        public bool CanReadMinds()
        {
            return activeItems.Any(item => item.CanReadMinds());
        }

        public bool CanProtectFromShadow()
        {
            return activeItems.Any(item => item.CanProtectFromShadow());
        }

        public bool CanBanishToShadow()
        {
            return activeItems.Any(item => item.CanBanishToShadow());
        }

        public bool CanResurrectMonsters()
        {
            return activeItems.Any(item => item.CanResurrectMonsters());
        }

        public List<MillenniumItem> GetCollectedItems()
        {
            return collectedItems.Values.ToList();
        }

        public List<MillenniumItem> GetActiveItems()
        {
            return activeItems;
        }
    }
} 