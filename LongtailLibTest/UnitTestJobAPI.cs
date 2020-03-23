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
            LongtailLib.Longtail_JobAPI* job_api = LongtailLib.API.Longtail_CreateBikeshedJobAPI(9);
            uint workerCount = LongtailLib.API.Longtail_Job_GetWorkerCount(job_api);
            Assert.AreEqual((uint)9, workerCount);
            LongtailLib.API.Longtail_DisposeAPI((LongtailLib.Longtail_API*)job_api);
        }
    }
}
