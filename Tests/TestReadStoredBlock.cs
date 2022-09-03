using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.IO.Abstractions;

namespace Tests
{
    [TestClass]
    public class TestReadStoredBlock
    {
        [TestMethod]
        public void ReadStoredBlock()
        {
            var localFS = new FileSystem();
            string testDataPath = TestData.GetTestDataPath(localFS);
            string sampleBlock = localFS.Path.Combine(new string[] { testDataPath, "store", "chunks", "0c75", "0x0c752d07f7d8a2b7.lrb" });

            byte[] readBuffer = File.ReadAllBytes(sampleBlock);
            using var storedBlock = LongtailLib.API.ReadStoredBlockFromBuffer(readBuffer);

            byte[] writeBuffer = LongtailLib.API.WriteStoredBlockToBuffer(storedBlock);
            Assert.AreEqual(readBuffer.Length, writeBuffer.Length);
        }
    }
}
