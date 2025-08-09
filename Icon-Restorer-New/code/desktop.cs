using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Automation;

namespace IconsRestorer.Code
{
    internal class desktop
    {
        private readonly IntPtr _desktopHandle;
        private readonly List<string> _currentIconsOrder;

        public desktop()
        {
            _desktopHandle = win32.GetDesktopWindow(win32.DesktopWindow.SysListView32);

            AutomationElement el = AutomationElement.FromHandle(_desktopHandle);

            TreeWalker walker = TreeWalker.ContentViewWalker;
            _currentIconsOrder = new List<string>();
            for (AutomationElement child = walker.GetFirstChild(el);
                child != null;
                child = walker.GetNextSibling(child))
            {
                _currentIconsOrder.Add(child.Current.Name);
            }
        }

        private int GetIconsNumber()
        {
            return (int)win32.SendMessage(_desktopHandle, win32.LVM_GETITEMCOUNT, IntPtr.Zero, IntPtr.Zero);
        }

        public NamedDesktopPoint[] GetIconsPositions()
        {
            uint desktopProcessId;
            win32.GetWindowThreadProcessId(_desktopHandle, out desktopProcessId);

            IntPtr desktopProcessHandle = IntPtr.Zero;
            try
            {
                desktopProcessHandle = win32.OpenProcess(win32.ProcessAccess.VmOperation | win32.ProcessAccess.VmRead |
                    win32.ProcessAccess.VmWrite, false, desktopProcessId);

                return GetIconsPositions(desktopProcessHandle);
            }
            finally
            {
                if (desktopProcessHandle != IntPtr.Zero)
                { win32.CloseHandle(desktopProcessHandle); }
            }
        }

        private NamedDesktopPoint[] GetIconsPositions(IntPtr desktopProcessHandle)
        {
            IntPtr sharedMemoryPointer = IntPtr.Zero;

            try
            {
                sharedMemoryPointer = win32.VirtualAllocEx(desktopProcessHandle, IntPtr.Zero, 4096, win32.AllocationType.Reserve | win32.AllocationType.Commit, win32.MemoryProtection.ReadWrite);

                return GetIconsPositions(desktopProcessHandle, sharedMemoryPointer);
            }
            finally
            {
                if (sharedMemoryPointer != IntPtr.Zero)
                {
                    win32.VirtualFreeEx(desktopProcessHandle, sharedMemoryPointer, 0, win32.FreeType.Release);
                }
            }

        }

        private NamedDesktopPoint[] GetIconsPositions(IntPtr desktopProcessHandle, IntPtr sharedMemoryPointer)
        {
            var listOfPoints = new LinkedList<NamedDesktopPoint>();

            var numberOfIcons = GetIconsNumber();

            for (int itemIndex = 0; itemIndex < numberOfIcons; itemIndex++)
            {
                uint numberOfBytes = 0;
                DesktopPoint[] points = new DesktopPoint[1];

                win32.WriteProcessMemory(desktopProcessHandle, sharedMemoryPointer,
                    Marshal.UnsafeAddrOfPinnedArrayElement(points, 0),
                    Marshal.SizeOf(typeof(DesktopPoint)),
                    ref numberOfBytes);

                win32.SendMessage(_desktopHandle, win32.LVM_GETITEMPOSITION, itemIndex, sharedMemoryPointer);

                win32.ReadProcessMemory(desktopProcessHandle, sharedMemoryPointer,
                    Marshal.UnsafeAddrOfPinnedArrayElement(points, 0),
                    Marshal.SizeOf(typeof(DesktopPoint)),
                    ref numberOfBytes);

                var point = points[0];
                listOfPoints.AddLast(new NamedDesktopPoint(_currentIconsOrder[itemIndex], point.X, point.Y));
            }

            return listOfPoints.ToArray();
        }

        public void SetIconPositions(IEnumerable<NamedDesktopPoint> iconPositions)
        {
            foreach (var position in iconPositions)
            {
                var iconIndex = _currentIconsOrder.IndexOf(position.Name);
                if (iconIndex == -1)
                { continue; }

                win32.SendMessage(_desktopHandle, win32.LVM_SETITEMPOSITION, iconIndex, win32.MakeLParam(position.X, position.Y));
            }
        }

        public void Refresh(bool immediate = false)
        {
            if (immediate)
            {
                // Immediate refresh (may cause flicker)
                win32.PostMessage(_desktopHandle, win32.WM_KEYDOWN, win32.VK_F5, 0);
            }
            else
            {
                // More gentle refresh that doesn't cause flickering
                win32.SHChangeNotify(win32.SHCNE_ASSOCCHANGED, win32.SHCNF_FLUSH, IntPtr.Zero, IntPtr.Zero);
            }
        }
    }
}
