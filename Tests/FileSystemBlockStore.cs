using System;
using System.IO;
using System.IO.Abstractions;
using LongtailLib;

namespace Tests
{
    internal class FileSystemBlockStore : IBlockStore
    {
        public FileSystemBlockStore(IFileSystem fileSystem, string storagePath)
        {
            m_FileSystem = fileSystem;
            m_StoragePath = storagePath;
        }

        public void PreflightGet(UInt64[] blockHashes, OnPreflightStartedComplete completeCallback)
        {
            completeCallback(blockHashes, null);
        }

        public void GetStoredBlock(UInt64 blockHash, OnGetBlockComplete completeCallback)
        {
            string blockName = string.Format("0x{0:X16}", blockHash).ToLower();
            string blockPath = m_FileSystem.Path.Combine(new string[] { m_StoragePath, "chunks", blockName.Substring(2, 4), blockName + ".lrb"});
            byte[] storedBlockBuffer = m_FileSystem.File.ReadAllBytes(blockPath);
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
            string storeContentIndexPath = m_FileSystem.Path.Combine(m_StoragePath, "store.lsi");
            byte[] storedBlockBuffer = m_FileSystem.File.ReadAllBytes(storeContentIndexPath);
            StoreIndex storeIndex = API.ReadStoreIndexFromBuffer(storedBlockBuffer);
            completeCallback(storeIndex, null);
        }

        public void Flush(OnFlushComplete completeCallback)
        {
            completeCallback(null);
        }

        public void PruneBlocks(ulong[] blockKeepHashes, OnPruneComplete completeCallback)
        {
            completeCallback(0, null);
        }

        private readonly IFileSystem m_FileSystem;
        private readonly string m_StoragePath;
    }
}
