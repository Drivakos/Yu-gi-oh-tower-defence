using UnityEngine;
using YuGiOhTowerDefense.Base;
using YuGiOhTowerDefense.Cards;

namespace YuGiOhTowerDefense.Cards
{
    public class MonsterCard : YuGiOhCard
    {
        [SerializeField] protected int attackPoints;
        [SerializeField] protected int defensePoints;
        [SerializeField] protected int level;
        [SerializeField] protected MonsterType monsterType;
        [SerializeField] protected MonsterAttribute monsterAttribute;
        [SerializeField] protected MonsterPosition monsterPosition;

        protected int currentAttack;
        protected int currentDefense;
        protected bool hasAttacked;

        public int AttackPoints => attackPoints;
        public int DefensePoints => defensePoints;
        public int Level => level;
        public MonsterType MonsterType => monsterType;
        public MonsterAttribute MonsterAttribute => monsterAttribute;
        public MonsterPosition MonsterPosition => monsterPosition;
        public int CurrentAttack => currentAttack;
        public int CurrentDefense => currentDefense;
        public bool HasAttacked => hasAttacked;

        public override void Initialize(string name, string desc, CardType type, Sprite image, int id)
        {
            base.Initialize(name, desc, type, image, id);
            currentAttack = attackPoints;
            currentDefense = defensePoints;
            hasAttacked = false;
        }

        public void SetMonsterStats(int atk, int def, int lvl, MonsterType type, MonsterAttribute attribute)
        {
            attackPoints = atk;
            defensePoints = def;
            level = lvl;
            monsterType = type;
            monsterAttribute = attribute;
            monsterPosition = MonsterPosition.Attack;
        }

        public void ChangePosition(MonsterPosition newPosition)
        {
            monsterPosition = newPosition;
        }

        public void ModifyAttack(int amount)
        {
            currentAttack += amount;
        }

        public void ModifyDefense(int amount)
        {
            currentDefense += amount;
        }

        public void Attack()
        {
            if (canAttack && !hasAttacked)
            {
                hasAttacked = true;
                // Attack logic will be implemented in the game manager
            }
        }

        public override void NewTurn()
        {
            base.NewTurn();
            hasAttacked = false;
        }

        public bool CanBeNormalSummoned()
        {
            return level <= 4;
        }

        public bool CanBeTributeSummoned()
        {
            return level >= 5;
        }
    }
} 