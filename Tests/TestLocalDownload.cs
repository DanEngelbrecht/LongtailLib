using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Threading;
using LongtailLib;
using System.IO.Abstractions.TestingHelpers;

namespace Tests
{
    class TestBlockStorage : IBlockStore
    {
        public TestBlockStorage(string storagePath)
        {
            m_StoragePath = storagePath;
        }

        public void PreflightGet(UInt64[] chunkHashes)
        {
        }

        public void GetStoredBlock(UInt64 blockHash, OnGetBlockComplete completeCallback)
        {
            string blockName = string.Format("0x{0:X16}", blockHash).ToLower();
            string blockPath = "\\chunks\\" + blockName.Substring(2, 4) + "\\" + blockName + ".lrb";
            byte[] storedBlockBuffer = File.ReadAllBytes(m_StoragePath + blockPath);
            StoredBlock storedBlock = API.ReadStoredBlockFromBuffer(storedBlockBuffer);
            completeCallback(storedBlock, null);
        }

        public void PutStoredBlock(StoredBlock storedBlock, OnPutBlockComplete completeCallback)
        {

            throw new NotImplementedException();
        }

        public BlockStoreStats GetStats()
        {
            return new BlockStoreStats();
        }

        public void GetExistingContent(UInt64[] chunkHashes, UInt32 minBlockUsagePercent, OnGetExistingContentComplete completeCallback)
        {
            string storeContentIndexPath = m_StoragePath + "\\store.lci";
            byte[] storedBlockBuffer = File.ReadAllBytes(storeContentIndexPath);
            ContentIndex contentIndex = API.ReadContentIndexFromBuffer(storedBlockBuffer);
            completeCallback(contentIndex, null);
        }

        public void Flush(OnFlushComplete completeCallback)
        {
            completeCallback(null);
        }

        private string m_StoragePath;
    }

    class DebugProgress
    {
        public DebugProgress(string title)
        {
            m_Title = title;
            m_Inited = false;
            m_OldPercent = 0;
        }
        string m_Title;
        bool m_Inited;
        uint m_OldPercent;
        public void OnProgress(uint totalCount, uint doneCount)
        {
            if (doneCount < totalCount)
            {
                if (!m_Inited)
                {
                    Debug.Write(m_Title + ":");
                    m_Inited = true;
                }
                uint percentDone = (100u * doneCount) / totalCount;
                if ((percentDone - m_OldPercent) >= 5)
                {
                    Debug.Write(" " + percentDone.ToString() + "%");
                    m_OldPercent = percentDone;
                }
                return;
            }
            if (m_Inited)
            {
                if (m_OldPercent != 100)
                {
                    Debug.Write(" 100%");
                }
                Debug.WriteLine(" Done");
            }
        }
    }

    [TestClass]
    public class UnitTestLocalDownload
    {
        [TestMethod]
        public async Task TestDownsyncTotalCmd()
        {
            var localFS = new FileSystem();
            var cacheFS = new MockFileSystem();
            var targetFS = new MockFileSystem();

            // Ugly and evil hack to get to test folder, but this entire class is a test-hack so...
            string cwd = localFS.Directory.GetCurrentDirectory();
            string projectRootDir = "Tests";
            while (!projectRootDir.Equals(localFS.Directory.GetParent(cwd).Name, StringComparison.InvariantCultureIgnoreCase))
            {
                cwd = localFS.Directory.GetParent(cwd).FullName;
            }
            string projectRoot = localFS.Directory.GetParent(cwd).FullName;

            string sourcePath = localFS.Path.Combine(new string[] { projectRoot, "TestData", "store", "index", "totalcmd.lvi" });
            string storePath = localFS.Path.Combine(new string[] { projectRoot, "TestData", "store" });
            string cachePath = "cache";
            string targetPath = "totalcmd";
            IFileSystem p = new FileSystem();
            targetFS.Directory.CreateDirectory(targetFS.Path.Combine(targetPath, "Saved"));
            targetFS.File.WriteAllText(targetFS.Path.Combine(new string[] { targetPath, "Saved", "log.txt" }), "This log should not be cleared by the sync operation");
            targetFS.File.WriteAllText(targetFS.Path.Combine(targetPath, "removeme.txt"), "This text file should be cleared by the operation");

            bool assertFailed = false;

            AssertHandle assertHandle = new AssertHandle(
                (string expression, string file, int line) => {
                    Debug.WriteLine(file + "(" + line.ToString() + ") Assert failure `" + expression + "`");
                    assertFailed = true;
                });
            LogHandle logHandle = new LogHandle(
                (LogContext logContext, string message) =>
                {
                    Debug.WriteLine(logContext.Level.ToString() + ": " + message);
                });
            API.SetLogLevel(API.LOG_LEVEL_WARNING);
            CancellationToken cancellationToken = new CancellationToken();

            VersionIndex targetVersionIndex = API.ReadVersionIndexFromBuffer(localFS.File.ReadAllBytes(sourcePath));
            DebugProgress indexingProgress = new DebugProgress("Indexing target");
            var targetFsStorage = new FileSystemStorage(targetFS);
            var targetStorage = API.MakeStorageAPI(targetFsStorage);
            var cacheFsStorage = new FileSystemStorage(cacheFS);
            var cacheStorage = API.MakeStorageAPI(cacheFsStorage);
            VersionIndex currentVersionIndex = await UpdateUtil.GetCurrentVersionIndex(
                targetStorage,
                targetPath,
                (string root_path, string asset_folder, string asset_name, bool isDir, ulong size, uint permissions) =>
                {
                    if (asset_folder.Length > root_path.Length)
                    {
                        // If we are not in the root folder, anything goes
                        return true;
                    }
                    // If there is a directory named "Saved" in the root folder, we ignore it
                    return !(isDir && asset_name == "Saved");
                },
                targetVersionIndex,
                (UInt32 totalCount, UInt32 doneCount) => { indexingProgress.OnProgress(totalCount, doneCount); },
                cancellationToken);

            TestBlockStorage remoteBlockStore = new TestBlockStorage(storePath);

            DebugProgress updateProgress = new DebugProgress("Updating");

            BlockStoreStats stats = await UpdateUtil.UpdateVersion(
                cacheStorage,
                cachePath,
                remoteBlockStore,
                targetStorage,
                targetPath,
                currentVersionIndex,
                targetVersionIndex,
                (UInt32 totalCount, UInt32 doneCount) => { updateProgress.OnProgress(totalCount, doneCount); },
                cancellationToken);

            currentVersionIndex.Dispose();
            targetVersionIndex.Dispose();

            cacheStorage.Dispose();
            targetStorage.Dispose();
            cacheFsStorage.Dispose();
            targetFsStorage.Dispose();

            logHandle.Dispose();
            assertHandle.Dispose();

            Assert.IsFalse(assertFailed);
            Assert.IsTrue(targetFS.File.Exists(targetFS.Path.Combine(new string[] { targetPath, "Saved", "log.txt" })));
            Assert.IsFalse(targetFS.File.Exists(targetFS.Path.Combine(targetPath, "removeme.txt")));
            Assert.IsTrue(targetFS.File.Exists(targetFS.Path.Combine(targetPath, "LANGUAGE", "WCMD_ENG.MNU")));
        }
    }
}
