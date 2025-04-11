using UnityEngine;
using YuGiOhTowerDefense.Base;
using YuGiOhTowerDefense.Cards;

namespace YuGiOhTowerDefense.Cards
{
    public class SpellCard : YuGiOhCard
    {
        [SerializeField] protected SpellType spellType;
        [SerializeField] protected SpellIcon spellIcon;
        [SerializeField] protected float effectRange;
        [SerializeField] protected float effectValue;
        [SerializeField] protected float effectDuration;

        protected bool isActivated;
        protected float remainingDuration;

        public SpellType SpellType => spellType;
        public SpellIcon SpellIcon => spellIcon;
        public float EffectRange => effectRange;
        public float EffectValue => effectValue;
        public float EffectDuration => effectDuration;
        public bool IsActivated => isActivated;
        public float RemainingDuration => remainingDuration;

        public override void Initialize(string name, string desc, CardType type, Sprite image, int id)
        {
            base.Initialize(name, desc, type, image, id);
            isActivated = false;
            remainingDuration = effectDuration;
        }

        public void SetSpellStats(SpellType type, SpellIcon icon, float range, float value, float duration)
        {
            spellType = type;
            spellIcon = icon;
            effectRange = range;
            effectValue = value;
            effectDuration = duration;
        }

        public virtual void Activate()
        {
            if (!isActivated)
            {
                isActivated = true;
                remainingDuration = effectDuration;
                // Spell activation logic will be implemented in the game manager
            }
        }

        public virtual void UpdateEffect(float deltaTime)
        {
            if (isActivated && spellType == SpellType.Continuous)
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
            // Spell deactivation logic will be implemented in the game manager
        }

        public bool CanBeSet()
        {
            return spellType != SpellType.QuickPlay;
        }

        public bool CanBeActivated()
        {
            return !isActivated;
        }
    }
} 