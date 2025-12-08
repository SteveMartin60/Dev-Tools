using System;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using CoenM.ImageHash;
using CoenM.ImageHash.HashAlgorithms;

namespace reddit_fetch
{
    public static class ImageFilterHelper
    {
        public static AppConfig Config { get; set; } = null!;

        private static readonly AverageHash HashAlgorithm = new AverageHash();

        public static async Task<(bool ok, string reason, string hash)> ValidateAndHashImageAsync(string filePath)
        {
            try
            {
                using var img = await Image.LoadAsync<Rgba32>(filePath);

                // Optional resolution / aspect checks could go here

                ulong hashVal = HashAlgorithm.Hash(img);
                return (true, "OK", hashVal.ToString());
            }
            catch (Exception ex)
            {
                return (false, $"Image load/hash failed: {ex.Message}", string.Empty);
            }
        }
    }
}
