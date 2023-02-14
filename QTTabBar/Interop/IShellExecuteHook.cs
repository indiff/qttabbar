using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

namespace QTTabBarLib.Interop
{
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [Guid("000214FB-0000-0000-C000-000000000046")]
  [SuppressUnmanagedCodeSecurity]
  [ComImport]
  internal interface IShellExecuteHook
  {
    [MethodImpl(MethodImplOptions.PreserveSig)]
    int Execute([In] ref SHELLEXECUTEINFO sei);
  }
}
