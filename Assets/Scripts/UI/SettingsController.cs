using TenshiNoTamago.Core;
using TenshiNoTamago.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TenshiNoTamago.UI
{
    public static class SettingsManager
    {
        public enum EntrySource { MainMenu, PauseMenu }
        public static EntrySource currentSource = EntrySource.MainMenu;
    }

    public class SettingsController : MonoBehaviour
    {
        [Header("音量")]
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider ambienceSlider;
        [SerializeField] private Slider sfxSlider;

        [Header("文字")]
        [SerializeField] private Slider textSpeedSlider;

        [Header("自动播放")]
        [SerializeField] private Toggle autoOnToggle;
        [SerializeField] private Toggle autoOffToggle;

        [Header("全屏显示")]
        [SerializeField] private Toggle fullscreenOnToggle;
        [SerializeField] private Toggle fullscreenOffToggle;

        [Header("解像度")]
        [SerializeField] private Text resolutionText;
        [SerializeField] private Button resolutionLeft;
        [SerializeField] private Button resolutionRight;

        [Header("语言")]
        [SerializeField] private Text languageText;
        [SerializeField] private Button languageLeft;
        [SerializeField] private Button languageRight;
        [Header("按钮")]
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button closeButton;

        [Header("标题")]
        [SerializeField] private Text titleText;

        [Header("标签")]
        [SerializeField] private Text masterVolumetitle;
        [SerializeField] private Text ambienceVolumetitle;
        [SerializeField] private Text sfxVolumetitle;
        [SerializeField] private Text textSpeedtitle;
        [SerializeField] private Text autoPlaytitle;      // 播放方式
        [SerializeField] private Text displayModetitle;   // 显示模式
        [SerializeField] private Text resolutiontitle;    // 分辨率
        [SerializeField] private Text languagetitle;      // 语言

        private Resolution[] resolutions;
        private int resolutionIndex;
        private string[] languages = { "中文", "日本語" };
        private int languageIndex;

        private void Start()
        {
            LoadSettings();
            BindEvents();
            InitResolutions();
            InitLanguage();
            UpdateAllUITexts();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnBack();
            }
        }

        private void BindEvents()
        {
            if (masterSlider != null) masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            if (ambienceSlider != null) ambienceSlider.onValueChanged.AddListener(OnAmbienceVolumeChanged);
            if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            if (textSpeedSlider != null) textSpeedSlider.onValueChanged.AddListener(OnTextSpeedChanged);
            if (autoOnToggle != null) autoOnToggle.onValueChanged.AddListener(OnAutoPlayChanged);
            if (autoOffToggle != null) autoOffToggle.onValueChanged.AddListener(OnAutoPlayChanged);
            if (fullscreenOnToggle != null) fullscreenOnToggle.onValueChanged.AddListener(OnFullscreenChanged);
            if (fullscreenOffToggle != null) fullscreenOffToggle.onValueChanged.AddListener(OnFullscreenChanged);
            if (resolutionLeft != null) resolutionLeft.onClick.AddListener(OnResolutionLeft);
            if (resolutionRight != null) resolutionRight.onClick.AddListener(OnResolutionRight);
            if (languageLeft != null) languageLeft.onClick.AddListener(OnLanguageLeft);
            if (languageRight != null) languageRight.onClick.AddListener(OnLanguageRight);
            if (closeButton != null) closeButton.onClick.AddListener(OnBack);
            if (confirmButton != null) confirmButton.onClick.AddListener(OnConfirm);
        }

        private void UpdateAllUITexts()
        {
            if (titleText != null) titleText.text = LanguageManager.Get("settingtitle");
            if (confirmButton != null && confirmButton.GetComponentInChildren<Text>() != null)
                confirmButton.GetComponentInChildren<Text>().text = LanguageManager.Get("confirm");
            if (closeButton != null && closeButton.GetComponentInChildren<Text>() != null)
                closeButton.GetComponentInChildren<Text>().text = LanguageManager.Get("back");

            if (masterVolumetitle != null) masterVolumetitle.text = LanguageManager.Get("master_volume");
            if (ambienceVolumetitle != null) ambienceVolumetitle.text = LanguageManager.Get("ambience_volume");
            if (sfxVolumetitle != null) sfxVolumetitle.text = LanguageManager.Get("sfx_volume");
            if (textSpeedtitle != null) textSpeedtitle.text = LanguageManager.Get("text_speed");
            if (autoPlaytitle != null) autoPlaytitle.text = LanguageManager.Get("auto_play");
            if (displayModetitle != null) displayModetitle.text = LanguageManager.Get("display_mode");
            if (resolutiontitle != null) resolutiontitle.text = LanguageManager.Get("resolution");
            if (languagetitle != null) languagetitle.text = LanguageManager.Get("language");

            // 自动/手动 Toggle 的文字
            if (autoOnToggle != null && autoOnToggle.GetComponentInChildren<Text>() != null)
                autoOnToggle.GetComponentInChildren<Text>().text = LanguageManager.Get("auto");
            if (autoOffToggle != null && autoOffToggle.GetComponentInChildren<Text>() != null)
                autoOffToggle.GetComponentInChildren<Text>().text = LanguageManager.Get("manual");

            // 全屏/窗口 Toggle 的文字
            if (fullscreenOnToggle != null && fullscreenOnToggle.GetComponentInChildren<Text>() != null)
                fullscreenOnToggle.GetComponentInChildren<Text>().text = LanguageManager.Get("fullscreen");
            if (fullscreenOffToggle != null && fullscreenOffToggle.GetComponentInChildren<Text>() != null)
                fullscreenOffToggle.GetComponentInChildren<Text>().text = LanguageManager.Get("window");
        }

        private void LoadSettings() {
            if (masterSlider != null) masterSlider.value = PlayerPrefs.GetFloat("MasterVolume", 15f);
            if (ambienceSlider != null) ambienceSlider.value = PlayerPrefs.GetFloat("AmbienceVolume", 10f);
            if (sfxSlider != null) sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 12f);
            if (textSpeedSlider != null) textSpeedSlider.value = PlayerPrefs.GetFloat("TextSpeed",8f);

            // 自动播放
            bool isAuto = PlayerPrefs.GetInt("AutoPlay", 0) == 1;
            autoOnToggle.isOn = isAuto;
            autoOffToggle.isOn = !isAuto;

            // 全屏
            bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
            fullscreenOnToggle.isOn = isFullscreen;
            fullscreenOffToggle.isOn = !isFullscreen;
            Screen.fullScreen = isFullscreen;

            languageIndex = PlayerPrefs.GetInt("LanguageIndex", 0);
            UpdateLanguageText();
            string lang = languages[languageIndex] == "日本語" ? "ja" : "zh";
            JsonLoader.currentLanguage = lang;
            LanguageManager.LoadLanguage(lang);
        }

        private void InitResolutions() {
            resolutions = Screen.resolutions;
            resolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", GetCurrentResolutionIndex());
            UpdateResolutionText();
        }

        private int GetCurrentResolutionIndex() {
            for (int i = 0; i < resolutions.Length; i++)
            {
                if (resolutions[i].width == Screen.currentResolution.width &&
                    resolutions[i].height == Screen.currentResolution.height) return i;
            }
            return 0;
        }

        private void UpdateResolutionText() {
            if(resolutionText!=null)resolutionText.text = $"{resolutions[resolutionIndex].width}*{resolutions[resolutionIndex].height}";
        }

        private void InitLanguage() {
            languageIndex = PlayerPrefs.GetInt("LanguageIndex", 0);
            UpdateLanguageText();
        }

        private void UpdateLanguageText() {
            if (languageText != null) languageText.text = languages[languageIndex];    
        }

        private void OnResolutionLeft() {
            resolutionIndex--;
            if (resolutionIndex < 0) resolutionIndex = resolutions.Length - 1;
            UpdateResolutionText();
            ApplyResolution();
        }

        private void OnResolutionRight()
        {
            resolutionIndex++;
            if (resolutionIndex >= resolutions.Length) resolutionIndex = 0;
            UpdateResolutionText();
            ApplyResolution();
        }

        private void ApplyResolution() {
            Resolution res = resolutions[resolutionIndex];
            Screen.SetResolution(res.width, res.height, Screen.fullScreen);
            PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
        }

        private void OnLanguageLeft()
        {
            languageIndex--;
            if (languageIndex < 0) languageIndex = languages.Length - 1;
            UpdateLanguageText();
            PlayerPrefs.SetInt("LanguageIndex", languageIndex);

            string lang = languages[languageIndex] == "日本語" ? "ja" : "zh";
            JsonLoader.currentLanguage = lang;
            LanguageManager.LoadLanguage(lang);

            UpdateAllUITexts();
        }

        private void OnLanguageRight()
        {
            languageIndex++;
            if (languageIndex >= languages.Length) languageIndex = 0;
            UpdateLanguageText();
            PlayerPrefs.SetInt("LanguageIndex", languageIndex);

            string lang = languages[languageIndex] == "日本語" ? "ja" : "zh";
            JsonLoader.currentLanguage = lang;
            LanguageManager.LoadLanguage(lang);

            UpdateAllUITexts();
        }

        private void OnMasterVolumeChanged(float value) {
            float volume = value / 15f;
            AudioManager.Instance.SetMasterVolume(volume);
        }

        private void OnAmbienceVolumeChanged(float value) {
            float volume = value / 15f;
            AudioManager.Instance.SetAmbienceVolume(volume);
        }

        private void OnSFXVolumeChanged(float value)
        {
            float volume = value / 15f;
            AudioManager.Instance.SetSFXVolume(volume);
        }

        private void OnTextSpeedChanged(float value) {
            float speed = value / 15f;
            PlayerPrefs.SetFloat("TextSpeed", speed);
        }

        private void OnAutoPlayChanged(bool isOn) {
            PlayerPrefs.SetInt("AutoPlay", autoOnToggle.isOn ? 1 : 0);
        }

        private void OnFullscreenChanged(bool isOn)
        {
            bool isFullscreen = fullscreenOnToggle.isOn;
            Screen.fullScreen = isFullscreen;
            PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        }


        public void OnConfirm() {
            AudioManager.Instance.PlaySFX("click");
            SaveAllSettings();
            OnBack();
        }

        private void OnBack() {
            if (SettingsManager.currentSource == SettingsManager.EntrySource.PauseMenu)
            {
                SceneManager.LoadScene("SampleScene");
            }
            else
            {
                SceneManager.LoadScene("MainMenu");
            }
        }

        private void SaveAllSettings() {
            PlayerPrefs.SetFloat("MasterVolume", masterSlider.value);
            PlayerPrefs.SetFloat("AmbienceVolume", ambienceSlider.value);
            PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value);
            PlayerPrefs.SetFloat("TextSpeed", textSpeedSlider.value);
            PlayerPrefs.SetInt("AutoPlay", autoOnToggle.isOn ? 1 : 0);
            PlayerPrefs.SetInt("Fullscreen", fullscreenOnToggle.isOn ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
