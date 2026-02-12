using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

                    return !temporaryPatterns.Any(pattern => fileName.StartsWith(pattern.Trim('*'), StringComparison.OrdinalIgnoreCase)) &&
                           !directoryName.Contains("bin", StringComparison.OrdinalIgnoreCase) &&
                           !directoryName.Contains("obj", StringComparison.OrdinalIgnoreCase) &&
                           !directoryName.Contains("deprecated", StringComparison.OrdinalIgnoreCase) &&
                           !directoryName.Contains("Temp", StringComparison.OrdinalIgnoreCase) &&
                           (filePath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) ||
                            filePath.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase) ||
                            filePath.EndsWith(".axaml", StringComparison.OrdinalIgnoreCase) ||
                            filePath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase) ||
                            filePath.EndsWith(".sln", StringComparison.OrdinalIgnoreCase));
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error processing files: {ex.Message}", ex);
            }

            return filteredFiles;
        }
    }
}
