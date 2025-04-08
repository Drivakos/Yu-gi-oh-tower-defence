using UnityEngine;
using System.Collections;

namespace YuGiOh.Cards
{
    public class SpellcasterBehavior : MonsterTypeBehavior
    {
        [Header("Spellcaster Special Ability")]
        [SerializeField] private float magicBoltSpeed = 20f;
        [SerializeField] private float magicBoltDamage = 40f;
        [SerializeField] private int magicBoltCount = 3;
        [SerializeField] private float magicBoltSpread = 15f;
        [SerializeField] private GameObject magicBoltPrefab;
        [SerializeField] private GameObject magicBoltEffectPrefab;
        [SerializeField] private AudioClip magicBoltSound;
        
        [Header("Spellcaster Aura")]
        [SerializeField] private float manaRegenRate = 5f;
        [SerializeField] private float manaRegenRange = 8f;
        [SerializeField] private GameObject manaAuraEffectPrefab;
        
        private GameObject currentManaAuraEffect;
        private float manaPool = 100f;
        private float maxManaPool = 100f;
        
        protected override void Start()
        {
            base.Start();
            StartCoroutine(RegenerateMana());
        }
        
        protected override void ApplySpecialAbilityEffects()
        {
            // Spellcaster's special ability: Multi-target Magic Bolts
            if (manaPool >= 30f)
            {
                CastMagicBolts();
                manaPool -= 30f;
            }
        }
        
        protected override void RemoveSpecialAbilityEffects()
        {
            // No cleanup needed for magic bolts
        }
        
        private void CastMagicBolts()
        {
            if (magicBoltPrefab == null)
            {
                Debug.LogError("Magic bolt prefab not assigned to SpellcasterBehavior");
                return;
            }
            
            // Get target direction
            Transform target = monsterCard.GetCurrentTarget();
            if (target == null)
            {
                return;
            }
            
            Vector3 baseDirection = (target.position - transform.position).normalized;
            
            // Calculate spread angle
            float angleStep = magicBoltSpread / (magicBoltCount - 1);
            float startAngle = -magicBoltSpread / 2f;
            
            // Cast multiple magic bolts in a spread pattern
            for (int i = 0; i < magicBoltCount; i++)
            {
                float currentAngle = startAngle + (angleStep * i);
                Vector3 direction = Quaternion.Euler(0, currentAngle, 0) * baseDirection;
                
                // Spawn magic bolt
                GameObject magicBolt = Instantiate(magicBoltPrefab, transform.position + direction * 2f, Quaternion.LookRotation(direction));
                
                // Set magic bolt properties
                MagicBolt boltComponent = magicBolt.GetComponent<MagicBolt>();
                if (boltComponent != null)
                {
                    boltComponent.Initialize(direction, magicBoltSpeed, magicBoltDamage);
                }
                
                // Spawn effect
                if (magicBoltEffectPrefab != null)
                {
                    GameObject effect = Instantiate(magicBoltEffectPrefab, magicBolt.transform.position, Quaternion.LookRotation(direction), magicBolt.transform);
                    Destroy(effect, 2f);
                }
            }
            
            // Play sound
            if (magicBoltSound != null)
            {
                AudioSource.PlayClipAtPoint(magicBoltSound, transform.position);
            }
        }
        
        private IEnumerator RegenerateMana()
        {
            while (true)
            {
                if (manaPool < maxManaPool)
                {
                    manaPool = Mathf.Min(manaPool + manaRegenRate * Time.deltaTime, maxManaPool);
                }
                yield return null;
            }
        }
        
        public override void OnMonsterSpawned()
        {
            base.OnMonsterSpawned();
            
            // Apply mana regeneration aura
            ApplyManaAura();
        }
        
        public override void OnMonsterDefeated()
        {
            base.OnMonsterDefeated();
            
            // Remove mana regeneration aura
            RemoveManaAura();
        }
        
        private void ApplyManaAura()
        {
            // Find all friendly monsters in range
            Collider[] colliders = Physics.OverlapSphere(transform.position, manaRegenRange);
            
            foreach (Collider collider in colliders)
            {
                MonsterCard otherMonster = collider.GetComponent<MonsterCard>();
                if (otherMonster != null && otherMonster != monsterCard && !otherMonster.IsEnemy())
                {
                    // Apply mana regeneration effect
                    SpellcasterBehavior otherSpellcaster = otherMonster.GetComponent<SpellcasterBehavior>();
                    if (otherSpellcaster != null)
                    {
                        otherSpellcaster.AddManaRegenBonus(manaRegenRate * 0.5f);
                    }
                    
                    // Spawn mana aura effect
                    if (manaAuraEffectPrefab != null && currentManaAuraEffect == null)
                    {
                        currentManaAuraEffect = Instantiate(manaAuraEffectPrefab, otherMonster.transform.position, Quaternion.identity, otherMonster.transform);
                    }
                }
            }
        }
        
        private void RemoveManaAura()
        {
            // Find all friendly monsters in range
            Collider[] colliders = Physics.OverlapSphere(transform.position, manaRegenRange);
            
            foreach (Collider collider in colliders)
            {
                MonsterCard otherMonster = collider.GetComponent<MonsterCard>();
                if (otherMonster != null && otherMonster != monsterCard && !otherMonster.IsEnemy())
                {
                    // Remove mana regeneration effect
                    SpellcasterBehavior otherSpellcaster = otherMonster.GetComponent<SpellcasterBehavior>();
                    if (otherSpellcaster != null)
                    {
                        otherSpellcaster.RemoveManaRegenBonus(manaRegenRate * 0.5f);
                    }
                }
            }
            
            // Destroy mana aura effect
            if (currentManaAuraEffect != null)
            {
                Destroy(currentManaAuraEffect);
                currentManaAuraEffect = null;
            }
        }
        
        public void AddManaRegenBonus(float bonus)
        {
            manaRegenRate += bonus;
        }
        
        public void RemoveManaRegenBonus(float bonus)
        {
            manaRegenRate -= bonus;
        }
    }
    
    public class MagicBolt : MonoBehaviour
    {
        private Vector3 direction;
        private float speed;
        private float damage;
        private bool hasHit;
        
        public void Initialize(Vector3 direction, float speed, float damage)
        {
            this.direction = direction;
            this.speed = speed;
            this.damage = damage;
            hasHit = false;
            
            // Destroy after 3 seconds
            Destroy(gameObject, 3f);
        }
        
        private void Update()
        {
            if (hasHit)
            {
                return;
            }
            
            // Move magic bolt
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
                
                hasHit = true;
                Destroy(gameObject);
            }
        }
    }
} 