using System;

namespace QTTabBarLib.Interop {
    public struct BROWSEINFO {
        // PENDING: or, creates the BIF class for the constants below
        #region ---------- Constants ----------
        public const uint BIF_RETURNONLYFSDIRS = 0x0001;  // For finding a folder to start document searching
        public const uint BIF_DONTGOBELOWDOMAIN = 0x0002;  // For starting the Find Computer
        public const uint BIF_STATUSTEXT = 0x0004;  // Top of the dialog has 2 lines of text for BROWSEINFO.lpszTitle and one line if
        // this flag is set.  Passing the message BFFM_SETSTATUSTEXTA to the hwnd can set the
        // rest of the text.  This is not used with BIF_USENEWUI and BROWSEINFO.lpszTitle gets
        // all three lines of text.
        public const uint BIF_RETURNFSANCESTORS = 0x0008;
        public const uint BIF_EDITBOX = 0x0010;   // Add an editbox to the dialog
        public const uint BIF_VALIDATE = 0x0020;   // insist on valid result (or CANCEL)

        public const uint BIF_NEWDIALOGSTYLE = 0x0040;   // Use the new dialog layout with the ability to resize
        // Caller needs to call OleInitialize() before using this API
        public const uint BIF_USENEWUI = 0x0040 + 0x0010; //(BIF_NEWDIALOGSTYLE | BIF_EDITBOX);

        public const uint BIF_BROWSEINCLUDEURLS = 0x0080;   // Allow URLs to be displayed or entered. (Requires BIF_USENEWUI)
        public const uint BIF_UAHINT = 0x0100;   // Add a UA hint to the dialog, in place of the edit box. May not be combined with BIF_EDITBOX
        public const uint BIF_NONEWFOLDERBUTTON = 0x0200;   // Do not add the "New Folder" button to the dialog.  Only applicable with BIF_NEWDIALOGSTYLE.
        public const uint BIF_NOTRANSLATETARGETS = 0x0400;  // don't traverse target as shortcut

        public const uint BIF_BROWSEFORCOMPUTER = 0x1000;  // Browsing for Computers.
        public const uint BIF_BROWSEFORPRINTER = 0x2000;// Browsing for Printers
        public const uint BIF_BROWSEINCLUDEFILES = 0x4000; // Browsing for Everything
        public const uint BIF_SHAREABLE = 0x8000;  // sharable resources displayed (remote shares, requires BIF_USENEWUI)
        #endregion

        public IntPtr hwndOwner;
        public IntPtr pidlRoot;
        public string pszDisplayName;
        public string lpszTitle;
        public uint ulFlags;
        public IntPtr lpfn;
        public IntPtr lParam;
        public int iImage;
    }
}
