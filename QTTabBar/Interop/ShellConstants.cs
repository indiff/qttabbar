//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2007-2010  Quizo, Paul Accisano
//
//    QTTabBar is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    QTTabBar is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with QTTabBar.  If not, see <http://www.gnu.org/licenses/>.

using System;

namespace QTTabBarLib.Interop {
    [Flags]
    public enum SVGIO : uint {
        BACKGROUND      = 0x0,
        SELECTION       = 0x1,
        ALLVIEW         = 0x2,
        CHECKED         = 0x3,
        TYPE_MASK       = 0xF,
        FLAG_VIEWORDER  = 0x80000000
    }

    [Flags]
    public enum SVSIF {
        DESELECT        = 0x000,
        SELECT          = 0x001,
        EDIT            = 0x003,
        DESELECTOTHERS  = 0x004,
        ENSUREVISIBLE   = 0x008,
        FOCUSED         = 0x010,
        TRANSLATEPT     = 0x020,
        SELECTIONMARK   = 00040,
        POSITIONITEM    = 0x080,
        CHECK           = 0x100,
        CHECK2          = 0x200,
        KEYBOARDSELECT  = 0x401,
        NOTAKEFOCUS     = 0x40000000
    }

    public enum FVM {
        AUTO        = -1,
        ICON        = 1,
        SMALLICON   = 2,
        LIST        = 3,
        DETAILS     = 4,
        THUMBNAIL   = 5,
        TILE        = 6,
        THUMBSTRIP  = 7,
        CONTENT     = 8, // Windows7
    }

    [Flags]
    public enum FWF : uint {
        NONE                    = 0x00000000,
        AUTOARRANGE             = 0x00000001,
        ABBREVIATEDNAMES        = 0x00000002,
        SNAPTOGRID              = 0x00000004,
        OWNERDATA               = 0x00000008,
        BESTFITWINDOW           = 0x00000010,
        DESKTOP                 = 0x00000020,
        SINGLESEL               = 0x00000040,
        NOSUBFOLDERS            = 0x00000080,
        TRANSPARENT             = 0x00000100,
        NOCLIENTEDGE            = 0x00000200,
        NOSCROLL                = 0x00000400,
        ALIGNLEFT               = 0x00000800,
        NOICONS                 = 0x00001000,
        SHOWSELALWAYS           = 0x00002000,
        NOVISIBLE               = 0x00004000,
        SINGLECLICKACTIVATE     = 0x00008000,
        NOWEBVIEW               = 0x00010000,
        HIDEFILENAMES           = 0x00020000,
        CHECKSELECT             = 0x00040000,
        NOENUMREFRESH           = 0x00080000,
        NOGROUPING              = 0x00100000,
        FULLROWSELECT           = 0x00200000,
        NOFILTERS               = 0x00400000,
        NOCOLUMNHEADER          = 0x00800000,
        NOHEADERINALLVIEWS      = 0x01000000,
        EXTENDEDTILES           = 0x02000000,
        TRICHECKSELECT          = 0x04000000,
        AUTOCHECKSELECT         = 0x08000000,
        NOBROWSERVIEWSTATE      = 0x10000000,
        SUBSETGROUPS            = 0x20000000,
        USESEARCHFOLDER         = 0x40000000,
        ALLOWRTLREADING         = 0x80000000,
    }

    [Flags]
    public enum SBSP : uint {
        DEFBROWSER              = 0x00000000,
        SAMEBROWSER             = 0x00000001,
        NEWBROWSER              = 0x00000002,
        DEFMODE                 = 0x00000000,
        OPENMODE                = 0x00000010,
        EXPLOREMODE             = 0x00000020,
        HELPMODE                = 0x00000040,
        NOTRANSFERHIST          = 0x00000080,
        AUTONAVIGATE            = 0x00000100,
        RELATIVE                = 0x00001000,
        PARENT                  = 0x00002000,
        NAVIGATEBACK            = 0x00004000,
        NAVIGATEFORWARD         = 0x00008000,
        ALLOW_AUTONAVIGATE      = 0x00010000,
        KEEPSAMETEMPLATE        = 0x00020000,
        KEEPWORDWHEELTEXT       = 0x00040000,
        ACTIVATE_NOFOCUS        = 0x00080000,
        CREATENOHISTORY         = 0x00100000,
        PLAYNOSOUND             = 0x00200000,
        CALLERUNTRUSTED         = 0x00800000,
        TRUSTFIRSTDOWNLOAD      = 0x01000000,
        UNTRUSTEDFORDOWNLOAD    = 0x02000000,
        NOAUTOSELECT            = 0x04000000,
        WRITENOHISTORY          = 0x08000000,
        TRUSTEDFORACTIVEX       = 0x10000000,
        FEEDNAVIGATION          = 0x20000000,
        REDIRECT                = 0x40000000,
        INITIATEDBYHLINKFRAME   = 0x80000000,
    }
}
