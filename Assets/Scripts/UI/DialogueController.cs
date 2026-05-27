using TenshiNoTamago.Core;
using TenshiNoTamago.Data;
using TenshiNoTamago.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace TenshiNoTamago.UI
{
    /// <summary>
    /// 对话控制器：管理帧的显示和切换 
    /// </summary>
    public class DialogueController : MonoBehaviour
    {
        [Header("UI 组件")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Text descriptionText;
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private Transform optionsContainer;      // 选项按钮的父容器
        [SerializeField] private GameObject optionButtonPrefab;   // 选项按钮预制体

        [Header("章节数据")]
        [SerializeField] private string chapterToLoad = "prologue";

        private ChapterData currentChapterData;
        private FrameData currentFrame;
        private bool isWaitingForInput = false;
        private float autoNextTimer = 0f;

        private void Start()
        {
            LoadChapter(chapterToLoad);
        }

        private void Update()
        {
            if (isWaitingForInput) {
                if (autoNextTimer > 0)
                {
                    autoNextTimer -= Time.deltaTime;
                    if (autoNextTimer <= 0)
                    {
                        AdvanceToNextFrame();
                    }
                }
                else if (Input.GetMouseButtonDown(0) && (currentFrame.options == null || currentFrame.options.Length == 0)) {
                    AdvanceToNextFrame();
                }
            }
        }

        private void LoadChapter(string chapterName) {
            currentChapterData = JsonLoader.LoadChapter(chapterName);
            if (currentChapterData == null) {
                Debug.LogError($"[DialogueController] Failed to load chapter: {chapterName}");
                return;
            }

            ShowFrame(currentChapterData.frames[0]);
        }

        private void ShowFrame(FrameData frame) {
            currentFrame = frame;

            // 应用本帧的卵完整度变化
            if (frame.eggDelta != 0)
            {
                GameManager.Instance.AddEggIntegrity(frame.eggDelta);
            }

            if (!string.IsNullOrEmpty(frame.backgroundPath)) {
                Sprite bg = Resources.Load<Sprite>(frame.backgroundPath);
                if (bg != null) backgroundImage.sprite = bg;
                else Debug.LogWarning($"[DialogueController] Background not found: {frame.backgroundPath}");
            }

            descriptionText.text = frame.descriptionText;

            // 处理立绘TODO

            if (frame.options != null && frame.options.Length > 0)
            {
                ShowOptions(frame.options);
                isWaitingForInput = false;
            }
            else {
                ClearOptions();
                isWaitingForInput = true;
                autoNextTimer = frame.autoNextSeconds;
            }
        }

        private void ShowOptions(OptionData[] options) {
            ClearOptions();
            foreach (var opt in options)
            {
                GameObject btnobj = Instantiate(optionButtonPrefab, optionsContainer);
                Button btn = btnobj.GetComponent<Button>();
                Text btnText = btnobj.GetComponentInChildren<Text>();
                if (btnText != null)
                    btnText.text = opt.text;

                OptionData capturedOpt = opt;
                btn.onClick.AddListener(() => OnOptionSelected(capturedOpt));
            }
        }

        private void ClearOptions() {
            foreach (Transform child in optionsContainer)
            {
                Destroy(child.gameObject);
            }
        }

        private void OnOptionSelected(OptionData option) {
            if (option.eggDelta != 0) {
                GameManager.Instance.AddEggIntegrity(option.eggDelta);
            }

            Debug.Log($"[DialogueController] 选择了: {option.text}");

            if (option.nextFrameId >= 0)
            {
                FrameData nextFrame = System.Array.Find(currentChapterData.frames, f => f.id == option.nextFrameId);
                if (nextFrame != null) ShowFrame(nextFrame);
                else Debug.LogError($"[DialogueController] Frame not found: {option.nextFrameId}");
            }
            else {
                AdvanceToNextFrame();
            }
        }

        private void AdvanceToNextFrame() {
            int currentIndex = System.Array.FindIndex(currentChapterData.frames, f => f.id == currentFrame.id);
            if (currentIndex >= 0 && currentIndex + 1 < currentChapterData.frames.Length)
            {
                ShowFrame(currentChapterData.frames[currentIndex + 1]);
            }
            else {
                EndChapter();
            }
        }

        private void EndChapter() 
        {
            dialoguePanel.SetActive(false);
            Debug.Log($"[DialogueController] 章节结束: {currentChapterData.chapterName}");
            Debug.Log($"[GameManager] 最终卵完整度: {GameManager.Instance.eggIntegrity}");
        }
    }
}
