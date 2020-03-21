using System;
using System.Runtime.InteropServices;

namespace Longtail
{
    public struct LongtailAPI { }
    public struct VersionIndex { }
    public struct ProgressAPI { }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate void DisposeFunc(LongtailAPI* api);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void AssertCallback(string expression, string file, int line);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate void LogCallback(void* context, int level, string str);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate void ProgressCallback(ProgressAPI* progress_api, uint total_count, uint done_count);

    public class Lib
    {
#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern void Longtail_DisposeAPI(LongtailAPI* api);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public static extern void Longtail_SetAssert([MarshalAs(UnmanagedType.FunctionPtr)] AssertCallback assertCallback);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern void Longtail_SetLog([MarshalAs(UnmanagedType.FunctionPtr)] LogCallback logCallback, void* context);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public static extern void Longtail_SetLogLevel(int level);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern void* Longtail_Alloc(ulong size);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern void Longtail_Free(void* data);


#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern int Longtail_ReadVersionIndexFromBuffer(byte[] buffer, ulong size, ref VersionIndex* outVersionIndex);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public static extern ulong Longtail_GetProgressAPISize();

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern ProgressAPI* Longtail_MakeProgressAPI(void* mem, [MarshalAs(UnmanagedType.FunctionPtr)] DisposeFunc disposeFunc, [MarshalAs(UnmanagedType.FunctionPtr)] ProgressCallback progressCallback);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern void Longtail_Progress_OnProgress(ProgressAPI* progressAPI, uint total_count, uint done_count);
    }
}
