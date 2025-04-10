using UnityEngine;

namespace YuGiOhTowerDefense.Effects
{
    public class HealEffect : StatusEffect
    {
        private readonly float healPerSecond;
        private float lastHealTime;

        public HealEffect(float duration, float healPerSecond)
            : base(StatusEffectType.Heal, duration, healPerSecond)
        {
            this.healPerSecond = healPerSecond;
            lastHealTime = Time.time;
        }

        public override void Update(MonsterCard target)
        {
            if (!IsActive) return;

            if (Time.time - lastHealTime >= 1f)
            {
                ApplyEffect(target);
                lastHealTime = Time.time;
            }

            base.Update(target);
        }

        public override void ApplyEffect(MonsterCard target)
        {
            if (target != null)
            {
                target.Heal(healPerSecond);
            }
        }
    }
} 