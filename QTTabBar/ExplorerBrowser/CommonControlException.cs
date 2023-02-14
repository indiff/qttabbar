
using System;
using System.Runtime.InteropServices;
using QTTabBarLib.Common;

namespace QTTabBarLib.ExplorerBrowser
{
    /// <summary>An exception thrown when an error occurs while dealing with Control objects.</summary>
    [Serializable]
    public class CommonControlException : COMException
    {
        /// <summary>Default constructor.</summary>
        public CommonControlException() { }

        /// <summary>Initializes an excpetion with a custom message.</summary>
        /// <param name="message"></param>
        public CommonControlException(string message) : base(message) { }

        /// <summary>Initializes an exception with custom message and inner exception.</summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public CommonControlException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>Initializes an exception with custom message and error code.</summary>
        /// <param name="message"></param>
        /// <param name="errorCode"></param>
        public CommonControlException(string message, int errorCode) : base(message, errorCode) { }

        /// <summary>Initializes an exception with custom message and error code.</summary>
        /// <param name="message"></param>
        /// <param name="errorCode"></param>
        internal CommonControlException(string message, HResult errorCode) : this(message, (int)errorCode) { }

        /// <summary>Initializes an exception from serialization info and a context.</summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected CommonControlException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}