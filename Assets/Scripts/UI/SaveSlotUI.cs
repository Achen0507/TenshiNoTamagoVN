using System.IO;
using UnityEngine;
using UnityEngine.UI;
using static TenshiNoTamago.Core.GameManager;

namespace TenshiNoTamago.UI
{
    public class SaveSlotUI : MonoBehaviour
    {
        [Header("UI 组件")]
        public Text slotIndexText;      // 槽位编号，例如 "槽位 1"
        public Text chapterNameText;
        public Text playTimeText;
        public Text eggIntegrityText;
        public Image thumbnailImage;    // 缩略图

        private int slotIndex;
        private SaveData data;
        private SaveLoadController controller;
        private bool isSaveMode;
        public int GetSlotIndex() => slotIndex;

        public void Setup(int index, SaveData saveData, bool saveMode, SaveLoadController parentController)
        {
            slotIndex = index;
            data = saveData;
            isSaveMode = saveMode;
            this.controller = parentController;

            slotIndexText.text = $"SLOT  {index + 1}";

            if (data != null)
            {
                // 有存档
                chapterNameText.text = data.chapterName;
                playTimeText.text = data.saveTime;
                eggIntegrityText.text = $"EggIntegrity : {data.eggIntegrity}";

                // 加载缩略图
                string thumbPath = Application.persistentDataPath + $"/thumb_{slotIndex}.png";
                if (File.Exists(thumbPath))
                {
                    byte[] bytes = File.ReadAllBytes(thumbPath);
                    Texture2D tex = new Texture2D(2, 2);
                    tex.LoadImage(bytes);
                    thumbnailImage.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                }
            }
            else
            {
                chapterNameText.text = "";
                playTimeText.text = "";
                eggIntegrityText.text = "";
            }

            Button slotButton = GetComponent<Button>();
            if (slotButton != null)
            {
                slotButton.onClick.RemoveAllListeners();
                slotButton.onClick.AddListener(() => controller.SelectSlot(slotIndex, this));
            }
        }

        public void SetHighlight(bool isSelected)
        {
            Image bg = GetComponent<Image>();
            if (bg != null)
            {
                Color color = bg.color;
                if (isSelected)
                {
                    color.a = 1f;   // 完全不透明
                }
                else
                {
                    color.a = 0.5f;
                }
                bg.color = color;
            }
        }
    }
}
