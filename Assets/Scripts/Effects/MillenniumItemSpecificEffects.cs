using UnityEngine;
using System.Collections;
using YuGiOhTowerDefense.Core;

namespace YuGiOhTowerDefense.Effects
{
    public class MillenniumItemSpecificEffects : MonoBehaviour
    {
        [Header("Ring Effects")]
        [SerializeField] private LineRenderer[] ringPointers;
        [SerializeField] private float pointerRotationSpeed = 120f;
        [SerializeField] private float pointerPulseSpeed = 2f;
        [SerializeField] private Color pointerActiveColor = Color.yellow;
        
        [Header("Puzzle Effects")]
        [SerializeField] private ParticleSystem puzzleAura;
        [SerializeField] private Material puzzleGlowMaterial;
        [SerializeField] private float glowIntensity = 2f;
        
        [Header("Eye Effects")]
        [SerializeField] private GameObject enemyHighlightPrefab;
        [SerializeField] private Material enemyOutlineMaterial;
        [SerializeField] private float outlineWidth = 0.2f;
        
        [Header("Rod Effects")]
        [SerializeField] private ParticleSystem mindControlBeam;
        [SerializeField] private Material controlledEnemyMaterial;
        [SerializeField] private float controlPulseSpeed = 1.5f;
        
        [Header("Key Effects")]
        [SerializeField] private GameObject shieldBubblePrefab;
        [SerializeField] private Material shieldMaterial;
        [SerializeField] private float shieldExpandSpeed = 3f;
        
        [Header("Scale Effects")]
        [SerializeField] private GameObject powerLevelIndicatorPrefab;
        [SerializeField] private Material powerLevelMaterial;
        [SerializeField] private Gradient powerLevelGradient;
        
        [Header("Necklace Effects")]
        [SerializeField] private Material futureVisionMaterial;
        [SerializeField] private float visionFadeSpeed = 1f;
        [SerializeField] private ParticleSystem timeflowParticles;
        
        private Coroutine[] activeEffectCoroutines;
        private GameObject[] activeEffectObjects;
        
        private void Awake()
        {
            activeEffectCoroutines = new Coroutine[System.Enum.GetValues(typeof(MillenniumItemType)).Length];
            activeEffectObjects = new GameObject[System.Enum.GetValues(typeof(MillenniumItemType)).Length];
        }
        
        public void PlayItemSpecificEffect(MillenniumItemType itemType, GameObject target)
        {
            int index = (int)itemType;
            
            // Stop any existing effect for this item
            if (activeEffectCoroutines[index] != null)
            {
                StopCoroutine(activeEffectCoroutines[index]);
                activeEffectCoroutines[index] = null;
            }
            
            if (activeEffectObjects[index] != null)
            {
                Destroy(activeEffectObjects[index]);
                activeEffectObjects[index] = null;
            }
            
            // Start new effect based on item type
            switch (itemType)
            {
                case MillenniumItemType.Ring:
                    activeEffectCoroutines[index] = StartCoroutine(PlayRingEffect(target));
                    break;
                    
                case MillenniumItemType.Puzzle:
                    activeEffectCoroutines[index] = StartCoroutine(PlayPuzzleEffect(target));
                    break;
                    
                case MillenniumItemType.Eye:
                    activeEffectCoroutines[index] = StartCoroutine(PlayEyeEffect(target));
                    break;
                    
                case MillenniumItemType.Rod:
                    activeEffectCoroutines[index] = StartCoroutine(PlayRodEffect(target));
                    break;
                    
                case MillenniumItemType.Key:
                    activeEffectCoroutines[index] = StartCoroutine(PlayKeyEffect(target));
                    break;
                    
                case MillenniumItemType.Scale:
                    activeEffectCoroutines[index] = StartCoroutine(PlayScaleEffect(target));
                    break;
                    
                case MillenniumItemType.Necklace:
                    activeEffectCoroutines[index] = StartCoroutine(PlayNecklaceEffect(target));
                    break;
            }
        }
        
        private IEnumerator PlayRingEffect(GameObject target)
        {
            // Activate and rotate ring pointers
            foreach (var pointer in ringPointers)
            {
                pointer.gameObject.SetActive(true);
                pointer.startColor = pointerActiveColor;
                pointer.endColor = pointerActiveColor;
            }
            
            float time = 0f;
            while (true)
            {
                time += Time.deltaTime;
                
                // Rotate pointers
                float rotation = time * pointerRotationSpeed;
                transform.rotation = Quaternion.Euler(0f, rotation, 0f);
                
                // Pulse color intensity
                float pulse = (Mathf.Sin(time * pointerPulseSpeed) + 1f) * 0.5f;
                Color pulseColor = Color.Lerp(pointerActiveColor * 0.5f, pointerActiveColor, pulse);
                
                foreach (var pointer in ringPointers)
                {
                    pointer.startColor = pulseColor;
                    pointer.endColor = pulseColor;
                }
                
                yield return null;
            }
        }
        
        private IEnumerator PlayPuzzleEffect(GameObject target)
        {
            // Start puzzle aura
            if (puzzleAura != null)
            {
                puzzleAura.gameObject.SetActive(true);
                puzzleAura.Play();
            }
            
            // Apply glow effect
            if (puzzleGlowMaterial != null && target != null)
            {
                Renderer renderer = target.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material originalMaterial = renderer.material;
                    renderer.material = puzzleGlowMaterial;
                    
                    float time = 0f;
                    while (true)
                    {
                        time += Time.deltaTime;
                        float intensity = (Mathf.Sin(time * 2f) + 1f) * 0.5f * glowIntensity;
                        puzzleGlowMaterial.SetFloat("_EmissionIntensity", intensity);
                        yield return null;
                    }
                }
            }
        }
        
        private IEnumerator PlayEyeEffect(GameObject target)
        {
            if (target != null && enemyHighlightPrefab != null)
            {
                // Create highlight effect
                GameObject highlight = Instantiate(enemyHighlightPrefab, target.transform.position, Quaternion.identity);
                highlight.transform.SetParent(target.transform);
                activeEffectObjects[(int)MillenniumItemType.Eye] = highlight;
                
                // Apply outline effect
                if (enemyOutlineMaterial != null)
                {
                    float time = 0f;
                    while (true)
                    {
                        time += Time.deltaTime;
                        float width = (Mathf.Sin(time * 2f) + 1f) * 0.5f * outlineWidth;
                        enemyOutlineMaterial.SetFloat("_OutlineWidth", width);
                        yield return null;
                    }
                }
            }
        }
        
        private IEnumerator PlayRodEffect(GameObject target)
        {
            if (target != null)
            {
                // Start mind control beam
                if (mindControlBeam != null)
                {
                    mindControlBeam.gameObject.SetActive(true);
                    mindControlBeam.Play();
                }
                
                // Apply control effect material
                if (controlledEnemyMaterial != null)
                {
                    Renderer renderer = target.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        Material originalMaterial = renderer.material;
                        renderer.material = controlledEnemyMaterial;
                        
                        float time = 0f;
                        while (true)
                        {
                            time += Time.deltaTime;
                            float pulse = (Mathf.Sin(time * controlPulseSpeed) + 1f) * 0.5f;
                            controlledEnemyMaterial.SetFloat("_ControlPulse", pulse);
                            yield return null;
                        }
                    }
                }
            }
        }
        
        private IEnumerator PlayKeyEffect(GameObject target)
        {
            if (shieldBubblePrefab != null)
            {
                // Create shield bubble
                GameObject shield = Instantiate(shieldBubblePrefab, transform.position, Quaternion.identity);
                activeEffectObjects[(int)MillenniumItemType.Key] = shield;
                
                // Expand shield
                float scale = 0f;
                while (scale < 1f)
                {
                    scale += Time.deltaTime * shieldExpandSpeed;
                    shield.transform.localScale = Vector3.one * scale;
                    yield return null;
                }
                
                // Maintain shield
                if (shieldMaterial != null)
                {
                    float time = 0f;
                    while (true)
                    {
                        time += Time.deltaTime;
                        float pulse = (Mathf.Sin(time * 2f) + 1f) * 0.5f;
                        shieldMaterial.SetFloat("_ShieldPulse", pulse);
                        yield return null;
                    }
                }
            }
        }
        
        private IEnumerator PlayScaleEffect(GameObject target)
        {
            if (target != null && powerLevelIndicatorPrefab != null)
            {
                // Create power level indicator
                GameObject indicator = Instantiate(powerLevelIndicatorPrefab, target.transform.position + Vector3.up * 2f, 
                                                Quaternion.identity);
                indicator.transform.SetParent(target.transform);
                activeEffectObjects[(int)MillenniumItemType.Scale] = indicator;
                
                // Update power level display
                if (powerLevelMaterial != null)
                {
                    float time = 0f;
                    while (true)
                    {
                        time += Time.deltaTime;
                        float level = (Mathf.Sin(time) + 1f) * 0.5f;
                        powerLevelMaterial.SetColor("_PowerColor", powerLevelGradient.Evaluate(level));
                        yield return null;
                    }
                }
            }
        }
        
        private IEnumerator PlayNecklaceEffect(GameObject target)
        {
            // Start timeflow particles
            if (timeflowParticles != null)
            {
                timeflowParticles.gameObject.SetActive(true);
                timeflowParticles.Play();
            }
            
            // Apply future vision effect
            if (futureVisionMaterial != null)
            {
                float alpha = 0f;
                while (alpha < 1f)
                {
                    alpha += Time.deltaTime * visionFadeSpeed;
                    futureVisionMaterial.SetFloat("_VisionAlpha", alpha);
                    yield return null;
                }
                
                while (true)
                {
                    float time = Time.time;
                    float distortion = Mathf.Sin(time * 2f) * 0.1f;
                    futureVisionMaterial.SetFloat("_TimeDistortion", distortion);
                    yield return null;
                }
            }
        }
        
        private void OnDestroy()
        {
            // Clean up all active effects
            for (int i = 0; i < activeEffectCoroutines.Length; i++)
            {
                if (activeEffectCoroutines[i] != null)
                {
                    StopCoroutine(activeEffectCoroutines[i]);
                }
                
                if (activeEffectObjects[i] != null)
                {
                    Destroy(activeEffectObjects[i]);
                }
            }
        }
    }
} 