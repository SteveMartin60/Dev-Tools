namespace ExportedFunctionsViewer.PE
{
    public static class PEConstants
    {
        public const ushort IMAGE_DOS_SIGNATURE = 0x5A4D;     // "MZ"
        public const uint IMAGE_NT_SIGNATURE = 0x00004550;    // "PE\0\0"

        // Directory entry indices
        public const int IMAGE_DIRECTORY_ENTRY_EXPORT = 0;
        public const int IMAGE_DIRECTORY_ENTRY_IMPORT = 1;
    }
}
