using UnityEngine;
using System.Collections;
using YuGiOhTowerDefense.Core;
using System.Collections.Generic;

namespace YuGiOhTowerDefense.Core
{
    public class MonsterCard : MonoBehaviour
    {
        [Header("Monster Settings")]
        [SerializeField] private float attackRange = 5f;
        [SerializeField] private float attackCooldown = 1f;
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float rotationSpeed = 5f;
        [SerializeField] private LayerMask enemyLayer;
        
        [Header("Visual Settings")]
        [SerializeField] private Transform modelTransform;
        [SerializeField] private ParticleSystem attackParticles;
        [SerializeField] private ParticleSystem idleParticles;
        [SerializeField] private AudioClip attackSound;
        [SerializeField] private AudioClip deathSound;
        [SerializeField] private AudioClip idleSound;
        
        [Header("Object Pooling")]
        [SerializeField] private GameObject attackEffectPrefab;
        [SerializeField] private GameObject deathEffectPrefab;
        [SerializeField] private int poolSize = 5;
        
        private YuGiOhCard cardData;
        private int currentHealth;
        private float attackTimer;
        private Transform target;
        private AudioSource audioSource;
        private Animator animator;
        private ObjectPool attackEffectPool;
        private ObjectPool deathEffectPool;
        
        private MonsterState currentState;
        private Dictionary<MonsterState, IMonsterState> states;
        
        private void Awake()
        {
            InitializeComponents();
            InitializeStateMachine();
            InitializeObjectPools();
        }
        
        private void InitializeComponents()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            animator = GetComponent<Animator>();
        }
        
        private void InitializeStateMachine()
        {
            states = new Dictionary<MonsterState, IMonsterState>
            {
                { MonsterState.Idle, new MonsterIdleState(this) },
                { MonsterState.Moving, new MonsterMovingState(this) },
                { MonsterState.Attacking, new MonsterAttackingState(this) },
                { MonsterState.Dead, new MonsterDeadState(this) }
            };
            
            SetState(MonsterState.Idle);
        }
        
        private void InitializeObjectPools()
        {
            if (attackEffectPrefab != null)
            {
                attackEffectPool = new ObjectPool(attackEffectPrefab, poolSize);
            }
            
            if (deathEffectPrefab != null)
            {
                deathEffectPool = new ObjectPool(deathEffectPrefab, poolSize);
            }
        }
        
        public void Initialize(YuGiOhCard card)
        {
            cardData = card;
            currentHealth = card.defense;
            attackRange = card.range;
            attackCooldown = 1f / card.attackSpeed;
            
            SetState(MonsterState.Idle);
        }
        
        private void Update()
        {
            if (cardData == null || currentState == null)
            {
                return;
            }
            
            currentState.Update();
        }
        
        public void SetState(MonsterState newState)
        {
            if (currentState != null)
            {
                currentState.Exit();
            }
            
            currentState = states[newState];
            currentState.Enter();
        }
        
        public void FindNearestEnemy()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, attackRange * 2f, enemyLayer);
            
            Transform nearestEnemy = null;
            float nearestDistance = float.MaxValue;
            
            foreach (Collider collider in colliders)
            {
                float distance = Vector3.Distance(transform.position, collider.transform.position);
                
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestEnemy = collider.transform;
                }
            }
            
            target = nearestEnemy;
        }
        
        public void MoveTowardsTarget()
        {
            if (target == null)
            {
                return;
            }
            
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
            
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        public void AttackTarget()
        {
            if (target == null || attackTimer > 0)
            {
                return;
            }
            
            attackTimer = attackCooldown;
            
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }
            
            if (attackSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(attackSound);
            }
            
            if (attackEffectPool != null)
            {
                GameObject effect = attackEffectPool.GetObject();
                effect.transform.position = target.position;
                effect.transform.rotation = Quaternion.identity;
            }
            
            if (attackParticles != null)
            {
                attackParticles.Play();
            }
            
            Enemy enemy = target.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(cardData.attack);
            }
        }
        
        public void TakeDamage(int damage)
        {
            currentHealth -= damage;
            
            if (animator != null)
            {
                animator.SetTrigger("Hit");
            }
            
            if (currentHealth <= 0)
            {
                SetState(MonsterState.Dead);
            }
        }
        
        private void Die()
        {
            if (animator != null)
            {
                animator.SetTrigger("Die");
            }
            
            if (deathSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(deathSound);
            }
            
            if (deathEffectPool != null)
            {
                GameObject effect = deathEffectPool.GetObject();
                effect.transform.position = transform.position;
                effect.transform.rotation = Quaternion.identity;
            }
            
            CardManager cardManager = FindObjectOfType<CardManager>();
            if (cardManager != null)
            {
                cardManager.RemoveCard(cardData);
            }
            
            Destroy(gameObject, 1f);
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
        
        // State interface and implementations
        private interface IMonsterState
        {
            void Enter();
            void Update();
            void Exit();
        }
        
        private class MonsterIdleState : IMonsterState
        {
            private readonly MonsterCard monster;
            
            public MonsterIdleState(MonsterCard monster)
            {
                this.monster = monster;
            }
            
            public void Enter()
            {
                if (monster.animator != null)
                {
                    monster.animator.SetBool("IsMoving", false);
                }
                
                if (monster.idleParticles != null)
                {
                    monster.idleParticles.Play();
                }
            }
            
            public void Update()
            {
                monster.FindNearestEnemy();
                
                if (monster.target != null)
                {
                    float distance = Vector3.Distance(monster.transform.position, monster.target.position);
                    
                    if (distance <= monster.attackRange && monster.attackTimer <= 0)
                    {
                        monster.SetState(MonsterState.Attacking);
                    }
                    else if (distance > monster.attackRange)
                    {
                        monster.SetState(MonsterState.Moving);
                    }
                }
            }
            
            public void Exit()
            {
                if (monster.idleParticles != null)
                {
                    monster.idleParticles.Stop();
                }
            }
        }
        
        private class MonsterMovingState : IMonsterState
        {
            private readonly MonsterCard monster;
            
            public MonsterMovingState(MonsterCard monster)
            {
                this.monster = monster;
            }
            
            public void Enter()
            {
                if (monster.animator != null)
                {
                    monster.animator.SetBool("IsMoving", true);
                }
            }
            
            public void Update()
            {
                if (monster.target == null)
                {
                    monster.SetState(MonsterState.Idle);
                    return;
                }
                
                float distance = Vector3.Distance(monster.transform.position, monster.target.position);
                
                if (distance <= monster.attackRange && monster.attackTimer <= 0)
                {
                    monster.SetState(MonsterState.Attacking);
                }
                else if (distance > monster.attackRange)
                {
                    monster.MoveTowardsTarget();
                }
            }
            
            public void Exit()
            {
                if (monster.animator != null)
                {
                    monster.animator.SetBool("IsMoving", false);
                }
            }
        }
        
        private class MonsterAttackingState : IMonsterState
        {
            private readonly MonsterCard monster;
            
            public MonsterAttackingState(MonsterCard monster)
            {
                this.monster = monster;
            }
            
            public void Enter()
            {
                monster.AttackTarget();
            }
            
            public void Update()
            {
                if (monster.target == null)
                {
                    monster.SetState(MonsterState.Idle);
                    return;
                }
                
                float distance = Vector3.Distance(monster.transform.position, monster.target.position);
                
                if (distance > monster.attackRange)
                {
                    monster.SetState(MonsterState.Moving);
                }
                else if (monster.attackTimer <= 0)
                {
                    monster.AttackTarget();
                }
            }
            
            public void Exit()
            {
                // Nothing to do here
            }
        }
        
        private class MonsterDeadState : IMonsterState
        {
            private readonly MonsterCard monster;
            
            public MonsterDeadState(MonsterCard monster)
            {
                this.monster = monster;
            }
            
            public void Enter()
            {
                monster.Die();
            }
            
            public void Update()
            {
                // Nothing to do here
            }
            
            public void Exit()
            {
                // Nothing to do here
            }
        }
    }
    
    public enum MonsterState
    {
        Idle,
        Moving,
        Attacking,
        Dead
    }
    
    public class ObjectPool
    {
        private readonly GameObject prefab;
        private readonly Queue<GameObject> pool;
        private readonly int maxSize;
        
        public ObjectPool(GameObject prefab, int maxSize)
        {
            this.prefab = prefab;
            this.maxSize = maxSize;
            this.pool = new Queue<GameObject>();
            
            for (int i = 0; i < maxSize; i++)
            {
                GameObject obj = GameObject.Instantiate(prefab);
                obj.SetActive(false);
                pool.Enqueue(obj);
            }
        }
        
        public GameObject GetObject()
        {
            if (pool.Count == 0)
            {
                return null;
            }
            
            GameObject obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        
        public void ReturnObject(GameObject obj)
        {
            if (pool.Count >= maxSize)
            {
                GameObject.Destroy(obj);
                return;
            }
            
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }
} 