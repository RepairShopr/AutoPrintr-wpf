using System;
using System.Runtime.InteropServices;
using AutoPrintr.Core.IServices;

namespace AutoPrintr.Helpers
{
    public class SystemTrayUpdater
    {
        // ReSharper disable once InconsistentNaming
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

        public static void Refresh(ILoggerService logger)
        {
            RefreshNotificationArea(logger);
            RefreshOverflowNotificationArea(logger);
        }

        private static void RefreshNotificationArea(ILoggerService logger)
        {
            try
            {
                var systemTrayContainerHandle = FindWindow("Shell_TrayWnd", null);
                if (systemTrayContainerHandle == IntPtr.Zero)
                {
                    logger?.WriteError("Not found Shell_TrayWnd in SystemTray Updater.");
                    return;
                }

                var systemTrayHandle = FindWindowEx(systemTrayContainerHandle, IntPtr.Zero, "TrayNotifyWnd", null);
                if (systemTrayHandle == IntPtr.Zero)
                {
                    logger?.WriteError("Not found TrayNotifyWnd in SystemTray Updater.");
                    return;
                }

                var sysPagerHandle = FindWindowEx(systemTrayHandle, IntPtr.Zero, "SysPager", null);
                if (sysPagerHandle == IntPtr.Zero)
                {
                    logger?.WriteError("Not found SysPager in SystemTray Updater.");
                    return;
                }

                var notificationAreaHandle = FindWindowEx(sysPagerHandle, IntPtr.Zero, "ToolbarWindow32", null);
                if (notificationAreaHandle == IntPtr.Zero)
                {
                    logger?.WriteError("Not found ToolbarWindow32 of SysPager in SystemTray Updater.");
                    return;
                }

                logger?.WriteInformation("Refresh the notification area.");
                RefreshTrayArea(notificationAreaHandle);
            }
            catch (Exception e)
            {
                logger?.WriteError($"Error refresh the notification area. {e}");
            }
        }

        private static void RefreshOverflowNotificationArea(ILoggerService logger)
        {
            try
            {
                var notifyIconOverflowWindowHandle = FindWindow("NotifyIconOverflowWindow", null);
                if (notifyIconOverflowWindowHandle == IntPtr.Zero)
                {
                    logger?.WriteError("Not found NotifyIconOverflowWindow in SystemTray Updater.");
                    return;
                }

                var overflowNotificationAreaHandle = FindWindowEx(notifyIconOverflowWindowHandle, IntPtr.Zero, "ToolbarWindow32", null);
                if (overflowNotificationAreaHandle == IntPtr.Zero)
                {
                    logger?.WriteError("Not found ToolbarWindow32 of NotifyIconOverflowWindow in SystemTray Updater.");
                    return;
                }

                logger?.WriteInformation("Refresh the overflow notification area.");
                RefreshTrayArea(overflowNotificationAreaHandle);
            }
            catch (Exception e)
            {
                logger?.WriteError($"Error refresh the overflow notification area. {e}");
            }
        }

        private static void RefreshTrayArea(IntPtr windowHandle)
        {
            const uint wmMousemove = 0x0200;
            GetClientRect(windowHandle, out var rect);
            for (var x = rect.left; x < rect.right; x += 5)
                for (var y = rect.top; y < rect.bottom; y += 5)
                    SendMessage(windowHandle, wmMousemove, 0, (y << 16) + x);
        }
    }
}