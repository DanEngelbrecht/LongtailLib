using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LongtailLibTest
{
    [TestClass]
    public class UnitTestCallbacks
    {
        [TestMethod]
        public unsafe void TestLogCallback()
        {
            int log_level = -1;
            string log_string = "";
            LongtailLib.LogCallback myLogCallBack =
                (void* context, int level, string str) =>
                {
                    log_level = level;
                    log_string = str;
                };
            LongtailLib.API.Longtail_SetLogLevel(0);
            LongtailLib.API.Longtail_SetLog(myLogCallBack, null);
            LongtailLib.API.Longtail_SetLogLevel(1);
            Assert.AreEqual(log_level, 1);
            Assert.AreEqual(log_string, "Longtail_SetLogLevel(1)");
        }
#if (DEBUG)
        [TestMethod]
        unsafe public void TestAssertCallback()
        {
            string given_assert_expression = "";
            LongtailLib.AssertCallback myAssertCallback =
                (string expression, string file, int line) =>
                {
                    given_assert_expression = expression;
                };
            LongtailLib.API.Longtail_SetAssert(myAssertCallback);
            LongtailLib.Longtail_VersionIndex* dummy = null;
            LongtailLib.API.Longtail_CreateVersionIndex(null, null, null, null, "", null, null, 0, ref dummy);
            LongtailLib.API.Longtail_SetAssert(null);
            Assert.AreEqual("storage_api != 0", given_assert_expression);
        }
#endif
        [TestMethod]
        unsafe public void TestProgress()
        {
            ulong progress_api_size = LongtailLib.API.Longtail_GetProgressAPISize();
            void* progress_api_mem = LongtailLib.API.Longtail_Alloc(progress_api_size);
            LongtailLib.DisposeFunc my_progress_dispose =
                (LongtailLib.Longtail_API* api) =>
                {
                    LongtailLib.API.Longtail_Free(api);
                };
            uint done = 0;
            uint total = 0;
            LongtailLib.ProgressCallback my_progress_callback =
                (LongtailLib.Longtail_ProgressAPI* progress_api, uint total_count, uint done_count) =>
                {
                    done = done_count;
                    total = total_count;
                };
            LongtailLib.Longtail_ProgressAPI* my_progress_api = LongtailLib.API.Longtail_MakeProgressAPI(progress_api_mem, my_progress_dispose, my_progress_callback);
            LongtailLib.API.Longtail_Progress_OnProgress(my_progress_api, 100, 50);
            LongtailLib.API.Longtail_DisposeAPI((LongtailLib.Longtail_API*)(my_progress_api));
            Assert.AreEqual((uint)50, done);
            Assert.AreEqual((uint)100, total);
        }

        class MyProgress : LongtailLib.IProgress
        {
            public void OnProgress(uint total_count, uint done_count)
            {
                total = total_count;
                done = done_count;
            }
            public uint total = 0;
            public uint done = 0;
        };

        [TestMethod]
        public void TestProgressHandle()
        {
            MyProgress progress = new MyProgress();
            LongtailLib.API.ProgressHandle progress_handle = new LongtailLib.API.ProgressHandle(progress);
            LongtailLib.API.Progress_OnProgress(progress_handle, 100, 50);
            Assert.AreEqual((uint)50, progress.done);
            Assert.AreEqual((uint)100, progress.total);
            progress_handle.Close();
        }

        class MyOnComplete : LongtailLib.IAsyncComplete
        {
            public void OnComplete(int err)
            {
                this.err = err;
            }
            public int err = -1;
        };

        [TestMethod]
        public void TestASyncCompleteHandle()
        {
            MyOnComplete onComplete = new MyOnComplete();
            LongtailLib.API.ASyncCompleteHandle async_complete_handle = new LongtailLib.API.ASyncCompleteHandle(onComplete);
            LongtailLib.API.ASyncComplete_OnComplete(async_complete_handle, 16);
            Assert.AreEqual((int)16, onComplete.err);
        }
    }

}
