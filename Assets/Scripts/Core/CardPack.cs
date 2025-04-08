using System.Collections.Generic;
using UnityEngine;
using System;

namespace YuGiOhTowerDefense.Cards
{
    [Serializable]
    public class CardPack
    {
        public string id;
        public string name;
        public string description;
        public int cost;
        public int cardCount;
        public Sprite packImage;
        
        [Header("Card Type Weights")]
        public float normalMonsterWeight = 0.4f;
        public float effectMonsterWeight = 0.3f;
        public float ritualMonsterWeight = 0.1f;
        public float fusionMonsterWeight = 0.1f;
        public float synchroMonsterWeight = 0.05f;
        public float xyzMonsterWeight = 0.03f;
        public float linkMonsterWeight = 0.02f;
        public float spellCardWeight = 0.3f;
        public float trapCardWeight = 0.2f;
        
        [Header("Rarity Weights")]
        public float commonWeight = 0.6f;
        public float rareWeight = 0.25f;
        public float superRareWeight = 0.1f;
        public float ultraRareWeight = 0.04f;
        public float secretRareWeight = 0.01f;
    }
} 