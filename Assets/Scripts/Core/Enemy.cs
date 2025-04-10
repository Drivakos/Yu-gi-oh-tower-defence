using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using YuGiOhTowerDefense.Base;
using YuGiOhTowerDefense.Utils;
using YuGiOhTowerDefense.Cards;

namespace YuGiOhTowerDefense.Monsters
{
    public class Enemy : MonoBehaviour
    {
        [Header("Enemy Settings")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private int damageToPlayer = 10;
        [SerializeField] private int duelPointsReward = 50;
        [SerializeField] private float attackCooldown = 1f;
        [SerializeField] private float attackRange = 1f;
        [SerializeField] private LayerMask playerLayer;
        
        [Header("Visual Settings")]
        [SerializeField] private ParticleSystem hitParticles;
        [SerializeField] private ParticleSystem deathParticles;
        [SerializeField] private AudioClip hitSound;
        [SerializeField] private AudioClip deathSound;
        [SerializeField] private AudioClip attackSound;
        
        [Header("Path Settings")]
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private float waypointReachedDistance = 0.5f;
        
        [Header("Object Pooling")]
        [SerializeField] private GameObject hitEffectPrefab;
        [SerializeField] private GameObject deathEffectPrefab;
        [SerializeField] private int poolSize = 5;
        
        private int currentHealth;
        private int currentWaypointIndex = 0;
        private float attackTimer;
        private AudioSource audioSource;
        private Animator animator;
        private CardManager cardManager;
        private float healthMultiplier = 1f;
        private float speedMultiplier = 1f;
        private int duelPointsRewardMultiplier = 1;
        
        private ObjectPool hitEffectPool;
        private ObjectPool deathEffectPool;
        
        private EnemyState currentState;
        private Dictionary<EnemyState, IEnemyState> states;
        
        public delegate void EnemyDeathHandler();
        public event EnemyDeathHandler OnEnemyDeath;
        
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
            cardManager = FindObjectOfType<CardManager>();
        }
        
        private void InitializeStateMachine()
        {
            states = new Dictionary<EnemyState, IEnemyState>
            {
                { EnemyState.Moving, new EnemyMovingState(this) },
                { EnemyState.Attacking, new EnemyAttackingState(this) },
                { EnemyState.Dead, new EnemyDeadState(this) }
            };
            
            SetState(EnemyState.Moving);
        }
        
        private void InitializeObjectPools()
        {
            if (hitEffectPrefab != null)
            {
                hitEffectPool = gameObject.AddComponent<ObjectPool>();
                hitEffectPool.Initialize(hitEffectPrefab, poolSize);
            }
            
            if (deathEffectPrefab != null)
            {
                deathEffectPool = gameObject.AddComponent<ObjectPool>();
                deathEffectPool.Initialize(deathEffectPrefab, poolSize);
            }
        }
        
        private void Start()
        {
            currentHealth = Mathf.RoundToInt(maxHealth * healthMultiplier);
            
            if (waypoints.Length > 0)
            {
                SetState(EnemyState.Moving);
            }
        }
        
        private void Update()
        {
            if (currentState == null)
            {
                return;
            }
            
            currentState.Update();
            
            if (attackTimer > 0)
            {
                attackTimer -= Time.deltaTime;
            }
        }
        
        public void SetState(EnemyState newState)
        {
            if (currentState != null)
            {
                currentState.Exit();
            }
            
            currentState = states[newState];
            currentState.Enter();
        }
        
        public void MoveTowardsWaypoint()
        {
            if (currentWaypointIndex >= waypoints.Length)
            {
                return;
            }
            
            Vector3 direction = (waypoints[currentWaypointIndex].position - transform.position).normalized;
            transform.position += direction * moveSpeed * speedMultiplier * Time.deltaTime;
            
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
            
            if (animator != null)
            {
                animator.SetBool("IsMoving", true);
            }
        }
        
        public void CheckWaypointReached()
        {
            if (currentWaypointIndex >= waypoints.Length)
            {
                return;
            }
            
            float distanceToWaypoint = Vector3.Distance(transform.position, waypoints[currentWaypointIndex].position);
            
            if (distanceToWaypoint <= waypointReachedDistance)
            {
                currentWaypointIndex++;
                
                if (currentWaypointIndex >= waypoints.Length)
                {
                    SetState(EnemyState.Attacking);
                }
            }
        }
        
        public void CheckForPlayerInRange()
        {
            if (attackTimer > 0)
            {
                return;
            }
            
            Collider[] colliders = Physics.OverlapSphere(transform.position, attackRange, playerLayer);
            
            if (colliders.Length > 0)
            {
                SetState(EnemyState.Attacking);
            }
        }
        
        public void AttackPlayer()
        {
            if (attackTimer > 0)
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
            
            if (cardManager != null)
            {
                cardManager.RemoveDuelPoints(damageToPlayer);
            }
        }
        
        public void TakeDamage(int damage)
        {
            if (currentState == EnemyState.Dead)
            {
                return;
            }
            
            currentHealth -= damage;
            
            if (animator != null)
            {
                animator.SetTrigger("Hit");
            }
            
            if (hitSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(hitSound);
            }
            
            if (hitEffectPool != null)
            {
                GameObject effect = hitEffectPool.GetObject();
                effect.transform.position = transform.position;
                effect.transform.rotation = Quaternion.identity;
            }
            
            if (hitParticles != null)
            {
                hitParticles.Play();
            }
            
            if (currentHealth <= 0)
            {
                SetState(EnemyState.Dead);
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
            
            if (deathParticles != null)
            {
                deathParticles.Play();
            }
            
            if (cardManager != null)
            {
                cardManager.AddDuelPoints(duelPointsReward * duelPointsRewardMultiplier);
            }
            
            OnEnemyDeath?.Invoke();
            
            Collider collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }
            
            Destroy(gameObject, 1f);
        }
        
        public void SetWaypoints(Transform[] newWaypoints)
        {
            waypoints = newWaypoints;
        }
        
        public void SetHealthMultiplier(float multiplier)
        {
            healthMultiplier = multiplier;
        }
        
        public void SetSpeedMultiplier(float multiplier)
        {
            speedMultiplier = multiplier;
        }
        
        public void SetDuelPointsRewardMultiplier(int multiplier)
        {
            duelPointsRewardMultiplier = multiplier;
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
            
            if (waypoints != null && waypoints.Length > 0)
            {
                Gizmos.color = Color.yellow;
                
                for (int i = 0; i < waypoints.Length; i++)
                {
                    if (waypoints[i] != null)
                    {
                        Gizmos.DrawSphere(waypoints[i].position, 0.5f);
                        
                        if (i < waypoints.Length - 1 && waypoints[i + 1] != null)
                        {
                            Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                        }
                    }
                }
            }
        }
        
        // State interface and implementations
        private interface IEnemyState
        {
            void Enter();
            void Update();
            void Exit();
        }
        
        private class EnemyMovingState : IEnemyState
        {
            private readonly Enemy enemy;
            
            public EnemyMovingState(Enemy enemy)
            {
                this.enemy = enemy;
            }
            
            public void Enter()
            {
                if (enemy.animator != null)
                {
                    enemy.animator.SetBool("IsMoving", true);
                }
            }
            
            public void Update()
            {
                enemy.MoveTowardsWaypoint();
                enemy.CheckWaypointReached();
                enemy.CheckForPlayerInRange();
            }
            
            public void Exit()
            {
                if (enemy.animator != null)
                {
                    enemy.animator.SetBool("IsMoving", false);
                }
            }
        }
        
        private class EnemyAttackingState : IEnemyState
        {
            private readonly Enemy enemy;
            
            public EnemyAttackingState(Enemy enemy)
            {
                this.enemy = enemy;
            }
            
            public void Enter()
            {
                enemy.AttackPlayer();
            }
            
            public void Update()
            {
                if (enemy.attackTimer <= 0)
                {
                    enemy.AttackPlayer();
                }
            }
            
            public void Exit()
            {
                // Nothing to do here
            }
        }
        
        private class EnemyDeadState : IEnemyState
        {
            private readonly Enemy enemy;
            
            public EnemyDeadState(Enemy enemy)
            {
                this.enemy = enemy;
            }
            
            public void Enter()
            {
                enemy.Die();
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
    
    public enum EnemyState
    {
        Moving,
        Attacking,
        Dead
    }
} 