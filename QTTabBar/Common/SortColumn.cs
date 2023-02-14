// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Runtime.InteropServices;

namespace QTTabBarLib.Common
{
    /// <summary>Stores information about how to sort a column that is displayed in the folder view.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SortColumn
    {
        /// <summary>Creates a sort column with the specified direction for the given property.</summary>
        /// <param name="propertyKey">Property key for the property that the user will sort.</param>
        /// <param name="direction">The direction in which the items are sorted.</param>
        public SortColumn(PropertyKey propertyKey, SortDirection direction)
            : this()
        {
            this.propertyKey = propertyKey;
            this.direction = direction;
        }

        /// <summary>
        /// The ID of the column by which the user will sort. A PropertyKey structure. For example, for the "Name" column, the property key
        /// is PKEY_ItemNameDisplay or <see cref="Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System.ItemName"/>.
        /// </summary>
        public PropertyKey PropertyKey
        {
            get { return propertyKey;}
            set
            {
                propertyKey = value;
            }
        }
        private PropertyKey propertyKey;

        /// <summary>The direction in which the items are sorted.</summary>
        // public SortDirection Direction { get => direction; set => direction = value; }
        public SortDirection Direction
        {
            get { return direction;}
            set
            {
                direction = value;
            }
        }

        private SortDirection direction;

        /// <summary>Implements the == (equality) operator.</summary>
        /// <param name="col1">First object to compare.</param>
        /// <param name="col2">Second object to compare.</param>
        /// <returns>True if col1 equals col2; false otherwise.</returns>
        public static bool operator ==(SortColumn col1, SortColumn col2)
        {
            return (col1.direction == col2.direction) &&
                   (col1.propertyKey == col2.propertyKey);
        }

        /// <summary>Implements the != (unequality) operator.</summary>
        /// <param name="col1">First object to compare.</param>
        /// <param name="col2">Second object to compare.</param>
        /// <returns>True if col1 does not equals col1; false otherwise.</returns>
        public static bool operator !=(SortColumn col1, SortColumn col2)
        {
            return !(col1 == col2);
        }

        /// <summary>Determines if this object is equal to another.</summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>Returns true if the objects are equal; false otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(SortColumn)) { return false; }
            return (this == (SortColumn)obj);
        }

        /// <summary>Generates a nearly unique hashcode for this structure.</summary>
        /// <returns>A hash code.</returns>
        public override int GetHashCode()
        {
            var hash = direction.GetHashCode();
            hash = hash * 31 + propertyKey.GetHashCode();
            return hash;
        }
    }
}