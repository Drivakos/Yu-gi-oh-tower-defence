using UnityEngine;
using YuGiOhTowerDefense.Core.Enemy;

namespace YuGiOhTowerDefense.Cards
{
    public class Monster : MonoBehaviour
    {
        [Header("Monster Stats")]
        [SerializeField] private int currentAttack;
        [SerializeField] private int currentDefense;
        [SerializeField] private MonsterPosition currentPosition;
        
        private YuGiOhCard cardData;
        private bool isSummoned;
        private bool canAttack;
        
        public YuGiOhCard CardData => cardData;
        public int CurrentAttack => currentAttack;
        public int CurrentDefense => currentDefense;
        public MonsterPosition CurrentPosition => currentPosition;
        
        public void Initialize(YuGiOhCard card)
        {
            cardData = card;
            currentAttack = card.AttackPoints;
            currentDefense = card.DefensePoints;
            currentPosition = card.MonsterPosition;
            isSummoned = false;
            canAttack = false;
        }
        
        public void Summon()
        {
            isSummoned = true;
            canAttack = false; // Can't attack the turn it's summoned
        }
        
        public void SetPosition(MonsterPosition position)
        {
            currentPosition = position;
            
            // Update visual rotation based on position
            transform.rotation = Quaternion.Euler(
                0,
                position == MonsterPosition.Attack ? 0 : 90,
                0
            );
        }
        
        public void Attack(Enemy enemy)
        {
            if (!canAttack || !isSummoned) return;
            
            if (currentPosition == MonsterPosition.Attack)
            {
                enemy.TakeDamage(currentAttack);
            }
            
            canAttack = false;
        }
        
        public void TakeDamage(int damage)
        {
            if (currentPosition == MonsterPosition.Attack)
            {
                // Compare attack points
                if (damage > currentAttack)
                {
                    // Monster is destroyed
                    Destroy(gameObject);
                }
            }
            else
            {
                // Compare defense points
                if (damage > currentDefense)
                {
                    // Monster is destroyed
                    Destroy(gameObject);
                }
            }
        }
        
        public void ApplyBuff(int attackBuff, int defenseBuff)
        {
            currentAttack += attackBuff;
            currentDefense += defenseBuff;
        }
        
        public void NewTurn()
        {
            canAttack = true;
        }
    }
} 