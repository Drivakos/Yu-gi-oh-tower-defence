using UnityEngine;
using YuGiOhTowerDefense.Cards;

namespace YuGiOhTowerDefense.Effects
{
    public enum StatusEffectType
    {
        Poison,
        Stun,
        Buff,
        Debuff,
        Burn,
        Freeze,
        Heal,
        Shield,
        SpeedBoost,
        Slow
    }

    public abstract class StatusEffect
    {
        public StatusEffectType Type { get; protected set; }
        public float Duration { get; protected set; }
        public float Value { get; protected set; }
        public bool IsActive => Duration > 0f;
        public string Name { get; protected set; }
        public string Description { get; protected set; }

        protected StatusEffect(StatusEffectType type, float duration, float value)
        {
            Type = type;
            Duration = duration;
            Value = value;
        }

        public virtual void Update(float deltaTime)
        {
            if (IsActive)
            {
                Duration -= deltaTime;
            }
        }

        public virtual void ApplyEffect(MonsterCard target)
        {
            // Base implementation does nothing
        }

        public virtual void RemoveEffect(MonsterCard target)
        {
            // Base implementation does nothing
        }
    }

    public class PoisonEffect : StatusEffect
    {
        private readonly float damagePerSecond;
        private float damageTimer;

        public PoisonEffect(float duration, float damagePerSecond)
            : base(StatusEffectType.Poison, duration, damagePerSecond)
        {
            this.damagePerSecond = damagePerSecond;
            Name = "Poison";
            Description = $"Deals {damagePerSecond} damage per second for {duration} seconds";
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            damageTimer += deltaTime;
            if (damageTimer >= 1f)
            {
                damageTimer = 0f;
                // Damage is applied in the monster's UpdateEffects method
            }
        }
    }

    public class StunEffect : StatusEffect
    {
        public StunEffect(float duration)
            : base(StatusEffectType.Stun, duration, 0f)
        {
            Name = "Stun";
            Description = $"Prevents movement and attacks for {duration} seconds";
        }

        public override void ApplyEffect(MonsterCard monster)
        {
            monster.SetState(MonsterState.Idle);
        }
    }

    public class BuffEffect : StatusEffect
    {
        private readonly float attackModifier;
        private readonly float defenseModifier;

        public BuffEffect(float duration, float attackModifier, float defenseModifier)
            : base(StatusEffectType.Buff, duration, 0f)
        {
            this.attackModifier = attackModifier;
            this.defenseModifier = defenseModifier;
            Name = "Buff";
            Description = $"Increases attack by {attackModifier}x and defense by {defenseModifier}x for {duration} seconds";
        }

        public override void ApplyEffect(MonsterCard monster)
        {
            monster.AttackModifier *= attackModifier;
            monster.DefenseModifier *= defenseModifier;
        }

        public override void RemoveEffect(MonsterCard monster)
        {
            monster.AttackModifier /= attackModifier;
            monster.DefenseModifier /= defenseModifier;
        }
    }

    public class DebuffEffect : StatusEffect
    {
        private readonly float attackModifier;
        private readonly float defenseModifier;

        public DebuffEffect(float duration, float attackModifier, float defenseModifier)
            : base(StatusEffectType.Debuff, duration, 0f)
        {
            this.attackModifier = attackModifier;
            this.defenseModifier = defenseModifier;
            Name = "Debuff";
            Description = $"Decreases attack by {attackModifier}x and defense by {defenseModifier}x for {duration} seconds";
        }

        public override void ApplyEffect(MonsterCard monster)
        {
            monster.AttackModifier /= attackModifier;
            monster.DefenseModifier /= defenseModifier;
        }

        public override void RemoveEffect(MonsterCard monster)
        {
            monster.AttackModifier *= attackModifier;
            monster.DefenseModifier *= defenseModifier;
        }
    }

    public class BurnEffect : StatusEffect
    {
        private readonly float damagePerSecond;
        private float damageTimer;

        public BurnEffect(float duration, float damagePerSecond)
            : base(StatusEffectType.Burn, duration, damagePerSecond)
        {
            this.damagePerSecond = damagePerSecond;
            Name = "Burn";
            Description = $"Deals {damagePerSecond} damage per second for {duration} seconds";
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            damageTimer += deltaTime;
            if (damageTimer >= 1f)
            {
                damageTimer = 0f;
                // Damage is applied in the monster's UpdateEffects method
            }
        }
    }

    public class FreezeEffect : StatusEffect
    {
        private readonly float speedReduction;

        public FreezeEffect(float duration, float speedReduction)
            : base(StatusEffectType.Freeze, duration, speedReduction)
        {
            this.speedReduction = speedReduction;
            Name = "Freeze";
            Description = $"Reduces movement speed by {speedReduction * 100}% for {duration} seconds";
        }

        public override void ApplyEffect(MonsterCard monster)
        {
            monster.MoveSpeed *= (1f - speedReduction);
        }

        public override void RemoveEffect(MonsterCard monster)
        {
            monster.MoveSpeed /= (1f - speedReduction);
        }
    }

    public class HealEffect : StatusEffect
    {
        private readonly float healPerSecond;
        private float healTimer;

        public HealEffect(float duration, float healPerSecond)
            : base(StatusEffectType.Heal, duration, healPerSecond)
        {
            this.healPerSecond = healPerSecond;
            Name = "Heal";
            Description = $"Heals {healPerSecond} health per second for {duration} seconds";
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            healTimer += deltaTime;
            if (healTimer >= 1f)
            {
                healTimer = 0f;
                // Healing is applied in the monster's UpdateEffects method
            }
        }
    }

    public class ShieldEffect : StatusEffect
    {
        private readonly float shieldAmount;

        public ShieldEffect(float duration, float shieldAmount)
            : base(StatusEffectType.Shield, duration, shieldAmount)
        {
            this.shieldAmount = shieldAmount;
            Name = "Shield";
            Description = $"Absorbs {shieldAmount} damage for {duration} seconds";
        }

        public override void ApplyEffect(MonsterCard monster)
        {
            monster.DefenseModifier *= (1f + shieldAmount / monster.CurrentHealth);
        }

        public override void RemoveEffect(MonsterCard monster)
        {
            monster.DefenseModifier /= (1f + shieldAmount / monster.CurrentHealth);
        }
    }

    public class SpeedBoostEffect : StatusEffect
    {
        private readonly float speedMultiplier;

        public SpeedBoostEffect(float duration, float speedMultiplier)
            : base(StatusEffectType.SpeedBoost, duration, speedMultiplier)
        {
            this.speedMultiplier = speedMultiplier;
            Name = "Speed Boost";
            Description = $"Increases movement speed by {speedMultiplier * 100}% for {duration} seconds";
        }

        public override void ApplyEffect(MonsterCard monster)
        {
            monster.MoveSpeed *= speedMultiplier;
        }

        public override void RemoveEffect(MonsterCard monster)
        {
            monster.MoveSpeed /= speedMultiplier;
        }
    }

    public class SlowEffect : StatusEffect
    {
        private readonly float speedReduction;

        public SlowEffect(float duration, float speedReduction)
            : base(StatusEffectType.Slow, duration, speedReduction)
        {
            this.speedReduction = speedReduction;
            Name = "Slow";
            Description = $"Reduces movement speed by {speedReduction * 100}% for {duration} seconds";
        }

        public override void ApplyEffect(MonsterCard monster)
        {
            monster.MoveSpeed *= (1f - speedReduction);
        }

        public override void RemoveEffect(MonsterCard monster)
        {
            monster.MoveSpeed /= (1f - speedReduction);
        }
    }

    public static class StatusEffectFactory
    {
        public static StatusEffect CreateEffect(StatusEffectType type, float duration, params float[] parameters)
        {
            return type switch
            {
                StatusEffectType.Poison => new PoisonEffect(duration, parameters[0]),
                StatusEffectType.Stun => new StunEffect(duration),
                StatusEffectType.Buff => new BuffEffect(duration, parameters[0], parameters[1]),
                StatusEffectType.Debuff => new DebuffEffect(duration, parameters[0], parameters[1]),
                StatusEffectType.Burn => new BurnEffect(duration, parameters[0]),
                StatusEffectType.Freeze => new FreezeEffect(duration, parameters[0]),
                StatusEffectType.Heal => new HealEffect(duration, parameters[0]),
                StatusEffectType.Shield => new ShieldEffect(duration, parameters[0]),
                StatusEffectType.SpeedBoost => new SpeedBoostEffect(duration, parameters[0]),
                StatusEffectType.Slow => new SlowEffect(duration, parameters[0]),
                _ => null
            };
        }
    }
} 