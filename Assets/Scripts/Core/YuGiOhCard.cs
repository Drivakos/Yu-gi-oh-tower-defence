using UnityEngine;
using System;
using System.Collections.Generic;

namespace YuGiOhTowerDefense.Core
{
    [Serializable]
    public class YuGiOhCard
    {
        // Basic card information
        public string id;
        public string name;
        public string type;
        public string desc;
        public string race;
        public string archetype;
        
        // Card stats for tower defense gameplay
        public int attack;
        public int defense;
        public int level;
        public int cost;
        
        // Card rarity and set information
        public string rarity;
        public List<CardSet> cardSets;
        
        // Card images
        public string imageUrl;
        public string imageUrlSmall;
        
        // Card prices (for shop system)
        public float marketPrice;
        public float tcgPlayerPrice;
        
        // Gameplay properties
        public bool isPlayable;
        public float cooldown;
        public float range;
        public float attackSpeed;
        public List<string> effects;
        
        [Serializable]
        public class CardSet
        {
            public string setName;
            public string setCode;
            public string setRarity;
            public string setPrice;
        }
        
        public YuGiOhCard()
        {
            cardSets = new List<CardSet>();
            effects = new List<string>();
        }
        
        public void SetDefaultStats()
        {
            // Set default gameplay stats based on card type
            switch (type.ToLower())
            {
                case "normal monster":
                    attack = 1500;
                    defense = 1000;
                    level = 4;
                    cost = 2;
                    range = 3f;
                    attackSpeed = 1f;
                    break;
                    
                case "effect monster":
                    attack = 1800;
                    defense = 1200;
                    level = 4;
                    cost = 3;
                    range = 3.5f;
                    attackSpeed = 1.2f;
                    break;
                    
                case "ritual monster":
                    attack = 2500;
                    defense = 2000;
                    level = 6;
                    cost = 5;
                    range = 4f;
                    attackSpeed = 0.8f;
                    break;
                    
                case "fusion monster":
                    attack = 2800;
                    defense = 2200;
                    level = 7;
                    cost = 6;
                    range = 4.5f;
                    attackSpeed = 0.7f;
                    break;
                    
                case "synchro monster":
                    attack = 3000;
                    defense = 2500;
                    level = 8;
                    cost = 7;
                    range = 5f;
                    attackSpeed = 0.6f;
                    break;
                    
                case "xyz monster":
                    attack = 3200;
                    defense = 2700;
                    level = 9;
                    cost = 8;
                    range = 5.5f;
                    attackSpeed = 0.5f;
                    break;
                    
                case "link monster":
                    attack = 3500;
                    defense = 3000;
                    level = 10;
                    cost = 9;
                    range = 6f;
                    attackSpeed = 0.4f;
                    break;
                    
                case "spell card":
                    attack = 0;
                    defense = 0;
                    level = 0;
                    cost = 1;
                    range = 0f;
                    attackSpeed = 0f;
                    break;
                    
                case "trap card":
                    attack = 0;
                    defense = 0;
                    level = 0;
                    cost = 1;
                    range = 0f;
                    attackSpeed = 0f;
                    break;
            }
            
            // Set default cooldown based on card type and level
            cooldown = 5f + (level * 0.5f);
            
            // Set default rarity if not set
            if (string.IsNullOrEmpty(rarity))
            {
                rarity = "Common";
            }
        }
        
        public float GetPower()
        {
            // Calculate card power based on stats and rarity
            float basePower = (attack + defense) * 0.5f;
            float rarityMultiplier = GetRarityMultiplier();
            float levelMultiplier = 1f + (level * 0.1f);
            
            return basePower * rarityMultiplier * levelMultiplier;
        }
        
        private float GetRarityMultiplier()
        {
            switch (rarity.ToLower())
            {
                case "common":
                    return 1.0f;
                case "rare":
                    return 1.2f;
                case "super rare":
                    return 1.5f;
                case "ultra rare":
                    return 1.8f;
                case "secret rare":
                    return 2.0f;
                default:
                    return 1.0f;
            }
        }
        
        public bool IsMonster()
        {
            return type.ToLower().Contains("monster");
        }
        
        public bool IsSpell()
        {
            return type.ToLower().Contains("spell");
        }
        
        public bool IsTrap()
        {
            return type.ToLower().Contains("trap");
        }
    }
} 