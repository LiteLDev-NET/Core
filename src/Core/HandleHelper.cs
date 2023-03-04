#define WINDOWS

#if (!WINDOWS)
#define LINUX
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace LiteLoader.NET.Internal;

public partial class HandleHelper
{
#if (WINDOWS)
    [LibraryImport("kernel32.dll", EntryPoint = "GetModuleHandleW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    public static partial nint GetModuleHandle(string lpLibFileNmae);
#elif (LINUX)
    private static nint GetModuleHandle(string lpLibFileNmae)
    {
        throw new NotImplementedException();
    }
#endif
}
