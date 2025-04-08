using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace YuGiOh.MillenniumItems
{
    public class MillenniumEye : MillenniumItem
    {
        [Header("Eye Specific Properties")]
        [SerializeField] private float visionRange = 15f;
        [SerializeField] private float visionAngle = 90f;
        [SerializeField] private LayerMask visionLayerMask;
        [SerializeField] private GameObject visionEffectPrefab;
        [SerializeField] private AudioClip visionSound;
        [SerializeField] private Material visionMaterial;
        
        private List<GameObject> visibleEntities = new List<GameObject>();
        private Coroutine visionCoroutine;
        private Camera mainCamera;
        
        protected override void OnItemActivated()
        {
            base.OnItemActivated();
            
            // Get the main camera
            mainCamera = Camera.main;
            
            // Start the vision coroutine
            if (visionCoroutine == null)
            {
                visionCoroutine = StartCoroutine(UpdateVision());
            }
        }
        
        protected override void OnItemDeactivated()
        {
            base.OnItemDeactivated();
            
            // Stop the vision coroutine
            if (visionCoroutine != null)
            {
                StopCoroutine(visionCoroutine);
                visionCoroutine = null;
            }
            
            // Clear visible entities
            ClearVisibleEntities();
        }
        
        private IEnumerator UpdateVision()
        {
            while (true)
            {
                // Find all entities within range
                Collider[] colliders = Physics.OverlapSphere(transform.position, visionRange, visionLayerMask);
                
                // Clear previous visible entities
                ClearVisibleEntities();
                
                foreach (Collider collider in colliders)
                {
                    // Check if entity is within vision angle
                    Vector3 directionToEntity = (collider.transform.position - transform.position).normalized;
                    float angle = Vector3.Angle(transform.forward, directionToEntity);
                    
                    if (angle <= visionAngle * 0.5f)
                    {
                        // Check if entity is visible (not blocked by obstacles)
                        RaycastHit hit;
                        if (Physics.Raycast(transform.position, directionToEntity, out hit, visionRange, visionLayerMask))
                        {
                            if (hit.collider.gameObject == collider.gameObject)
                            {
                                // Entity is visible
                                AddVisibleEntity(collider.gameObject);
                            }
                        }
                    }
                }
                
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        private void AddVisibleEntity(GameObject entity)
        {
            // Add to visible list
            visibleEntities.Add(entity);
            
            // Apply vision effect
            IVisible visible = entity.GetComponent<IVisible>();
            if (visible != null)
            {
                visible.OnBecameVisible();
                
                // Apply vision material if available
                Renderer renderer = entity.GetComponent<Renderer>();
                if (renderer != null && visionMaterial != null)
                {
                    renderer.material = visionMaterial;
                }
            }
            
            // Play vision effect
            if (visionEffectPrefab != null)
            {
                GameObject effect = Instantiate(visionEffectPrefab, entity.transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }
            
            // Play vision sound
            if (visionSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(visionSound);
            }
        }
        
        private void ClearVisibleEntities()
        {
            foreach (GameObject entity in visibleEntities)
            {
                if (entity != null)
                {
                    // Remove vision effect
                    IVisible visible = entity.GetComponent<IVisible>();
                    if (visible != null)
                    {
                        visible.OnBecameInvisible();
                    }
                }
            }
            
            visibleEntities.Clear();
        }
        
        public List<GameObject> GetVisibleEntities()
        {
            return new List<GameObject>(visibleEntities);
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw vision range and angle in the editor
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, visionRange);
            
            // Draw vision angle
            Vector3 rightDir = Quaternion.Euler(0, visionAngle * 0.5f, 0) * transform.forward;
            Vector3 leftDir = Quaternion.Euler(0, -visionAngle * 0.5f, 0) * transform.forward;
            
            Gizmos.DrawRay(transform.position, rightDir * visionRange);
            Gizmos.DrawRay(transform.position, leftDir * visionRange);
        }
    }
    
    public interface IVisible
    {
        void OnBecameVisible();
        void OnBecameInvisible();
    }
} 