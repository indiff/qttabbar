//Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Globalization;
using System.Text;

namespace QTTabBarLib.Common
{
    /// <summary>Common Helper methods</summary>
    public static class CoreHelpers
    {
        /// <summary>Determines if the application is running on Vista</summary>
        public static bool RunningOnVista
        {
            get
            {
                return Environment.OSVersion.Version.Major >= 6;
            }
        }

        /// <summary>Determines if the application is running on Windows 7</summary>
        public static bool RunningOnWin7
        {
            // Verifies that OS version is 6.1 or greater, and the Platform is WinNT.
            get
            {
                return Environment.OSVersion.Platform == PlatformID.Win32NT &&
                       Environment.OSVersion.Version.CompareTo(new Version(6, 1)) >= 0;
            }
        }


        /// <summary>Determines if the application is running on XP</summary>
        public static bool RunningOnXP()
        {
            return Environment.OSVersion.Platform == PlatformID.Win32NT &&
                   Environment.OSVersion.Version.Major >= 5;
        }

        /// <summary>Get a string resource given a resource Id</summary>
        /// <param name="resourceId">The resource Id</param>
        /// <returns>
        /// The string resource corresponding to the given resource Id. Returns null if the resource id is invalid or the string cannot be
        /// retrieved for any other reason.
        /// </returns>
        public static string GetStringResource(string resourceId)
        {
            string[] parts;
            string library;
            int index;

            if (string.IsNullOrEmpty(resourceId)) { return string.Empty; }

            // Known folder "Recent" has a malformed resource id for its tooltip. This causes the resource id to parse into 3 parts instead
            // of 2 parts if we don't fix.
            resourceId = resourceId.Replace("shell32,dll", "shell32.dll");
            parts = resourceId.Split(new char[] { ',' });

            library = parts[0];
            library = library.Replace(@"@", string.Empty);
            library = Environment.ExpandEnvironmentVariables(library);
            var handle = CoreNativeMethods.LoadLibrary(library);

            parts[1] = parts[1].Replace("-", string.Empty);
            index = int.Parse(parts[1], CultureInfo.InvariantCulture);

            var stringValue = new StringBuilder(255);
            var retval = CoreNativeMethods.LoadString(handle, index, stringValue, 255);

            return retval != 0 ? stringValue.ToString() : null;
        }

        /// <summary>Throws PlatformNotSupportedException if the application is not running on Windows Vista</summary>
        public static void ThrowIfNotVista()
        {
            if (!CoreHelpers.RunningOnVista)
            {
                throw new PlatformNotSupportedException(LocalizedMessages.CoreHelpersRunningOnVista);
            }
        }

        /// <summary>Throws PlatformNotSupportedException if the application is not running on Windows 7</summary>
        public static void ThrowIfNotWin7()
        {
            if (!CoreHelpers.RunningOnWin7)
            {
                throw new PlatformNotSupportedException(LocalizedMessages.CoreHelpersRunningOn7);
            }
        }

        /// <summary>Throws PlatformNotSupportedException if the application is not running on Windows XP</summary>
        public static void ThrowIfNotXP()
        {
            if (!CoreHelpers.RunningOnXP())
            {
                throw new PlatformNotSupportedException(LocalizedMessages.CoreHelpersRunningOnXp);
            }
        }
    }
}