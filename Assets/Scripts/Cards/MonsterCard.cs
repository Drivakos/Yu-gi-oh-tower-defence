using UnityEngine;
using System.Collections;
using YuGiOh.Gameplay;
using YuGiOh.Interfaces;
using YuGiOh.Managers;

namespace YuGiOh.Cards
{
    public class MonsterCard : Card, IDamageable
    {
        [Header("Monster Stats")]
        [SerializeField] protected int baseAttack;
        [SerializeField] protected int baseDefense;
        [SerializeField] protected float baseSpeed = 5f;
        [SerializeField] protected float attackRange = 2f;
        [SerializeField] protected float attackCooldown = 1f;
        
        [Header("Monster Type")]
        [SerializeField] protected MonsterType monsterType;
        [SerializeField] protected MonsterAttribute attribute;
        
        [Header("Targeting")]
        [SerializeField] protected LayerMask targetLayer;
        [SerializeField] protected float detectionRange = 10f;
        
        protected int currentAttack;
        protected int currentDefense;
        protected float currentSpeed;
        protected float lastAttackTime;
        protected Transform currentTarget;
        protected bool isEnemy;
        protected PathFollower pathFollower;
        protected int currentHealth;
        protected MonsterTypeBehavior typeBehavior;
        
        protected override void Start()
        {
            base.Start();
            ResetStats();
            pathFollower = GetComponent<PathFollower>();
            if (pathFollower == null)
            {
                pathFollower = gameObject.AddComponent<PathFollower>();
            }
            
            // Add appropriate type behavior
            AddTypeBehavior();
        }
        
        protected override void ApplyEffect()
        {
            // Monster cards don't have an activation effect
            // They are summoned directly
        }
        
        protected virtual void AddTypeBehavior()
        {
            // Remove any existing type behavior
            MonsterTypeBehavior existingBehavior = GetComponent<MonsterTypeBehavior>();
            if (existingBehavior != null)
            {
                Destroy(existingBehavior);
            }
            
            // Add the appropriate type behavior based on monster type
            switch (monsterType)
            {
                case MonsterType.Dragon:
                    typeBehavior = gameObject.AddComponent<DragonBehavior>();
                    break;
                case MonsterType.Warrior:
                    typeBehavior = gameObject.AddComponent<WarriorBehavior>();
                    break;
                case MonsterType.Spellcaster:
                    typeBehavior = gameObject.AddComponent<SpellcasterBehavior>();
                    break;
                case MonsterType.Fiend:
                    typeBehavior = gameObject.AddComponent<FiendBehavior>();
                    break;
                case MonsterType.Zombie:
                    typeBehavior = gameObject.AddComponent<ZombieBehavior>();
                    break;
                case MonsterType.Machine:
                    typeBehavior = gameObject.AddComponent<MachineBehavior>();
                    break;
                case MonsterType.Beast:
                    typeBehavior = gameObject.AddComponent<BeastBehavior>();
                    break;
                case MonsterType.Rock:
                    typeBehavior = gameObject.AddComponent<RockBehavior>();
                    break;
                case MonsterType.Plant:
                    typeBehavior = gameObject.AddComponent<PlantBehavior>();
                    break;
                case MonsterType.Pyro:
                    typeBehavior = gameObject.AddComponent<PyroBehavior>();
                    break;
                case MonsterType.Thunder:
                    typeBehavior = gameObject.AddComponent<ThunderBehavior>();
                    break;
                case MonsterType.SeaSerpent:
                    typeBehavior = gameObject.AddComponent<SeaSerpentBehavior>();
                    break;
                default:
                    Debug.LogWarning($"No behavior implemented for monster type: {monsterType}");
                    break;
            }
        }
        
        protected override void Update()
        {
            base.Update();
            
            if (isEnemy)
            {
                UpdateEnemyBehavior();
            }
            else
            {
                UpdateDefenderBehavior();
            }
        }
        
        protected virtual void UpdateEnemyBehavior()
        {
            if (currentTarget == null)
            {
                FindTarget();
            }
            
            if (currentTarget != null)
            {
                // Update path to target
                pathFollower.SetObjective(currentTarget);
                
                // Attack if in range
                if (Vector3.Distance(transform.position, currentTarget.position) <= attackRange)
                {
                    TryAttack();
                }
            }
            else
            {
                // Move towards objective if no target
                MoveTowardsObjective();
            }
        }
        
        protected virtual void UpdateDefenderBehavior()
        {
            if (currentTarget == null)
            {
                FindTarget();
            }
            
            if (currentTarget != null)
            {
                // Update path to target
                pathFollower.SetObjective(currentTarget);
                
                // Attack if in range
                if (Vector3.Distance(transform.position, currentTarget.position) <= attackRange)
                {
                    TryAttack();
                }
            }
        }
        
        protected virtual void FindTarget()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRange, targetLayer);
            
            float closestDistance = float.MaxValue;
            Transform closestTarget = null;
            
            foreach (Collider collider in colliders)
            {
                float distance = Vector3.Distance(transform.position, collider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = collider.transform;
                }
            }
            
            currentTarget = closestTarget;
        }
        
        protected virtual void TryAttack()
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                Attack();
            }
        }
        
        protected virtual void Attack()
        {
            if (currentTarget != null)
            {
                // Apply damage to target
                IDamageable damageable = currentTarget.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(currentAttack);
                }
                
                lastAttackTime = Time.time;
            }
        }
        
        protected virtual void MoveTowardsObjective()
        {
            // Set objective to player's base or other objective
            Transform objective = GameObject.FindGameObjectWithTag("Objective")?.transform;
            if (objective != null)
            {
                pathFollower.SetObjective(objective);
            }
        }
        
        public virtual void ModifyStats(float healthMultiplier, float speedMultiplier)
        {
            currentAttack = Mathf.RoundToInt(baseAttack * healthMultiplier);
            currentDefense = Mathf.RoundToInt(baseDefense * healthMultiplier);
            currentSpeed = baseSpeed * speedMultiplier;
            currentHealth = Mathf.RoundToInt(currentDefense * healthMultiplier);
            
            if (pathFollower != null)
            {
                pathFollower.SetMoveSpeed(currentSpeed);
            }
        }
        
        public virtual void ResetStats()
        {
            currentAttack = baseAttack;
            currentDefense = baseDefense;
            currentSpeed = baseSpeed;
            currentHealth = currentDefense;
            
            if (pathFollower != null)
            {
                pathFollower.SetMoveSpeed(currentSpeed);
            }
        }
        
        public virtual void SetAsEnemy(bool isEnemy)
        {
            this.isEnemy = isEnemy;
        }
        
        public virtual void OnDefeated()
        {
            if (isEnemy)
            {
                // Notify wave manager
                WaveManager waveManager = FindObjectOfType<WaveManager>();
                if (waveManager != null)
                {
                    waveManager.OnEnemyDefeated();
                }
            }
            
            // Notify type behavior
            if (typeBehavior != null)
            {
                typeBehavior.OnMonsterDefeated();
            }
            
            Destroy();
        }
        
        public void TakeDamage(int damage)
        {
            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                OnDefeated();
            }
        }
        
        public bool IsAlive()
        {
            return currentHealth > 0;
        }
        
        public void OnDeath()
        {
            OnDefeated();
        }
        
        public MonsterType GetMonsterType() => monsterType;
        public MonsterAttribute GetAttribute() => attribute;
        public int GetCurrentAttack() => currentAttack;
        public int GetCurrentDefense() => currentDefense;
        public float GetCurrentSpeed() => currentSpeed;
        public int GetCurrentHealth() => currentHealth;
        public Transform GetCurrentTarget() => currentTarget;
        public bool IsEnemy() => isEnemy;
    }
} 