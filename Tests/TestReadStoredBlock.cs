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

            // Ugly and evil hack to get to test folder, but this entire class is a test-hack so...
            string cwd = localFS.Directory.GetCurrentDirectory();
            string projectRootDir = "Tests";
            while (!projectRootDir.Equals(localFS.Directory.GetParent(cwd).Name, StringComparison.InvariantCultureIgnoreCase))
            {
                cwd = localFS.Directory.GetParent(cwd).FullName;
            }
            string projectRoot = localFS.Directory.GetParent(cwd).FullName;
            string sampleBlock = localFS.Path.Combine(new string[] { projectRoot, "TestData", "store", "chunks", "0c75", "0x0c752d07f7d8a2b7.lrb" });

            byte[] readBuffer = File.ReadAllBytes(sampleBlock);
            var storedBlock = LongtailLib.API.ReadStoredBlockFromBuffer(readBuffer);

            byte[] writeBuffer = LongtailLib.API.WriteStoredBlockToBuffer(storedBlock);
            Assert.AreEqual(readBuffer.Length, writeBuffer.Length);
        }
    }
}
