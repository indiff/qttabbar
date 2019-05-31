using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using BandObjectLib;
using QTTabBarLib;
using QTTabBarLib.Interop;


using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using QTTabBarLib.Interop;
using System.Diagnostics;

namespace WindowsFormsApplication1
{
    static class Program
    {

 /*       private static Regex singleLinebreakAtStart = new Regex(@"^(\r\n)?");
        public static Dictionary<string, string[]> ReadLanguageFile(string path)
        {

            const string linebreak = "\r\n";
            const string linebreakLiteral = @"\r\n";

            //We have to remove the first linebreak in the XML element's value, before we can split 
            //on the linebreak. It's there in the XML, when the XML is created using the editor.
            //Other linebreaks should be left in place, even if the line is empty, in order to preserve
            //the relative places of the other substrings.
            //The simplest way to do this is with a regular expression.

            try
            {
                var dictionary = XElement.Load(path).Elements().ToDictionary(
                    element => element.Name.ToString(),
                    element =>
                    {
                        string[] substrings =
                            ((string)element)
                            .Replace(singleLinebreakAtStart, "")
                            .Split(new[] { linebreak }, StringSplitOptions.None)
                            .Select(
                                s => s.Replace(linebreakLiteral, linebreak)
                            )
                            .ToArray();
                        return substrings;
                    }
                );
                return dictionary;
            }
            catch (XmlException xmlException)
            {
                string msg = String.Join("\r\n", new[] {
                    "Invalid language file.",
                    "",
                    xmlException.SourceUri,
                    "Line: " + xmlException.LineNumber,
                    "Position: " + xmlException.LinePosition,
                    "Detail: " + xmlException.Message
                });
                MessageBox.Show(msg);
                return null;
            }
            catch (Exception exception)
            {
                QTUtility2.MakeErrorLog(exception);
                return null;
            }
        }
*/

        static void test1( string arg )
        {
            // PENDING: Instead of something like GetFileType.
            SHFILEINFO psfi = new SHFILEINFO();
            int sz = System.Runtime.InteropServices.Marshal.SizeOf(psfi);
            // SHGFI_TYPENAME | SHGFI_USEFILEATTRIBUTES
            if (IntPtr.Zero == PInvoke.SHGetFileInfo("*." + arg , 0x80, ref psfi, sz, 0x400 | 0x10))
            {
               // System.Windows.Forms.MessageBox("");
                MessageBox.Show("zero");
            }
            else if (string.IsNullOrEmpty(psfi.szTypeName))
            {
                MessageBox.Show("null: ");
            }

            MessageBox.Show(arg + " -> " + psfi.szTypeName);
        }




        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {

            ///////// test for the extension files description ////////
/*            test1("txt");
            test1("log");
            test1("sql");
            test1("java");
            test1("c");
            test1("aspx");
            test1("xaml");*/
            ///////// test for the extension files description ////////


           
       //     QTTabBarClass.OpenOptionDialog();


            //
            //    UserControl1 u1 = new UserControl1();

            //     u1.InitializeComponent();


            testFileExists();

            // new Form1();

//            Dictionary<String,String[]> dict = QTTabBarClass.testQTUtilityReadLanguageFile("C:\\Users\\Administrator\\Desktop\\Lng_QTTabBar_.xml");
//            string[] values = null;
//            //dict.TryGetValue("TrayIcon", out values);
//            if ( null != values ) 
//            MessageBox.Show( values[0] );
//            MessageBox.Show("FUCK");
          //  MyMethod();


         //   app();


           // MessageBox.Show("" + Screen.PrimaryScreen.Bounds.Width);
           // MessageBox.Show("" + Screen.PrimaryScreen.Bounds.Height * 0.1 );
            // Process.Start(Environment.GetEnvironmentVariable("systemroot")  + "\\system32\\notepad.exe");
        }

        private static void testFileExists()
        {
            MessageBox.Show( "File exists : " + File.Exists(@"c://one") );

            MessageBox.Show("File exists: " + File.Exists(Environment.GetEnvironmentVariable("ProgramData") + @"\qttabbar\qtconfig.dll"));
            MessageBox.Show("File exists: " + File.Exists(Environment.GetEnvironmentVariable("ProgramData") + @"\qttabbar\config.dll"));
        }

        private static void app()
        {
            Form form = new Form1();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(form);
        }
        private static void MyMethod()
        {
            Regex singleLinebreakAtStart = new Regex(@"^(\r\n)?");

            const string linebreak = "\r\n";
            const string linebreakLiteral = @"\r\n";
            try
            {

                string[] substrings =
                    ((string)
                   "标签组1\r\n" +
                   "标签2\r\n" +
                   "标签3\r\n" +
                   "标签4\r\n" +
                   "标签5\r\n" +
                   "标签6\r\n" +
                   "标签7\r\n" +
                   "标签8\r\n" +
"恢复关闭的标签"
                    )
                    // .Replace(singleLinebreakAtStart, "")
                    .Split(new[] { linebreak }, StringSplitOptions.None)
                    .Select(
                   s => s.Replace(linebreakLiteral, linebreak)
                    )
                    .ToArray();

                // MessageBox.Show( "" + substrings.Length);
            }
            catch (Exception e)
            {
            }
        }
    }
}
