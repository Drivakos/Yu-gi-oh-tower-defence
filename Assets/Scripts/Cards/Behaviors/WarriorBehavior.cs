using UnityEngine;
using System.Collections;

namespace YuGiOh.Cards
{
    public class WarriorBehavior : MonsterTypeBehavior
    {
        [Header("Warrior Special Ability")]
        [SerializeField] private float chargeSpeed = 15f;
        [SerializeField] private float chargeDamage = 75f;
        [SerializeField] private float chargeDistance = 10f;
        [SerializeField] private GameObject chargeEffectPrefab;
        [SerializeField] private AudioClip chargeSound;
        
        [Header("Warrior Formation")]
        [SerializeField] private float formationDamageMultiplier = 1.3f;
        [SerializeField] private float formationDefenseMultiplier = 1.3f;
        [SerializeField] private float formationRange = 8f;
        [SerializeField] private GameObject formationEffectPrefab;
        
        private GameObject currentFormationEffect;
        private Vector3 chargeStartPosition;
        private Vector3 chargeDirection;
        private bool isCharging;
        
        protected override void ApplySpecialAbilityEffects()
        {
            // Warrior's special ability: Charge attack
            StartCharge();
        }
        
        protected override void RemoveSpecialAbilityEffects()
        {
            // No cleanup needed for charge
        }
        
        private void StartCharge()
        {
            if (isCharging)
            {
                return;
            }
            
            // Get target direction
            Transform target = monsterCard.GetCurrentTarget();
            if (target == null)
            {
                return;
            }
            
            chargeDirection = (target.position - transform.position).normalized;
            chargeStartPosition = transform.position;
            isCharging = true;
            
            // Play charge sound
            if (chargeSound != null)
            {
                AudioSource.PlayClipAtPoint(chargeSound, transform.position);
            }
            
            // Spawn charge effect
            if (chargeEffectPrefab != null)
            {
                GameObject effect = Instantiate(chargeEffectPrefab, transform.position, Quaternion.LookRotation(chargeDirection), transform);
                Destroy(effect, 1f);
            }
            
            // Start charge coroutine
            StartCoroutine(ChargeCoroutine());
        }
        
        private IEnumerator ChargeCoroutine()
        {
            float chargeTime = 0f;
            float chargeDuration = chargeDistance / chargeSpeed;
            
            // Disable path following during charge
            PathFollower pathFollower = GetComponent<PathFollower>();
            if (pathFollower != null)
            {
                pathFollower.enabled = false;
            }
            
            // Charge forward
            while (chargeTime < chargeDuration)
            {
                transform.position += chargeDirection * chargeSpeed * Time.deltaTime;
                chargeTime += Time.deltaTime;
                
                // Check for collisions
                CheckChargeCollisions();
                
                yield return null;
            }
            
            // Re-enable path following
            if (pathFollower != null)
            {
                pathFollower.enabled = true;
            }
            
            isCharging = false;
        }
        
        private void CheckChargeCollisions()
        {
            // Check for enemies in front
            RaycastHit[] hits = Physics.RaycastAll(transform.position, chargeDirection, 2f);
            
            foreach (RaycastHit hit in hits)
            {
                IDamageable damageable = hit.collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    // Apply charge damage
                    damageable.TakeDamage(Mathf.RoundToInt(chargeDamage));
                    
                    // Apply knockback
                    Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.AddForce(chargeDirection * 10f, ForceMode.Impulse);
                    }
                }
            }
        }
        
        public override void OnMonsterSpawned()
        {
            base.OnMonsterSpawned();
            
            // Apply warrior formation effect
            ApplyWarriorFormation();
        }
        
        public override void OnMonsterDefeated()
        {
            base.OnMonsterDefeated();
            
            // Remove warrior formation effect
            RemoveWarriorFormation();
        }
        
        private void ApplyWarriorFormation()
        {
            // Find all warrior monsters in range
            Collider[] colliders = Physics.OverlapSphere(transform.position, formationRange);
            int warriorCount = 0;
            
            foreach (Collider collider in colliders)
            {
                MonsterCard otherMonster = collider.GetComponent<MonsterCard>();
                if (otherMonster != null && otherMonster != monsterCard && !otherMonster.IsEnemy())
                {
                    // Check if it's a warrior
                    if (otherMonster.GetMonsterType() == MonsterType.Warrior)
                    {
                        warriorCount++;
                    }
                }
            }
            
            // Apply formation bonus based on number of warriors
            if (warriorCount > 0)
            {
                float damageMultiplier = 1f + (formationDamageMultiplier - 1f) * warriorCount;
                float defenseMultiplier = 1f + (formationDefenseMultiplier - 1f) * warriorCount;
                
                monsterCard.ModifyStats(defenseMultiplier, 1f);
                
                // Spawn formation effect
                if (formationEffectPrefab != null && currentFormationEffect == null)
                {
                    currentFormationEffect = Instantiate(formationEffectPrefab, transform.position, Quaternion.identity, transform);
                }
            }
        }
        
        private void RemoveWarriorFormation()
        {
            // Reset stats
            monsterCard.ResetStats();
            
            // Destroy formation effect
            if (currentFormationEffect != null)
            {
                Destroy(currentFormationEffect);
                currentFormationEffect = null;
            }
        }
    }
} 