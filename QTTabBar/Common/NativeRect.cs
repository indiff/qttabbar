using System.Runtime.InteropServices;
using QTTabBarLib.Interop;

namespace QTTabBarLib.Common
{
    /// <summary>A wrapper for a RECT struct</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct NativeRect
    {
        /// <summary>Position of left edge</summary>
        public int Left { get; set; }

        /// <summary>Position of top edge</summary>
        public int Top { get; set; }

        /// <summary>Position of right edge</summary>
        public int Right { get; set; }

        /// <summary>Position of bottom edge</summary>
        public int Bottom { get; set; }

        /// <summary>Creates a new NativeRect initialized with supplied values.</summary>
        /// <param name="left">Position of left edge</param>
        /// <param name="top">Position of top edge</param>
        /// <param name="right">Position of right edge</param>
        /// <param name="bottom">Position of bottom edge</param>
        public NativeRect(int left, int top, int right, int bottom)
            : this()
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        /// <summary>Determines if two NativeRects are equal.</summary>
        /// <param name="first">First NativeRect</param>
        /// <param name="second">Second NativeRect</param>
        /// <returns>True if first NativeRect is equal to second; false otherwise.</returns>
        public static bool operator ==(NativeRect first, NativeRect second)
        {
            return first.Left == second.Left
                   && first.Top == second.Top
                   && first.Right == second.Right
                   && first.Bottom == second.Bottom;
        }

        /// <summary>Determines if two NativeRects are not equal</summary>
        /// <param name="first">First NativeRect</param>
        /// <param name="second">Second NativeRect</param>
        /// <returns>True if first is not equal to second; false otherwise.</returns>
        public static bool operator !=(NativeRect first, NativeRect second)
        {
            return !(first == second);
        }

        /// <summary>Determines if the NativeRect is equal to another Rect.</summary>
        /// <param name="obj">Another NativeRect to compare</param>
        /// <returns>True if this NativeRect is equal to the one provided; false otherwise.</returns>
        public override bool Equals(object obj)
        {
            return (obj != null && obj is NativeRect) ? this == (NativeRect)obj : false;
        }

        /// <summary>Creates a hash code for the NativeRect</summary>
        /// <returns>Returns hash code for this NativeRect</returns>
        public override int GetHashCode()
        {
            var hash = Left.GetHashCode();
            hash = hash * 31 + Top.GetHashCode();
            hash = hash * 31 + Right.GetHashCode();
            hash = hash * 31 + Bottom.GetHashCode();
            return hash;
        }

        /// <summary>Performs a conversion from <see cref="RECT"/> to <see cref="NativeRect"/>.</summary>
        /// <param name="r">The RECT.</param>
        /// <returns>The result of the conversion.</returns>
        internal static NativeRect FromRECT(RECT r)
        {
            return new NativeRect(r.left, r.top, r.right, r.bottom);
        }
    }
}