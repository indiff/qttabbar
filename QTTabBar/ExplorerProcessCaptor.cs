using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using QTTabBarLib.Interop;

namespace QTTabBarLib
{
    // ABCDEF
    [ComVisible(true)]
    //[Guid("ABCD0000-0000-0000-0000-0000000000EF")]  // 会导致很多功能被捕获
    [Guid("ABCD0000-0000-1111-0000-0000000000EF")]
    class ExplorerProcessCaptor1 : IShellExecuteHook
    {
        private int S_OK = 1;
        private int S_FALSE = 0;
        private static volatile bool fEntered;

        public int Execute(ref SHELLEXECUTEINFO sei)
        {
            if (fEntered)
            {
                QTUtility2.log("****************** ExplorerProcessCaptor *****  fEntered  true");
                return S_OK;
            }
            fEntered = true;
            try
            {
                QTUtility2.log("****************** ExplorerProcessCaptor *****  start ");
                // QTUtility2.HasFlag(lparam->flags, SWP.NOMOVE)
                if (sei.lpParameters != IntPtr.Zero)
                {
                    string stringUni2 = Marshal.PtrToStringUni(sei.lpParameters);
                    QTUtility2.log("****************** ExplorerProcessCaptor *****  stringUni2 " + stringUni2);
                    if (!string.IsNullOrEmpty(stringUni2))
                    {
                        string path;
                        string selection;
                        if (!TryParseCommandlineParams(stringUni2, out path, out selection))
                            path = stringUni2;
                        if (!string.IsNullOrEmpty(selection))
                        {
                            QTUtility2.log("****************** ExplorerProcessCaptor ***** " + selection);
                            // ExplorerProcessCaptor.WriteSelection(selection);
                            // opt |= BarCommandOptions.StartUpSelection;
                        }
                    }
                }

                if (sei.lpFile != IntPtr.Zero)
                {
                    string pathUri = Marshal.PtrToStringUni(sei.lpFile);
                    QTUtility2.log("****************** ExplorerProcessCaptor *****  pathUri " + pathUri);

                    if (!string.Equals(pathUri, "explorer.exe", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!string.Equals(pathUri, QTUtility2.ExplorerPath, StringComparison.OrdinalIgnoreCase))
                        {
                            // return S_OK;
                        }

                    }
                    IntPtr ppidl;
                    // 检索名为 ITEMIDLIST 结构的已知文件夹的路径。
                    // if (si == null && PInvoke.SHGetKnownFolderIDList(COMGUIDS.FOLDERID_UsersLibraries, 0, IntPtr.Zero, out ppidl) == 0)
                }
            }
            finally
            {
                fEntered = false;
            }
            QTUtility2.log("****************** ExplorerProcessCaptor *****  end ");
            return S_OK;
        }




        private static bool TryParseCommandlineParams(
            string param,
            out string path,
            out string selection)
        {
            selection = (string)null;
            Match match = new Regex("( ?(/|,)select, ?((?<SELQ>\"[^\"/]+\")|(?<SEL>[^,/]+))| ?(/|,)root,\\s?((?<ROOTQ>\"[^\"/]+\")|(?<ROOT>[^,/]+)))+", RegexOptions.IgnoreCase).Match(param);
            if (match.Success)
            {
                var group1 = match.Groups["SEL"];
                var group2 = match.Groups["SELQ"];
                var group3 = match.Groups["ROOT"];
                var group4 = match.Groups["ROOTQ"];
                try
                {
                    if (group3.Success)
                    {
                        path = group3.Value;
                        return true;
                    }
                    if (group4.Success)
                    {
                        path = group4.Value.Trim('"');
                        return true;
                    }
                    if (group1.Success)
                    {
                        selection = group1.Value;
                        path = !QTUtility2.IsDrive(selection) ? Path.GetDirectoryName(selection) : "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}";
                        return true;
                    }
                    if (group2.Success)
                    {
                        selection = group2.Value.Trim('"');
                        path = !QTUtility2.IsDrive(selection) ? Path.GetDirectoryName(selection) : "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}";
                        return true;
                    }
                }
                catch
                {
                }
            }
            path = (string)null;
            return false;
        }
    }
}
