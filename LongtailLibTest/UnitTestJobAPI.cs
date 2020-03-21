using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LongtailLibTest
{
    [TestClass]
    public class UnitTestJobAPI
    {
        [TestMethod]
        public unsafe void TestBikshedJobAPI()
        {
            Longtail.Longtail_JobAPI* job_api = Longtail.Lib.Longtail_CreateBikeshedJobAPI(9);
            uint workerCount = Longtail.Lib.Longtail_Job_GetWorkerCount(job_api);
            Assert.AreEqual((uint)9, workerCount);
            Longtail.Lib.Longtail_DisposeAPI((Longtail.Longtail_API*)job_api);
        }
    }
}
