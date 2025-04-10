using UnityEngine;

namespace YuGiOhTowerDefense.Effects
{
    public class SlowEffect : StatusEffect
    {
        private readonly float speedReduction;
        private float originalMoveSpeed;

        public SlowEffect(float duration, float speedReduction)
            : base(StatusEffectType.Slow, duration, speedReduction)
        {
            this.speedReduction = speedReduction;
        }

        public override void ApplyEffect(MonsterCard target)
        {
            if (target != null)
            {
                // Store original speed
                originalMoveSpeed = target.MoveSpeed;

                // Apply speed reduction
                target.MoveSpeed *= (1f - speedReduction);
            }
        }

        public override void RemoveEffect(MonsterCard target)
        {
            if (target != null)
            {
                // Restore original speed
                target.MoveSpeed = originalMoveSpeed;
            }
        }
    }
} 