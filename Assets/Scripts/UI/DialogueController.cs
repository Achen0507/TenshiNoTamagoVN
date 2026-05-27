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

        [Header("测试数据（临时）")]
        [SerializeField] private Sprite[] testBackgrounds;
        [SerializeField] private string[] testDescriptions;

        private int currentFrameIndex = -1;
        private bool isWaitingForInput = false;

        private void Start()
        {
            ShowNextFrame();
        }

        private void Update()
        {
            if (isWaitingForInput && Input.GetMouseButtonDown(0)) {
                ShowNextFrame();
            }
        }

        /// <summary>
        /// 显示下一帧
        /// </summary>
        private void ShowNextFrame() {
            currentFrameIndex++;

            // 检查是否结束
            if (currentFrameIndex >= testDescriptions.Length)
            {
                EndDialogue();
                return;
            }

            descriptionText.text = testDescriptions[currentFrameIndex];

            if (testBackgrounds != null && currentFrameIndex < testBackgrounds.Length) {
                backgroundImage.sprite = testBackgrounds[currentFrameIndex];
            }
            isWaitingForInput = true;
        }

        /// <summary>
        /// 对话结束
        /// </summary>
        private void EndDialogue() 
        {
            dialoguePanel.SetActive(false);
            isWaitingForInput = false;
            Debug.Log("[DialogueController] 序章结束");
        }
    }
}
