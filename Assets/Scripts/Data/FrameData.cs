namespace TenshiNoTamago.Data
{
    [System.Serializable]
    public class FrameData 
    {
        public int id;
        public string backgroundPath;
        public string descriptionText;
        public float autoNextSeconds;   // 0=等待点击，>0=自动跳转
        public OptionData[] options;  // 选项列表
        public int eggDelta;
    }

    [System.Serializable]
    public class OptionData
    {
        public string text;
        public string descriptionOnSelect;
        public int eggDelta;
    }
}
