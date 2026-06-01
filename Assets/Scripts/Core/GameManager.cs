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
        public string pendingAmbienceKey;


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

        public void SaveGame(int slotIndex) {  // Application.persistentDataPath
            SaveData data =new SaveData();
            data.chapterName = currentChapter;
            data.frameId = currentFrameId;
            data.eggIntegrity = eggIntegrity;
            data.saveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            data.ambienceKey = AudioManager.Instance.GetCurrentAmbienceKey();

            string json =JsonUtility.ToJson(data);
            string path = Application.persistentDataPath + "/save_" + slotIndex + ".json";
            System.IO.File.WriteAllText(path, json);
        }

        public SaveData LoadGame(int slotIndex) {
            string path = Application.persistentDataPath + "/save_" + slotIndex + ".json";
            if (System.IO.File.Exists(path))
            {
                string json = System.IO.File.ReadAllText(path);
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                Debug.Log($"从槽位 {slotIndex} 加载存档");
                return data;
            }
            else {
                Debug.LogWarning($"存档槽位 {slotIndex} 不存在");
                return null;
            }
        }

        public void LoadAndApply(int slotIndex) {
            SaveData data = LoadGame(slotIndex);
            if (data != null) {
                currentChapter = data.chapterName;
                currentFrameId = data.frameId;
                eggIntegrity = data.eggIntegrity;

                pendingAmbienceKey = data.ambienceKey;

                Time.timeScale = 1f;
                UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
            }
        }

        public void ResetGame() {
            eggIntegrity = 0;
            currentChapter = "prologue"; //序章
            currentFrameId = 0;
        }

        [System.Serializable]
        public class SaveData
        {
            public string chapterName;
            public int frameId;
            public int eggIntegrity;
            public string saveTime; 
            public string ambienceKey;   // 当前播放的环境音
        }
    }
}
