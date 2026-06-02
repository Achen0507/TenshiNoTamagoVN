using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

namespace TenshiNoTamago.Core
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("音频源")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource ambienceSource; //氛围音
        [SerializeField] private AudioSource sfxSource;

        [Header("音量设置")]
        [SerializeField] private float targetAmbienceVolume = 0.6f;

        [Header("音效映射")]
        [SerializeField] private AudioMapping[] audioMappings;

        private Dictionary<string, AudioClip> audioDict;
        private string currentAmbienceKey = "";

        [System.Serializable]
        public struct AudioMapping {
            public string key;
            public AudioClip clip;
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

            //映射
            audioDict = new Dictionary<string, AudioClip>();
            foreach (var mapping in audioMappings)
            {
                if (!audioDict.ContainsKey(mapping.key))
                {
                    audioDict.Add(mapping.key, mapping.clip);
                }
                else
                {
                    Debug.LogWarning($"重复的音效 key: {mapping.key}");
                }
            }
        }

        private void Start()
        {
            LoadVolumeSettings();
        }

        /// <summary>
        /// 播放背景音乐
        /// </summary>
        public void PlayBGM(AudioClip clip, bool loop = true) {
            if (bgmSource == null || clip == null) return;
            bgmSource.clip = clip;
            bgmSource.loop = loop;
            bgmSource.Play();
        }

        /// <summary>
        /// 播放环境音（风声、雨声等）
        /// </summary>
        public void PlayAmbience(string key, bool loop = true, float fadeTime = 1f) {
            currentAmbienceKey = key;

            if (ambienceSource == null) return;
            if (audioDict.TryGetValue(key, out AudioClip clip))
            {
                if (ambienceSource.isPlaying && ambienceSource.clip == clip) return;

                if (ambienceSource.isPlaying)
                {
                    ambienceSource.DOFade(0f, fadeTime).OnComplete(() =>
                    {
                        ambienceSource.Stop();
                        ambienceSource.clip = clip;
                        ambienceSource.loop = loop;
                        ambienceSource.volume = 0f;
                        ambienceSource.Play();
                        ambienceSource.DOFade(targetAmbienceVolume, fadeTime);
                    });
                }
                else
                {
                    ambienceSource.clip = clip;
                    ambienceSource.loop = loop;
                    ambienceSource.volume = 0f;
                    ambienceSource.Play();
                    ambienceSource.DOFade(targetAmbienceVolume, fadeTime);
                }
            }
            else {
                Debug.LogWarning($"未找到环境音 key: {key}");
            }
        }

        /// <summary>
        /// 播放一击音效（翻页、选项、钟声等）
        /// </summary>
        public void PlaySFX(string key) {
            if (sfxSource == null) { Debug.LogError("sfxSource 为空"); return; }
            if (audioDict == null) { Debug.LogError("audioDict 未初始化"); return; }

            if (audioDict.TryGetValue(key, out AudioClip clip))
            {
                sfxSource.PlayOneShot(clip);
            }
            else
            {
                Debug.LogWarning($"未找到音效 key: '{key}'，现有的keys: {string.Join(", ", audioDict.Keys)}");
            }
        }

        /// <summary>
        /// 直接播放 AudioClip（备用）
        /// </summary>
        public void PlaySFXClip(AudioClip clip) {
            if (sfxSource != null && clip != null) sfxSource.PlayOneShot(clip);
        }

        /// <summary>
        /// 停止背景音乐
        /// </summary>
        public void StopBGM() {
            if (bgmSource != null) bgmSource.Stop();
        }

        /// <summary>
        /// 停止环境音
        /// </summary>
        public void StopAmbience(float fadeTime = 1f)
        {
            currentAmbienceKey = "";

            if (ambienceSource != null && ambienceSource.isPlaying)
            {
                ambienceSource.DOFade(0f, fadeTime).OnComplete(() =>
                {
                    ambienceSource.Stop();
                    ambienceSource.volume = targetAmbienceVolume;  // 恢复音量
                });
            }
        }

        public AudioSource PlaySFXAndReturnSource(string key) {
            if (sfxSource == null) return null;
            if (audioDict.TryGetValue(key, out AudioClip clip))
            {
                sfxSource.PlayOneShot(clip);
                return sfxSource;
            }
            return null;
        }

        public void PauseBGM()
        {
            if (bgmSource != null && bgmSource.isPlaying)
                bgmSource.Pause();
            if (sfxSource != null && sfxSource.isPlaying)
                sfxSource.Pause();
            if (ambienceSource != null && ambienceSource.isPlaying)
                ambienceSource.Pause();
        }

        public void ResumeBGM()
        {
            if (bgmSource != null)
                bgmSource.UnPause();
            if (sfxSource != null)
                sfxSource.UnPause();
            if (ambienceSource != null)
                ambienceSource.UnPause();
        }

        public string GetCurrentAmbienceKey() => currentAmbienceKey;

        public void SetMasterVolume(float volume)
        {
            AudioListener.volume = volume;
            PlayerPrefs.SetFloat("MasterVolume", volume);
        }

        public void SetAmbienceVolume(float volume) {
            if (ambienceSource != null) ambienceSource.volume = volume;
            PlayerPrefs.SetFloat("AmbienceVolume", volume);
        }

        public void SetSFXVolume(float volume) {
            if (sfxSource != null) sfxSource.volume = volume;
            PlayerPrefs.SetFloat("SFXVolume", volume);
        }

        private void LoadVolumeSettings() {
            AudioListener.volume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            if (ambienceSource != null) ambienceSource.volume = PlayerPrefs.GetFloat("AmbienceVolume", 0.6f);
            if (sfxSource != null) sfxSource.volume = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
        }
    }
}
