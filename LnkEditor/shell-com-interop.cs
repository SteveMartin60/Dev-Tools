using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LnkEditor
{
    public partial class link_editor : Window
    {
        #region ----------  Shell32 / COM interop  ----------

        [ComImport]
        [Guid("000214EE-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellLinkW
        {
            [PreserveSig]
            int GetPath(
                [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile,
                int cchMaxPath,
                IntPtr pfd,
                uint fFlags);

            [PreserveSig] int GetIDList(out IntPtr ppidl);
            [PreserveSig] int SetIDList(IntPtr pidl);

            [PreserveSig]
            int GetDescription(
                [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName,
                int cchMaxName);

            [PreserveSig] int SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);

            [PreserveSig]
            int GetWorkingDirectory(
                [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir,
                int cchMaxPath);

            [PreserveSig] int SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);

            [PreserveSig]
            int GetArguments(
                [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs,
                int cchMaxPath);

            [PreserveSig] int SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);

            [PreserveSig] int GetHotkey(out ushort pwHotkey);
            [PreserveSig] int SetHotkey(ushort wHotkey);

            [PreserveSig] int GetShowCmd(out int piShowCmd);
            [PreserveSig] int SetShowCmd(int iShowCmd);

            [PreserveSig]
            int GetIconLocation(
                [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath,
                int cchIconPath,
                out int piIcon);

            [PreserveSig] int SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);

            [PreserveSig] int SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, uint dwReserved);
            [PreserveSig] int Resolve(IntPtr hwnd, uint fFlags);
        }

        [ComImport]
        [Guid("0000010B-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IPersistFile
        {
            void GetCurFile(out IntPtr ppszFileName);
            void Load([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, uint dwMode);
            void Save([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, bool fRemember);
            void SaveCompleted([MarshalAs(UnmanagedType.LPWStr)] string pszFileName);
            void IsDirty();
        }

        [ComImport]
        [Guid("00021401-0000-0000-C000-000000000046")]
        private class ShellLinkCoClass { }

        #endregion
    }
}
