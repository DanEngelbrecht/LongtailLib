using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LongtailLibTest
{
    [TestClass]
    public class UnitTestHashAPI
    {
        [TestMethod]
        public unsafe void TestBlake2API()
        {
            LongtailLib.Longtail_HashAPI* hash_api = LongtailLib.API.Longtail_CreateBlake2HashAPI();
            uint identifier = LongtailLib.API.Longtail_Hash_GetIdentifier(hash_api);
            Assert.AreNotEqual((uint)0, identifier);
            LongtailLib.API.Longtail_DisposeAPI((LongtailLib.Longtail_API*)hash_api);
        }
        [TestMethod]
        public unsafe void TestBlake3API()
        {
            LongtailLib.Longtail_HashAPI* hash_api = LongtailLib.API.Longtail_CreateBlake3HashAPI();
            uint identifier = LongtailLib.API.Longtail_Hash_GetIdentifier(hash_api);
            Assert.AreNotEqual((uint)0, identifier);
            LongtailLib.API.Longtail_DisposeAPI((LongtailLib.Longtail_API*)hash_api);
        }
        [TestMethod]
        public unsafe void TestMeowHashAPI()
        {
            LongtailLib.Longtail_HashAPI* hash_api = LongtailLib.API.Longtail_CreateMeowHashAPI();
            uint identifier = LongtailLib.API.Longtail_Hash_GetIdentifier(hash_api);
            Assert.AreNotEqual((uint)0, identifier);
            LongtailLib.API.Longtail_DisposeAPI((LongtailLib.Longtail_API*)hash_api);
        }
    }
}
