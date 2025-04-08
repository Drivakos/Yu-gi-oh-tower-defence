using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace YuGiOh.Cards
{
    public class FiendBehavior : MonsterTypeBehavior
    {
        [Header("Fiend Special Ability")]
        [SerializeField] private float darkEnergyCost = 25f;
        [SerializeField] private float darkEnergyDamage = 50f;
        [SerializeField] private float darkEnergyRadius = 5f;
        [SerializeField] private GameObject darkEnergyPrefab;
        [SerializeField] private GameObject darkEnergyEffectPrefab;
        [SerializeField] private AudioClip darkEnergySound;
        
        [Header("Sacrifice Mechanics")]
        [SerializeField] private float sacrificeRange = 10f;
        [SerializeField] private float sacrificeHealthBonus = 100f;
        [SerializeField] private float sacrificeDamageBonus = 20f;
        [SerializeField] private GameObject sacrificeEffectPrefab;
        [SerializeField] private AudioClip sacrificeSound;
        
        private float darkEnergyPool = 100f;
        private float maxDarkEnergyPool = 100f;
        private float darkEnergyRegenRate = 5f;
        private List<GameObject> sacrificedMonsters = new List<GameObject>();
        
        protected override void Start()
        {
            base.Start();
            StartCoroutine(RegenerateDarkEnergy());
        }
        
        protected override void ApplySpecialAbilityEffects()
        {
            // Fiend's special ability: Dark Energy Burst
            if (darkEnergyPool >= darkEnergyCost)
            {
                CastDarkEnergyBurst();
                darkEnergyPool -= darkEnergyCost;
            }
        }
        
        protected override void RemoveSpecialAbilityEffects()
        {
            // No cleanup needed for dark energy burst
        }
        
        private void CastDarkEnergyBurst()
        {
            if (darkEnergyPrefab == null)
            {
                Debug.LogError("Dark energy prefab not assigned to FiendBehavior");
                return;
            }
            
            // Get target position
            Transform target = monsterCard.GetCurrentTarget();
            if (target == null)
            {
                return;
            }
            
            // Spawn dark energy at target position
            GameObject darkEnergy = Instantiate(darkEnergyPrefab, target.position, Quaternion.identity);
            
            // Set dark energy properties
            DarkEnergy energyComponent = darkEnergy.GetComponent<DarkEnergy>();
            if (energyComponent != null)
            {
                energyComponent.Initialize(darkEnergyRadius, darkEnergyDamage);
            }
            
            // Spawn effect
            if (darkEnergyEffectPrefab != null)
            {
                GameObject effect = Instantiate(darkEnergyEffectPrefab, target.position, Quaternion.identity, darkEnergy.transform);
                Destroy(effect, 2f);
            }
            
            // Play sound
            if (darkEnergySound != null)
            {
                AudioSource.PlayClipAtPoint(darkEnergySound, target.position);
            }
        }
        
        private IEnumerator RegenerateDarkEnergy()
        {
            while (true)
            {
                if (darkEnergyPool < maxDarkEnergyPool)
                {
                    darkEnergyPool = Mathf.Min(darkEnergyPool + darkEnergyRegenRate * Time.deltaTime, maxDarkEnergyPool);
                }
                yield return null;
            }
        }
        
        public override void OnMonsterSpawned()
        {
            base.OnMonsterSpawned();
            
            // Check for potential sacrifices
            CheckForSacrifices();
        }
        
        public override void OnMonsterDefeated()
        {
            base.OnMonsterDefeated();
            
            // Release sacrificed monsters
            ReleaseSacrifices();
        }
        
        private void CheckForSacrifices()
        {
            // Find all friendly monsters in range
            Collider[] colliders = Physics.OverlapSphere(transform.position, sacrificeRange);
            
            foreach (Collider collider in colliders)
            {
                MonsterCard otherMonster = collider.GetComponent<MonsterCard>();
                if (otherMonster != null && otherMonster != monsterCard && !otherMonster.IsEnemy())
                {
                    // Check if monster is already sacrificed
                    if (!sacrificedMonsters.Contains(otherMonster.gameObject))
                    {
                        // Apply sacrifice effect
                        ApplySacrificeEffect(otherMonster);
                        
                        // Add to sacrificed list
                        sacrificedMonsters.Add(otherMonster.gameObject);
                        
                        // Spawn sacrifice effect
                        if (sacrificeEffectPrefab != null)
                        {
                            GameObject effect = Instantiate(sacrificeEffectPrefab, otherMonster.transform.position, Quaternion.identity, otherMonster.transform);
                            Destroy(effect, 2f);
                        }
                        
                        // Play sound
                        if (sacrificeSound != null)
                        {
                            AudioSource.PlayClipAtPoint(sacrificeSound, otherMonster.transform.position);
                        }
                    }
                }
            }
        }
        
        private void ApplySacrificeEffect(MonsterCard sacrificedMonster)
        {
            // Apply health and damage bonuses to the Fiend
            monsterCard.AddHealthBonus(sacrificeHealthBonus);
            monsterCard.AddDamageBonus(sacrificeDamageBonus);
            
            // Disable the sacrificed monster
            sacrificedMonster.gameObject.SetActive(false);
        }
        
        private void ReleaseSacrifices()
        {
            foreach (GameObject sacrificedMonster in sacrificedMonsters)
            {
                if (sacrificedMonster != null)
                {
                    // Re-enable the sacrificed monster
                    sacrificedMonster.SetActive(true);
                    
                    // Remove health and damage bonuses from the Fiend
                    monsterCard.RemoveHealthBonus(sacrificeHealthBonus);
                    monsterCard.RemoveDamageBonus(sacrificeDamageBonus);
                }
            }
            
            // Clear sacrificed list
            sacrificedMonsters.Clear();
        }
    }
    
    public class DarkEnergy : MonoBehaviour
    {
        private float radius;
        private float damage;
        private bool hasExploded;
        
        public void Initialize(float radius, float damage)
        {
            this.radius = radius;
            this.damage = damage;
            hasExploded = false;
            
            // Destroy after 1 second
            Destroy(gameObject, 1f);
        }
        
        private void Update()
        {
            if (hasExploded)
            {
                return;
            }
            
            // Check for enemies in radius
            Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
            
            foreach (Collider collider in colliders)
            {
                IDamageable damageable = collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    // Apply damage
                    damageable.TakeDamage(Mathf.RoundToInt(damage));
                }
            }
            
            hasExploded = true;
        }
    }
} 