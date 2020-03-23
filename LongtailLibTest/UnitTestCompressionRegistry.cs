using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LongtailLibTest
{
    [TestClass]
    public class UnitTestCompressionRegistry
    {
        [TestMethod]
        public unsafe void TestCreateCompressionRegistry()
        {
            LongtailLib.Longtail_CompressionRegistryAPI* compression_registry = LongtailLib.API.Longtail_CreateCompressionRegistry();
            LongtailLib.API.Longtail_DisposeAPI((LongtailLib.Longtail_API*)compression_registry);
        }
    }
}
