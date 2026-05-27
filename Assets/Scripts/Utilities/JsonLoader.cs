using System.IO;
using TenshiNoTamago.Data;
using UnityEngine;

namespace TenshiNoTamago.Utilities
{
    public static class JsonLoader 
    {
        public static ChapterData LoadChapter(string chapterFileName)
        {
            string path = Path.Combine(Application.streamingAssetsPath, "Chapters", chapterFileName + ".json");

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
