using UnityEngine;

namespace YuGiOhTowerDefense.Effects
{
    public class StunEffect : StatusEffect
    {
        private float originalMoveSpeed;
        private float originalAttackSpeed;

        public StunEffect(float duration)
            : base(StatusEffectType.Stun, duration, 0f)
        {
        }

        public override void ApplyEffect(MonsterCard target)
        {
            if (target != null)
            {
                // Store original values
                originalMoveSpeed = target.MoveSpeed;
                originalAttackSpeed = target.GetAttackSpeed();

                // Set speeds to 0 to prevent movement and attacks
                target.MoveSpeed = 0f;
                target.SetAttackSpeed(0f);
            }
        }

        public override void RemoveEffect(MonsterCard target)
        {
            if (target != null)
            {
                // Restore original values
                target.MoveSpeed = originalMoveSpeed;
                target.SetAttackSpeed(originalAttackSpeed);
            }
        }
    }
} 