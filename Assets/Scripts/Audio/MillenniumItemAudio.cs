using UnityEngine;
using System.Collections.Generic;
using YuGiOhTowerDefense.Core;

namespace YuGiOhTowerDefense.Audio
{
    public class MillenniumItemAudio : MonoBehaviour
    {
        [System.Serializable]
        public class ItemSoundEffect
        {
            public MillenniumItemType itemType;
            public AudioClip activationSound;
            public AudioClip deactivationSound;
            public AudioClip collectionSound;
            public AudioClip cooldownReadySound;
        }
        
        [Header("Audio Sources")]
        [SerializeField] private AudioSource itemAudioSource;
        [SerializeField] private AudioSource ambientAudioSource;
        
        [Header("Sound Effects")]
        [SerializeField] private List<ItemSoundEffect> itemSoundEffects = new List<ItemSoundEffect>();
        [SerializeField] private AudioClip defaultActivationSound;
        [SerializeField] private AudioClip defaultDeactivationSound;
        [SerializeField] private AudioClip defaultCollectionSound;
        [SerializeField] private AudioClip defaultCooldownReadySound;
        
        [Header("Ambient Sounds")]
        [SerializeField] private AudioClip shadowRealmAmbience;
        [SerializeField] private float ambientVolume = 0.3f;
        [SerializeField] private float ambientFadeInDuration = 2.0f;
        [SerializeField] private float ambientFadeOutDuration = 1.5f;
        
        private Dictionary<MillenniumItemType, ItemSoundEffect> soundEffectMap = new Dictionary<MillenniumItemType, ItemSoundEffect>();
        private Coroutine ambientFadeCoroutine;
        
        private void Awake()
        {
            // Create audio sources if they don't exist
            if (itemAudioSource == null)
            {
                GameObject audioObj = new GameObject("ItemAudioSource");
                audioObj.transform.SetParent(transform);
                itemAudioSource = audioObj.AddComponent<AudioSource>();
                itemAudioSource.playOnAwake = false;
            }
            
            if (ambientAudioSource == null)
            {
                GameObject ambientObj = new GameObject("AmbientAudioSource");
                ambientObj.transform.SetParent(transform);
                ambientAudioSource = ambientObj.AddComponent<AudioSource>();
                ambientAudioSource.playOnAwake = false;
                ambientAudioSource.loop = true;
                ambientAudioSource.volume = 0f;
            }
            
            // Build sound effect map
            foreach (var effect in itemSoundEffects)
            {
                soundEffectMap[effect.itemType] = effect;
            }
        }
        
        public void PlayItemActivation(MillenniumItemType itemType)
        {
            AudioClip clip = GetSoundEffect(itemType, true);
            if (clip != null)
            {
                itemAudioSource.PlayOneShot(clip);
            }
        }
        
        public void PlayItemDeactivation(MillenniumItemType itemType)
        {
            AudioClip clip = GetSoundEffect(itemType, false);
            if (clip != null)
            {
                itemAudioSource.PlayOneShot(clip);
            }
        }
        
        public void PlayItemCollection(MillenniumItemType itemType)
        {
            AudioClip clip = GetCollectionSound(itemType);
            if (clip != null)
            {
                itemAudioSource.PlayOneShot(clip);
            }
        }
        
        public void PlayCooldownReady(MillenniumItemType itemType)
        {
            AudioClip clip = GetCooldownReadySound(itemType);
            if (clip != null)
            {
                itemAudioSource.PlayOneShot(clip);
            }
        }
        
        private AudioClip GetSoundEffect(MillenniumItemType itemType, bool isActivation)
        {
            if (soundEffectMap.TryGetValue(itemType, out ItemSoundEffect effect))
            {
                return isActivation ? effect.activationSound : effect.deactivationSound;
            }
            
            return isActivation ? defaultActivationSound : defaultDeactivationSound;
        }
        
        private AudioClip GetCollectionSound(MillenniumItemType itemType)
        {
            if (soundEffectMap.TryGetValue(itemType, out ItemSoundEffect effect))
            {
                return effect.collectionSound;
            }
            
            return defaultCollectionSound;
        }
        
        private AudioClip GetCooldownReadySound(MillenniumItemType itemType)
        {
            if (soundEffectMap.TryGetValue(itemType, out ItemSoundEffect effect))
            {
                return effect.cooldownReadySound;
            }
            
            return defaultCooldownReadySound;
        }
        
        public void StartAmbientSound()
        {
            if (ambientAudioSource.clip != shadowRealmAmbience)
            {
                ambientAudioSource.clip = shadowRealmAmbience;
            }
            
            if (!ambientAudioSource.isPlaying)
            {
                ambientAudioSource.Play();
            }
            
            if (ambientFadeCoroutine != null)
            {
                StopCoroutine(ambientFadeCoroutine);
            }
            
            ambientFadeCoroutine = StartCoroutine(FadeAmbientSound(0f, ambientVolume, ambientFadeInDuration));
        }
        
        public void StopAmbientSound()
        {
            if (ambientFadeCoroutine != null)
            {
                StopCoroutine(ambientFadeCoroutine);
            }
            
            ambientFadeCoroutine = StartCoroutine(FadeAmbientSound(ambientAudioSource.volume, 0f, ambientFadeOutDuration));
        }
        
        private System.Collections.IEnumerator FadeAmbientSound(float startVolume, float targetVolume, float duration)
        {
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                ambientAudioSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
                yield return null;
            }
            
            ambientAudioSource.volume = targetVolume;
            
            if (targetVolume == 0f && ambientAudioSource.isPlaying)
            {
                ambientAudioSource.Stop();
            }
            
            ambientFadeCoroutine = null;
        }
    }
} 