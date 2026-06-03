using DG.Tweening;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using TenshiNoTamago.Core;
using TenshiNoTamago.Data;
using TenshiNoTamago.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TenshiNoTamago.UI
{
    /// <summary>
    /// 对话控制器：管理帧的显示和切换 
    /// </summary>
    public class DialogueController : MonoBehaviour
    {
        public static DialogueController Instance { get; private set; }

        [Header("UI 组件")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Text descriptionText;
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private Transform optionsContainer;      // 选项按钮的父容器
        [SerializeField] private GameObject optionButtonPrefab;   // 选项按钮预制体
        [SerializeField] private GameObject nextIndicator;

        [Header("立绘")]
        [SerializeField] private Image characterImage;        
        [SerializeField] private Transform characterLeft;    
        [SerializeField] private Transform characterRight; 

        [Header("章节数据")]
        [SerializeField] private string chapterToLoad = "prologue";

        [Header("淡入淡出")]
        [SerializeField] private Image fadePanel;   
        [SerializeField] private GameObject titleObject;

        [Header("背景飘移")]
        [SerializeField] private bool enableParallax = true;
        [SerializeField] private float moveRangePercent = 0.5f;
        [SerializeField] private float moveDuration = 12f;

        [Header("自动模式提示")]
        [SerializeField] private CanvasGroup autoModeHint;

        private ChapterData currentChapterData;
        private FrameData currentFrame;
        private bool isWaitingForInput = false;
        private float autoNextTimer = 0f;
        private int pendingNextFrameId = -1;  // 待跳转的帧ID
        private bool isTextFullyDisplayed = false;  // 当前帧文字是否已完整显示
        private AudioSource currentVoiceSource; //当前播放语音
        public static System.Action<string, int> OnFrameChanged;  // 氛围音乐

        private bool titleShown = false;
        public bool canInput = true;

        private Vector3 originalBgPosition;
        private Tween bgTweenX;
        private Tween bgTweenY;

        private void Awake()
        {
            if (Instance == null) {
                Instance = this;
            }

            if (OnFrameChanged == null)
                OnFrameChanged = delegate { };
        }

        private void Start()
        {
            Debug.Log("DialogueController Start 被调用");
            int langIndex =PlayerPrefs.GetInt("LanguageIndex", 0);
            JsonLoader.currentLanguage = langIndex == 1 ? "ja" : "zh";
            Debug.Log($"当前语言: {JsonLoader.currentLanguage}");

            if (!string.IsNullOrEmpty(GameManager.Instance.currentChapter))
            {
                LoadChapter(GameManager.Instance.currentChapter);
            }
            else
            {
                LoadChapter(chapterToLoad);
            }

            if (!string.IsNullOrEmpty(GameManager.Instance.pendingAmbienceKey))
            {
                AudioManager.Instance.PlayAmbience(GameManager.Instance.pendingAmbienceKey, true);
                GameManager.Instance.pendingAmbienceKey = null;
            }

            originalBgPosition = backgroundImage.rectTransform.anchoredPosition;

            float screenWidth = Screen.width;
            float actualMoveRange = screenWidth * moveRangePercent / 100f;

            if (enableParallax)
            {
                StartMicroMotion(actualMoveRange);
            }

            UpdateAutoModeHint();
        }

        private void UpdateAutoModeHint()
        {
            bool isAuto = PlayerPrefs.GetInt("AutoPlay", 0) == 1;
            if (autoModeHint != null)
            {
                autoModeHint.gameObject.SetActive(isAuto);
                if (isAuto)
                {
                    autoModeHint.DOFade(0.3f, 1f).SetLoops(-1, LoopType.Yoyo);
                }
                else
                {
                    autoModeHint.DOKill();
                    autoModeHint.alpha = 1f;
                }
            }
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
            if (!canInput) return;

            if (currentFrame != null && currentFrame.options != null && currentFrame.options.Length > 0) {
                return;
            }

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

            if (GameManager.Instance.currentFrameId > 0)
            {
                FrameData targetFrame = System.Array.Find(currentChapterData.frames, f => f.id == GameManager.Instance.currentFrameId);
                if (targetFrame != null)
                {
                    ShowFrame(targetFrame);
                }
                else
                {
                    Debug.LogWarning($"找不到帧 {GameManager.Instance.currentFrameId}，从第一帧开始");
                    ShowFrame(currentChapterData.frames[0]);
                }
            }
            else
            {
                ShowFrame(currentChapterData.frames[0]);
            }
        }

        private void ShowFrame(FrameData frame)
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
                animationCoroutine = null;
            }

            currentFrame = frame;
            GameManager.Instance.currentChapter = currentChapterData.chapterName;
            GameManager.Instance.currentFrameId = frame.id;
            isTextFullyDisplayed = false;

            if (chapterToLoad == "Prologue" && frame.id == 37 && !titleShown)
            {
                titleShown = true;
                ShowTitle();
                return;
            }

            // 应用本帧的卵完整度变化
            if (frame.eggDelta != 0)
            {
                GameManager.Instance.AddEggIntegrity(frame.eggDelta);
            }

            if (!string.IsNullOrEmpty(frame.backgroundPath))
            {
                if (frame.isAnimation)
                {
                    PlayAnimation(frame);
                }
                else
                {
                    _ = LoadBackgroundAsync(frame.backgroundPath);
                }
            }

            // 播放本帧音效
            if (!string.IsNullOrEmpty(frame.sfxKey))
            {
                PlayVoice(frame.sfxKey);
            }

            OnFrameChanged?.Invoke(currentChapterData.chapterName, frame.id);

            SetDescriptionText(frame.descriptionText, () =>
            {
                if (frame.options != null && frame.options.Length > 0)
                {
                    if (PlayerPrefs.GetInt("AutoPlay", 0) == 1)
                    {
                        OnOptionSelected(frame.options[0]);
                    }
                    else {
                        ShowOptions(frame.options);
                        isWaitingForInput = false;
                        nextIndicator.SetActive(false);
                    }          
                }
                else
                {
                    ClearOptions();
                    isWaitingForInput = true;

                    float delay = frame.autoNextSeconds;

                    if (PlayerPrefs.GetInt("AutoPlay", 0) == 1)
                    {
                        if (delay < 2f) delay = 2f;
                    }

                    autoNextTimer = delay;

                    if (autoNextTimer == 0)
                    {
                        nextIndicator.SetActive(true);
                        _ = BlinkIndicatorAsync();
                    }
                    else
                    {
                        nextIndicator.SetActive(false);
                    }
                }
            });

            // 处理立绘
            if (!string.IsNullOrEmpty(frame.characterSpritePath))
            {
                _ = LoadCharacterAsync(frame.characterSpritePath, frame.characterPosition);
            }
            else
            {
                characterImage.gameObject.SetActive(false);
            }
        }

        private async Task LoadBackgroundAsync(string path) {
            Sprite bg = await ResourceCache.LoadSpriteAsync(path);
            if (bg != null && backgroundImage != null) backgroundImage.sprite = bg;
            else Debug.LogWarning($"Background not found: {path}");
        }

        private async Task LoadCharacterAsync(string path, string position)
        {
            Sprite characterSprite = await ResourceCache.LoadSpriteAsync(path);
            if (characterSprite != null)
            {
                characterImage.sprite = characterSprite;
                characterImage.gameObject.SetActive(true);

                switch (position)
                {
                    case "left":
                        characterImage.rectTransform.position = characterLeft.position;
                        break;
                    case "right":
                        characterImage.rectTransform.position = characterRight.position;
                        break;
                }
            }
            else
            {
                Debug.LogWarning($"立绘未找到: {path}");
                characterImage.gameObject.SetActive(false);
            }
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

            //自动模式
            if (PlayerPrefs.GetInt("AutoPlay", 0) == 1) {
                if (option.nextFrameId != -1)
                    JumpToFrame(option.nextFrameId);
                else
                    AdvanceToNextFrame();

                currentFrame.options = null;
                return;
            }

            //手动模式
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
            currentFrame.options = null;
        }

        private void AdvanceToNextFrame()
        {
            //优先处理 9999（结局分支入口）
            if (currentFrame.nextFrameId == 9999)
            {
                GotoEnding();
                return;
            }

            if (currentFrame.nextFrameId != -1 && currentFrame.nextFrameId != 0)
            {
                JumpToFrame(currentFrame.nextFrameId);
                return;
            }

            int currentIndex = System.Array.FindIndex(currentChapterData.frames, f => f.id == currentFrame.id);
            if (currentIndex >= 0 && currentIndex + 1 < currentChapterData.frames.Length)
            {
                ShowFrame(currentChapterData.frames[currentIndex + 1]);
            }
            else
            {
                EndChapter();
            }
        }

        private void EndChapter() 
        {
            dialoguePanel.SetActive(false);
            PlayerPrefs.SetInt("LastEndingType", GameManager.Instance.lastEndingType);
            PlayerPrefs.Save();
            Debug.Log($"[DialogueController] 章节结束: {currentChapterData.chapterName}");
            Debug.Log($"[GameManager] 最终卵完整度: {GameManager.Instance.eggIntegrity}");

            if (currentChapterData.chapterName == "Chapter1")
            {
                LoadChapter("chapter2");
            }
            else if (currentChapterData.chapterName == "Chapter2")
            {
                LoadChapter("chapter3");
            }
            else if (currentChapterData.chapterName == "Chapter3")
            {
                Debug.Log("游戏结束");
                SceneManager.LoadScene("MainMenu");
            }
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

                            GameManager.Instance.currentFrameId = 0;

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

            float speedValue = PlayerPrefs.GetFloat("TextSpeed", 7.5f);
            float baseSpeed = 0.18f - (speedValue / 15f) * 0.15f;

            foreach (char c in fullText)
            {
                descriptionText.text += c;
                float typeSpeed = baseSpeed;

                if (c == '。' || c == '、' || c == '…') typeSpeed = baseSpeed * 1.5f;

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

        private void GotoEnding()
        {
            int egg = GameManager.Instance.eggIntegrity;
            int targetId = -1;

            // 判断是从哪个入口进入的
            if (currentFrame.id == 201)
            {
                // 女孩雕塑分支
                if (egg >= 60)
                {
                    GameManager.Instance.lastEndingType = 1;
                    targetId = 2021;
                }
                else if (egg >= 30)
                {
                    GameManager.Instance.lastEndingType = 2;
                    targetId = 2022;
                }
                else
                {
                    GameManager.Instance.lastEndingType = 3;
                    targetId = 2023;
                }
            }
            else if (currentFrame.id == 214)
            {
                // 树与蛋分支
                if (egg >= 60)
                {
                    GameManager.Instance.lastEndingType = 1;
                    targetId = 2151;
                }
                else if (egg >= 30)
                {
                    GameManager.Instance.lastEndingType = 2;
                    targetId = 2152;
                }
                else
                {
                    GameManager.Instance.lastEndingType = 3;
                    targetId = 2153;
                }
            }
            else if (currentFrame.id == 148)  
            {
                // 砸蛋分支
                if (egg >= 60)
                {
                    GameManager.Instance.lastEndingType = 1;
                    targetId = 1491;
                }
                else if (egg >= 30)
                {
                    GameManager.Instance.lastEndingType = 2;
                    targetId = 1492;
                }
                else
                {
                    GameManager.Instance.lastEndingType = 3;
                    targetId = 1493;
                }
            }
            else
            {
                Debug.LogWarning($"GotoEnding: 未知的入口帧 {currentFrame.id}");
            }

            if (targetId != -1)
            {
                JumpToFrame(targetId);
            }
        }

        private void PlayVoice(string sfxKey)
        {
            if (currentVoiceSource != null && currentVoiceSource.isPlaying)
            {
                currentVoiceSource.Stop();
            }

            currentVoiceSource = AudioManager.Instance.PlaySFXAndReturnSource(sfxKey);
        }

        public void ReloadCurrentChapter() {
            Debug.Log($"ReloadCurrentChapter 被调用，当前语言: {JsonLoader.currentLanguage}");
            int currentId = currentFrame.id;
            LoadChapter(GameManager.Instance.currentChapter);
            JumpToFrame(currentId);
        }
    }
}
