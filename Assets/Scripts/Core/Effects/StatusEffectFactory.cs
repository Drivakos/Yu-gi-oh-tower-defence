using UnityEngine;
using YuGiOhTowerDefense.Effects;

namespace YuGiOhTowerDefense.Factories
{
    public class StatusEffectFactory
    {
        public static StatusEffect CreateEffect(StatusEffectType type, float duration, float value)
        {
            return type switch
            {
                StatusEffectType.Poison => new PoisonEffect(duration, value),
                StatusEffectType.Stun => new StunEffect(duration),
                StatusEffectType.Buff => new BuffEffect(duration, value),
                StatusEffectType.Debuff => new DebuffEffect(duration, value),
                StatusEffectType.Burn => new BurnEffect(duration, value),
                StatusEffectType.Freeze => new FreezeEffect(duration),
                StatusEffectType.Heal => new HealEffect(duration, value),
                StatusEffectType.Shield => new ShieldEffect(duration, value),
                StatusEffectType.SpeedBoost => new SpeedBoostEffect(duration, value),
                StatusEffectType.Slow => new SlowEffect(duration, value),
                _ => null
            };
        }

        public static StatusEffect CreateRandomEffect(float minDuration, float maxDuration, float minValue, float maxValue)
        {
            var types = System.Enum.GetValues(typeof(StatusEffectType));
            var randomType = (StatusEffectType)types.GetValue(Random.Range(0, types.Length));
            var duration = Random.Range(minDuration, maxDuration);
            var value = Random.Range(minValue, maxValue);

            return CreateEffect(randomType, duration, value);
        }
    }
} 