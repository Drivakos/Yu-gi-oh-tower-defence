using UnityEngine;
using System.Collections.Generic;

namespace YuGiOh.MillenniumItems
{
    public class MillenniumItemPrefabManager : MonoBehaviour
    {
        [Header("Base Prefab")]
        [SerializeField] private GameObject baseItemPrefab;
        
        [Header("Item Models")]
        [SerializeField] private GameObject ringModel;
        [SerializeField] private GameObject puzzleModel;
        [SerializeField] private GameObject eyeModel;
        [SerializeField] private GameObject rodModel;
        [SerializeField] private GameObject keyModel;
        [SerializeField] private GameObject scaleModel;
        [SerializeField] private GameObject necklaceModel;
        
        [Header("Particle Effects")]
        [SerializeField] private GameObject activationParticlePrefab;
        [SerializeField] private GameObject deactivationParticlePrefab;
        
        [Header("Audio Clips")]
        [SerializeField] private AudioClip defaultActivationSound;
        [SerializeField] private AudioClip defaultDeactivationSound;
        
        private Dictionary<MillenniumItemType, GameObject> itemModels = new Dictionary<MillenniumItemType, GameObject>();
        
        private void Awake()
        {
            InitializeItemModels();
        }
        
        private void InitializeItemModels()
        {
            itemModels[MillenniumItemType.Ring] = ringModel;
            itemModels[MillenniumItemType.Puzzle] = puzzleModel;
            itemModels[MillenniumItemType.Eye] = eyeModel;
            itemModels[MillenniumItemType.Rod] = rodModel;
            itemModels[MillenniumItemType.Key] = keyModel;
            itemModels[MillenniumItemType.Scale] = scaleModel;
            itemModels[MillenniumItemType.Necklace] = necklaceModel;
        }
        
        public GameObject CreateItemPrefab(MillenniumItemType itemType)
        {
            if (baseItemPrefab == null)
            {
                Debug.LogError("Base item prefab is not assigned!");
                return null;
            }
            
            // Create a new instance of the base prefab
            GameObject itemPrefab = Instantiate(baseItemPrefab);
            itemPrefab.name = $"{itemType}Prefab";
            
            // Get the model for this item type
            GameObject modelPrefab = GetModelForItemType(itemType);
            if (modelPrefab != null)
            {
                // Instantiate the model as a child of the item prefab
                GameObject modelInstance = Instantiate(modelPrefab, itemPrefab.transform);
                modelInstance.name = "Model";
                
                // Position the model at the center of the item
                modelInstance.transform.localPosition = Vector3.zero;
                modelInstance.transform.localRotation = Quaternion.identity;
            }
            
            // Add the appropriate MillenniumItem component
            MillenniumItem itemComponent = GetItemComponentForType(itemType);
            if (itemComponent != null)
            {
                // Remove any existing MillenniumItem component
                MillenniumItem existingComponent = itemPrefab.GetComponent<MillenniumItem>();
                if (existingComponent != null)
                {
                    DestroyImmediate(existingComponent);
                }
                
                // Add the new component
                itemPrefab.AddComponent(itemComponent.GetType());
            }
            
            // Set up the renderer
            Renderer renderer = itemPrefab.GetComponentInChildren<Renderer>();
            if (renderer == null)
            {
                // Add a renderer if none exists
                renderer = itemPrefab.AddComponent<MeshRenderer>();
            }
            
            // Set up the audio source
            AudioSource audioSource = itemPrefab.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = itemPrefab.AddComponent<AudioSource>();
            }
            
            // Set up the particle system
            ParticleSystem particleSystem = itemPrefab.GetComponentInChildren<ParticleSystem>();
            if (particleSystem == null && activationParticlePrefab != null)
            {
                GameObject particleInstance = Instantiate(activationParticlePrefab, itemPrefab.transform);
                particleInstance.name = "ActivationParticles";
                particleInstance.transform.localPosition = Vector3.zero;
            }
            
            // Configure the item component
            MillenniumItem item = itemPrefab.GetComponent<MillenniumItem>();
            if (item != null)
            {
                // Set the item type
                var itemTypeField = item.GetType().GetField("itemType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (itemTypeField != null)
                {
                    itemTypeField.SetValue(item, itemType);
                }
                
                // Set the renderer
                var rendererField = item.GetType().GetField("itemRenderer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (rendererField != null)
                {
                    rendererField.SetValue(item, renderer);
                }
                
                // Set the audio source
                var audioSourceField = item.GetType().GetField("audioSource", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (audioSourceField != null)
                {
                    audioSourceField.SetValue(item, audioSource);
                }
                
                // Set the particle system
                var particleSystemField = item.GetType().GetField("activationParticles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (particleSystemField != null)
                {
                    particleSystemField.SetValue(item, itemPrefab.GetComponentInChildren<ParticleSystem>());
                }
                
                // Set the activation sound
                var activationSoundField = item.GetType().GetField("activationSound", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (activationSoundField != null)
                {
                    activationSoundField.SetValue(item, defaultActivationSound);
                }
                
                // Set the deactivation sound
                var deactivationSoundField = item.GetType().GetField("deactivationSound", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (deactivationSoundField != null)
                {
                    deactivationSoundField.SetValue(item, defaultDeactivationSound);
                }
            }
            
            return itemPrefab;
        }
        
        private GameObject GetModelForItemType(MillenniumItemType itemType)
        {
            if (itemModels.TryGetValue(itemType, out GameObject model))
            {
                return model;
            }
            return null;
        }
        
        private MillenniumItem GetItemComponentForType(MillenniumItemType itemType)
        {
            switch (itemType)
            {
                case MillenniumItemType.Ring:
                    return new MillenniumRing();
                case MillenniumItemType.Puzzle:
                    return new MillenniumPuzzle();
                case MillenniumItemType.Eye:
                    return new MillenniumEye();
                case MillenniumItemType.Rod:
                    return new MillenniumRod();
                case MillenniumItemType.Key:
                    return new MillenniumKey();
                case MillenniumItemType.Scale:
                    return new MillenniumScale();
                case MillenniumItemType.Necklace:
                    return new MillenniumNecklace();
                default:
                    return new MillenniumItem();
            }
        }
    }
} 