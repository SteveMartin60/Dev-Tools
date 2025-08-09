using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ExportedFunctionsViewer.PE
{
    public static class PEExportChecker
    {
        private const ushort IMAGE_DOS_SIGNATURE = 0x5A4D;     // MZ
        private const uint IMAGE_NT_SIGNATURE = 0x00004550;   // PE00
        private const int IMAGE_DIRECTORY_ENTRY_EXPORT = 0;

        public static bool HasExports(string filePath)
        {
            if (!File.Exists(filePath))
                return false;

            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                using (var reader = new BinaryReader(fs))
                {
                    // Read DOS header
                    var dosHeader = ReadStruct<IMAGE_DOS_HEADER>(reader);
                    if (dosHeader.e_magic != IMAGE_DOS_SIGNATURE)
                        return false;

                    // Seek to NT headers
                    fs.Seek(dosHeader.e_lfanew, SeekOrigin.Begin);

                    // Read NT headers signature
                    uint ntSignature = reader.ReadUInt32();
                    if (ntSignature != IMAGE_NT_SIGNATURE)
                        return false;

                    // Read file header
                    var fileHeader = ReadStruct<IMAGE_FILE_HEADER>(reader);

                    // Read optional header (32 or 64 bit)
                    bool is32Bit = fileHeader.SizeOfOptionalHeader == 0xE0;
                    IMAGE_DATA_DIRECTORY exportDirectory;

                    if (is32Bit)
                    {
                        var optionalHeader = ReadStruct<IMAGE_OPTIONAL_HEADER32>(reader);
                        exportDirectory = optionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_EXPORT];
                    }
                    else
                    {
                        var optionalHeader = ReadStruct<IMAGE_OPTIONAL_HEADER64>(reader);
                        exportDirectory = optionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_EXPORT];
                    }

                    // Check if export directory exists and has entries
                    return exportDirectory.VirtualAddress != 0 && exportDirectory.Size > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private static T ReadStruct<T>(BinaryReader reader) where T : struct
        {
            byte[] bytes = reader.ReadBytes(Marshal.SizeOf(typeof(T)));
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free();
            }
        }
    }
}
