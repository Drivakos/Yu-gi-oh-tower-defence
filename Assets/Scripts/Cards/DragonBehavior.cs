using UnityEngine;
using System.Collections;

namespace YuGiOh.Cards
{
    public class DragonBehavior : MonsterTypeBehavior
    {
        [Header("Dragon Special Ability")]
        [SerializeField] private float fireballSpeed = 15f;
        [SerializeField] private float fireballDamage = 50f;
        [SerializeField] private float fireballRadius = 3f;
        [SerializeField] private GameObject fireballPrefab;
        [SerializeField] private GameObject fireballEffectPrefab;
        [SerializeField] private AudioClip fireballSound;
        
        [Header("Dragon Aura")]
        [SerializeField] private float auraDamageMultiplier = 1.5f;
        [SerializeField] private float auraRange = 5f;
        [SerializeField] private GameObject auraEffectPrefab;
        
        private GameObject currentAuraEffect;
        private float originalAttack;
        
        protected override void Start()
        {
            base.Start();
            originalAttack = monsterCard.GetCurrentAttack();
        }
        
        protected override void ApplySpecialAbilityEffects()
        {
            // Dragon's special ability: Fireball attack
            LaunchFireball();
        }
        
        protected override void RemoveSpecialAbilityEffects()
        {
            // No cleanup needed for fireball
        }
        
        private void LaunchFireball()
        {
            if (fireballPrefab == null)
            {
                Debug.LogError("Fireball prefab not assigned to DragonBehavior");
                return;
            }
            
            // Get target direction
            Transform target = monsterCard.GetCurrentTarget();
            if (target == null)
            {
                return;
            }
            
            Vector3 direction = (target.position - transform.position).normalized;
            
            // Spawn fireball
            GameObject fireball = Instantiate(fireballPrefab, transform.position + direction * 2f, Quaternion.LookRotation(direction));
            
            // Set fireball properties
            Fireball fireballComponent = fireball.GetComponent<Fireball>();
            if (fireballComponent != null)
            {
                fireballComponent.Initialize(direction, fireballSpeed, fireballDamage, fireballRadius);
            }
            
            // Play sound
            if (fireballSound != null)
            {
                AudioSource.PlayClipAtPoint(fireballSound, transform.position);
            }
        }
        
        public override void OnMonsterSpawned()
        {
            base.OnMonsterSpawned();
            
            // Apply dragon aura effect
            ApplyDragonAura();
        }
        
        public override void OnMonsterDefeated()
        {
            base.OnMonsterDefeated();
            
            // Remove dragon aura effect
            RemoveDragonAura();
        }
        
        private void ApplyDragonAura()
        {
            // Increase attack for all friendly monsters in range
            Collider[] colliders = Physics.OverlapSphere(transform.position, auraRange);
            foreach (Collider collider in colliders)
            {
                MonsterCard otherMonster = collider.GetComponent<MonsterCard>();
                if (otherMonster != null && otherMonster != monsterCard && !otherMonster.IsEnemy())
                {
                    // Apply aura effect
                    otherMonster.ModifyStats(auraDamageMultiplier, 1f);
                    
                    // Spawn aura effect
                    if (auraEffectPrefab != null && currentAuraEffect == null)
                    {
                        currentAuraEffect = Instantiate(auraEffectPrefab, otherMonster.transform.position, Quaternion.identity, otherMonster.transform);
                    }
                }
            }
        }
        
        private void RemoveDragonAura()
        {
            // Remove aura effect from all affected monsters
            Collider[] colliders = Physics.OverlapSphere(transform.position, auraRange);
            foreach (Collider collider in colliders)
            {
                MonsterCard otherMonster = collider.GetComponent<MonsterCard>();
                if (otherMonster != null && otherMonster != monsterCard && !otherMonster.IsEnemy())
                {
                    // Remove aura effect
                    otherMonster.ResetStats();
                }
            }
            
            // Destroy aura effect
            if (currentAuraEffect != null)
            {
                Destroy(currentAuraEffect);
                currentAuraEffect = null;
            }
        }
    }
    
    public class Fireball : MonoBehaviour
    {
        private Vector3 direction;
        private float speed;
        private float damage;
        private float radius;
        private bool hasHit;
        
        public void Initialize(Vector3 direction, float speed, float damage, float radius)
        {
            this.direction = direction;
            this.speed = speed;
            this.damage = damage;
            this.radius = radius;
            hasHit = false;
            
            // Destroy after 5 seconds
            Destroy(gameObject, 5f);
        }
        
        private void Update()
        {
            if (hasHit)
            {
                return;
            }
            
            // Move fireball
            transform.position += direction * speed * Time.deltaTime;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (hasHit)
            {
                return;
            }
            
            // Check if hit an enemy
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                // Apply damage
                damageable.TakeDamage(Mathf.RoundToInt(damage));
                
                // Apply area damage
                ApplyAreaDamage();
                
                hasHit = true;
                Destroy(gameObject);
            }
        }
        
        private void ApplyAreaDamage()
        {
            // Find all colliders in radius
            Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
            
            foreach (Collider collider in colliders)
            {
                // Skip the direct hit target
                if (collider.gameObject == gameObject)
                {
                    continue;
                }
                
                // Apply reduced damage to nearby enemies
                IDamageable damageable = collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    float distance = Vector3.Distance(transform.position, collider.transform.position);
                    float damageMultiplier = 1f - (distance / radius);
                    int areaDamage = Mathf.RoundToInt(damage * damageMultiplier * 0.5f);
                    
                    if (areaDamage > 0)
                    {
                        damageable.TakeDamage(areaDamage);
                    }
                }
            }
        }
    }
} 