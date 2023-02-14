//Copyright (c) Microsoft Corporation.  All rights reserved.

using System;

namespace QTTabBarLib.Common
{
    /// <summary>A refence to an icon resource</summary>
    public struct IconReference
    {
        private static readonly char[] commaSeparator = new char[] { ',' };
        private string moduleName;
        private string referencePath;

        /// <summary>Overloaded constructor takes in the module name and resource id for the icon reference.</summary>
        /// <param name="moduleName">String specifying the name of an executable file, DLL, or icon file</param>
        /// <param name="resourceId">Zero-based index of the icon</param>
        public IconReference(string moduleName, int resourceId)
            : this()
        {
            if (string.IsNullOrEmpty(moduleName))
            {
                throw new ArgumentNullException("moduleName");
            }

            this.moduleName = moduleName;
            ResourceId = resourceId;
            referencePath = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "{0},{1}", moduleName, resourceId);
        }

        /// <summary>Overloaded constructor takes in the module name and resource id separated by a comma.</summary>
        /// <param name="refPath">Reference path for the icon consiting of the module name and resource id.</param>
        public IconReference(string refPath)
            : this()
        {
            if (string.IsNullOrEmpty(refPath))
            {
                throw new ArgumentNullException("refPath");
            }

            var refParams = refPath.Split(commaSeparator);

            if (refParams.Length != 2 || string.IsNullOrEmpty(refParams[0]) || string.IsNullOrEmpty(refParams[1]))
            {
                throw new ArgumentException(LocalizedMessages.InvalidReferencePath, "refPath");
            }

            moduleName = refParams[0];
            ResourceId = int.Parse(refParams[1], System.Globalization.CultureInfo.InvariantCulture);

            referencePath = refPath;
        }

        /// <summary>String specifying the name of an executable file, DLL, or icon file</summary>
        public string ModuleName
        {
            get
            {
                return moduleName;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("value");
                }
                moduleName = value;
            }
        }

        /// <summary>Reference to a specific icon within a EXE, DLL or icon file.</summary>
        public string ReferencePath
        {
            get
            {
                return referencePath;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("value");
                }

                var refParams = value.Split(commaSeparator);

                if (refParams.Length != 2 || string.IsNullOrEmpty(refParams[0]) || string.IsNullOrEmpty(refParams[1]))
                {
                    throw new ArgumentException(LocalizedMessages.InvalidReferencePath, "value");
                }

                ModuleName = refParams[0];
                ResourceId = int.Parse(refParams[1], System.Globalization.CultureInfo.InvariantCulture);

                referencePath = value;
            }
        }

        /// <summary>Zero-based index of the icon</summary>
        public int ResourceId { get; set; }

        /// <summary>Implements the != (unequality) operator.</summary>
        /// <param name="icon1">First object to compare.</param>
        /// <param name="icon2">Second object to compare.</param>
        /// <returns>True if icon1 does not equals icon1; false otherwise.</returns>
        public static bool operator !=(IconReference icon1, IconReference icon2)
        {
            return !(icon1 == icon2);
        }

        /// <summary>Implements the == (equality) operator.</summary>
        /// <param name="icon1">First object to compare.</param>
        /// <param name="icon2">Second object to compare.</param>
        /// <returns>True if icon1 equals icon1; false otherwise.</returns>
        public static bool operator ==(IconReference icon1, IconReference icon2)
        {
            return (icon1.moduleName == icon2.moduleName) &&
                   (icon1.referencePath == icon2.referencePath) &&
                   (icon1.ResourceId == icon2.ResourceId);
        }

        /// <summary>Determines if this object is equal to another.</summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>Returns true if the objects are equal; false otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is IconReference)) { return false; }
            return (this == (IconReference)obj);
        }

        /// <summary>Generates a nearly unique hashcode for this structure.</summary>
        /// <returns>A hash code.</returns>
        public override int GetHashCode()
        {
            var hash = moduleName.GetHashCode();
            hash = hash * 31 + referencePath.GetHashCode();
            hash = hash * 31 + ResourceId.GetHashCode();
            return hash;
        }
    }
}