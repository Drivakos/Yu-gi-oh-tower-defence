using UnityEngine;
using YuGiOhTowerDefense.Base;

namespace YuGiOhTowerDefense.Cards
{
    public abstract class CardEffect : ScriptableObject
    {
        [SerializeField] protected string effectName;
        [SerializeField] protected string description;
        [SerializeField] protected float cooldown;
        [SerializeField] protected float duration;
        
        protected float lastActivationTime;
        protected bool isActive;
        
        public string EffectName => effectName;
        public string Description => description;
        public float Cooldown => cooldown;
        public float Duration => duration;
        public bool IsActive => isActive;
        
        public virtual bool CanActivate(YuGiOhCard card)
        {
            return Time.time >= lastActivationTime + cooldown;
        }
        
        public virtual void OnCardActivated(YuGiOhCard card)
        {
            if (CanActivate(card))
            {
                ApplyEffect(card);
                lastActivationTime = Time.time;
                isActive = true;
            }
        }
        
        public virtual void OnCardDeactivated(YuGiOhCard card)
        {
            RemoveEffect(card);
            isActive = false;
        }
        
        public virtual void OnCardFlipped(YuGiOhCard card)
        {
            if (card.IsFaceUp)
            {
                OnCardActivated(card);
            }
            else
            {
                OnCardDeactivated(card);
            }
        }
        
        protected abstract void ApplyEffect(YuGiOhCard card);
        protected abstract void RemoveEffect(YuGiOhCard card);
    }
} 