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
            Longtail.Longtail_HashAPI* hash_api = Longtail.Lib.Longtail_CreateBlake2HashAPI();
            uint identifier = Longtail.Lib.Longtail_Hash_GetIdentifier(hash_api);
            Assert.AreNotEqual((uint)0, identifier);
            Longtail.Lib.Longtail_DisposeAPI((Longtail.Longtail_API*)hash_api);
        }
        [TestMethod]
        public unsafe void TestBlake3API()
        {
            Longtail.Longtail_HashAPI* hash_api = Longtail.Lib.Longtail_CreateBlake3HashAPI();
            uint identifier = Longtail.Lib.Longtail_Hash_GetIdentifier(hash_api);
            Assert.AreNotEqual((uint)0, identifier);
            Longtail.Lib.Longtail_DisposeAPI((Longtail.Longtail_API*)hash_api);
        }
        [TestMethod]
        public unsafe void TestMeowHashAPI()
        {
            Longtail.Longtail_HashAPI* hash_api = Longtail.Lib.Longtail_CreateMeowHashAPI();
            uint identifier = Longtail.Lib.Longtail_Hash_GetIdentifier(hash_api);
            Assert.AreNotEqual((uint)0, identifier);
            Longtail.Lib.Longtail_DisposeAPI((Longtail.Longtail_API*)hash_api);
        }
    }
}
