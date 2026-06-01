using DG.Tweening;
using System.Collections;
using TenshiNoTamago.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TenshiNoTamago.UI
{
    public class MainMenuAnimController : MonoBehaviour
    {
        [Header("UI组件")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private CanvasGroup buttonsGroup;
        [SerializeField] private RawImage noiseImage;      // 噪点层（RawImage）
        [SerializeField] private Text buttonText;          
        [SerializeField] private Text leftArrow;          
        [SerializeField] private Text rightArrow;

        [Header("按钮切换")]
        private string[] buttonNames = { "スタート", "ツヅケル‌", "システム","シュウリョウ‌" };
        private int currentIndex = 0;

        private void Start()
        {
            int endingType = PlayerPrefs.GetInt("LastEndingType", 0);
            ChangeBackgroundByEnding(endingType);

            buttonsGroup.alpha = 1;
            UpdateButtonText();

            // 启动噪点动画
            StartCoroutine(NoiseAnimation());

            StartCoroutine(BlinkArrow(leftArrow));
            StartCoroutine(BlinkArrow(rightArrow));

            StartCoroutine(PlayEntranceAnimation());
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                OnLeftArrow();
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                OnRightArrow();
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                OnConfirm();
            }
        }

        public void OnConfirm() {
            switch (currentIndex)
            {
                case 0: OnStartGame(); break;
                case 1: OnContinueGame(); break;
                case 2: OnSettings(); break;
                case 3: OnQuitGame(); break;
            }
        }

        private void ChangeBackgroundByEnding(int endingType)
        {
            string bgPath = "MainMenu/menu_default";  // 默认背景

            switch (endingType)
            {
                case 1:
                    bgPath = "MainMenu/menu_high";
                    break;
                case 2:
                    bgPath = "MainMenu/menu_mid";
                    break;
                case 3:
                    bgPath = "MainMenu/menu_low";
                    break;
            }

            Sprite bg = Resources.Load<Sprite>(bgPath);
            if (bg != null)
                backgroundImage.sprite = bg;
            else
                Debug.LogWarning($"未找到背景图: {bgPath}");
        }

        private IEnumerator PlayEntranceAnimation() {
            yield return new WaitForSeconds(0.2f);
            buttonsGroup.DOFade(1, 0.5f);
        }

        private IEnumerator NoiseAnimation()
        {
            RectTransform rt = noiseImage.rectTransform;
            while (true)
            {
                rt.anchoredPosition = new Vector2(Random.Range(-2f, 2f), Random.Range(-2f, 2f));
                yield return new WaitForSeconds(0.05f);
            }
        }

        private IEnumerator BlinkArrow(Text arrow) {
            if (arrow == null) yield break;
            while (true) {
                arrow.DOFade(0.4f, 0.8f);
                yield return new WaitForSeconds(0.8f);
                arrow.DOFade(1f, 0.8f);
                yield return new WaitForSeconds(0.8f);
            }
        }

        private void PulseButton() {
            if (buttonText == null) return;
            buttonText.transform.DOScale(1.08f, 0.08f).SetLoops(2, LoopType.Yoyo);
        }

        public void OnLeftArrow()
        {
            currentIndex--;
            if (currentIndex < 0) currentIndex = buttonNames.Length - 1;
            UpdateButtonText();
            PulseButton();
        }

        public void OnRightArrow()
        {
            currentIndex++;
            if (currentIndex >= buttonNames.Length) currentIndex = 0;
            UpdateButtonText();
            PulseButton();
        }

        private void UpdateButtonText()
        {
            if (buttonText != null)
                buttonText.text = buttonNames[currentIndex];
        }

        public void OnStartGame()
        {
            GameManager.Instance.ResetGame();
            SceneManager.LoadScene("SampleScene");
        }

        public void OnContinueGame()
        {
            SaveLoadManager.currentMode = SaveLoadManager.Mode.Load;
            SaveLoadManager.currentSource = SaveLoadManager.EntrySource.MainMenu;
            SceneManager.LoadScene("SaveLoad");
            Debug.Log("存档页面打开");
        }

        // 系统设置
        public void OnSettings()
        {
            Debug.Log("系统设置 - 待实现");
            // TODO: 打开设置面板（音量、文字速度等）
        }


        public void OnQuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}

