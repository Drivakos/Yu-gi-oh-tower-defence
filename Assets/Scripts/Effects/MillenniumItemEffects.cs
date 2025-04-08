using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using YuGiOhTowerDefense.Core;

namespace YuGiOhTowerDefense.Effects
{
    public class MillenniumItemEffects : MonoBehaviour
    {
        [System.Serializable]
        public class ItemEffectPrefab
        {
            public MillenniumItemType itemType;
            public GameObject activationEffect;
            public GameObject deactivationEffect;
            public GameObject collectionEffect;
            public GameObject activeStateEffect;
        }
        
        [Header("Effect Prefabs")]
        [SerializeField] private List<ItemEffectPrefab> itemEffectPrefabs = new List<ItemEffectPrefab>();
        [SerializeField] private GameObject defaultActivationEffect;
        [SerializeField] private GameObject defaultDeactivationEffect;
        [SerializeField] private GameObject defaultCollectionEffect;
        
        [Header("Effect Settings")]
        [SerializeField] private Transform effectContainer;
        [SerializeField] private float effectDuration = 2.0f;
        [SerializeField] private float effectScale = 1.5f;
        
        [Header("Active State Effects")]
        [SerializeField] private float activeStatePulseSpeed = 1.0f;
        [SerializeField] private float activeStatePulseIntensity = 0.2f;
        [SerializeField] private Color activeStateColor = new Color(1f, 0.8f, 0f, 0.5f);
        
        private Dictionary<MillenniumItemType, ItemEffectPrefab> effectPrefabMap = new Dictionary<MillenniumItemType, ItemEffectPrefab>();
        private Dictionary<MillenniumItemType, GameObject> activeEffects = new Dictionary<MillenniumItemType, GameObject>();
        private Dictionary<MillenniumItemType, Coroutine> pulseCoroutines = new Dictionary<MillenniumItemType, Coroutine>();
        
        private void Awake()
        {
            // Create effect container if it doesn't exist
            if (effectContainer == null)
            {
                GameObject container = new GameObject("MillenniumItemEffects");
                container.transform.SetParent(transform);
                effectContainer = container.transform;
            }
            
            // Build effect prefab map
            foreach (var effect in itemEffectPrefabs)
            {
                effectPrefabMap[effect.itemType] = effect;
            }
        }
        
        public void PlayActivationEffect(MillenniumItemType itemType, Vector3 position)
        {
            GameObject effectPrefab = GetEffectPrefab(itemType, true);
            if (effectPrefab != null)
            {
                GameObject effect = Instantiate(effectPrefab, position, Quaternion.identity, effectContainer);
                effect.transform.localScale = Vector3.one * effectScale;
                Destroy(effect, effectDuration);
            }
        }
        
        public void PlayDeactivationEffect(MillenniumItemType itemType, Vector3 position)
        {
            GameObject effectPrefab = GetEffectPrefab(itemType, false);
            if (effectPrefab != null)
            {
                GameObject effect = Instantiate(effectPrefab, position, Quaternion.identity, effectContainer);
                effect.transform.localScale = Vector3.one * effectScale;
                Destroy(effect, effectDuration);
            }
        }
        
        public void PlayCollectionEffect(MillenniumItemType itemType, Vector3 position)
        {
            GameObject effectPrefab = GetCollectionEffectPrefab(itemType);
            if (effectPrefab != null)
            {
                GameObject effect = Instantiate(effectPrefab, position, Quaternion.identity, effectContainer);
                effect.transform.localScale = Vector3.one * effectScale;
                Destroy(effect, effectDuration);
            }
        }
        
        public void StartActiveStateEffect(MillenniumItemType itemType, GameObject target)
        {
            // Stop any existing active state effect
            StopActiveStateEffect(itemType);
            
            // Get the active state effect prefab
            if (effectPrefabMap.TryGetValue(itemType, out ItemEffectPrefab effectPrefab) && 
                effectPrefab.activeStateEffect != null)
            {
                GameObject effect = Instantiate(effectPrefab.activeStateEffect, target.transform.position, 
                                              Quaternion.identity, target.transform);
                effect.transform.localScale = Vector3.one * effectScale;
                
                activeEffects[itemType] = effect;
                
                // Start pulsing effect
                if (pulseCoroutines.ContainsKey(itemType) && pulseCoroutines[itemType] != null)
                {
                    StopCoroutine(pulseCoroutines[itemType]);
                }
                
                pulseCoroutines[itemType] = StartCoroutine(PulseActiveEffect(effect));
            }
        }
        
        public void StopActiveStateEffect(MillenniumItemType itemType)
        {
            // Stop pulsing coroutine
            if (pulseCoroutines.TryGetValue(itemType, out Coroutine coroutine) && coroutine != null)
            {
                StopCoroutine(coroutine);
                pulseCoroutines[itemType] = null;
            }
            
            // Destroy active effect
            if (activeEffects.TryGetValue(itemType, out GameObject effect) && effect != null)
            {
                Destroy(effect);
                activeEffects.Remove(itemType);
            }
        }
        
        private GameObject GetEffectPrefab(MillenniumItemType itemType, bool isActivation)
        {
            if (effectPrefabMap.TryGetValue(itemType, out ItemEffectPrefab effect))
            {
                return isActivation ? effect.activationEffect : effect.deactivationEffect;
            }
            
            return isActivation ? defaultActivationEffect : defaultDeactivationEffect;
        }
        
        private GameObject GetCollectionEffectPrefab(MillenniumItemType itemType)
        {
            if (effectPrefabMap.TryGetValue(itemType, out ItemEffectPrefab effect))
            {
                return effect.collectionEffect;
            }
            
            return defaultCollectionEffect;
        }
        
        private IEnumerator PulseActiveEffect(GameObject effect)
        {
            Renderer[] renderers = effect.GetComponentsInChildren<Renderer>();
            Material[] materials = new Material[renderers.Length];
            
            // Store original colors
            Color[] originalColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                materials[i] = renderers[i].material;
                originalColors[i] = materials[i].color;
            }
            
            float time = 0f;
            
            while (true)
            {
                time += Time.deltaTime * activeStatePulseSpeed;
                float pulseValue = Mathf.Sin(time) * activeStatePulseIntensity;
                
                for (int i = 0; i < renderers.Length; i++)
                {
                    if (materials[i] != null)
                    {
                        materials[i].color = Color.Lerp(originalColors[i], activeStateColor, pulseValue);
                    }
                }
                
                yield return null;
            }
        }
        
        private void OnDestroy()
        {
            // Clean up all active effects
            foreach (var effect in activeEffects.Values)
            {
                if (effect != null)
                {
                    Destroy(effect);
                }
            }
            
            activeEffects.Clear();
            
            // Stop all coroutines
            foreach (var coroutine in pulseCoroutines.Values)
            {
                if (coroutine != null)
                {
                    StopCoroutine(coroutine);
                }
            }
            
            pulseCoroutines.Clear();
        }
    }
} 