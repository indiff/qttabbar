using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using BandObjectLib;
using QTPlugin;
using QTTabBarLib.Interop;

namespace QTTabBarLib
{
    /**
     internal 只有在同一程序集的文件中，内部类型或成员才可访问
     */
    public abstract class TabBarBase : BandObject
    {
        // 添加到分组
        protected ToolStripMenuItem tsmiAddToGroup;
        protected ToolStripMenuItem tsmiBrowseFolder;
        protected ToolStripMenuItem tsmiCloneThis;
        protected ToolStripMenuItem tsmiClose;
        protected ToolStripMenuItem tsmiCloseAllButCurrent;
        protected ToolStripMenuItem tsmiCloseAllButThis;
        protected ToolStripMenuItem tsmiCloseLeft;
        protected ToolStripMenuItem tsmiCloseRight;
        protected ToolStripMenuItem tsmiCloseWindow;
        protected ToolStripMenuItem tsmiCopy;
        protected ToolStripMenuItem tsmiCreateGroup;
        protected ToolStripMenuItem tsmiCreateWindow;
        protected ToolStripMenuItem tsmiExecuted;
        protected ToolStripMenuItem tsmiGroups;
        protected ToolStripMenuItem tsmiHistory;
        protected ToolStripMenuItem tsmiLastActiv;
        protected ToolStripMenuItem tsmiLockThis;
        protected ToolStripMenuItem tsmiLockToolbar;
        protected ToolStripMenuItem tsmiMergeWindows;
        protected ToolStripMenuItem tsmiOption;
        protected ToolStripMenuItem tsmiProp;
        protected ToolStripMenuItem tsmiTabOrder;
        protected ToolStripMenuItem tsmiUndoClose;

        /*add by qwop 2012.07.13*/
        protected ToolStripMenuItem tsmiOpenCmd;
        protected ToolStripMenuItem enableApiHook;
        /*add by qwop 2012.07.13*/

        protected ToolStripSeparator tssep_Sys1;
        protected ToolStripSeparator tssep_Sys2;
        protected ToolStripSeparator tssep_Tab1;
        protected ToolStripSeparator tssep_Tab2;
        protected ToolStripSeparator tssep_Tab3;
        protected ContextMenuStripEx contextMenuSys;
        protected ContextMenuStripEx contextMenuTab;

        internal virtual bool IsBottomBar()
        {
            return false;
        }

        internal virtual bool IsVertical()
        {
            return false;
        }
        // internal abstract TargetView TargetView { get; }
        protected Color HorizontalExplorerBarBackgroundColor
        {
            get
            {
                if (Config.Skin.DrawHorizontalExplorerBarBgColor)
                    return ShellColors.ExplorerBarHrztBGColor;
                return !QTUtility.InNightMode ? ShellColors.ExplorerBarHrztBGColor : ShellColors.Default;
            }
        }

        protected Color VerticalExplorerBarBackgroundColor
        {
            get
            {
                if (Config.Skin.DrawVerticalExplorerBarBgColor)
                {
                    return ShellColors.ExplorerBarVertBGColor;
                }
                return !QTUtility.InNightMode ? ShellColors.ExplorerBarVertBGColor : ShellColors.Default;
            }
        }

        protected abstract bool IsTabSubFolderMenuVisible { get; }
        protected abstract int CalcBandHeight(int count);

        protected QTabControl tabControl1;

    }
}
