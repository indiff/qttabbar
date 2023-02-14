using System.Runtime.InteropServices;

namespace QTTabBarLib.Common
{
    /// <summary>A wrapper for the native POINT structure.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct NativePoint
    {
        /// <summary>Initialize the NativePoint</summary>
        /// <param name="x">The x coordinate of the point.</param>
        /// <param name="y">The y coordinate of the point.</param>
        public NativePoint(int x, int y)
            : this()
        {
            X = x;
            Y = y;
        }

        /// <summary>The X coordinate of the point</summary>
        public int X { get; set; }

        /// <summary>The Y coordinate of the point</summary>
        public int Y { get; set; }

        /// <summary>Determines if two NativePoints are equal.</summary>
        /// <param name="first">First NativePoint</param>
        /// <param name="second">Second NativePoint</param>
        /// <returns>True if first NativePoint is equal to the second; false otherwise.</returns>
        public static bool operator ==(NativePoint first, NativePoint second)
        {
            return first.X == second.X
                   && first.Y == second.Y;
        }

        /// <summary>Determines if two NativePoints are not equal.</summary>
        /// <param name="first">First NativePoint</param>
        /// <param name="second">Second NativePoint</param>
        /// <returns>True if first NativePoint is not equal to the second; false otherwise.</returns>
        public static bool operator !=(NativePoint first, NativePoint second)
        {
            return !(first == second);
        }

        /// <summary>Determines if this NativePoint is equal to another.</summary>
        /// <param name="obj">Another NativePoint to compare</param>
        /// <returns>True if this NativePoint is equal obj; false otherwise.</returns>
        public override bool Equals(object obj)
        {
            return (obj != null && obj is NativePoint) ? this == (NativePoint)obj : false;
        }

        /// <summary>Gets a hash code for the NativePoint.</summary>
        /// <returns>Hash code for the NativePoint</returns>
        public override int GetHashCode()
        {
            var hash = X.GetHashCode();
            hash = hash * 31 + Y.GetHashCode();
            return hash;
        }
    }
}