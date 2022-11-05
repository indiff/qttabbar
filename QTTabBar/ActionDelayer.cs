using System;
using System.Windows.Forms;

namespace QTTabBarLib
{
    class ActionDelayer
    {
            public static void Add(Action action, int msec)
            {
                Timer timer = new Timer();
                timer.Interval = msec;
                timer.Tick += (EventHandler) ((s, e) =>
                {
                    timer.Stop();
                    try
                    {
                        action();
                    }
                    catch
                    {
                    }
                    timer.Dispose();
                });
                timer.Start();
            }

            public static void Add(Func<bool> func, int wait, int interval, int retry)
            {
                Timer timer = new Timer();
                timer.Interval = wait;
                int c = 0;
                timer.Tick += (EventHandler) ((s, e) =>
                {
                    bool flag = false;
                    try
                    {
                        if (++c > retry)
                        {
                            timer.Dispose();
                            return;
                        }
                        flag = func();
                    }
                    catch
                    {
                    }
                    if (flag)
                        timer.Dispose();
                    else
                        timer.Interval = interval;
                });
                timer.Start();
            }
    }
}
