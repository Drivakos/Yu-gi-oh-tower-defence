using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace YuGiOh.MillenniumItems
{
    public class MillenniumRing : MillenniumItem
    {
        [Header("Ring Specific Properties")]
        [SerializeField] private float detectionRange = 15f;
        [SerializeField] private float pulseInterval = 2f;
        [SerializeField] private LayerMask itemLayerMask;
        [SerializeField] private GameObject detectionEffectPrefab;
        [SerializeField] private AudioClip detectionSound;
        
        private List<MillenniumItem> detectedItems = new List<MillenniumItem>();
        private Coroutine detectionCoroutine;
        
        protected override void OnItemActivated()
        {
            base.OnItemActivated();
            
            // Start the detection coroutine
            if (detectionCoroutine == null)
            {
                detectionCoroutine = StartCoroutine(DetectItems());
            }
        }
        
        protected override void OnItemDeactivated()
        {
            base.OnItemDeactivated();
            
            // Stop the detection coroutine
            if (detectionCoroutine != null)
            {
                StopCoroutine(detectionCoroutine);
                detectionCoroutine = null;
            }
            
            // Clear detected items
            detectedItems.Clear();
        }
        
        private IEnumerator DetectItems()
        {
            while (true)
            {
                // Find all Millennium Items within detection range
                Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRange, itemLayerMask);
                
                // Clear previous detections
                detectedItems.Clear();
                
                foreach (Collider collider in colliders)
                {
                    // Skip this item
                    if (collider.gameObject == gameObject)
                    {
                        continue;
                    }
                    
                    // Check if the object has a MillenniumItem component
                    MillenniumItem item = collider.GetComponent<MillenniumItem>();
                    if (item != null)
                    {
                        detectedItems.Add(item);
                        
                        // Play detection effect
                        PlayDetectionEffect(item.transform.position);
                    }
                }
                
                // Wait for the next pulse
                yield return new WaitForSeconds(pulseInterval);
            }
        }
        
        private void PlayDetectionEffect(Vector3 targetPosition)
        {
            // Create detection effect
            if (detectionEffectPrefab != null)
            {
                GameObject effect = Instantiate(detectionEffectPrefab, targetPosition, Quaternion.identity);
                Destroy(effect, 2f);
            }
            
            // Play detection sound
            if (detectionSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(detectionSound);
            }
        }
        
        public List<MillenniumItem> GetDetectedItems()
        {
            return new List<MillenniumItem>(detectedItems);
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw detection range in the editor
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
        }
    }
} 