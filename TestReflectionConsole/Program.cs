using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;


namespace TestReflectionConsole
{
    class Program
    {

        public static String name = "TestReflectionConsole";
        public static Program WorkingProgram = new Program();


  /*      public String one = "1";
        public int two = 2;
        public bool isFamale = true;

        public short three = 3;
        public ushort five = 5;
        */

        public _Window window { get; set; }
        public _Tabs tabs { get; set; }
        public _BBar bbar { get; set; }

        public Program()
        {
            window = new _Window();
            tabs = new _Tabs();
            bbar = new _BBar();
         }

        public void dummy() {
           /* one = "1";
            two = 2;
            isFamale = true;
            three = 3;
            five = 5; */
        }

        /////

        [Serializable]
        public class _Window
        {
            public bool CaptureNewWindows { get; set; }
            public bool RestoreSession { get; set; }
            public bool RestoreOnlyLocked { get; set; }
            public bool CloseBtnClosesUnlocked { get; set; }
            public bool CloseBtnClosesSingleTab { get; set; }
            public bool TrayOnClose { get; set; }
            public bool TrayOnMinimize { get; set; }
            public byte[] DefaultLocation { get; set; }

            public _Window()
            {
                CaptureNewWindows = false;
                RestoreSession = false;
                RestoreOnlyLocked = false;
                CloseBtnClosesSingleTab = true;
                CloseBtnClosesUnlocked = false;
                TrayOnClose = false;
                TrayOnMinimize = false;

                string idl = Environment.OSVersion.Version >= new Version(6, 1)
                        ? "::{031E4825-7B94-4DC3-B131-E946B44C8DD5}"  // Libraries
                        : "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}"; // Computer
                
            }
        }


        [Serializable]
        public class _Tabs
        {
            
            public bool ActivateNewTab { get; set; }
            public bool NeverOpenSame { get; set; }
            public bool RenameAmbTabs { get; set; }
            public bool DragOverTabOpensSDT { get; set; }
            public bool ShowFolderIcon { get; set; }
            public bool ShowSubDirTipOnTab { get; set; }
            public bool ShowDriveLetters { get; set; }
            public bool ShowCloseButtons { get; set; }
            public bool CloseBtnsWithAlt { get; set; }
            public bool CloseBtnsOnHover { get; set; }
            public bool ShowNavButtons { get; set; }
            public bool NavButtonsOnRight { get; set; }
            public bool MultipleTabRows { get; set; }
            public bool ActiveTabOnBottomRow { get; set; }

            public _Tabs()
            {

                ActivateNewTab = true;
                NeverOpenSame = true;
                RenameAmbTabs = false;
                DragOverTabOpensSDT = false;
                ShowFolderIcon = true;
                ShowSubDirTipOnTab = true;
                ShowDriveLetters = false;
                ShowCloseButtons = false;
                CloseBtnsWithAlt = false;
                CloseBtnsOnHover = false;
                ShowNavButtons = false;
                MultipleTabRows = true;
                ActiveTabOnBottomRow = true;
            }
        }


        [Serializable]
        public class _BBar
        {
            public int[] ButtonIndexes { get; set; }
            public string[] ActivePluginIDs { get; set; }
            public bool LargeButtons { get; set; }
            public bool LockSearchBarWidth { get; set; }
            public bool LockDropDownButtons { get; set; }
            public bool ShowButtonLabels { get; set; }
            public string ImageStripPath { get; set; }

            public _BBar()
            {
                /* // the old 
                ButtonIndexes = QTUtility.IsXP 
                        ? new int[] {1, 2, 0, 3, 4, 5, 0, 6, 7, 0, 11, 13, 12, 14, 15, 0, 9, 20} 
                        : new int[] {3, 4, 5, 0, 6, 7, 0, 11, 13, 12, 14, 15, 0, 9, 20};
                ActivePluginIDs = new string[0];
                LockDropDownButtons = false;
                LargeButtons = true;
                LockSearchBarWidth = false;
                ShowButtonLabels = false;
                ImageStripPath = ""; */

                ButtonIndexes = 
                        /*isXP false
                        ? new int[] { 1, 2, 0, 3, 4, 5, 0, 6, 7, 0, 11, 13, 12, 14, 15, 0, 9, 20 }
                        : */new int[] { 3, 4, 5, 0, 6, 7, 0, 11, 13, 12, 14, 15, 0, 9, 20 };
                ActivePluginIDs = new string[0];
                LockDropDownButtons = false;
                LargeButtons = true;
                LockSearchBarWidth = false;
                ShowButtonLabels = false;
                ImageStripPath = "";
            }
        }


        //////



        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
 
           // NewMethod();

            PropertyInfo[] ps = WorkingProgram.GetType().GetProperties();

            Object pObj = null;
            PropertyInfo[] pifs = null;

            StreamWriter sw = File.CreateText("c:\\qttabbar_default_config_init.txt"); 
            foreach (PropertyInfo p in ps) {
                pObj = p.GetValue(WorkingProgram, null);

                if (pObj != null) {
                    pifs = pObj.GetType().GetProperties();
                    sw.WriteLine( pObj );
                    foreach (PropertyInfo pif in pifs)
                    {
                        StringBuilder b = new StringBuilder();

                        object po = pif.GetValue(pObj, null);

                        if (null != po)
                            if (pif.PropertyType == typeof(String))
                            {
                                b.Append("\"").Append(pif.GetValue(pObj, null)).Append("\"");
                            }
                            else if( pif.PropertyType.IsArray ) /* property type is array. */
                            {
                                 /* join like this "new System.Int32[] {1, 2, 3}; " */
                                Array arr = (Array)pif.GetValue(pObj, null);
                                b.Append( "new ").Append(arr.GetType()).Append( "{" );
                                for (int i = 0; i < arr.Length; i++) {
                                    b.Append(arr.GetValue(i)).Append(",");
                                }
                                b.Append("}");
                            }
                            else
                            {
                                b.Append(pif.GetValue(pObj, null).ToString().ToLower());
                            }
                        else
                        {
                            b.Append("null");
                        }
                        b.Append(";");
                        sw.WriteLine(pif.Name + "\t=\t" + b.ToString());
                    }// end for each 
                }
                sw.WriteLine( );
            }


            sw.Flush();
            sw.Close();
         }

        private static void NewMethod()
        {
            Type type = typeof(Program);
            FieldInfo[] fs = type.GetFields(BindingFlags.GetProperty);
            Object obj = null;
            FieldInfo[] pifs;
            Console.WriteLine(fs.Length);
            foreach (FieldInfo field in fs)
            {
                obj = field.GetValue(null);

                if (null != obj)
                {
                    pifs = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

                    foreach (FieldInfo pif in pifs)
                    {
                        StringBuilder b = new StringBuilder();
                        if (pif.FieldType == typeof(String))
                        {
                            b.Append("\"").Append(pif.GetValue(obj)).Append("\"");
                        }
                        else
                        {

                            b.Append(pif.GetValue(obj).ToString().ToLower());
                        }

                        b.Append(";");
                        Console.WriteLine(pif.Name + "\t=\t" + b.ToString());
                    }// end for each 
                }
            }// end for each 
        }
    }
}
