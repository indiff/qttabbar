using System;
using System.Runtime.InteropServices;

namespace QTTabBarLib.Common
{
    /// <summary>An exception thrown when an error occurs while dealing with the Property System API.</summary>
    [Serializable]
    public class PropertySystemException : ExternalException
    {
        /// <summary>Default constructor.</summary>
        public PropertySystemException() { }

        /// <summary>Initializes an excpetion with a custom message.</summary>
        /// <param name="message"></param>
        public PropertySystemException(string message) : base(message) { }

        /// <summary>Initializes an exception with custom message and inner exception.</summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public PropertySystemException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>Initializes an exception with custom message and error code.</summary>
        /// <param name="message"></param>
        /// <param name="errorCode"></param>
        public PropertySystemException(string message, int errorCode) : base(message, errorCode) { }

        /// <summary>Initializes an exception from serialization info and a context.</summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected PropertySystemException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}