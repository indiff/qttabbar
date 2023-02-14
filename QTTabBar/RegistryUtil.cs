using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace QTTabBarLib
{
    class RegistryUtil
    {
        public static string LimitLength(string str, int nMax)
        {
            if (str == null)
                return (string)null;
            return str.Length > nMax ? str.Substring(0, nMax) : str;
        }

        public static void WriteSelection(string dirKey , string pathSelection)
        {
            try
            {
                using (RegistryKey registryKey = RegistryUtil.OpenUserRoot("Volatile", true, true))
                    registryKey.SetValue(dirKey, (object)LimitLength(pathSelection, 5120));
            }
            catch
            {
            }
        }
        public static T GetValueSafe<T>(RegistryKey rk, string valName, T defaultVal)
        {
            object obj1 = rk.GetValue(valName, (object) defaultVal);
            return obj1 != null && obj1 is T ? (T) obj1 : defaultVal;
        }

        public static string ReadSelection(string dirKey, bool fNoDelete = false)
        {
            try
            {
                using (RegistryKey rk = RegistryUtil.OpenUserRoot("Volatile", true))
                {
                    if (rk != null)
                    {
                        string valueSafe = RegistryUtil.GetValueSafe<string>(rk, dirKey, (string)null);
                        if (!fNoDelete)
                        {
                            rk.DeleteValue(dirKey, false);
                        }
                        return valueSafe;
                    }
                }
            }
            catch
            {
            }
            return (string)null;
        }


        public static RegistryKey OpenUserRoot(
            string subKeyName,
            bool writable = false,
            bool fCreate = false)
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\QTTabBar\\" + subKeyName, writable);
            if (registryKey == null & fCreate)
                registryKey = Registry.CurrentUser.CreateSubKey("Software\\QTTabBar\\" + subKeyName);
            return registryKey;
        }
    }
}
