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

        [Header("≈‰÷√±Ì")]
        [SerializeField] private AmbienceEvent[] events;

        private void Start()
        {
            if (DialogueController.OnFrameChanged != null) 
                DialogueController.OnFrameChanged += OnFrameChanged;
        }

        private void OnDestroy()
        {
            if (DialogueController.OnFrameChanged != null)
                DialogueController.OnFrameChanged -= OnFrameChanged;
        }

        private void OnFrameChanged(string chapter, int frameId) {
            foreach (var evt in events) {
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
