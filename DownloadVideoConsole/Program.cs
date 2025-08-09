using System;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        string videoUrl = "https://x-tg.tube/get_file/3/2872989e0191e72d1f81a25d9728cff39c5c3d8fd5/181000/181468/181468_720p.mp4";
        string savePath = @"C:\Downloads\video.mp4";

        using (HttpClient client = new HttpClient())
        {
            // Set headers to mimic a browser request (optional)
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
            client.DefaultRequestHeaders.Add("Referer", "https://x-tg.tube/");

            try
            {
                byte[] videoData = await client.GetByteArrayAsync(videoUrl);
                await System.IO.File.WriteAllBytesAsync(savePath, videoData);
                Console.WriteLine("Download completed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
