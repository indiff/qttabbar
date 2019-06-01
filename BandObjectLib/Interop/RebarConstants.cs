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

namespace BandObjectLib {
    public static class RB {
        public const int INSERTBANDA        = (0x400 + 1);
        public const int DELETEBAND         = (0x400 + 2);
        public const int GETBARINFO         = (0x400 + 3);
        public const int SETBARINFO         = (0x400 + 4);
        public const int SETBANDINFOA       = (0x400 + 6);
        public const int SETPARENT          = (0x400 + 7);
        public const int HITTEST            = (0x400 + 8);
        public const int GETRECT            = (0x400 + 9);
        public const int INSERTBANDW        = (0x400 + 10);
        public const int SETBANDINFOW       = (0x400 + 11);
        public const int GETBANDCOUNT       = (0x400 + 12);
        public const int GETROWCOUNT        = (0x400 + 13);
        public const int GETROWHEIGHT       = (0x400 + 14);
        public const int IDTOINDEX          = (0x400 + 16); // wParam == id
        public const int GETTOOLTIPS        = (0x400 + 17);
        public const int SETTOOLTIPS        = (0x400 + 18);
        public const int SETBKCOLOR         = (0x400 + 19); // sets the default BK color
        public const int GETBKCOLOR         = (0x400 + 20); // defaults to CLR_NONE
        public const int SETTEXTCOLOR       = (0x400 + 21);
        public const int GETTEXTCOLOR       = (0x400 + 22); // defaults to 0x00000000
        public const int SIZETORECT         = (0x400 + 23); // resize the rebar/break bands and such to this rect (lparam)
        public const int INSERTBAND         = INSERTBANDW;
        public const int SETBANDINFO        = SETBANDINFOW;
        public const int BEGINDRAG          = (0x400 + 24);
        public const int ENDDRAG            = (0x400 + 25);
        public const int DRAGMOVE           = (0x400 + 26);
        public const int GETBARHEIGHT       = (0x400 + 27);
        public const int GETBANDINFOW       = (0x400 + 28);
        public const int GETBANDINFOA       = (0x400 + 29);
        public const int GETBANDINFO        = GETBANDINFOW;
        public const int MINIMIZEBAND       = (0x400 + 30);
        public const int MAXIMIZEBAND       = (0x400 + 31);
        public const int GETBANDBORDERS     = (0x400 + 34); // returns in lparam = lprc the amount of edges added to band wparam
        public const int SHOWBAND           = (0x400 + 35); // show/hide band
        public const int SETPALETTE         = (0x400 + 37);
        public const int GETPALETTE         = (0x400 + 38);
        public const int MOVEBAND           = (0x400 + 39);
        public const int GETBANDMARGINS     = (0x400 + 40);
        public const int SETEXTENDEDSTYLE   = (0x400 + 41);
        public const int GETEXTENDEDSTYLE   = (0x400 + 42);
        public const int PUSHCHEVRON        = (0x400 + 43);
        public const int SETBANDWIDTH       = (0x400 + 44); // set width for docked band
    }

    public static class RBBIM {
        public const int STYLE              = 0x0001;
        public const int COLORS             = 0x0002;
        public const int TEXT               = 0x0004;
        public const int IMAGE              = 0x0008;
        public const int CHILD              = 0x0010;
        public const int CHILDSIZE          = 0x0020;
        public const int SIZE               = 0x0040;
        public const int BACKGROUND         = 0x0080;
        public const int ID                 = 0x0100;
        public const int IDEALSIZE          = 0x0200;
        public const int LPARAM             = 0x0400;
        public const int HEADERSIZE         = 0x0800; // control the size of the header
        public const int CHEVRONLOCATION    = 0x1000;
        public const int CHEVRONSTATE       = 0x2000;
    }

    public static class RBBS {
        public const int BREAK              = 0x0001;  // break to new line
        public const int FIXEDSIZE          = 0x0002;  // band can't be sized
        public const int CHILDEDGE          = 0x0004;  // edge around top & bottom of child window
        public const int HIDDEN             = 0x0008;  // don't show
        public const int NOVERT             = 0x0010;  // don't show when vertical
        public const int FIXEDBMP           = 0x0020;  // bitmap doesn't move during band resize
        public const int VARIABLEHEIGHT     = 0x0040;  // allow autosizing of this child vertically
        public const int GRIPPERALWAYS      = 0x0080;  // always show the gripper
        public const int NOGRIPPER          = 0x0100;  // never show the gripper
        public const int USECHEVRON         = 0x0200;  // display drop-down button for this band if it's sized smaller than ideal width
        public const int HIDETITLE          = 0x0400;  // keep band title hidden
        public const int TOPALIGN           = 0x0800;  // keep band in top row
    }
}
