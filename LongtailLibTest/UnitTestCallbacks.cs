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
            Longtail.LogCallback myLogCallBack =
                (void* context, int level, string str) =>
                {
                    log_level = level;
                    log_string = str;
                };
            Longtail.Lib.Longtail_SetLogLevel(0);
            Longtail.Lib.Longtail_SetLog(myLogCallBack, null);
            Longtail.Lib.Longtail_SetLogLevel(1);
            Assert.AreEqual(log_level, 1);
            Assert.AreEqual(log_string, "Longtail_SetLogLevel(1)");
        }

        [TestMethod]
        unsafe public void TestAssertCallback()
        {
            string given_assert_expression = "";
            Longtail.AssertCallback myAssertCallback =
                (string expression, string file, int line) =>
                {
                    given_assert_expression = expression;
                };
            Longtail.Lib.Longtail_SetAssert(myAssertCallback);
            Longtail.Longtail_VersionIndex* version_index = null;
            Longtail.Lib.Longtail_ReadVersionIndexFromBuffer(null, 0, ref version_index);
            Longtail.Lib.Longtail_SetAssert(null);
            Assert.AreEqual("buffer != 0", given_assert_expression);
        }

        [TestMethod]
        unsafe public void TestProgress()
        {
            ulong progress_api_size = Longtail.Lib.Longtail_GetProgressAPISize();
            void* progress_api_mem = Longtail.Lib.Longtail_Alloc(progress_api_size);
            Longtail.DisposeFunc my_progress_dispose =
                (Longtail.Longtail_API* api) =>
                {
                    Longtail.Lib.Longtail_Free(api);
                };
            uint done = 0;
            uint total = 0;
            Longtail.ProgressCallback my_progress_callback =
                (Longtail.Longtail_ProgressAPI* progress_api, uint total_count, uint done_count) =>
                {
                    done = done_count;
                    total = total_count;
                };
            Longtail.Longtail_ProgressAPI* my_progress_api = Longtail.Lib.Longtail_MakeProgressAPI(progress_api_mem, my_progress_dispose, my_progress_callback);
            Longtail.Lib.Longtail_Progress_OnProgress(my_progress_api, 100, 50);
            Longtail.Lib.Longtail_DisposeAPI((Longtail.Longtail_API*)(my_progress_api));
            Assert.AreEqual((uint)50, done);
            Assert.AreEqual((uint)100, total);
        }

        class MyProgress : Longtail.IProgress
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
            Longtail.Lib.ProgressHandle progress_handle = new Longtail.Lib.ProgressHandle(progress);
            Longtail.Lib.Progress_OnProgress(progress_handle, 100, 50);
            Assert.AreEqual((uint)50, progress.done);
            Assert.AreEqual((uint)100, progress.total);
        }

        class MyOnComplete : Longtail.IAsyncComplete
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
            Longtail.Lib.ASyncCompleteHandle async_complete_handle = new Longtail.Lib.ASyncCompleteHandle(onComplete);
            Longtail.Lib.ASyncComplete_OnComplete(async_complete_handle, 16);
            Assert.AreEqual((int)16, onComplete.err);
        }
    }

}
