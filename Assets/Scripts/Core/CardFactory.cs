using UnityEngine;
using System.Collections.Generic;
using YuGiOhTowerDefense.Base;
using YuGiOhTowerDefense.Utils;
using YuGiOhTowerDefense.Cards;

namespace YuGiOhTowerDefense.Factories
{
    public class CardFactory : MonoBehaviour
    {
        [Header("Card Prefabs")]
        [SerializeField] private GameObject monsterCardPrefab;
        [SerializeField] private GameObject spellCardPrefab;
        [SerializeField] private GameObject trapCardPrefab;
        
        [Header("Card Settings")]
        [SerializeField] private float defaultCooldown = 1f;
        [SerializeField] private float defaultRange = 5f;
        [SerializeField] private float defaultAttackSpeed = 1f;
        
        private Dictionary<string, GameObject> cardPrefabs = new Dictionary<string, GameObject>();
        private ObjectPool monsterCardPool;
        private ObjectPool spellCardPool;
        private ObjectPool trapCardPool;
        private int poolSize = 20;
        
        private void Awake()
        {
            InitializeCardPrefabs();
            InitializeObjectPools();
        }
        
        private void InitializeCardPrefabs()
        {
            cardPrefabs["Monster"] = monsterCardPrefab;
            cardPrefabs["Spell"] = spellCardPrefab;
            cardPrefabs["Trap"] = trapCardPrefab;
        }
        
        private void InitializeObjectPools()
        {
            if (monsterCardPrefab != null)
            {
                monsterCardPool = gameObject.AddComponent<ObjectPool>();
                monsterCardPool.Initialize(monsterCardPrefab, poolSize);
            }
            
            if (spellCardPrefab != null)
            {
                spellCardPool = gameObject.AddComponent<ObjectPool>();
                spellCardPool.Initialize(spellCardPrefab, poolSize);
            }
            
            if (trapCardPrefab != null)
            {
                trapCardPool = gameObject.AddComponent<ObjectPool>();
                trapCardPool.Initialize(trapCardPrefab, poolSize);
            }
        }
        
        public GameObject CreateCard(YuGiOhCard cardData, Vector3 position)
        {
            if (cardData == null)
            {
                Debug.LogError("CardFactory: Cannot create card with null data");
                return null;
            }
            
            GameObject cardObject = null;
            
            if (cardData.IsMonster())
            {
                cardObject = CreateMonsterCard(cardData, position);
            }
            else if (cardData.IsSpell())
            {
                cardObject = CreateSpellCard(cardData, position);
            }
            else if (cardData.IsTrap())
            {
                cardObject = CreateTrapCard(cardData, position);
            }
            else
            {
                Debug.LogError($"CardFactory: Unknown card type for {cardData.name}");
                return null;
            }
            
            if (cardObject != null)
            {
                // Set default values if not already set
                MonsterCard monsterCard = cardObject.GetComponent<MonsterCard>();
                if (monsterCard != null)
                {
                    if (monsterCard.GetCooldown() <= 0)
                    {
                        monsterCard.SetCooldown(defaultCooldown);
                    }
                    
                    if (monsterCard.GetRange() <= 0)
                    {
                        monsterCard.SetRange(defaultRange);
                    }
                    
                    if (monsterCard.GetAttackSpeed() <= 0)
                    {
                        monsterCard.SetAttackSpeed(defaultAttackSpeed);
                    }
                }
            }
            
            return cardObject;
        }
        
        private GameObject CreateMonsterCard(YuGiOhCard cardData, Vector3 position)
        {
            GameObject cardObject;
            
            if (monsterCardPool != null)
            {
                cardObject = monsterCardPool.GetObject();
                cardObject.transform.position = position;
                cardObject.transform.rotation = Quaternion.identity;
            }
            else if (monsterCardPrefab != null)
            {
                cardObject = Instantiate(monsterCardPrefab, position, Quaternion.identity);
            }
            else
            {
                Debug.LogError("CardFactory: Monster card prefab is missing");
                return null;
            }
            
            MonsterCard monsterCard = cardObject.GetComponent<MonsterCard>();
            if (monsterCard != null)
            {
                monsterCard.Initialize(cardData);
            }
            else
            {
                Debug.LogError("CardFactory: MonsterCard component not found on prefab");
                return null;
            }
            
            return cardObject;
        }
        
        private GameObject CreateSpellCard(YuGiOhCard cardData, Vector3 position)
        {
            GameObject cardObject;
            
            if (spellCardPool != null)
            {
                cardObject = spellCardPool.GetObject();
                cardObject.transform.position = position;
                cardObject.transform.rotation = Quaternion.identity;
            }
            else if (spellCardPrefab != null)
            {
                cardObject = Instantiate(spellCardPrefab, position, Quaternion.identity);
            }
            else
            {
                Debug.LogError("CardFactory: Spell card prefab is missing");
                return null;
            }
            
            // Initialize spell card component if it exists
            SpellCard spellCard = cardObject.GetComponent<SpellCard>();
            if (spellCard != null)
            {
                spellCard.Initialize(cardData);
            }
            
            return cardObject;
        }
        
        private GameObject CreateTrapCard(YuGiOhCard cardData, Vector3 position)
        {
            GameObject cardObject;
            
            if (trapCardPool != null)
            {
                cardObject = trapCardPool.GetObject();
                cardObject.transform.position = position;
                cardObject.transform.rotation = Quaternion.identity;
            }
            else if (trapCardPrefab != null)
            {
                cardObject = Instantiate(trapCardPrefab, position, Quaternion.identity);
            }
            else
            {
                Debug.LogError("CardFactory: Trap card prefab is missing");
                return null;
            }
            
            // Initialize trap card component if it exists
            TrapCard trapCard = cardObject.GetComponent<TrapCard>();
            if (trapCard != null)
            {
                trapCard.Initialize(cardData);
            }
            
            return cardObject;
        }
        
        public void ReturnCardToPool(GameObject cardObject)
        {
            if (cardObject == null)
            {
                return;
            }
            
            MonsterCard monsterCard = cardObject.GetComponent<MonsterCard>();
            if (monsterCard != null && monsterCardPool != null)
            {
                monsterCardPool.ReturnObject(cardObject);
                return;
            }
            
            SpellCard spellCard = cardObject.GetComponent<SpellCard>();
            if (spellCard != null && spellCardPool != null)
            {
                spellCardPool.ReturnObject(cardObject);
                return;
            }
            
            TrapCard trapCard = cardObject.GetComponent<TrapCard>();
            if (trapCard != null && trapCardPool != null)
            {
                trapCardPool.ReturnObject(cardObject);
                return;
            }
            
            // If no pool found, destroy the object
            Destroy(cardObject);
        }
    }
} 