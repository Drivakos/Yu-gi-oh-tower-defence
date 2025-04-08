using UnityEngine;
using System.Collections;

namespace YuGiOh.MillenniumItems
{
    public class MillenniumItem : MonoBehaviour
    {
        [Header("Item Properties")]
        [SerializeField] private MillenniumItemType itemType;
        [SerializeField] private string itemName;
        [SerializeField] private string description;
        [SerializeField] private Sprite itemIcon;
        
        [Header("Visual Effects")]
        [SerializeField] private Renderer itemRenderer;
        [SerializeField] private ParticleSystem activationParticles;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip activationSound;
        [SerializeField] private AudioClip deactivationSound;
        
        [Header("Animation")]
        [SerializeField] private float hoverHeight = 0.2f;
        [SerializeField] private float hoverSpeed = 1.0f;
        [SerializeField] private float rotationSpeed = 30.0f;
        
        private Vector3 startPosition;
        private bool isActive = false;
        private MillenniumItemMaterials materialsManager;
        
        private void Awake()
        {
            startPosition = transform.position;
            materialsManager = FindObjectOfType<MillenniumItemMaterials>();
            
            if (itemRenderer == null)
            {
                itemRenderer = GetComponent<Renderer>();
            }
            
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }
        }
        
        private void Start()
        {
            ApplyMaterials();
            StartCoroutine(HoverAnimation());
            StartCoroutine(RotationAnimation());
        }
        
        private void ApplyMaterials()
        {
            if (materialsManager != null && itemRenderer != null)
            {
                // Apply the glow effect material
                Material glowMaterial = materialsManager.GetItemMaterial(itemType, "Glow");
                if (glowMaterial != null)
                {
                    itemRenderer.material = glowMaterial;
                }
                
                // Apply the specific effect material based on item type
                string effectType = GetEffectTypeForItem();
                Material effectMaterial = materialsManager.GetItemMaterial(itemType, effectType);
                if (effectMaterial != null)
                {
                    // Create a new material instance to avoid modifying the shared material
                    Material instanceMaterial = new Material(effectMaterial);
                    itemRenderer.material = instanceMaterial;
                }
            }
        }
        
        private string GetEffectTypeForItem()
        {
            return itemType switch
            {
                MillenniumItemType.Ring => "Outline",
                MillenniumItemType.Puzzle => "Shield",
                MillenniumItemType.Eye => "FutureVision",
                MillenniumItemType.Rod => "PowerLevel",
                MillenniumItemType.Key => "Shield",
                MillenniumItemType.Scale => "PowerLevel",
                MillenniumItemType.Necklace => "MindControl",
                _ => "Glow"
            };
        }
        
        private IEnumerator HoverAnimation()
        {
            float time = 0;
            
            while (true)
            {
                time += Time.deltaTime * hoverSpeed;
                float yOffset = Mathf.Sin(time) * hoverHeight;
                transform.position = startPosition + new Vector3(0, yOffset, 0);
                yield return null;
            }
        }
        
        private IEnumerator RotationAnimation()
        {
            while (true)
            {
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
                yield return null;
            }
        }
        
        public void Activate()
        {
            if (!isActive)
            {
                isActive = true;
                
                // Play activation particles
                if (activationParticles != null)
                {
                    activationParticles.Play();
                }
                
                // Play activation sound
                if (audioSource != null && activationSound != null)
                {
                    audioSource.clip = activationSound;
                    audioSource.Play();
                }
                
                // Apply active material
                ApplyActiveMaterial();
                
                // Notify any listeners
                OnItemActivated();
            }
        }
        
        public void Deactivate()
        {
            if (isActive)
            {
                isActive = false;
                
                // Play deactivation sound
                if (audioSource != null && deactivationSound != null)
                {
                    audioSource.clip = deactivationSound;
                    audioSource.Play();
                }
                
                // Apply inactive material
                ApplyInactiveMaterial();
                
                // Notify any listeners
                OnItemDeactivated();
            }
        }
        
        private void ApplyActiveMaterial()
        {
            if (materialsManager != null && itemRenderer != null)
            {
                // Increase emission intensity when active
                Material currentMaterial = itemRenderer.material;
                if (currentMaterial.HasProperty("_EmissionIntensity"))
                {
                    currentMaterial.SetFloat("_EmissionIntensity", currentMaterial.GetFloat("_EmissionIntensity") * 2.0f);
                }
            }
        }
        
        private void ApplyInactiveMaterial()
        {
            if (materialsManager != null && itemRenderer != null)
            {
                // Reset emission intensity when inactive
                Material currentMaterial = itemRenderer.material;
                if (currentMaterial.HasProperty("_EmissionIntensity"))
                {
                    currentMaterial.SetFloat("_EmissionIntensity", currentMaterial.GetFloat("_EmissionIntensity") * 0.5f);
                }
            }
        }
        
        protected virtual void OnItemActivated()
        {
            // Override in derived classes to implement specific activation behavior
        }
        
        protected virtual void OnItemDeactivated()
        {
            // Override in derived classes to implement specific deactivation behavior
        }
        
        public MillenniumItemType ItemType => itemType;
        public string ItemName => itemName;
        public string Description => description;
        public Sprite ItemIcon => itemIcon;
        public bool IsActive => isActive;
    }
} 