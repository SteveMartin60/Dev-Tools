using System;
using System.Collections.Generic;
using System.IO;

namespace CodeConsolidator.Models
{
    public class FileProcessor
    {
        public List<string> GetFilteredFiles(string folderPath, bool searchSubfolders)
        {
            var filteredFiles = new List<string>();
            try
            {
                var searchOption = searchSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                var allFiles = Directory.GetFiles(folderPath, "*.*", searchOption);
                var temporaryPatterns = new List<string> { "~$*", "*.tmp", "*.bak", "*.swp" };

                filteredFiles = allFiles.Where(filePath =>
                {
                    var fileName = Path.GetFileName(filePath);
                    var directoryName = Path.GetDirectoryName(filePath);

                    return !temporaryPatterns.Any(pattern => fileName.StartsWith(pattern.Trim('*'))) &&
                           !directoryName.Contains("bin") &&
                           !directoryName.Contains("obj") &&
                           !directoryName.Contains("deprecated") &&
                           !directoryName.Contains("Temp") &&
                           (filePath.EndsWith(".cs") || filePath.EndsWith(".xaml") || filePath.EndsWith(".axaml") || filePath.EndsWith(".csproj") || filePath.EndsWith(".sln"));
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error processing files: {ex.Message}");
            }

            return filteredFiles;
        }
    }
}
