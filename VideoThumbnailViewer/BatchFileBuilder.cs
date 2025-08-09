using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using File = System.IO.File;

namespace VideoThumbnailViewer
{
    public static class BatchFileBuilder
    {
        public static void GenerateBatchFile(string outputPath)
        {
            string ytdlpPath = @"D:\yt-dl\yt-dlp-code.exe";
            string jqPath = @"D:\yt-dl\JQ\jq.exe"; // Updated jq.exe path
            string tempJsonPath = @"D:\yt-dl\temp_output.json";
            string tempTxtPath = @"D:\yt-dl\temp_output.txt";
            string tempBatchPath = Path.Combine(Path.GetTempPath(), "temp_batch.bat");

            // Validate paths
            if (!File.Exists(ytdlpPath))
            {
                Debug.WriteLine($"yt-dlp.exe not found at: {ytdlpPath}");
                return;
            }
            if (!File.Exists(jqPath))
            {
                Debug.WriteLine($"jq.exe not found at: {jqPath}");
                return;
            }
            Directory.CreateDirectory(Path.GetDirectoryName(tempJsonPath) ?? throw new InvalidOperationException("Invalid temp path"));

            try
            {
                // Step 1: Create a temporary batch file for header command
                string url = "\"https://www.youtube.com/@ChinaDocs/videos\"";
                string headerCommand = $"\"{ytdlpPath}\" --playlist-end 1 --dump-json --flat-playlist {url} | \"{jqPath}\" -R \"fromjson? | {{Channel: .playlist_channel, Playlist: .playlist, PlaylistID: .playlist_id, UploaderId: .playlist_uploader_id, Source: .playlist_webpage_url}}\" > \"{tempJsonPath}\"";
                File.WriteAllText(tempBatchPath, $"@echo off\n{headerCommand}");

                // Execute the temporary batch file
                ExecuteCommand(tempBatchPath);

                // Step 2: Create a temporary batch file for body command
                string bodyCommand = $"\"{ytdlpPath}\" --match-filter \"duration > 1500\" --dump-json --flat-playlist {url} | \"{jqPath}\" -R \"fromjson? | {{Index: .playlist_index, Url: .url, Title: .title, Duration: .duration_string, Description: .description}}\" > \"{tempTxtPath}\"";
                File.WriteAllText(tempBatchPath, $"@echo off\n{bodyCommand}");

                // Execute the temporary batch file
                ExecuteCommand(tempBatchPath);

                // Step 3: Generate batch file
                using StreamWriter writer = new(outputPath);

                writer.WriteLine("@echo off");
                writer.WriteLine("cls");
                writer.WriteLine();

                foreach (var line in File.ReadLines(tempTxtPath))
                {
                    string jsonLine = line.Trim();
                    if (string.IsNullOrEmpty(jsonLine)) continue;

                    try
                    {
                        JsonDocument doc = JsonDocument.Parse(jsonLine);
                        JsonElement root = doc.RootElement;

                        int index = root.GetProperty("Index").GetInt32();
                        string urlValue = root.GetProperty("Url").GetString() ?? "";
                        string title = root.GetProperty("Title").GetString()?.Replace("\"", "'") ?? ""; // Escape quotes

                        string videoId = index.ToString("D3"); // e.g., 21 → "021"

                        writer.WriteLine("echo \"===============================================================================\"");
                        writer.WriteLine($"echo \"Processing Video {videoId}\"");
                        writer.WriteLine("echo \"--------------------\"");
                        writer.WriteLine($"\"{ytdlpPath}\" \"{urlValue}\" --write-thumbnail --convert-thumbnails jpg --embed-metadata --embed-chapters --concurrent-fragments 8 --max-downloads 8 --no-config -o \"E:/Videos/%%(channel)s/%%(title)s.%%(ext)s\" -f \"bestvideo+bestaudio\" --merge-output-format mp4 --xattrs");
                        writer.WriteLine("echo \"===============================================================================\"");
                        writer.WriteLine("echo.");
                        writer.WriteLine();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error parsing line: {ex.Message}");
                    }
                }

                Debug.WriteLine($"Batch file generated at: {outputPath}");
            }
            finally
            {
                // Clean up temporary files
                if (File.Exists(tempJsonPath)) File.Delete(tempJsonPath);
                if (File.Exists(tempTxtPath)) File.Delete(tempTxtPath);
                if (File.Exists(tempBatchPath)) File.Delete(tempBatchPath);
            }
        }

        private static void ExecuteCommand(string command)
        {
            Process process = new()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c \"{command}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardInput = true // Allow input if needed
                },
                EnableRaisingEvents = true // Required for events
            };

            StringBuilder outputBuilder = new();
            StringBuilder errorBuilder = new();

            // Log the command being executed
            Debug.WriteLine($"Executing command: cmd.exe /c {command}");

            // Subscribe to output and error events
            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    outputBuilder.AppendLine(e.Data);
                    Debug.WriteLine($"[STDOUT]: {e.Data}");
                }
                else
                {
                    Debug.WriteLine("[STDOUT]: Received empty or null data");
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    errorBuilder.AppendLine(e.Data);
                    Debug.WriteLine($"[STDERR]: {e.Data}");
                }
                else
                {
                    Debug.WriteLine("[STDERR]: Received empty or null data");
                }
            };

            try
            {
                // Start the process
                Debug.WriteLine("Starting process...");
                process.Start();

                // Begin asynchronous reading
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // Wait for the process to exit
                Debug.WriteLine("Waiting for process to exit...");
                process.WaitForExit();

                // Log results
                string output = outputBuilder.ToString();
                string error = errorBuilder.ToString();

                Debug.WriteLine($"Process exited with code: {process.ExitCode}");

                if (!string.IsNullOrEmpty(output))
                {
                    Debug.WriteLine($"Final STDOUT:\n{output}");
                }
                else
                {
                    Debug.WriteLine("No STDOUT captured.");
                }

                if (!string.IsNullOrEmpty(error))
                {
                    Debug.WriteLine($"Final STDERR:\n{error}");
                }
                else
                {
                    Debug.WriteLine("No STDERR captured.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception during command execution: {ex.Message}");
            }
            finally
            {
                process.Dispose();
                Debug.WriteLine("Process disposed.");
            }
        }
    }
}
