using System.Collections;
using TenshiNoTamago.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TenshiNoTamago.UI
{
    public class PauseMenuController : MonoBehaviour
    {
        [Header("Canvas")]
        [SerializeField] private DialogueController dialogueController;
        [SerializeField] private GameObject pauseCanvasRoot;

        [Header("°´Å¥")]
        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button returnButton;
        [SerializeField] private Button backToMenuButton;

        private bool isPaused = false;

        private void Start()
        {
            if (pauseCanvasRoot == null)
            {
                pauseCanvasRoot = GameObject.Find("PauseCanvas");
            }
            if (pauseCanvasRoot != null)
            {
                pauseCanvasRoot.SetActive(false);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                if (isPaused) ResumeGame();
                else PauseGame();
            }
        }

        private void UpdateButtonTexts()
        {
            if (saveButton != null && saveButton.GetComponentInChildren<Text>() != null)
                saveButton.GetComponentInChildren<Text>().text = LanguageManager.Get("save");

            if (loadButton != null && loadButton.GetComponentInChildren<Text>() != null)
                loadButton.GetComponentInChildren<Text>().text = LanguageManager.Get("load");

            if (settingsButton != null && settingsButton.GetComponentInChildren<Text>() != null)
                settingsButton.GetComponentInChildren<Text>().text = LanguageManager.Get("setting");

            if (returnButton != null && returnButton.GetComponentInChildren<Text>() != null)
                returnButton.GetComponentInChildren<Text>().text = LanguageManager.Get("return");

            if (backToMenuButton != null && backToMenuButton.GetComponentInChildren<Text>() != null)
                backToMenuButton.GetComponentInChildren<Text>().text = LanguageManager.Get("return_main");
        }

        private void PauseGame() {
            isPaused = true;
            UpdateButtonTexts();
            pauseCanvasRoot.SetActive(true);
            if (dialogueController != null) dialogueController.canInput = false;
            AudioManager.Instance.PauseBGM();
            Time.timeScale = 0f;
        }

        public void ResumeGame() {
            isPaused = false; 
            pauseCanvasRoot.SetActive(false);
            if (dialogueController != null) dialogueController.canInput = true;
            AudioManager.Instance.ResumeBGM();
            Time.timeScale = 1f;
        }

        public void OnSaveGame()
        {
            StartCoroutine(CaptureAndGoToSaveLoad());
        }

        private IEnumerator CaptureAndGoToSaveLoad()
        {
            GameObject pauseRoot = GameObject.Find("PauseCanvas");
            if (pauseRoot != null) pauseRoot.SetActive(false);

            yield return new WaitForEndOfFrame();

            string tempPath = Application.persistentDataPath + "/temp_thumb.png";
            ScreenCapture.CaptureScreenshot(tempPath);

            SaveLoadManager.tempThumbnailPath = tempPath;
            SaveLoadManager.currentMode = SaveLoadManager.Mode.Save;
            SaveLoadManager.currentSource = SaveLoadManager.EntrySource.PauseMenu;
            Time.timeScale = 1f;
            SceneManager.LoadScene("SaveLoad");
        }


        public void OnLoadGame() {
            SaveLoadManager.currentMode = SaveLoadManager.Mode.Load;
            SaveLoadManager.currentSource = SaveLoadManager.EntrySource.PauseMenu;
            Time.timeScale = 1f;
            SceneManager.LoadScene("SaveLoad");
        }

        public void OnSettings()
        {
            SettingsManager.currentSource = SettingsManager.EntrySource.PauseMenu;
            Time.timeScale = 1f;
            SceneManager.LoadScene("SettingMenu");
        }

        public void OnBackToMainMenu() {
            AudioManager.Instance.StopAmbience();
            AudioManager.Instance.StopBGM();
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }
    }
}
