using System;
using System.Runtime.InteropServices;

namespace Longtail
{
    public interface ProgressAPI
    {
        void OnProgress(uint total_count, uint done_count);
    };
    public struct Native_LongtailAPI { }
    public struct Native_VersionIndex { }
    public struct Native_ProgressAPI { }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate void DisposeFunc(Native_LongtailAPI* api);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void AssertCallback(string expression, string file, int line);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate void LogCallback(void* context, int level, string str);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate void ProgressCallback(Native_ProgressAPI* progress_api, uint total_count, uint done_count);

    public class Lib
    {
        public class ProgressHandle
        {
            public unsafe ProgressHandle(ProgressAPI progress)
            {
                m_Progress = progress;
                ulong mem_size = Longtail_GetProgressAPISize();
                byte* mem = (byte*)Longtail_Alloc(mem_size);
                Longtail.DisposeFunc my_progress_dispose =
                    (Longtail.Native_LongtailAPI* api) =>
                    {
                        Longtail.Lib.Longtail_Free(m_ProgressAPI);
                    };
                Longtail.ProgressCallback my_progress_callback =
                    (Longtail.Native_ProgressAPI* progress_api, uint total_count, uint done_count) =>
                    {
                        m_Progress.OnProgress(total_count, done_count);
                    };
                m_ProgressAPI = Longtail.Lib.Longtail_MakeProgressAPI(mem, my_progress_dispose, my_progress_callback);
            }
            unsafe ~ProgressHandle()
            {
                Longtail_DisposeAPI((Native_LongtailAPI*)m_ProgressAPI);
            }
            unsafe public Native_ProgressAPI* AsProgressAPI()
            {
                return m_ProgressAPI;
            }
            ProgressAPI m_Progress;
            unsafe Native_ProgressAPI* m_ProgressAPI;
        };

        public static unsafe void Progress_OnProgress(Longtail.Lib.ProgressHandle progress, uint total_count, uint done_count)
        {
            Longtail_Progress_OnProgress(progress.AsProgressAPI(), total_count, done_count);
        }

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern void Longtail_DisposeAPI(Native_LongtailAPI* api);

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
        public unsafe static extern int Longtail_ReadVersionIndexFromBuffer(byte[] buffer, ulong size, ref Native_VersionIndex* outVersionIndex);

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
        public unsafe static extern Native_ProgressAPI* Longtail_MakeProgressAPI(void* mem, [MarshalAs(UnmanagedType.FunctionPtr)] DisposeFunc disposeFunc, [MarshalAs(UnmanagedType.FunctionPtr)] ProgressCallback progressCallback);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern void Longtail_Progress_OnProgress(Native_ProgressAPI* progressAPI, uint total_count, uint done_count);
    }
}
