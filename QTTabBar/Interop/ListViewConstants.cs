//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright =C 2007-2010  Quizo, Paul Accisano
//
//    QTTabBar is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    =at your option any later version.
//
//    QTTabBar is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with QTTabBar.  If not, see <http://www.gnu.org/licenses/>.

namespace QTTabBarLib.Interop {
    static class LVM {
        public const int FIRST                  = 0x1000;
        public const int GETBKCOLOR             = FIRST + 0;
        public const int SETBKCOLOR             = FIRST + 1;
        public const int GETIMAGELIST           = FIRST + 2;
        public const int SETIMAGELIST           = FIRST + 3;
        public const int GETITEMCOUNT           = FIRST + 4;
        public const int GETITEMA               = FIRST + 5;
        public const int GETITEMW               = FIRST + 75;
        public const int GETITEM                = GETITEMW;
        public const int SETITEMA               = FIRST + 6;
        public const int SETITEMW               = FIRST + 76;
        public const int SETITEM                = SETITEMW;
        public const int INSERTITEMA            = FIRST + 7;
        public const int INSERTITEMW            = FIRST + 77;
        public const int INSERTITEM             = INSERTITEMW;
        public const int DELETEITEM             = FIRST + 8;
        public const int DELETEALLITEMS         = FIRST + 9;
        public const int GETCALLBACKMASK        = FIRST + 10;
        public const int SETCALLBACKMASK        = FIRST + 11;
        public const int GETNEXTITEM            = FIRST + 12;
        public const int FINDITEMA              = FIRST + 13;
        public const int FINDITEMW              = FIRST + 83;
        public const int FINDITEM               = FINDITEMW;
        public const int GETITEMRECT            = FIRST + 14;
        public const int SETITEMPOSITION        = FIRST + 15;
        public const int GETITEMPOSITION        = FIRST + 16;
        public const int GETSTRINGWIDTHA        = FIRST + 17;
        public const int GETSTRINGWIDTHW        = FIRST + 87;
        public const int GETSTRINGWIDTH         = GETSTRINGWIDTHW;
        public const int HITTEST                = FIRST + 18;
        public const int ENSUREVISIBLE          = FIRST + 19;
        public const int SCROLL                 = FIRST + 20;
        public const int REDRAWITEMS            = FIRST + 21;
        public const int ARRANGE                = FIRST + 22;
        public const int EDITLABELA             = FIRST + 23;
        public const int EDITLABELW             = FIRST + 118;
        public const int EDITLABEL              = EDITLABELW;
        public const int GETEDITCONTROL         = FIRST + 24;
        public const int GETCOLUMNA             = FIRST + 25;
        public const int GETCOLUMNW             = FIRST + 95;
        public const int GETCOLUMN              = GETCOLUMNW;
        public const int SETCOLUMNA             = FIRST + 26;
        public const int SETCOLUMNW             = FIRST + 96;
        public const int SETCOLUMN              = SETCOLUMNW;
        public const int INSERTCOLUMNA          = FIRST + 27;
        public const int INSERTCOLUMNW          = FIRST + 97;
        public const int INSERTCOLUMN           = INSERTCOLUMNW;
        public const int DELETECOLUMN           = FIRST + 28;
        public const int GETCOLUMNWIDTH         = FIRST + 29;
        public const int SETCOLUMNWIDTH         = FIRST + 30;
        public const int GETHEADER              = FIRST + 31;
        public const int CREATEDRAGIMAGE        = FIRST + 33;
        public const int GETVIEWRECT            = FIRST + 34;
        public const int GETTEXTCOLOR           = FIRST + 35;
        public const int SETTEXTCOLOR           = FIRST + 36;
        public const int GETTEXTBKCOLOR         = FIRST + 37;
        public const int SETTEXTBKCOLOR         = FIRST + 38;
        public const int GETTOPINDEX            = FIRST + 39;
        public const int GETCOUNTPERPAGE        = FIRST + 40;
        public const int GETORIGIN              = FIRST + 41;
        public const int UPDATE                 = FIRST + 42;
        public const int SETITEMSTATE           = FIRST + 43;
        public const int GETITEMSTATE           = FIRST + 44;
        public const int GETITEMTEXTA           = FIRST + 45;
        public const int GETITEMTEXTW           = FIRST + 115;
        public const int GETITEMTEXT            = GETITEMTEXTW;
        public const int SETITEMTEXTA           = FIRST + 46;
        public const int SETITEMTEXTW           = FIRST + 116;
        public const int SETITEMTEXT            = SETITEMTEXTW;
        public const int SETITEMCOUNT           = FIRST + 47;
        public const int SORTITEMS              = FIRST + 48;
        public const int SETITEMPOSITION32      = FIRST + 49;
        public const int GETSELECTEDCOUNT       = FIRST + 50;
        public const int GETITEMSPACING         = FIRST + 51;
        public const int GETISEARCHSTRINGA      = FIRST + 52;
        public const int GETISEARCHSTRINGW      = FIRST + 117;
        public const int GETISEARCHSTRING       = GETISEARCHSTRINGW;
        public const int SETICONSPACING         = FIRST + 53;
        public const int SETEXTENDEDLISTVIEWSTYLE   = FIRST + 54;
        public const int GETEXTENDEDLISTVIEWSTYLE   = FIRST + 55;
        public const int GETSUBITEMRECT         = FIRST + 56;
        public const int SUBITEMHITTEST         = FIRST + 57;
        public const int SETCOLUMNORDERARRAY    = FIRST + 58;
        public const int GETCOLUMNORDERARRAY    = FIRST + 59;
        public const int SETHOTITEM             = FIRST + 60;
        public const int GETHOTITEM             = FIRST + 61;
        public const int SETHOTCURSOR           = FIRST + 62;
        public const int GETHOTCURSOR           = FIRST + 63;
        public const int APPROXIMATEVIEWRECT    = FIRST + 64;
        public const int SETWORKAREAS           = FIRST + 65;
        public const int GETWORKAREAS           = FIRST + 70;
        public const int GETNUMBEROFWORKAREAS   = FIRST + 73;
        public const int GETSELECTIONMARK       = FIRST + 66;
        public const int SETSELECTIONMARK       = FIRST + 67;
        public const int SETHOVERTIME           = FIRST + 71;
        public const int GETHOVERTIME           = FIRST + 72;
        public const int SETTOOLTIPS            = FIRST + 74;
        public const int GETTOOLTIPS            = FIRST + 78;
        public const int SORTITEMSEX            = FIRST + 81;
        public const int SETBKIMAGEA            = FIRST + 68;
        public const int SETBKIMAGEW            = FIRST + 138;
        public const int GETBKIMAGEA            = FIRST + 69;
        public const int GETBKIMAGEW            = FIRST + 139;
        public const int SETSELECTEDCOLUMN      = FIRST + 140;
        public const int SETTILEWIDTH           = FIRST + 141;
        public const int SETVIEW                = FIRST + 142;
        public const int GETVIEW                = FIRST + 143;
        public const int INSERTGROUP            = FIRST + 145;
        public const int SETGROUPINFO           = FIRST + 147;
        public const int GETGROUPINFO           = FIRST + 149;
        public const int REMOVEGROUP            = FIRST + 150;
        public const int MOVEGROUP              = FIRST + 151;
        public const int MOVEITEMTOGROUP        = FIRST + 154;
        public const int SETGROUPMETRICS        = FIRST + 155;
        public const int GETGROUPMETRICS        = FIRST + 156;
        public const int ENABLEGROUPVIEW        = FIRST + 157;
        public const int SORTGROUPS             = FIRST + 158;
        public const int INSERTGROUPSORTED      = FIRST + 159;
        public const int REMOVEALLGROUPS        = FIRST + 160;
        public const int HASGROUP               = FIRST + 161;
        public const int SETTILEVIEWINFO        = FIRST + 162;
        public const int GETTILEVIEWINFO        = FIRST + 163;
        public const int SETTILEINFO            = FIRST + 164;
        public const int GETTILEINFO            = FIRST + 165;
        public const int SETINSERTMARK          = FIRST + 166;
        public const int GETINSERTMARK          = FIRST + 167;
        public const int INSERTMARKHITTEST      = FIRST + 168;
        public const int GETINSERTMARKRECT      = FIRST + 169;
        public const int SETINSERTMARKCOLOR     = FIRST + 170;
        public const int GETINSERTMARKCOLOR     = FIRST + 171;
        public const int SETINFOTIP             = FIRST + 173;
        public const int GETSELECTEDCOLUMN      = FIRST + 174;
        public const int ISGROUPVIEWENABLED     = FIRST + 175;
        public const int GETOUTLINECOLOR        = FIRST + 176;
        public const int SETOUTLINECOLOR        = FIRST + 177;
        public const int CANCELEDITLABEL        = FIRST + 179;
        public const int MAPINDEXTOID           = FIRST + 180;
        public const int MAPIDTOINDEX           = FIRST + 181;
        public const int SETBKIMAGE             = SETBKIMAGEW;
        public const int GETBKIMAGE             = GETBKIMAGEW;
    };

    static class LVS_EX {
        public const uint GRIDLINES             = 0x00000001;
        public const uint SUBITEMIMAGES         = 0x00000002;
        public const uint CHECKBOXES            = 0x00000004;
        public const uint TRACKSELECT           = 0x00000008;
        public const uint HEADERDRAGDROP        = 0x00000010;
        public const uint FULLROWSELECT         = 0x00000020; // applies to report mode only
        public const uint ONECLICKACTIVATE      = 0x00000040;
        public const uint TWOCLICKACTIVATE      = 0x00000080;
        public const uint FLATSB                = 0x00000100;
        public const uint REGIONAL              = 0x00000200;
        public const uint INFOTIP               = 0x00000400; // listview does InfoTips for you
        public const uint UNDERLINEHOT          = 0x00000800;
        public const uint UNDERLINECOLD         = 0x00001000;
        public const uint MULTIWORKAREAS        = 0x00002000;
        public const uint LABELTIP              = 0x00004000; // listview unfolds partly hidden labels if it does not have infotip text
        public const uint BORDERSELECT          = 0x00008000; // border selection style instead of highlight
        public const uint DOUBLEBUFFER          = 0x00010000;
        public const uint HIDELABELS            = 0x00020000;
        public const uint SINGLEROW             = 0x00040000;
        public const uint SNAPTOGRID            = 0x00080000; // Icons automatically snap to grid.
        public const uint SIMPLESELECT          = 0x00100000; // Also changes overlay rendering to top right for icon mode.
        public const uint JUSTIFYCOLUMNS        = 0x00200000; // Icons are lined up in columns that use up the whole view area.
        public const uint TRANSPARENTBKGND      = 0x00400000; // Background is painted by the parent via WM_PRINTCLIENT
        public const uint TRANSPARENTSHADOWTEXT = 0x00800000; // Enable shadow text on transparent backgrounds only =useful with bitmaps
        public const uint AUTOAUTOARRANGE       = 0x01000000; // Icons automatically arrange if no icon positions have been set
        public const uint HEADERINALLVIEWS      = 0x02000000; // Display column header in all view modes
        public const uint AUTOCHECKSELECT       = 0x08000000;
        public const uint AUTOSIZECOLUMNS       = 0x10000000;
        public const uint COLUMNSNAPPOINTS      = 0x40000000;
        public const uint COLUMNOVERFLOW        = 0x80000000;
    };

    static class LVN {
        public const int FIRST                  = -100;
        public const int ITEMCHANGING           = FIRST - 0;
        public const int ITEMCHANGED            = FIRST - 1;
        public const int INSERTITEM             = FIRST - 2;
        public const int DELETEITEM             = FIRST - 3;
        public const int DELETEALLITEMS         = FIRST - 4;
        public const int BEGINLABELEDITA        = FIRST - 5;
        public const int BEGINLABELEDITW        = FIRST - 75;
        public const int ENDLABELEDITA          = FIRST - 6;
        public const int ENDLABELEDITW          = FIRST - 76;
        public const int COLUMNCLICK            = FIRST - 8;
        public const int BEGINDRAG              = FIRST - 9;
        public const int BEGINRDRAG             = FIRST - 11;
        public const int ODCACHEHINT            = FIRST - 13;
        public const int ODFINDITEMA            = FIRST - 52;
        public const int ODFINDITEMW            = FIRST - 79;
        public const int ITEMACTIVATE           = FIRST - 14;
        public const int ODSTATECHANGED         = FIRST - 15;
        public const int ODFINDITEM             = ODFINDITEMW;
        public const int HOTTRACK               = FIRST - 21;
        public const int GETDISPINFOA           = FIRST - 50;
        public const int GETDISPINFOW           = FIRST - 77;
        public const int SETDISPINFOA           = FIRST - 51;
        public const int SETDISPINFOW           = FIRST - 78;
        public const int BEGINLABELEDIT         = BEGINLABELEDITW;
        public const int ENDLABELEDIT           = ENDLABELEDITW;
        public const int GETDISPINFO            = GETDISPINFOW;
        public const int SETDISPINFO            = SETDISPINFOW;
        public const int KEYDOWN                = FIRST - 55;
        public const int MARQUEEBEGIN           = FIRST - 56;
        public const int GETINFOTIPA            = FIRST - 57;
        public const int GETINFOTIPW            = FIRST - 58;
        public const int GETINFOTIP             = GETINFOTIPW;
        public const int INCREMENTALSEARCHA     = FIRST - 62;
        public const int INCREMENTALSEARCHW     = FIRST - 63;
        public const int COLUMNDROPDOWN         = FIRST - 64;
        public const int COLUMNOVERFLOWCLICK    = FIRST - 66;
        public const int BEGINSCROLL            = FIRST - 80;
        public const int ENDSCROLL              = FIRST - 81;
        public const int LINKCLICK              = FIRST - 84;
        public const int GETEMPTYMARKUP         = FIRST - 87;
    };

    static class LVIS {
        public const int FOCUSED            = 0x0001;
        public const int SELECTED           = 0x0002;
        public const int CUT                = 0x0004;
        public const int DROPHILITED        = 0x0008;
        public const int GLOW               = 0x0010;
        public const int ACTIVATING         = 0x0020;
        public const int OVERLAYMASK        = 0x0F00;
        public const int STATEIMAGEMASK     = 0xF000;
    }

    static class LVIR {
        public const int BOUNDS         = 0;
        public const int ICON           = 1;
        public const int LABEL          = 2;
        public const int SELECTBOUNDS   = 3;
    }
    
    static class LVNI {
        public const int ALL            = 0x0000;
        public const int FOCUSED        = 0x0001;
        public const int SELECTED       = 0x0002;
        public const int CUT            = 0x0004;
        public const int DROPHILITED    = 0x0008;
        public const int STATEMASK      = (FOCUSED | SELECTED | CUT | DROPHILITED);
        public const int VISIBLEORDER   = 0x0010;
        public const int PREVIOUS       = 0x0020;
        public const int VISIBLEONLY    = 0x0040;
        public const int SAMEGROUPONLY  = 0x0080;
        public const int ABOVE          = 0x0100;
        public const int BELOW          = 0x0200;
        public const int TOLEFT         = 0x0400;
        public const int TORIGHT        = 0x0800;
        public const int DIRECTIONMASK  = (ABOVE | BELOW | TOLEFT | TORIGHT);
    }

    static class CDDS {
        public const int PREPAINT       = 1;
        public const int POSTPAINT      = 2;
        public const int PREERASE       = 3;
        public const int POSTERASE      = 4;
        public const int ITEM           = 0x10000;
        public const int ITEMPREPAINT   = (ITEM | PREPAINT);
        public const int ITEMPOSTPAINT  = (ITEM | POSTPAINT);
        public const int ITEMPREERASE   = (ITEM | PREERASE);
        public const int ITEMPOSTERASE  = (ITEM | POSTERASE);
        public const int SUBITEM        = 0x20000;
    }

    static class CDRF {
        public const int DODEFAULT          = 0x000;
        public const int NEWFONT            = 0x002;
        public const int SKIPDEFAULT        = 0x004;
        public const int DOERASE            = 0x008;
        public const int NOTIFYPOSTPAINT    = 0x010;
        public const int NOTIFYITEMDRAW     = 0x020;
        public const int NOTIFYSUBITEMDRAW  = 0x020;
        public const int NOTIFYPOSTERASE    = 0x040;
        public const int SKIPPOSTPAINT      = 0x100;
    }
}
