﻿using System;

namespace QTTabBarLib.Interop
{
  [Flags]
  public enum SWP
  {
    NOSIZE = 1,
    NOMOVE = 2,
    NOZORDER = 4,
    NOREDRAW = 8,
    NOACTIVATE = 16, // 0x00000010
    FRAMECHANGED = 32, // 0x00000020
    SHOWWINDOW = 64, // 0x00000040
    HIDEWINDOW = 128, // 0x00000080
    NOCOPYBITS = 256, // 0x00000100
    NOOWNERZORDER = 512, // 0x00000200
    NOSENDCHANGING = 1024, // 0x00000400
    DEFERERASE = 8192, // 0x00002000
    ASYNCWINDOWPOS = 16384, // 0x00004000
    STATECHANGED = 32768, // 0x00008000
  }
}
