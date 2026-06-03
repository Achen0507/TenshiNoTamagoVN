using System.IO;
using TenshiNoTamago.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static TenshiNoTamago.Core.GameManager;

namespace TenshiNoTamago.UI
{
    public class SaveLoadController : MonoBehaviour
    {
        [Header("模式")]
        [SerializeField] private bool isSaveMode; //true=存档, false=读档

        [Header("UI 组件")]
        [SerializeField] private Transform slotContainer;
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button backButton;
        [SerializeField] private Text titleText;

        private int selectedSlot = -1;
        private SaveSlotUI[] slotUIs;

        private void Start()
        {
            isSaveMode = (SaveLoadManager.currentMode == SaveLoadManager.Mode.Save);

            UpdateUITexts();

            RefreshSlotList();

            if (confirmButton != null) confirmButton.onClick.AddListener(OnConfirm);
            if (backButton != null) backButton.onClick.AddListener(OnBack);
        }

        private void UpdateUITexts() {
            if (titleText != null)
                titleText.text = LanguageManager.Get(isSaveMode ? "savetitle" : "loadtitle");

            if (confirmButton != null && confirmButton.GetComponentInChildren<Text>() != null)
                confirmButton.GetComponentInChildren<Text>().text = LanguageManager.Get("confirm");

            if (backButton != null && backButton.GetComponentInChildren<Text>() != null)
                backButton.GetComponentInChildren<Text>().text = LanguageManager.Get("back");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnBack(); 
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                AudioManager.Instance.PlaySFX("click");
                if (slotUIs == null || slotUIs.Length == 0) return;
                selectedSlot--;
                if (selectedSlot < 0) selectedSlot = slotUIs.Length - 1;
                UpdateAllHighlights();
                ScrollToSlot(selectedSlot);
                slotUIs[selectedSlot].GetComponent<Button>()?.Select();
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                AudioManager.Instance.PlaySFX("click");
                if (slotUIs == null || slotUIs.Length == 0) return;
                selectedSlot++;
                if (selectedSlot >= slotUIs.Length) selectedSlot = 0;
                UpdateAllHighlights();
                ScrollToSlot(selectedSlot);
                slotUIs[selectedSlot].GetComponent<Button>()?.Select();
            }
        

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                OnConfirm();
            }
        }

        private void ScrollToSlot(int slotIndex)
        {
            if (slotUIs == null || slotUIs.Length == 0) return;

            ScrollRect scrollRect = slotContainer.GetComponentInParent<ScrollRect>();
            if (scrollRect == null) return;

            float totalHeight = slotUIs.Length * 250f; 
            float targetPos = (slotIndex * 250f) / totalHeight;

            scrollRect.verticalNormalizedPosition = 1 - targetPos;
        }

        void RefreshSlotList()
        {
            foreach (Transform child in slotContainer)
            {
                Destroy(child.gameObject);
            }

            slotUIs = new SaveSlotUI[5];

            for (int i = 0; i < 5; i++)
            {
                SaveData data = GameManager.Instance.LoadGame(i);
                GameObject slotObj = Instantiate(slotPrefab, slotContainer);
                SaveSlotUI slotUI = slotObj.GetComponent<SaveSlotUI>();
                slotUI.Setup(i, data, isSaveMode, this);
                slotUIs[i] = slotUI;
            }

            for (int i = 0; i < slotUIs.Length; i++)
            {
                Button slotButton = slotUIs[i].GetComponent<Button>();
                if (slotButton == null) continue;

                int capturedIndex = i;
                slotButton.onClick.RemoveAllListeners();
                slotButton.onClick.AddListener(() => SelectSlot(capturedIndex, slotUIs[capturedIndex]));
            }

            RectTransform contentRect = slotContainer.GetComponent<RectTransform>();
            float slotHeight = 250f;
            float spacing = 0;
            float totalHeight = slotUIs.Length * slotHeight + (slotUIs.Length - 1) * spacing;
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, totalHeight);

            if (slotUIs.Length > 0)
            {
                selectedSlot = 0;
                UpdateAllHighlights();
                Button firstButton = slotUIs[0].GetComponent<Button>();
                if (firstButton != null) firstButton.Select();
            }
        }

        public void SelectSlot(int slotIndex, SaveSlotUI selectedUI)
        {
            selectedSlot = slotIndex;
            UpdateAllHighlights();
        }

        private void UpdateAllHighlights()
        {
            if (slotUIs == null) return;
            foreach (var slotUI in slotUIs)
            {
                if (slotUI != null)
                {
                    slotUI.SetHighlight(slotUI.GetSlotIndex() == selectedSlot);
                }
            }
        }

        void OnConfirm() {
            AudioManager.Instance.PlaySFX("click");

            if (selectedSlot == -1)
            {
                Debug.Log("请先选择一个存档槽位");
                return;
            }
            if (isSaveMode)
            {
                if (!string.IsNullOrEmpty(SaveLoadManager.tempThumbnailPath))
                {
                    string destPath = Application.persistentDataPath + $"/thumb_{selectedSlot}.png";
                    if (File.Exists(SaveLoadManager.tempThumbnailPath))
                    {
                        File.Copy(SaveLoadManager.tempThumbnailPath, destPath, true);
                    }
                }

                GameManager.Instance.SaveGame(selectedSlot);
                RefreshSlotList();
            }
            else
            {
                SaveData data = GameManager.Instance.LoadGame(selectedSlot);
                if (data != null)
                {
                    GameManager.Instance.LoadAndApply(selectedSlot);
                }
                else
                {
                    Debug.Log("空档无法读取");
                }
            }
        }

        void OnBack()
        {
            if (SaveLoadManager.currentSource == SaveLoadManager.EntrySource.PauseMenu)
            {
                SceneManager.LoadScene("SampleScene");  
            }
            else
            {
                SceneManager.LoadScene("MainMenu");  
            }
        }
    }
}
