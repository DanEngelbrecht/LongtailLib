using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LongtailLibTest
{
    [TestClass]
    public class UnitTestCallbacks
    {
        public int LogLevel = -1;
        public string LogString;

        [TestMethod]
        public unsafe void TestLogCallback()
        {
            GCHandle objHandle = GCHandle.Alloc(this, GCHandleType.WeakTrackResurrection);
            IntPtr context_ptr = GCHandle.ToIntPtr(objHandle);

            Longtail.LogCallback myLogCallBack =
                (void* context, int level, string str) =>
                {
                    UnitTestCallbacks obj = (UnitTestCallbacks)GCHandle.FromIntPtr((IntPtr)context).Target;
                    if (obj != null)
                    {
                        obj.LogLevel = level;
                        obj.LogString = str;
                    }
                };
            Longtail.Lib.Longtail_SetLogLevel(0);
            Longtail.Lib.Longtail_SetLog(myLogCallBack, (void*)context_ptr);
            Longtail.Lib.Longtail_SetLogLevel(1);
            Assert.AreEqual(LogString, "Longtail_SetLogLevel(1)");
            Assert.AreEqual(LogLevel, 1);
        }

        [TestMethod]
        unsafe public void TestAssertCallback()
        {
            Longtail.AssertCallback myAssertCallback = new Longtail.AssertCallback(AssertCB);
            Longtail.Lib.Longtail_SetAssert(myAssertCallback);
            Longtail.VersionIndex* version_index = null;
            Longtail.Lib.Longtail_ReadVersionIndexFromBuffer(null, 0, ref version_index);
            Longtail.Lib.Longtail_SetAssert(null);
            Assert.AreEqual("buffer != 0", given_assert_expression);
        }

        static string given_assert_expression;

        public static void AssertCB(string expression, string file, int line)
        {
            given_assert_expression = expression;
        }

        [TestMethod]
        unsafe public void TestProgress()
        {
            ulong progress_api_size = Longtail.Lib.Longtail_GetProgressAPISize();
            void* progress_api_mem = Longtail.Lib.Longtail_Alloc(progress_api_size);
            Longtail.DisposeFunc my_progress_dispose =
                (Longtail.LongtailAPI* api) =>
                {
                    Longtail.Lib.Longtail_Free(api);
                };
            uint done = 0;
            uint total = 0;
            Longtail.ProgressCallback my_progress_callback =
                (Longtail.ProgressAPI* progress_api, uint total_count, uint done_count) =>
                {
                    done = done_count;
                    total = total_count;
                };
            Longtail.ProgressAPI* my_progress_api = Longtail.Lib.Longtail_MakeProgressAPI(progress_api_mem, my_progress_dispose, my_progress_callback);
            Longtail.Lib.Longtail_Progress_OnProgress(my_progress_api, 100, 50);
            Longtail.Lib.Longtail_DisposeAPI((Longtail.LongtailAPI*)(my_progress_api));
            Assert.AreEqual((uint)50, done);
            Assert.AreEqual((uint)100, total);
        }
    }

}
