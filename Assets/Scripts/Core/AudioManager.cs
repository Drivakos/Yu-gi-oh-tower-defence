using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace YuGiOhTowerDefense.Core
{
    [Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        
        [Range(0f, 1f)]
        public float volume = 1f;
        
        [Range(0.1f, 3f)]
        public float pitch = 1f;
        
        public bool loop = false;
        public bool playOnAwake = false;
        
        [Range(0f, 1f)]
        public float spatialBlend = 0f;
        
        public AudioCategory category = AudioCategory.SFX;
        
        [HideInInspector]
        public AudioSource source;
    }
    
    public enum AudioCategory
    {
        Master,
        Music,
        SFX,
        UI,
        Ambient
    }
    
    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private Sound[] sounds;
        [SerializeField] private AudioMixerGroup masterMixerGroup;
        [SerializeField] private AudioMixerGroup musicMixerGroup;
        [SerializeField] private AudioMixerGroup sfxMixerGroup;
        [SerializeField] private AudioMixerGroup uiMixerGroup;
        [SerializeField] private AudioMixerGroup ambientMixerGroup;
        
        [Header("Default Audio")]
        [SerializeField] private string backgroundMusicName = "BackgroundMusic";
        [SerializeField] private string buttonClickSound = "ButtonClick";
        [SerializeField] private string cardPlaceSound = "CardPlace";
        [SerializeField] private string cardFlipSound = "CardFlip";
        
        [Header("Volume Settings")]
        [SerializeField] private string masterVolumeParam = "MasterVolume";
        [SerializeField] private string musicVolumeParam = "MusicVolume";
        [SerializeField] private string sfxVolumeParam = "SFXVolume";
        [SerializeField] private string uiVolumeParam = "UIVolume";
        [SerializeField] private string ambientVolumeParam = "AmbientVolume";
        
        [Header("Mobile Settings")]
        [SerializeField] private bool muteWhenInBackground = true;
        [SerializeField] private bool pauseMusicWhenInBackground = true;
        
        private Dictionary<string, Sound> soundDictionary = new Dictionary<string, Sound>();
        private Dictionary<AudioCategory, AudioMixerGroup> mixerGroups = new Dictionary<AudioCategory, AudioMixerGroup>();
        
        private Sound currentBackgroundMusic;
        private float currentBackgroundMusicTime = 0f;
        
        public static AudioManager Instance { get; private set; }
        
        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            // Create audio sources for each sound
            foreach (Sound sound in sounds)
            {
                // Create audio source
                AudioSource audioSource = gameObject.AddComponent<AudioSource>();
                sound.source = audioSource;
                
                audioSource.clip = sound.clip;
                audioSource.volume = sound.volume;
                audioSource.pitch = sound.pitch;
                audioSource.loop = sound.loop;
                audioSource.playOnAwake = sound.playOnAwake;
                audioSource.spatialBlend = sound.spatialBlend;
                
                // Add to dictionary for quick lookup
                if (!soundDictionary.ContainsKey(sound.name))
                {
                    soundDictionary.Add(sound.name, sound);
                }
                else
                {
                    Debug.LogWarning($"Duplicate sound name found: {sound.name}");
                }
            }
            
            // Initialize mixer groups
            mixerGroups[AudioCategory.Master] = masterMixerGroup;
            mixerGroups[AudioCategory.Music] = musicMixerGroup;
            mixerGroups[AudioCategory.SFX] = sfxMixerGroup;
            mixerGroups[AudioCategory.UI] = uiMixerGroup;
            mixerGroups[AudioCategory.Ambient] = ambientMixerGroup;
            
            // Assign mixer groups to sounds
            foreach (Sound sound in sounds)
            {
                if (mixerGroups.TryGetValue(sound.category, out AudioMixerGroup mixerGroup))
                {
                    sound.source.outputAudioMixerGroup = mixerGroup;
                }
            }
            
            // Load volume settings from PlayerPrefs
            LoadVolumeSettings();
            
            // Play sounds marked as playOnAwake
            foreach (Sound sound in sounds)
            {
                if (sound.playOnAwake)
                {
                    sound.source.Play();
                }
            }
            
            // Play background music if available
            if (!string.IsNullOrEmpty(backgroundMusicName))
            {
                PlayBackgroundMusic(backgroundMusicName);
            }
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (muteWhenInBackground)
            {
                AudioListener.pause = !hasFocus;
            }
            
            if (pauseMusicWhenInBackground && currentBackgroundMusic != null)
            {
                if (hasFocus)
                {
                    if (!currentBackgroundMusic.source.isPlaying)
                    {
                        currentBackgroundMusic.source.time = currentBackgroundMusicTime;
                        currentBackgroundMusic.source.Play();
                    }
                }
                else
                {
                    if (currentBackgroundMusic.source.isPlaying)
                    {
                        currentBackgroundMusicTime = currentBackgroundMusic.source.time;
                        currentBackgroundMusic.source.Pause();
                    }
                }
            }
        }
        
        private void LoadVolumeSettings()
        {
            if (masterMixerGroup != null && masterMixerGroup.audioMixer != null)
            {
                float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
                float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
                float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
                float uiVolume = PlayerPrefs.GetFloat("UIVolume", 1f);
                float ambientVolume = PlayerPrefs.GetFloat("AmbientVolume", 0.7f);
                
                SetCategoryVolume(AudioCategory.Master, masterVolume);
                SetCategoryVolume(AudioCategory.Music, musicVolume);
                SetCategoryVolume(AudioCategory.SFX, sfxVolume);
                SetCategoryVolume(AudioCategory.UI, uiVolume);
                SetCategoryVolume(AudioCategory.Ambient, ambientVolume);
            }
        }
        
        public void SaveVolumeSettings()
        {
            float masterVolume = GetCategoryVolume(AudioCategory.Master);
            float musicVolume = GetCategoryVolume(AudioCategory.Music);
            float sfxVolume = GetCategoryVolume(AudioCategory.SFX);
            float uiVolume = GetCategoryVolume(AudioCategory.UI);
            float ambientVolume = GetCategoryVolume(AudioCategory.Ambient);
            
            PlayerPrefs.SetFloat("MasterVolume", masterVolume);
            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
            PlayerPrefs.SetFloat("UIVolume", uiVolume);
            PlayerPrefs.SetFloat("AmbientVolume", ambientVolume);
            
            PlayerPrefs.Save();
        }
        
        public void Play(string name)
        {
            if (soundDictionary.TryGetValue(name, out Sound sound))
            {
                sound.source.Play();
            }
            else
            {
                Debug.LogWarning($"Sound {name} not found!");
            }
        }
        
        public void PlayOneShot(string name)
        {
            if (soundDictionary.TryGetValue(name, out Sound sound))
            {
                sound.source.PlayOneShot(sound.clip);
            }
            else
            {
                Debug.LogWarning($"Sound {name} not found!");
            }
        }
        
        public void PlayAtPosition(string name, Vector3 position)
        {
            if (soundDictionary.TryGetValue(name, out Sound sound))
            {
                AudioSource.PlayClipAtPoint(sound.clip, position, sound.volume);
            }
            else
            {
                Debug.LogWarning($"Sound {name} not found!");
            }
        }
        
        public void Stop(string name)
        {
            if (soundDictionary.TryGetValue(name, out Sound sound))
            {
                sound.source.Stop();
            }
            else
            {
                Debug.LogWarning($"Sound {name} not found!");
            }
        }
        
        public void StopAll()
        {
            foreach (Sound sound in sounds)
            {
                sound.source.Stop();
            }
        }
        
        public void PlayBackgroundMusic(string name)
        {
            if (currentBackgroundMusic != null)
            {
                currentBackgroundMusic.source.Stop();
            }
            
            if (soundDictionary.TryGetValue(name, out Sound sound))
            {
                sound.source.Play();
                currentBackgroundMusic = sound;
                currentBackgroundMusicTime = 0f;
            }
            else
            {
                Debug.LogWarning($"Background music {name} not found!");
                currentBackgroundMusic = null;
            }
        }
        
        public void PlayButtonClickSound()
        {
            if (!string.IsNullOrEmpty(buttonClickSound))
            {
                PlayOneShot(buttonClickSound);
            }
        }
        
        public void PlayCardPlaceSound()
        {
            if (!string.IsNullOrEmpty(cardPlaceSound))
            {
                PlayOneShot(cardPlaceSound);
            }
        }
        
        public void PlayCardFlipSound()
        {
            if (!string.IsNullOrEmpty(cardFlipSound))
            {
                PlayOneShot(cardFlipSound);
            }
        }
        
        public void SetCategoryVolume(AudioCategory category, float volume)
        {
            if (mixerGroups.TryGetValue(category, out AudioMixerGroup mixerGroup) && mixerGroup.audioMixer != null)
            {
                string paramName = GetMixerParameterName(category);
                
                // Convert from linear volume (0-1) to decibels (-80 to 0)
                float decibelValue = volume > 0.0001f ? 20f * Mathf.Log10(volume) : -80f;
                mixerGroup.audioMixer.SetFloat(paramName, decibelValue);
            }
        }
        
        public float GetCategoryVolume(AudioCategory category)
        {
            if (mixerGroups.TryGetValue(category, out AudioMixerGroup mixerGroup) && mixerGroup.audioMixer != null)
            {
                string paramName = GetMixerParameterName(category);
                
                if (mixerGroup.audioMixer.GetFloat(paramName, out float decibelValue))
                {
                    // Convert from decibels (-80 to 0) to linear volume (0-1)
                    return decibelValue <= -79.9f ? 0f : Mathf.Pow(10f, decibelValue / 20f);
                }
            }
            
            return 1f;
        }
        
        public bool IsSoundPlaying(string name)
        {
            return soundDictionary.TryGetValue(name, out Sound sound) && sound.source.isPlaying;
        }
        
        public bool SoundExists(string name)
        {
            return soundDictionary.ContainsKey(name);
        }
        
        private string GetMixerParameterName(AudioCategory category)
        {
            switch (category)
            {
                case AudioCategory.Master:
                    return masterVolumeParam;
                case AudioCategory.Music:
                    return musicVolumeParam;
                case AudioCategory.SFX:
                    return sfxVolumeParam;
                case AudioCategory.UI:
                    return uiVolumeParam;
                case AudioCategory.Ambient:
                    return ambientVolumeParam;
                default:
                    return masterVolumeParam;
            }
        }
        
        public void SetPitch(string name, float pitch)
        {
            if (soundDictionary.TryGetValue(name, out Sound sound))
            {
                sound.pitch = Mathf.Clamp(pitch, 0.1f, 3f);
                sound.source.pitch = sound.pitch;
            }
        }
        
        public void SetVolume(string name, float volume)
        {
            if (soundDictionary.TryGetValue(name, out Sound sound))
            {
                sound.volume = Mathf.Clamp01(volume);
                sound.source.volume = sound.volume;
            }
        }
        
        public void FadeIn(string name, float duration)
        {
            if (soundDictionary.TryGetValue(name, out Sound sound))
            {
                StartCoroutine(FadeCoroutine(sound, 0f, sound.volume, duration, true));
            }
        }
        
        public void FadeOut(string name, float duration)
        {
            if (soundDictionary.TryGetValue(name, out Sound sound))
            {
                StartCoroutine(FadeCoroutine(sound, sound.volume, 0f, duration, false));
            }
        }
        
        private System.Collections.IEnumerator FadeCoroutine(Sound sound, float startVolume, float targetVolume, float duration, bool playOnStart)
        {
            AudioSource source = sound.source;
            float startTime = Time.time;
            float endTime = startTime + duration;
            
            if (playOnStart && !source.isPlaying)
            {
                source.volume = startVolume;
                source.Play();
            }
            
            while (Time.time < endTime)
            {
                float t = (Time.time - startTime) / duration;
                source.volume = Mathf.Lerp(startVolume, targetVolume, t);
                yield return null;
            }
            
            source.volume = targetVolume;
            
            if (targetVolume <= 0.01f && source.isPlaying)
            {
                source.Stop();
            }
        }
    }
} 