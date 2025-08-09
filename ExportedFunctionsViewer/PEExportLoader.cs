using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ExportedFunctionsViewer.PE
{
    public static class PEExportLoader
    {
        public static List<ExportedFunction> GetAllExports(string filePath)
        {
            var exports = new List<ExportedFunction>();

            if (!File.Exists(filePath))
                return exports;

            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                using (var reader = new BinaryReader(fs))
                {
                    // Read DOS header
                    var dosHeader = ReadStruct<IMAGE_DOS_HEADER>(reader);
                    if (dosHeader.e_magic != PEConstants.IMAGE_DOS_SIGNATURE)
                        return exports;

                    // Read NT headers
                    fs.Seek(dosHeader.e_lfanew, SeekOrigin.Begin);
                    if (reader.ReadUInt32() != PEConstants.IMAGE_NT_SIGNATURE)
                        return exports;

                    var fileHeader = ReadStruct<IMAGE_FILE_HEADER>(reader);
                    bool is32Bit = fileHeader.SizeOfOptionalHeader == 0xE0;

                    IMAGE_DATA_DIRECTORY exportDirectory;
                    uint imageBase;

                    if (is32Bit)
                    {
                        var optionalHeader = ReadStruct<IMAGE_OPTIONAL_HEADER32>(reader);
                        exportDirectory = optionalHeader.DataDirectory[PEConstants.IMAGE_DIRECTORY_ENTRY_EXPORT];
                        imageBase = optionalHeader.ImageBase;
                    }
                    else
                    {
                        var optionalHeader = ReadStruct<IMAGE_OPTIONAL_HEADER64>(reader);
                        exportDirectory = optionalHeader.DataDirectory[PEConstants.IMAGE_DIRECTORY_ENTRY_EXPORT];
                        imageBase = (uint)optionalHeader.ImageBase;
                    }

                    if (exportDirectory.VirtualAddress == 0 || exportDirectory.Size == 0)
                        return exports;

                    // Get export directory
                    uint exportDirOffset = RvaToOffset(reader, exportDirectory.VirtualAddress);
                    fs.Seek(exportDirOffset, SeekOrigin.Begin);
                    var exportDir = ReadStruct<IMAGE_EXPORT_DIRECTORY>(reader);

                    // Read function addresses
                    uint[] functions = ReadRvaArray(reader,
                        RvaToOffset(reader, exportDir.AddressOfFunctions),
                        exportDir.NumberOfFunctions);

                    // Read names and ordinals if available
                    if (exportDir.NumberOfNames > 0)
                    {
                        uint[] names = ReadRvaArray(reader,
                            RvaToOffset(reader, exportDir.AddressOfNames),
                            exportDir.NumberOfNames);
                        ushort[] ordinals = ReadOrdinalArray(reader,
                            RvaToOffset(reader, exportDir.AddressOfNameOrdinals),
                            exportDir.NumberOfNames);

                        for (int i = 0; i < exportDir.NumberOfNames; i++)
                        {
                            fs.Seek(RvaToOffset(reader, names[i]), SeekOrigin.Begin);
                            string name = ReadNullTerminatedString(reader);

                            exports.Add(new ExportedFunction
                            {
                                Ordinal = (int)(ordinals[i] + exportDir.Base),
                                Name = name,
                                RVA = $"0x{functions[ordinals[i]]:X8}",
                                Hint = ordinals[i].ToString(),
                                DemangledName = DemangleName(name)
                            });
                        }
                    }

                    // Add remaining exports by ordinal
                    for (uint i = 0; i < exportDir.NumberOfFunctions; i++)
                    {
                        if (functions[i] != 0 && !exports.Any(e => e.Ordinal == i + exportDir.Base))
                        {
                            exports.Add(new ExportedFunction
                            {
                                Ordinal = (int)(i + exportDir.Base),
                                Name = $"Ordinal_{i + exportDir.Base}",
                                RVA = $"0x{functions[i]:X8}",
                                Hint = i.ToString(),
                                DemangledName = string.Empty
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading exports: {ex.Message}");
            }

            return exports;
        }

        private static string DemangleName(string name)
        {
            // Simple demangling - would need more complex logic for full C++ demangling
            return name;
        }

        private static uint RvaToOffset(BinaryReader reader, uint rva)
        {
            // Simplified version - assumes RVA == offset for most cases
            return rva;
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

        private static string ReadNullTerminatedString(BinaryReader reader)
        {
            var bytes = new List<byte>();
            byte b;
            while ((b = reader.ReadByte()) != 0)
                bytes.Add(b);
            return Encoding.ASCII.GetString(bytes.ToArray());
        }

        private static uint[] ReadRvaArray(BinaryReader reader, uint rva, uint count)
        {
            long originalPos = reader.BaseStream.Position;
            reader.BaseStream.Seek(rva, SeekOrigin.Begin);

            var array = new uint[count];
            for (int i = 0; i < count; i++)
                array[i] = reader.ReadUInt32();

            reader.BaseStream.Seek(originalPos, SeekOrigin.Begin);
            return array;
        }

        private static ushort[] ReadOrdinalArray(BinaryReader reader, uint rva, uint count)
        {
            long originalPos = reader.BaseStream.Position;
            reader.BaseStream.Seek(rva, SeekOrigin.Begin);

            var array = new ushort[count];
            for (int i = 0; i < count; i++)
                array[i] = reader.ReadUInt16();

            reader.BaseStream.Seek(originalPos, SeekOrigin.Begin);
            return array;
        }
    }

    public class ExportedFunction
    {
        public int Ordinal { get; set; }
        public string Hint { get; set; } = string.Empty;
        public string RVA { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string DemangledName { get; set; } = string.Empty;
    }
}
