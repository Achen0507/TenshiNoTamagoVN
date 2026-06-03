using System.Collections.Generic;
using UnityEngine;

namespace TenshiNoTamago.Core
{
    public static class LanguageManager 
    {
        private static Dictionary<string, string> currentDict;
        public static string currentLanguage = "zh";

        public static void LoadLanguage(string lang) {
            currentLanguage = lang;
            TextAsset jsonFile = Resources.Load<TextAsset>($"Languages/{lang}");
            if (jsonFile != null)
            {
                var wrapper = JsonUtility.FromJson<LanguageWrapper>(jsonFile.text);
                currentDict = new Dictionary<string, string>();
                foreach (var item in wrapper.data)
                {
                    currentDict[item.key] = item.value;
                }
                Debug.Log($"语言已加载: {lang}, 共 {currentDict.Count} 条");
            }
            else {
                Debug.LogError($"语言文件不存在: Languages/{lang}.json");
            }
        }

        public static string Get(string key)
        {
            if (currentDict != null && currentDict.ContainsKey(key))
            {
                return currentDict[key];
            }
            Debug.LogWarning($"未找到翻译 key: {key}");
            return key;
        }
    }

    [System.Serializable]
    public class LanguageItem
    {
        public string key;
        public string value;
    }

    [System.Serializable]
    public class LanguageWrapper
    {
        public List<LanguageItem> data;
    }
}
