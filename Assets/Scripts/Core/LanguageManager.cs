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
                var wrapper = JsonUtility.FromJson<DictionaryWrapper>(jsonFile.text);
                currentDict = wrapper.data;
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
    public class DictionaryWrapper
    {
        public Dictionary<string, string> data;
    }
}
