namespace TenshiNoTamago.Core
{
    public static class SaveLoadManager
    {
        public enum Mode
        {
            Save,   
            Load    
        }

        public static Mode currentMode = Mode.Load;

        public enum EntrySource
        {
            MainMenu,   // 从主菜单进入
            PauseMenu   // 从暂停菜单进入
        }

        public static EntrySource currentSource = EntrySource.MainMenu;
        public static string tempThumbnailPath;
    }
}

