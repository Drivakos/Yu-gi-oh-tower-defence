using UnityEngine;

namespace YuGiOhTowerDefense.Effects
{
    public class SpeedBoostEffect : StatusEffect
    {
        private readonly float speedMultiplier;
        private float originalMoveSpeed;

        public SpeedBoostEffect(float duration, float speedMultiplier)
            : base(StatusEffectType.SpeedBoost, duration, speedMultiplier)
        {
            this.speedMultiplier = speedMultiplier;
        }

        public override void ApplyEffect(MonsterCard target)
        {
            if (target != null)
            {
                // Store original speed
                originalMoveSpeed = target.MoveSpeed;

                // Apply speed boost
                target.MoveSpeed *= speedMultiplier;
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