using UnityEngine;
using YuGiOhTowerDefense.Base;
using YuGiOhTowerDefense.Cards;

namespace YuGiOhTowerDefense.Cards
{
    public class TrapCard : YuGiOhCard
    {
        [SerializeField] protected TrapType trapType;
        [SerializeField] protected float effectRange;
        [SerializeField] protected float effectValue;
        [SerializeField] protected float effectDuration;

        protected bool isSet;
        protected bool isActivated;
        protected float remainingDuration;

        public TrapType TrapType => trapType;
        public float EffectRange => effectRange;
        public float EffectValue => effectValue;
        public float EffectDuration => effectDuration;
        public bool IsSet => isSet;
        public bool IsActivated => isActivated;
        public float RemainingDuration => remainingDuration;

        public override void Initialize(string name, string desc, CardType type, Sprite image, int id)
        {
            base.Initialize(name, desc, type, image, id);
            isSet = false;
            isActivated = false;
            remainingDuration = effectDuration;
        }

        public void SetTrapStats(TrapType type, float range, float value, float duration)
        {
            trapType = type;
            effectRange = range;
            effectValue = value;
            effectDuration = duration;
        }

        public void Set()
        {
            if (!isSet && !isActivated)
            {
                isSet = true;
                // Trap setting logic will be implemented in the game manager
            }
        }

        public virtual void Activate()
        {
            if (isSet && !isActivated)
            {
                isSet = false;
                isActivated = true;
                remainingDuration = effectDuration;
                // Trap activation logic will be implemented in the game manager
            }
        }

        public virtual void UpdateEffect(float deltaTime)
        {
            if (isActivated && trapType == TrapType.Continuous)
            {
                remainingDuration -= deltaTime;
                if (remainingDuration <= 0)
                {
                    Deactivate();
                }
            }
        }

        public virtual void Deactivate()
        {
            isActivated = false;
            remainingDuration = 0;
            // Trap deactivation logic will be implemented in the game manager
        }

        public bool CanBeSet()
        {
            return !isSet && !isActivated;
        }

        public bool CanBeActivated()
        {
            return isSet && !isActivated;
        }
    }
} 