using System.IO;
using TenshiNoTamago.Data;
using UnityEngine;

namespace TenshiNoTamago.Utilities
{
    public static class JsonLoader
    {
        public static string currentLanguage = "zh";
        public static ChapterData LoadChapter(string chapterFileName)
        {
            string fileName = chapterFileName.ToLower();
            if (currentLanguage == "ja") {
                fileName = fileName + "_ja";
            }

            string path = Path.Combine(Application.streamingAssetsPath, "Chapters", fileName + ".json");
            Debug.Log($"尝试加载: {path}");

            if (File.Exists(path))
            {
                string json = File.ReadAllText(path); 

                // 检查是否有 BOM
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
                ChapterData data = JsonUtility.FromJson<ChapterData>(json);
                return data;
            }
            else
            {
                Debug.LogError("文件不存在: " + path);
                return null;
            }
        }
    }
}
