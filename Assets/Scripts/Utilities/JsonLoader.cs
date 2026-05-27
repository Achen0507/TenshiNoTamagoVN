using System.IO;
using TenshiNoTamago.Data;
using UnityEngine;

namespace TenshiNoTamago.Utilities
{
    public static class JsonLoader 
    {
        public static ChapterData LoadChapter(string chapterFileName) {
            string path = Path.Combine(Application.streamingAssetsPath, "Chapters", chapterFileName + ".json");

            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                ChapterData data = JsonUtility.FromJson<ChapterData>(json);
                return data;
            }
            else {
                Debug.LogError($"[JsonLoader] File not found: {path}");
                return null;
            }
        }
    }
}
