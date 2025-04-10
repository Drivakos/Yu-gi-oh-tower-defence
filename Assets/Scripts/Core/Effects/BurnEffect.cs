using UnityEngine;

namespace YuGiOhTowerDefense.Effects
{
    public class BurnEffect : StatusEffect
    {
        private readonly float damagePerSecond;

        public BurnEffect(float duration, float damagePerSecond)
            : base(StatusEffectType.Burn, duration, damagePerSecond)
        {
            this.damagePerSecond = damagePerSecond;
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            if (IsActive)
            {
                // Apply damage every second
                if (Duration % 1f < deltaTime)
                {
                    ApplyEffect(null); // We'll handle the damage in the MonsterCard class
                }
            }
        }

        public override void ApplyEffect(MonsterCard target)
        {
            if (target != null)
            {
                target.TakeDamage(damagePerSecond);
            }
        }
    }
} 