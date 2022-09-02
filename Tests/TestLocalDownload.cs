using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Threading;
using LongtailLib;
using System.IO.Abstractions.TestingHelpers;
using System.Text;

namespace Tests
{
    internal class DebugProgress
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
        static string[] LogLevel = { "DEBUG", "INFO", "WARNING", "ERROR" };
        [TestMethod]
        public async Task TestDownsyncTotalCmd()
        {
            var localFS = new FileSystem();
            var cacheFS = new MockFileSystem();
            var targetFS = new MockFileSystem();

            string testDataPath = TestData.GetTestDataPath(localFS);
            string sourcePath = localFS.Path.Combine(new string[] { testDataPath, "store", "index", "totalcmd.lvi" });
            string storePath = localFS.Path.Combine(new string[] { testDataPath, "store" });
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
                    var db = new StringBuilder();
                    db.Append($"{logContext.File}({logContext.Line}) {logContext.Function}() {LogLevel[logContext.Level]}");
                    db.Append(" {");
                    for (int f = 0; f < logContext.FieldCount; ++f)
                    {
                        db.Append($" {logContext.FieldName(f)} : {logContext.FieldValue(f)}");
                    }
                    db.Append("} ");
                    db.Append($": {message}");
                    Debug.WriteLine(db.ToString());
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

            FileSystemBlockStore remoteBlockStore = new FileSystemBlockStore(localFS, storePath);

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
