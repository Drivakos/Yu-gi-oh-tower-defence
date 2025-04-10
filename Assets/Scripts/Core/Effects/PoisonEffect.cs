using UnityEngine;

namespace YuGiOhTowerDefense.Effects
{
    public class PoisonEffect : StatusEffect
    {
        private readonly float damagePerSecond;
        private float lastDamageTime;

        public PoisonEffect(float duration, float damagePerSecond)
            : base(StatusEffectType.Poison, duration, damagePerSecond)
        {
            this.damagePerSecond = damagePerSecond;
            lastDamageTime = Time.time;
        }

        public override void Update(MonsterCard target)
        {
            if (!IsActive) return;

            if (Time.time - lastDamageTime >= 1f)
            {
                ApplyEffect(target);
                lastDamageTime = Time.time;
            }

            base.Update(target);
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