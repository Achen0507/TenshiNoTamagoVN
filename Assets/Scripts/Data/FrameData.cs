using System;

namespace TenshiNoTamago.Data
{
    [Serializable]
    public class OptionData
    {
        public string text;
        public string descriptionOnSelect;
        public int eggDelta;
        public int nextFrameId;  // 跳转到指定帧（-1表示顺序下一帧）
    }

    [Serializable]
    public class FrameData
    {
        public int id;
        public string backgroundPath;
        public string characterSpritePath;   // 立绘
        public string characterPosition;     // left / right / center
        public string descriptionText;
        public float autoNextSeconds;
        public OptionData[] options;
        public int eggDelta;
        public bool isAnimation;
        public float animationSpeed;
        public bool loopAnimation;   // true=循环播放，false=播一次停住
        public int nextFrameId = -1;
        public string sfxKey;
    }

    [Serializable]
    public class ChapterData
    {
        public string chapterName;
        public FrameData[] frames;
    }
}