using UnityEngine;
using System;

namespace YuGiOhTowerDefense.Monsters
{
    public enum MonsterType
    {
        Beast,
        Dragon,
        Spellcaster,
        Fiend,
        Zombie,
        Machine,
        BeastWarrior
    }

    public enum Attribute
    {
        Fire,
        Water,
        Earth,
        Wind,
        Light,
        Dark
    }

    [Serializable]
    public class MonsterStats
    {
        public float attack;
        public float defense;
        public float attackSpeed;
        public float range;
        public int level;
        public int cost;
    }

    public abstract class Monster : MonoBehaviour
    {
        [SerializeField] protected string monsterName;
        [SerializeField] protected MonsterType type;
        [SerializeField] protected Attribute attribute;
        [SerializeField] protected MonsterStats stats;
        [SerializeField] protected GameObject attackEffectPrefab;
        
        protected float currentHealth;
        protected bool isActive;
        protected float attackCooldown;
        protected Transform target;

        public event Action<Monster> OnMonsterDestroyed;
        public event Action<float> OnHealthChanged;

        protected virtual void Start()
        {
            currentHealth = stats.defense;
            isActive = true;
            attackCooldown = 0f;
        }

        protected virtual void Update()
        {
            if (!isActive) return;

            if (attackCooldown > 0)
            {
                attackCooldown -= Time.deltaTime;
            }

            if (target != null && attackCooldown <= 0)
            {
                Attack();
            }
        }

        public virtual void Initialize(MonsterStats newStats)
        {
            stats = newStats;
            currentHealth = stats.defense;
        }

        protected virtual void Attack()
        {
            if (target == null) return;

            attackCooldown = 1f / stats.attackSpeed;
            
            // Spawn attack effect
            if (attackEffectPrefab != null)
            {
                Instantiate(attackEffectPrefab, transform.position, Quaternion.identity);
            }

            // Deal damage to target
            var enemy = target.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(stats.attack);
            }
        }

        public virtual void TakeDamage(float damage)
        {
            currentHealth -= damage;
            OnHealthChanged?.Invoke(currentHealth);

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        protected virtual void Die()
        {
            isActive = false;
            OnMonsterDestroyed?.Invoke(this);
            Destroy(gameObject);
        }

        public virtual void LevelUp()
        {
            stats.level++;
            stats.attack *= 1.2f;
            stats.defense *= 1.2f;
            currentHealth = stats.defense;
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        public MonsterStats GetStats()
        {
            return stats;
        }

        public MonsterType GetMonsterType()
        {
            return type;
        }

        public Attribute GetAttribute()
        {
            return attribute;
        }
    }
} 