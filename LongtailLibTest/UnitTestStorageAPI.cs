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
            LongtailLib.Longtail_StorageAPI* storage_api = LongtailLib.API.Longtail_CreateFSStorageAPI();
            LongtailLib.API.Longtail_DisposeAPI((LongtailLib.Longtail_API*)storage_api);
        }

        [TestMethod]
        public unsafe void TestCreateFSStorageAPI()
        {
            LongtailLib.Longtail_StorageAPI* storage_api = LongtailLib.API.Longtail_CreateInMemStorageAPI();
            LongtailLib.API.Longtail_DisposeAPI((LongtailLib.Longtail_API*)storage_api);
        }
    }
}
