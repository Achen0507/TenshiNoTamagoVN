using TenshiNoTamago.Core;
using TenshiNoTamago.UI;
using UnityEngine;

namespace TenshiNoTamago.Audio
{
    public class AmbienceController : MonoBehaviour
    {
        [System.Serializable]
        public class AmbienceEvent {
            public string chapterName;
            public int frameId;
            public string ambienceKey;
            public bool loop;
        }

        [Header("配置表")]
        [SerializeField] private AmbienceEvent[] events;

        private void Start()
        {
            Debug.Log("AmbienceController Start 被调用");
            if (DialogueController.OnFrameChanged != null) 
                DialogueController.OnFrameChanged += OnFrameChanged;
        }

        private void OnDestroy()
        {
            if (DialogueController.OnFrameChanged != null)
                DialogueController.OnFrameChanged -= OnFrameChanged;
        }

        private void OnFrameChanged(string chapter, int frameId) {
            Debug.Log($"收到事件: 章节={chapter}, 帧={frameId}");
            foreach (var evt in events) {
                Debug.Log($"检查配置: 章节={evt.chapterName}, 帧={evt.frameId}, key={evt.ambienceKey}");
                if (evt.chapterName == chapter && evt.frameId == frameId) {
                    if (string.IsNullOrEmpty(evt.ambienceKey))
                    {
                        AudioManager.Instance.StopAmbience();
                    }
                    else {
                        AudioManager.Instance.PlayAmbience(evt.ambienceKey, evt.loop);
                    }
                    break;
                }
            }
        }
    }
}
