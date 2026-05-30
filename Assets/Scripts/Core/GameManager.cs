using UnityEngine;

namespace TenshiNoTamago.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("运行时状态")]
        public int eggIntegrity;           // 卵完整度
        public string currentChapter;      // 当前章节名
        public int currentFrameId;         // 当前帧ID
        public int lastEndingType = 0;  // 0=未知, 1=高完整度, 2=中完整度, 3=低完整度

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
        }

        public void AddEggIntegrity(int delta) {
            eggIntegrity += delta;
            Debug.Log($"[GameManager] 卵完整度: {eggIntegrity} (变化: {delta})");
        }

        public void SetCurrentFrame(string chapter, int frameId) {
            currentChapter = chapter;
            currentFrameId = frameId;
        }

        public void ResetGame() {
            eggIntegrity = 0;
            currentChapter = "prologue"; //序章
            currentFrameId = 1;
        }
    }
}
