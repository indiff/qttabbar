using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QTTabBarLib
{
    internal interface ICommandInvokerWindow
    {
        // bool InvokeCommand(CommandInfo info);

        // CustomViewBase GetView(TargetView targetView);

        IntPtr CommandWindowHandle { get; }
    }
}
