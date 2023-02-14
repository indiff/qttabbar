using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QTTabBarLib
{
    internal enum Toolbar
    {
        TabBar = 0,
        [Obsolete]
        ButtonBar = 1,
        CommandBar1 = 1,
        CommandBar2 = 2,
        [Obsolete]
        HorizontalVersatileBar = 2,
        CommandBarVertical = 3,
        [Obsolete]
        VerticalVersatileBar = 3,
        [Obsolete]
        MenuBar = 4,
        [Obsolete]
        StatusBar = 5,
        BottomTabBar = 6,
        ManagementBar = 7,
        ExtraViewBottom = 8,
        SecondViewBar = 8,
        ExtraViewLeft = 9,
        ThirdViewBar = 9,
    }
}
