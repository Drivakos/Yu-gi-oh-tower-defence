using UnityEngine;
using System;
using System.Collections.Generic;
using YuGiOhTowerDefense.Cards;

namespace YuGiOhTowerDefense.Base
{
    [Serializable]
    public class YuGiOhCard : MonoBehaviour
    {
        // Basic card information
        [SerializeField] protected string cardName;
        [SerializeField] protected string description;
        [SerializeField] protected Sprite cardImage;
        [SerializeField] protected CardRarity rarity;
        [SerializeField] protected CardType cardType;
        
        // Card stats
        [SerializeField] protected int level;
        [SerializeField] protected int attack;
        [SerializeField] protected int defense;
        
        // Card effects
        [SerializeField] protected List<CardEffect> effects = new List<CardEffect>();
        
        // Card state
        protected bool isActive = false;
        protected bool isFaceUp = true;
        
        public string CardName => cardName;
        public string Description => description;
        public Sprite CardImage => cardImage;
        public CardRarity Rarity => rarity;
        public CardType CardType => cardType;
        public int Level => level;
        public int Attack => attack;
        public int Defense => defense;
        public bool IsActive => isActive;
        public bool IsFaceUp => isFaceUp;
        
        public virtual void OnCardActivated()
        {
            isActive = true;
            foreach (var effect in effects)
            {
                effect.OnCardActivated(this);
            }
        }
        
        public virtual void OnCardDeactivated()
        {
            isActive = false;
            foreach (var effect in effects)
            {
                effect.OnCardDeactivated(this);
            }
        }
        
        public virtual void FlipCard()
        {
            isFaceUp = !isFaceUp;
            foreach (var effect in effects)
            {
                effect.OnCardFlipped(this);
            }
        }
        
        public virtual void AddEffect(CardEffect effect)
        {
            effects.Add(effect);
        }
        
        public virtual void RemoveEffect(CardEffect effect)
        {
            effects.Remove(effect);
        }
    }
} 