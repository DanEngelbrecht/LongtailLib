using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LongtailLibTest
{
    [TestClass]
    public unsafe class UnitTestStorageAPI
    {
        [TestMethod]
        public unsafe void TestCreateInMemStorageAPI()
        {
            Longtail.Longtail_StorageAPI* storage_api = Longtail.Lib.Longtail_CreateFSStorageAPI();
            Longtail.Lib.Longtail_DisposeAPI((Longtail.Longtail_API*)storage_api);
        }

        [TestMethod]
        public unsafe void TestCreateFSStorageAPI()
        {
            Longtail.Longtail_StorageAPI* storage_api = Longtail.Lib.Longtail_CreateInMemStorageAPI();
            Longtail.Lib.Longtail_DisposeAPI((Longtail.Longtail_API*)storage_api);
        }
    }
}
