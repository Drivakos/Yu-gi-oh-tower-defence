using UnityEngine;

namespace YuGiOhTowerDefense.Effects
{
    public class FreezeEffect : StatusEffect
    {
        private readonly float speedReduction;

        public FreezeEffect(float duration, float speedReduction)
            : base(StatusEffectType.Freeze, duration, speedReduction)
        {
            this.speedReduction = speedReduction;
        }

        public override void ApplyEffect(MonsterCard target)
        {
            if (target != null)
            {
                target.MovementSpeed *= (1f - speedReduction);
            }
        }

        public override void RemoveEffect(MonsterCard target)
        {
            if (target != null)
            {
                target.MovementSpeed /= (1f - speedReduction);
            }
        }
    }
} 