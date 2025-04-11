using UnityEngine;
using System;

namespace YuGiOhTowerDefense.Core.Card
{
    /// <summary>
    /// Base class for all card data
    /// </summary>
    [Serializable]
    public abstract class CardData
    {
        public string Name;
        public string Description;
        public int Cost;
        public GameObject Prefab;
        public Sprite CardImage;
    }
    
    /// <summary>
    /// Data for monster cards
    /// </summary>
    [Serializable]
    public class MonsterCardData : CardData
    {
        public int Attack;
        public int Defense;
        public MonsterType MonsterType;
        public MonsterAttribute Attribute;
        public float AttackRange;
        public float AttackCooldown;
    }
    
    /// <summary>
    /// Data for spell cards
    /// </summary>
    [Serializable]
    public class SpellCardData : CardData
    {
        public SpellType SpellType;
        public SpellIcon SpellIcon;
        public int EffectValue;
        public float EffectDuration;
    }
    
    /// <summary>
    /// Data for trap cards
    /// </summary>
    [Serializable]
    public class TrapCardData : CardData
    {
        public TrapType TrapType;
        public TrapIcon TrapIcon;
        public int EffectValue;
        public float EffectDuration;
    }
    
    /// <summary>
    /// Types of monsters
    /// </summary>
    public enum MonsterType
    {
        Normal,
        Effect,
        Fusion,
        Ritual,
        Synchro,
        XYZ,
        Pendulum,
        Link
    }
    
    /// <summary>
    /// Monster attributes
    /// </summary>
    public enum MonsterAttribute
    {
        Light,
        Dark,
        Earth,
        Water,
        Fire,
        Wind,
        Divine
    }
    
    /// <summary>
    /// Types of spells
    /// </summary>
    public enum SpellType
    {
        Normal,
        Continuous,
        QuickPlay,
        Field,
        Equip,
        Ritual
    }
    
    /// <summary>
    /// Spell effect icons
    /// </summary>
    public enum SpellIcon
    {
        Destroy,
        Damage,
        Modify,
        SpecialSummon,
        Draw,
        Search
    }
    
    /// <summary>
    /// Types of traps
    /// </summary>
    public enum TrapType
    {
        Normal,
        Continuous,
        Counter
    }
    
    /// <summary>
    /// Trap effect icons
    /// </summary>
    public enum TrapIcon
    {
        Destroy,
        Damage,
        Negate,
        Bounce,
        Banish,
        SpecialSummon
    }
} 