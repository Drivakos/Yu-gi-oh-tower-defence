using UnityEngine;
using System.Collections.Generic;
using YuGiOhTowerDefense.Base;
using YuGiOhTowerDefense.Utils;
using YuGiOhTowerDefense.Cards;
using YuGiOhTowerDefense.StateMachine;
using YuGiOhTowerDefense.Effects;

namespace YuGiOhTowerDefense.Cards
{
    public interface IMonsterCardData
    {
        int Attack { get; }
        int Defense { get; }
        float Range { get; }
        float AttackSpeed { get; }
        string Name { get; }
        string Description { get; }
        CardType Type { get; }
        CardRarity Rarity { get; }
    }

    public class MonsterCard : MonoBehaviour
    {
        private const float MIN_ATTACK_RANGE = 1f;
        private const float MAX_ATTACK_RANGE = 10f;
        private const float MIN_ATTACK_SPEED = 0.5f;
        private const float MAX_ATTACK_SPEED = 5f;
        private const float MIN_MOVE_SPEED = 1f;
        private const float MAX_MOVE_SPEED = 10f;

        [Header("Monster Settings")]
        [SerializeField] private float attackRange = 5f;
        [SerializeField] private float attackCooldown = 1f;
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float rotationSpeed = 5f;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private float enemySearchInterval = 0.5f;
        [SerializeField] private float attackModifier = 1f;
        [SerializeField] private float defenseModifier = 1f;
        
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
        
        private IMonsterCardData cardData;
        private float currentHealth;
        private float currentCooldown;
        private float attackTimer;
        private Transform target;
        private AudioSource audioSource;
        private Animator animator;
        private ObjectPool attackEffectPool;
        private ObjectPool deathEffectPool;
        private float lastEnemySearchTime;
        private Vector3 lastPosition;
        private float distanceMoved;
        private bool isInitialized;
        private CardManager cardManager;
        private SpatialPartition spatialPartition;
        private MonsterStateMachine stateMachine;
        private List<StatusEffect> activeEffects = new List<StatusEffect>();
        
        public event System.Action<MonsterCard> OnDeath;
        public event System.Action<MonsterCard, float> OnDamageTaken;
        public event System.Action<MonsterCard, Transform> OnTargetChanged;
        public event System.Action<MonsterCard, StatusEffect> OnEffectApplied;
        public event System.Action<MonsterCard, StatusEffect> OnEffectRemoved;
        
        public IMonsterCardData CardData => cardData;
        public float CurrentHealth => currentHealth;
        public float MaxHealth => cardData?.Attack ?? 0;
        public float AttackModifier => attackModifier;
        public float DefenseModifier => defenseModifier;
        public float MoveSpeed => moveSpeed;
        public bool IsAlive => currentHealth > 0;
        public bool IsInitialized => isInitialized;
        public Transform Target => target;
        public float AttackRange => attackRange;
        public float AttackTimer => attackTimer;
        public Animator Animator => animator;
        public ParticleSystem IdleParticles => idleParticles;
        
        private void Awake()
        {
            InitializeComponents();
            InitializeStateMachine();
            InitializeObjectPools();
            InitializeSpatialPartition();
            lastPosition = transform.position;
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
            
            if (modelTransform == null)
            {
                modelTransform = transform;
            }
        }
        
        private void InitializeStateMachine()
        {
            stateMachine = new MonsterStateMachine(this);
        }
        
        private void InitializeObjectPools()
        {
            if (attackEffectPrefab != null)
            {
                attackEffectPool = gameObject.AddComponent<ObjectPool>();
                attackEffectPool.Initialize(attackEffectPrefab, poolSize);
            }
            
            if (deathEffectPrefab != null)
            {
                deathEffectPool = gameObject.AddComponent<ObjectPool>();
                deathEffectPool.Initialize(deathEffectPrefab, poolSize);
            }
        }
        
        private void InitializeSpatialPartition()
        {
            spatialPartition = new SpatialPartition(attackRange, enemyLayer);
        }
        
        public void Initialize(IMonsterCardData card)
        {
            if (isInitialized)
            {
                Debug.LogWarning("MonsterCard already initialized!");
                return;
            }
            
            cardData = card;
            currentHealth = Mathf.RoundToInt(card.Defense * defenseModifier);
            attackRange = Mathf.Clamp(card.Range, MIN_ATTACK_RANGE, MAX_ATTACK_RANGE);
            attackCooldown = Mathf.Clamp(1f / card.AttackSpeed, MIN_ATTACK_SPEED, MAX_ATTACK_SPEED);
            moveSpeed = Mathf.Clamp(moveSpeed, MIN_MOVE_SPEED, MAX_MOVE_SPEED);
            
            isInitialized = true;
            SetState(MonsterState.Idle);
        }
        
        private void Update()
        {
            if (!isInitialized)
            {
                return;
            }
            
            UpdateEffects();
            UpdateMovement();
            stateMachine.Update();
            
            if (attackTimer > 0)
            {
                attackTimer -= Time.deltaTime;
            }
        }
        
        private void UpdateEffects()
        {
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                StatusEffect effect = activeEffects[i];
                effect.Update(Time.deltaTime);
                
                if (effect.IsExpired)
                {
                    RemoveEffect(effect);
                }
                else
                {
                    switch (effect.Type)
                    {
                        case StatusEffectType.Poison:
                        case StatusEffectType.Burn:
                            TakeDamage(((effect as PoisonEffect)?.damagePerSecond ?? 0) * Time.deltaTime);
                            break;
                        case StatusEffectType.Heal:
                            Heal(((effect as HealEffect)?.healPerSecond ?? 0) * Time.deltaTime);
                            break;
                    }
                }
            }
        }
        
        private void UpdateMovement()
        {
            Vector3 currentPosition = transform.position;
            distanceMoved = Vector3.Distance(lastPosition, currentPosition);
            lastPosition = currentPosition;
            
            if (spatialPartition != null)
            {
                spatialPartition.UpdateObject(transform, lastPosition);
            }
        }
        
        public void ApplyEffect(StatusEffect effect)
        {
            if (effect == null)
            {
                return;
            }
            
            activeEffects.Add(effect);
            effect.Apply(this);
            OnEffectApplied?.Invoke(this, effect);
        }
        
        public void RemoveEffect(StatusEffect effect)
        {
            if (effect == null)
            {
                return;
            }
            
            if (activeEffects.Remove(effect))
            {
                effect.Remove(this);
                OnEffectRemoved?.Invoke(this, effect);
            }
        }
        
        public void SetState(MonsterState newState)
        {
            stateMachine.SetState(newState);
        }
        
        public void FindNearestEnemy()
        {
            if (Time.time - lastEnemySearchTime < enemySearchInterval)
            {
                return;
            }
            
            lastEnemySearchTime = Time.time;
            
            if (spatialPartition != null)
            {
                target = spatialPartition.FindNearest(transform.position, attackRange);
            }
            else
            {
                Collider[] colliders = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);
                
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
            
            if (target != null)
            {
                OnTargetChanged?.Invoke(this, target);
            }
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
            PlayAttackAnimation();
            PlayAttackSound();
            SpawnAttackEffect();
            
            Enemy enemy = target.GetComponent<Enemy>();
            if (enemy != null)
            {
                int damage = Mathf.RoundToInt(cardData.Attack * attackModifier);
                enemy.TakeDamage(damage);
            }
        }
        
        private void PlayAttackAnimation()
        {
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }
        }
        
        private void PlayAttackSound()
        {
            if (attackSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(attackSound);
            }
        }
        
        private void SpawnAttackEffect()
        {
            if (attackEffectPool != null)
            {
                GameObject effect = attackEffectPool.Get();
                if (effect != null)
                {
                    effect.transform.position = target.position;
                    effect.transform.rotation = Quaternion.identity;
                }
            }
            
            if (attackParticles != null)
            {
                attackParticles.Play();
            }
        }
        
        public void TakeDamage(float damage)
        {
            if (!IsAlive) return;

            float actualDamage = damage / defenseModifier;
            currentHealth = Mathf.Max(0, currentHealth - actualDamage);
            OnDamageTaken?.Invoke(this, actualDamage);
            
            if (animator != null)
            {
                animator.SetTrigger("Hit");
            }
            
            if (currentHealth <= 0)
            {
                SetState(MonsterState.Dead);
            }
        }
        
        public void Heal(float amount)
        {
            if (!IsAlive) return;

            currentHealth = Mathf.Min(MaxHealth, currentHealth + amount);
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
                GameObject effect = deathEffectPool.Get();
                if (effect != null)
                {
                    effect.transform.position = transform.position;
                    effect.transform.rotation = Quaternion.identity;
                }
            }
            
            OnDeath?.Invoke(this);
            
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

        public float GetCooldown()
        {
            return attackCooldown;
        }

        public void SetCooldown(float cooldown)
        {
            attackCooldown = cooldown;
        }

        public float GetRange()
        {
            return attackRange;
        }

        public void SetRange(float range)
        {
            attackRange = Mathf.Clamp(range, MIN_ATTACK_RANGE, MAX_ATTACK_RANGE);
        }

        public float GetAttackSpeed()
        {
            return 1f / attackCooldown;
        }

        public void SetAttackSpeed(float attackSpeed)
        {
            attackCooldown = 1f / Mathf.Clamp(attackSpeed, MIN_ATTACK_SPEED, MAX_ATTACK_SPEED);
        }
    }
    
    public enum MonsterState
    {
        Idle,
        Moving,
        Attacking,
        Dead
    }
    
    public abstract class StatusEffect
    {
        public float Duration { get; protected set; }
        public bool IsExpired => Duration <= 0;
        
        public virtual void Apply(MonsterCard monster) { }
        public virtual void Update(float deltaTime) { Duration -= deltaTime; }
        public virtual void Remove(MonsterCard monster) { }
    }
} 