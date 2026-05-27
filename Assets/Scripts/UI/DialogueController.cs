using DG.Tweening;
using System.Collections;
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

        [Header("淡入淡出")]
        [SerializeField] private Image fadePanel;   
        [SerializeField] private GameObject titleObject; 

        private ChapterData currentChapterData;
        private FrameData currentFrame;
        private bool isWaitingForInput = false;
        private float autoNextTimer = 0f;
        private bool isTyping = false;
        private int pendingNextFrameId = -1;  // 待跳转的帧ID
        private bool titleShown = false;

        private void Start()
        {
            LoadChapter(chapterToLoad);
        }

        private void Update()
        {
            if (isWaitingForInput)
            {
                // 自动跳转逻辑
                if (autoNextTimer > 0)
                {
                    autoNextTimer -= Time.deltaTime;
                    if (autoNextTimer <= 0)
                    {
                        AdvanceToNextFrame();
                    }
                }
                // 点击跳转逻辑
                else if (Input.GetMouseButtonDown(0))
                {
                    if (pendingNextFrameId != -1)
                    {
                        JumpToFrame(pendingNextFrameId);
                        pendingNextFrameId = -1;
                    }
                    else
                    {
                        AdvanceToNextFrame();
                    }
                }
            }
        }

        private void JumpToFrame(int frameId) {
            FrameData targetFrame = System.Array.Find(currentChapterData.frames, f => f.id == frameId);
            if (targetFrame != null)
            {
                ShowFrame(targetFrame);
            }
            else
            {
                Debug.LogError($"找不到帧 {frameId}");
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

            if (frame.id == 37 && !titleShown)
            {
                titleShown = true;
                ShowTitle();
                return;  // 标题的变黑
            }

            if (!string.IsNullOrEmpty(frame.backgroundPath)) {
                if (frame.isAnimation)
                {
                    PlayAnimation(frame);
                }
                else
                {
                    Sprite bg = Resources.Load<Sprite>(frame.backgroundPath);
                    if (bg != null) backgroundImage.sprite = bg;
                    else Debug.LogWarning($"[DialogueController] Background not found: {frame.backgroundPath}");
                }
            }

            SetDescriptionText(frame.descriptionText, () =>
            {
                if (frame.options != null && frame.options.Length > 0)
                {
                    ShowOptions(frame.options);
                    isWaitingForInput = false;
                }
                else
                {
                    ClearOptions();
                    isWaitingForInput = true;
                    autoNextTimer = frame.autoNextSeconds;
                }
            });

            // 处理立绘TODO
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
            ClearOptions();

            if (!string.IsNullOrEmpty(option.descriptionOnSelect))
            {
                SetDescriptionText(option.descriptionOnSelect);
                pendingNextFrameId = option.nextFrameId;
                isWaitingForInput = true;  // 切换到等待点击状态
            }
            else {
                // 没有追加文字：直接跳转
                if (option.nextFrameId != -1)
                {
                    JumpToFrame(option.nextFrameId);
                }
                else
                {
                    AdvanceToNextFrame();
                }
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

        private void ShowTitle()
        {
            isWaitingForInput = false;

            Debug.Log("ShowTitle 开始");
            // 1. 先变黑（底图被遮住）
            fadePanel.DOFade(1f, 1f).OnComplete(() =>
            {
                Debug.Log("黑屏完成，准备显示标题");
                // 2. 黑屏后，标题慢慢淡入
                titleObject.SetActive(true);
                Debug.Log($"titleObject active: {titleObject.activeSelf}");
                CanvasGroup titleGroup = titleObject.GetComponent<CanvasGroup>();
                if (titleGroup == null)
                    titleGroup = titleObject.AddComponent<CanvasGroup>();
                Debug.Log($"CanvasGroup alpha 初始: {titleGroup.alpha}");

                titleGroup.alpha = 0;
                titleGroup.DOFade(1f, 1f).OnComplete(() =>
                {
                    // 3. 标题停留 1.5 秒
                    DOVirtual.DelayedCall(1.5f, () =>
                    {
                        // 4. 标题慢慢淡出
                        titleGroup.DOFade(0f, 1f).OnComplete(() =>
                        {
                            titleObject.SetActive(false);

                            // 5. 换第一章背景
                            Sprite chapter1Bg = Resources.Load<Sprite>("Image/Backgrounds/1_1");
                            backgroundImage.sprite = chapter1Bg;

                            // 6. 黑屏淡出，显示第一章
                            fadePanel.DOFade(0f, 1f).OnComplete(() =>
                            {
                                AdvanceToNextFrame();
                            });
                        });
                    });
                });
            });
        }

        //打字机效果
        private Coroutine typingCoroutine;

        private void SetDescriptionText(string text,System.Action onComplete =null)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);

            if (string.IsNullOrEmpty(text))
            {
                descriptionText.text = "";
                onComplete?.Invoke();
                return;
            }

            typingCoroutine = StartCoroutine(TypeText(text,onComplete));
        }

        private IEnumerator TypeText(string fullText,System.Action onComplete) {
            isTyping = true;
            descriptionText.text = "";
            float typeSpeed = 0.1f;  // 每个字间隔秒数
            foreach (char c in fullText)
            {
                descriptionText.text += c;
                yield return new WaitForSeconds(typeSpeed); 
            }
            isTyping = false;
            onComplete?.Invoke();
        }

        //序列帧模拟动画
        private Coroutine animationCoroutine;

        private void PlayAnimation(FrameData frame) {
            if (animationCoroutine != null)
                StopCoroutine(animationCoroutine);
            animationCoroutine = StartCoroutine(PlaySequence(frame));
        }

        private IEnumerator PlaySequence(FrameData frame) {
            Sprite[] frames = Resources.LoadAll<Sprite>(frame.backgroundPath);
            if (frames == null || frames.Length == 0)
            {
                Debug.LogWarning($"未找到动画序列: {frame.backgroundPath}");
                yield break;
            }
            int current = 0;
            float timer = 0;

            while (current < frames.Length)
            {
                backgroundImage.sprite = frames[current];
                timer += Time.deltaTime;

                if (timer >= frame.animationSpeed)
                {
                    timer = 0;
                    current++;
                }
                yield return null;
            }
        }
    }
}
