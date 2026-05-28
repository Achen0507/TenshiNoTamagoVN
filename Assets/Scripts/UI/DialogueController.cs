using DG.Tweening;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        [SerializeField] private GameObject nextIndicator;   

        [Header("章节数据")]
        [SerializeField] private string chapterToLoad = "prologue";

        [Header("淡入淡出")]
        [SerializeField] private Image fadePanel;   
        [SerializeField] private GameObject titleObject;

        [Header("背景飘移")]
        [SerializeField] private bool enableParallax = true;
        [SerializeField] private float moveRangePercent = 0.5f;
        [SerializeField] private float moveDuration = 12f;    

        private ChapterData currentChapterData;
        private FrameData currentFrame;
        private bool isWaitingForInput = false;
        private float autoNextTimer = 0f;
        private int pendingNextFrameId = -1;  // 待跳转的帧ID
        private bool isTextFullyDisplayed = false;  // 当前帧文字是否已完整显示

        private bool titleShown = false;

        private Vector3 originalBgPosition;
        private Tween bgTweenX;
        private Tween bgTweenY;

        private void Start()
        {
            originalBgPosition = backgroundImage.rectTransform.anchoredPosition;

            float screenWidth = Screen.width;
            float actualMoveRange = screenWidth * moveRangePercent / 100f;

            if (enableParallax)
            {
                StartMicroMotion(actualMoveRange);
            }

            LoadChapter(chapterToLoad);
        }

        private void StartMicroMotion(float range)
        {
            bgTweenX = backgroundImage.rectTransform.DOAnchorPosX(originalBgPosition.x + range, moveDuration)
        .SetEase(Ease.InOutSine)
        .SetLoops(-1, LoopType.Yoyo);

            bgTweenY = backgroundImage.rectTransform.DOAnchorPosY(originalBgPosition.y + range * 0.6f, moveDuration * 1.3f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void OnDestroy()
        {
            bgTweenX?.Kill();
            bgTweenY?.Kill();
        }

        private void Update()
        {
            if (isWaitingForInput)
            {
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
                    if (!isTextFullyDisplayed)
                    {
                        StopCoroutine(typingCoroutine);
                        descriptionText.text = currentFrame.descriptionText;
                        isTextFullyDisplayed = true;
                    }
                    else
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
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
                animationCoroutine = null;
            }

            currentFrame = frame;
            isTextFullyDisplayed = false;

            // 应用本帧的卵完整度变化
            if (frame.eggDelta != 0)
            {
                GameManager.Instance.AddEggIntegrity(frame.eggDelta);
            }

            if (chapterToLoad == "prologue" && frame.id == 37 && !titleShown)
            {
                titleShown = true;
                ShowTitle();
                return; 
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
                    nextIndicator.SetActive(false);
                }
                else
                {
                    ClearOptions();
                    isWaitingForInput = true;
                    autoNextTimer = frame.autoNextSeconds;

                    if (autoNextTimer == 0)
                    {
                        nextIndicator.SetActive(true);
                        _ = BlinkIndicatorAsync();
                    }
                    else {
                        nextIndicator.SetActive(false);
                    }
                }
            });

            // 处理立绘TODO
        }

        private CancellationTokenSource blinkCts;
        private async Task BlinkIndicatorAsync()
        {
            blinkCts?.Cancel();
            blinkCts = new CancellationTokenSource();
            var token = blinkCts.Token;

            Text indicatorText = nextIndicator.GetComponent<Text>();
            if (indicatorText == null) return;

            indicatorText.color = new Color(indicatorText.color.r, indicatorText.color.g, indicatorText.color.b, 1f);

            while (nextIndicator != null && nextIndicator.activeSelf && !token.IsCancellationRequested)
            {
                await indicatorText.DOFade(0.3f, 0.5f).AsyncWaitForCompletion();
                if (token.IsCancellationRequested) break;
                await indicatorText.DOFade(1f, 0.5f).AsyncWaitForCompletion();
                if (token.IsCancellationRequested) break;
                await Task.Delay(500, token);
            }
            if (indicatorText != null && !token.IsCancellationRequested)
            {
                indicatorText.color = new Color(indicatorText.color.r, indicatorText.color.g, indicatorText.color.b, 1f);
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

            ClearOptions();

            if (!string.IsNullOrEmpty(option.descriptionOnSelect))
            {
                SetDescriptionText(option.descriptionOnSelect, () =>
                {
                    // 显示追加文字后的逻辑：没有选项，直接进入等待点击
                    isWaitingForInput = true;
                    autoNextTimer = 0;

                    // 显示箭头
                    nextIndicator.SetActive(true);
                    _ = BlinkIndicatorAsync();

                    pendingNextFrameId = option.nextFrameId;
                });
            }
            else
            {
                // 没有追加文字，直接跳转
                if (option.nextFrameId != -1)
                    JumpToFrame(option.nextFrameId);
                else
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

        private void ShowTitle()
        {
            isWaitingForInput = false;

            fadePanel.DOFade(1f, 1f).OnComplete(() =>
            {
                titleObject.SetActive(true);

                CanvasGroup titleGroup = titleObject.GetComponent<CanvasGroup>();
                if (titleGroup == null)
                    titleGroup = titleObject.AddComponent<CanvasGroup>();

                titleGroup.alpha = 0;
                titleGroup.DOFade(1f, 1f).OnComplete(() =>
                {
                    DOVirtual.DelayedCall(1.5f, () =>
                    {
                        titleGroup.DOFade(0f, 1f).OnComplete(() =>
                        {
                            titleObject.SetActive(false);

                            LoadChapter("chapter1");

                            fadePanel.DOFade(0f, 1f);
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
            isTextFullyDisplayed = false;

            if (string.IsNullOrEmpty(text))
            {
                descriptionText.text = "";
                isTextFullyDisplayed = true;
                onComplete?.Invoke();
                return;
            }
            typingCoroutine = StartCoroutine(TypeText(text,onComplete));
        }

        private IEnumerator TypeText(string fullText,System.Action onComplete) {
            descriptionText.text = "";
            float typeSpeed = 0.1f;

            foreach (char c in fullText)
            {
                descriptionText.text += c;
                if (c == '。' || c == '、' || c == '…') typeSpeed = 0.2f;
                else typeSpeed = 0.1f;
                yield return new WaitForSeconds(typeSpeed); 
            }

            isTextFullyDisplayed = true;
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

            if (frame.loopAnimation)
            {
                while (true)
                {
                    backgroundImage.sprite = frames[current];
                    timer += Time.deltaTime;
                    if (timer >= frame.animationSpeed)
                    {
                        timer = 0;
                        current++;
                        if (current >= frames.Length) current = 0;
                    }
                    yield return null;
                }
            }
            else {
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
                animationCoroutine = null;
            }           
        }
    }
}
