//Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace QTTabBarLib.Common
{
    /// <summary>Wrappers for Native Methods and Structs. This type is intended for internal use only</summary>
    internal static class CoreNativeMethods
    {
        // Disabled non-client rendering; window style is ignored.
        internal const int DWMNCRP_DISABLED = 1;

        // Enabled non-client rendering; window style is ignored.
        internal const int DWMNCRP_ENABLED = 2;

        // Enable/disable non-client rendering based on window style.
        internal const int DWMNCRP_USEWINDOWSTYLE = 0;

        // Enable/disable non-client rendering Use DWMNCRP_* values.
        internal const int DWMWA_NCRENDERING_ENABLED = 1;

        // Non-client rendering policy.
        internal const int DWMWA_NCRENDERING_POLICY = 2;

        // Potentially enable/forcibly disable transitions 0 or 1.
        internal const int DWMWA_TRANSITIONS_FORCEDISABLED = 3;

        internal const int EnterIdleMessage = 0x0121;

        // FormatMessage constants and structs.
        internal const int FormatMessageFromSystem = 0x00001000;

        // App recovery and restart return codes
        internal const uint ResultFailed = 0x80004005;

        internal const uint ResultFalse = 1;

        internal const uint ResultInvalidArgument = 0x80070057;

        internal const uint ResultNotFound = 0x80070490;

        internal const uint StatusAccessDenied = 0xC0000022;

        // Various important window messages
        internal const int UserMessage = 0x0400;

        public delegate int WNDPROC(IntPtr hWnd,
                    uint uMessage,
                    IntPtr wParam,
                    IntPtr lParam);

        /// <summary>Gets the HiWord</summary>
        /// <param name="value">The value to get the hi word from.</param>
        /// <param name="size">Size</param>
        /// <returns>The upper half of the dword.</returns>
        public static int GetHiWord(long value, int size)
        {
            return (short)(value >> size);
        }

        /// <summary>Gets the LoWord</summary>
        /// <param name="value">The value to get the low word from.</param>
        /// <returns>The lower half of the dword.</returns>
        public static int GetLoWord(long value)
        {
            return (short)(value & 0xFFFF);
        }

        /// <summary>
        /// Places (posts) a message in the message queue associated with the thread that created the specified window and returns without
        /// waiting for the thread to process the message.
        /// </summary>
        /// <param name="windowHandle">
        /// Handle to the window whose window procedure will receive the message. If this parameter is HWND_BROADCAST, the message is sent to
        /// all top-level windows in the system, including disabled or invisible unowned windows, overlapped windows, and pop-up windows; but
        /// the message is not sent to child windows.
        /// </param>
        /// <param name="message">Specifies the message to be sent.</param>
        /// <param name="wparam">Specifies additional message-specific information.</param>
        /// <param name="lparam">Specifies additional message-specific information.</param>
        /// <returns>A return code specific to the message being sent.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, PreserveSig = false, SetLastError = true)]
        public static extern void PostMessage(
            IntPtr windowHandle,
            WindowMessage message,
            IntPtr wparam,
            IntPtr lparam
        );

        /// <summary>
        /// Sends the specified message to a window or windows. The SendMessage function calls the window procedure for the specified window
        /// and does not return until the window procedure has processed the message.
        /// </summary>
        /// <param name="windowHandle">
        /// Handle to the window whose window procedure will receive the message. If this parameter is HWND_BROADCAST, the message is sent to
        /// all top-level windows in the system, including disabled or invisible unowned windows, overlapped windows, and pop-up windows; but
        /// the message is not sent to child windows.
        /// </param>
        /// <param name="message">Specifies the message to be sent.</param>
        /// <param name="wparam">Specifies additional message-specific information.</param>
        /// <param name="lparam">Specifies additional message-specific information.</param>
        /// <returns>A return code specific to the message being sent.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SendMessage(
            IntPtr windowHandle,
            WindowMessage message,
            IntPtr wparam,
            IntPtr lparam
        );

        /// <summary>
        /// Sends the specified message to a window or windows. The SendMessage function calls the window procedure for the specified window
        /// and does not return until the window procedure has processed the message.
        /// </summary>
        /// <param name="windowHandle">
        /// Handle to the window whose window procedure will receive the message. If this parameter is HWND_BROADCAST, the message is sent to
        /// all top-level windows in the system, including disabled or invisible unowned windows, overlapped windows, and pop-up windows; but
        /// the message is not sent to child windows.
        /// </param>
        /// <param name="message">Specifies the message to be sent.</param>
        /// <param name="wparam">Specifies additional message-specific information.</param>
        /// <param name="lparam">Specifies additional message-specific information.</param>
        /// <returns>A return code specific to the message being sent.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SendMessage(
            IntPtr windowHandle,
            uint message,
            IntPtr wparam,
            IntPtr lparam
        );

        /// <summary>
        /// Sends the specified message to a window or windows. The SendMessage function calls the window procedure for the specified window
        /// and does not return until the window procedure has processed the message.
        /// </summary>
        /// <param name="windowHandle">
        /// Handle to the window whose window procedure will receive the message. If this parameter is HWND_BROADCAST, the message is sent to
        /// all top-level windows in the system, including disabled or invisible unowned windows, overlapped windows, and pop-up windows; but
        /// the message is not sent to child windows.
        /// </param>
        /// <param name="message">Specifies the message to be sent.</param>
        /// <param name="wparam">Specifies additional message-specific information.</param>
        /// <param name="lparam">Specifies additional message-specific information.</param>
        /// <returns>A return code specific to the message being sent.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SendMessage(
           IntPtr windowHandle,
           uint message,
           IntPtr wparam,
           [MarshalAs(UnmanagedType.LPWStr)] string lparam);

        /// <summary>
        /// Sends the specified message to a window or windows. The SendMessage function calls the window procedure for the specified window
        /// and does not return until the window procedure has processed the message.
        /// </summary>
        /// <param name="windowHandle">
        /// Handle to the window whose window procedure will receive the message. If this parameter is HWND_BROADCAST, the message is sent to
        /// all top-level windows in the system, including disabled or invisible unowned windows, overlapped windows, and pop-up windows; but
        /// the message is not sent to child windows.
        /// </param>
        /// <param name="message">Specifies the message to be sent.</param>
        /// <param name="wparam">Specifies additional message-specific information.</param>
        /// <param name="lparam">Specifies additional message-specific information.</param>
        /// <returns>A return code specific to the message being sent.</returns>
        public static IntPtr SendMessage(
            IntPtr windowHandle,
            uint message,
            int wparam,
            string lparam)
        {
            return SendMessage(windowHandle, message, (IntPtr)wparam, lparam);
        }

        /// <summary>
        /// Sends the specified message to a window or windows. The SendMessage function calls the window procedure for the specified window
        /// and does not return until the window procedure has processed the message.
        /// </summary>
        /// <param name="windowHandle">
        /// Handle to the window whose window procedure will receive the message. If this parameter is HWND_BROADCAST, the message is sent to
        /// all top-level windows in the system, including disabled or invisible unowned windows, overlapped windows, and pop-up windows; but
        /// the message is not sent to child windows.
        /// </param>
        /// <param name="message">Specifies the message to be sent.</param>
        /// <param name="wparam">Specifies additional message-specific information.</param>
        /// <param name="lparam">Specifies additional message-specific information.</param>
        /// <returns>A return code specific to the message being sent.</returns>

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SendMessage(
            IntPtr windowHandle,
            uint message,
            ref int wparam,
            [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lparam);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteObject(IntPtr graphicsObjectHandle);

        /// <summary>Destroys an icon and frees any memory the icon occupied.</summary>
        /// <param name="hIcon">Handle to the icon to be destroyed. The icon must not be in use.</param>
        /// <returns>
        /// If the function succeeds, the return value is nonzero. If the function fails, the return value is zero. To get extended error
        /// information, call GetLastError.
        /// </returns>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("user32.dll", SetLastError = true, EntryPoint = "DestroyWindow", CallingConvention = CallingConvention.StdCall)]
        internal static extern int DestroyWindow(IntPtr handle);

        // Various helpers for forcing binding to proper version of Comctl32 (v6).
        [DllImport("kernel32.dll", SetLastError = true, ThrowOnUnmappableChar = true, BestFitMapping = false)]
        internal static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string fileName);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern int LoadString(
            IntPtr instanceHandle,
            int id,
            StringBuilder buffer,
            int bufferSize);

        [DllImport("Kernel32.dll", EntryPoint = "LocalFree")]
        internal static extern IntPtr LocalFree(ref Guid guid);

        /// <summary>A Wrapper for a SIZE struct</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Size
        {
            private int width;
            private int height;

            /// <summary>Width</summary>
            public int Width
            {
                get { return width;}
                set
                {
                    width = value;
                }
            }

            /// <summary>Height</summary>
            public int Height
            {
                get { return height; }

                set { height = value; }
            }
        };
    }
}