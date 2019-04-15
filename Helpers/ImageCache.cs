using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;

namespace DiscordBot.Helpers
{
    public static class ImageCache
    {
        private static readonly ConcurrentDictionary<string, Image> ImageStore = new ConcurrentDictionary<string, Image>();

        public static Image GetOrAdd(string path)
        {
            if (!ImageStore.TryGetValue(path, out var image))
                image = CacheImageFromDisk(path);
            return image;
        }

        private static Image CacheImageFromDisk(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var image = Image.FromStream(fs);
                ImageStore.TryAdd(path, image);
                return image;
            }
        }

        public static void Dispose()
        {
            foreach (var image in ImageStore)
                image.Value.Dispose();

            GC.Collect();
        }
    }
}
