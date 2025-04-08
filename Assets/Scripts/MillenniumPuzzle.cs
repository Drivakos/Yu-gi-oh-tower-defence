using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace YuGiOh.MillenniumItems
{
    public class MillenniumPuzzle : MillenniumItem
    {
        [Header("Puzzle Specific Properties")]
        [SerializeField] private float summonRange = 10f;
        [SerializeField] private float summonDuration = 5f;
        [SerializeField] private LayerMask summonableLayerMask;
        [SerializeField] private GameObject summonEffectPrefab;
        [SerializeField] private AudioClip summonSound;
        [SerializeField] private AudioClip desummonSound;
        
        private List<GameObject> summonedEntities = new List<GameObject>();
        private Coroutine summonCoroutine;
        
        protected override void OnItemActivated()
        {
            base.OnItemActivated();
            
            // Start the summon coroutine
            if (summonCoroutine == null)
            {
                summonCoroutine = StartCoroutine(SummonEntities());
            }
        }
        
        protected override void OnItemDeactivated()
        {
            base.OnItemDeactivated();
            
            // Stop the summon coroutine
            if (summonCoroutine != null)
            {
                StopCoroutine(summonCoroutine);
                summonCoroutine = null;
            }
            
            // Desummon all entities
            DesummonAllEntities();
        }
        
        private IEnumerator SummonEntities()
        {
            while (true)
            {
                // Find all summonable entities within range
                Collider[] colliders = Physics.OverlapSphere(transform.position, summonRange, summonableLayerMask);
                
                foreach (Collider collider in colliders)
                {
                    // Skip if already summoned
                    if (summonedEntities.Contains(collider.gameObject))
                    {
                        continue;
                    }
                    
                    // Summon the entity
                    SummonEntity(collider.gameObject);
                }
                
                // Wait for the next summon cycle
                yield return new WaitForSeconds(summonDuration);
            }
        }
        
        private void SummonEntity(GameObject entity)
        {
            // Add to summoned list
            summonedEntities.Add(entity);
            
            // Play summon effect
            if (summonEffectPrefab != null)
            {
                GameObject effect = Instantiate(summonEffectPrefab, entity.transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }
            
            // Play summon sound
            if (summonSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(summonSound);
            }
            
            // Apply summon effects to the entity
            ISummonable summonable = entity.GetComponent<ISummonable>();
            if (summonable != null)
            {
                summonable.OnSummoned();
            }
        }
        
        private void DesummonAllEntities()
        {
            foreach (GameObject entity in summonedEntities)
            {
                if (entity != null)
                {
                    // Play desummon effect
                    if (summonEffectPrefab != null)
                    {
                        GameObject effect = Instantiate(summonEffectPrefab, entity.transform.position, Quaternion.identity);
                        Destroy(effect, 2f);
                    }
                    
                    // Play desummon sound
                    if (desummonSound != null && audioSource != null)
                    {
                        audioSource.PlayOneShot(desummonSound);
                    }
                    
                    // Apply desummon effects to the entity
                    ISummonable summonable = entity.GetComponent<ISummonable>();
                    if (summonable != null)
                    {
                        summonable.OnDesummoned();
                    }
                }
            }
            
            summonedEntities.Clear();
        }
        
        public List<GameObject> GetSummonedEntities()
        {
            return new List<GameObject>(summonedEntities);
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw summon range in the editor
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, summonRange);
        }
    }
    
    public interface ISummonable
    {
        void OnSummoned();
        void OnDesummoned();
    }
} 