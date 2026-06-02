using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace TenshiNoTamago.Core
{
    public static class ResourceCache
    {
        private static Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();
        private static Dictionary<string, Task<Sprite>> loadingTasks = new Dictionary<string, Task<Sprite>>();

        public static async Task<Sprite> LoadSpriteAsync(string path) {
            if (string.IsNullOrEmpty(path)) return null;

            if (spriteCache.ContainsKey(path))
                return spriteCache[path];

            if (loadingTasks.ContainsKey(path))
                return await loadingTasks[path];

            var task = LoadSpriteTask(path);
            loadingTasks[path] = task;

            Sprite sprite = await task;
            loadingTasks.Remove(path);

            if (sprite != null)
                spriteCache[path] = sprite;

            return sprite;
        }

        private static async Task<Sprite> LoadSpriteTask(string path) {
            var tcs = new TaskCompletionSource<Sprite>();

            var request = Resources.LoadAsync<Sprite>(path);
            request.completed += (op) =>
            {
                tcs.SetResult(request.asset as Sprite);
            };

            return await tcs.Task;
        }

        public static void ClearCache() {
            spriteCache.Clear();
            loadingTasks.Clear();
        }
    }
}
