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
                EventHandler handler = null;
                handler = (EventHandler)((s, e) =>
                {
                    
                    try
                    {
                        action();
                        timer.Stop();
                    }
                    catch
                    {
                    } finally {
                        timer.Tick -= handler;
                        timer.Dispose(); // 释放资源，防止对象累积
                    }
                });
                timer.Tick += handler;
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
