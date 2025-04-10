using UnityEngine;

namespace YuGiOhTowerDefense.Effects
{
    public class ShieldEffect : StatusEffect
    {
        private readonly float shieldAmount;
        private float remainingShield;

        public ShieldEffect(float duration, float shieldAmount)
            : base(StatusEffectType.Shield, duration, shieldAmount)
        {
            this.shieldAmount = shieldAmount;
            this.remainingShield = shieldAmount;
        }

        public override void ApplyEffect(MonsterCard target)
        {
            if (target != null)
            {
                // Add shield to the monster's defense
                target.DefenseModifier *= (1f + shieldAmount / target.MaxHealth);
            }
        }

        public override void RemoveEffect(MonsterCard target)
        {
            if (target != null)
            {
                // Remove shield from the monster's defense
                target.DefenseModifier /= (1f + shieldAmount / target.MaxHealth);
            }
        }

        public bool AbsorbDamage(float damage)
        {
            if (remainingShield <= 0) return false;

            remainingShield -= damage;
            return remainingShield > 0;
        }
    }
} 