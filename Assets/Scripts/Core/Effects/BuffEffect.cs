using UnityEngine;

namespace YuGiOhTowerDefense.Effects
{
    public class BuffEffect : StatusEffect
    {
        private readonly float attackMultiplier;
        private readonly float defenseMultiplier;
        private float originalAttackModifier;
        private float originalDefenseModifier;

        public BuffEffect(float duration, float attackMultiplier, float defenseMultiplier)
            : base(StatusEffectType.Buff, duration, attackMultiplier)
        {
            this.attackMultiplier = attackMultiplier;
            this.defenseMultiplier = defenseMultiplier;
        }

        public override void ApplyEffect(MonsterCard target)
        {
            if (target != null)
            {
                // Store original values
                originalAttackModifier = target.AttackModifier;
                originalDefenseModifier = target.DefenseModifier;

                // Apply buffs
                target.AttackModifier *= attackMultiplier;
                target.DefenseModifier *= defenseMultiplier;
            }
        }

        public override void RemoveEffect(MonsterCard target)
        {
            if (target != null)
            {
                // Restore original values
                target.AttackModifier = originalAttackModifier;
                target.DefenseModifier = originalDefenseModifier;
            }
        }
    }
} 