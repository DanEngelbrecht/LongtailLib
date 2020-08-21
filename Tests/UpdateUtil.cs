using System;
using System.Threading;
using System.Threading.Tasks;

namespace LongtailLib
{
    public class UpdateUtil
    {
        public static async Task<VersionIndex> GetCurrentVersionIndex(
            StorageAPI installStorage,
            string installPath,
            PathFilterFunc installFilter,
            VersionIndex targetVersionIndex,
            ProgressFunc progress,
            CancellationToken cancellationToken)
        {
            uint workerCount = (uint)Environment.ProcessorCount;
            using (var jobAPI = API.CreateBikeshedJobAPI(workerCount, -1))
            using (var hashRegistry = API.CreateFullHashRegistry())
            {
                UInt32 hashIdentifier = targetVersionIndex.HashIdentifier;
                if (hashIdentifier == 0)
                {
                    throw new Exception("Can not determine hash algorithm to use");
                }

                HashAPI hashAPI = API.GetHashAPI(hashRegistry, hashIdentifier);

                UInt32 targetChunkSize = targetVersionIndex.TargetChunkSize;

                VersionIndex result = await ScanVersion(
                    installStorage,
                    installPath,
                    installFilter,
                    jobAPI,
                    hashAPI,
                    targetChunkSize,
                    progress,
                    cancellationToken).ConfigureAwait(false);

                return result;
            }
        }

        public static async Task<BlockStoreStats> UpdateVersion(
            StorageAPI cacheStorage,
            string cachePath,
            IBlockStore remoteBlockStoreInterface,
            StorageAPI installStorage,
            string installPath,
            VersionIndex currentVersionIndex,
            VersionIndex targetVersionIndex,
            ProgressFunc updateProgress,
            CancellationToken cancellationToken)
        {
            uint workerCount = (uint)Environment.ProcessorCount;
            using (var jobAPI = API.CreateBikeshedJobAPI(workerCount, -1))
            using (var hashRegistry = API.CreateFullHashRegistry())
            using (var compressionRegistryAPI = API.CreateFullCompressionRegistry())
            using (var remoteBlockStore = API.MakeBlockStore(remoteBlockStoreInterface))
            using (var cacheLocalBlockStore = API.CreateFSBlockStoreAPI(jobAPI, cacheStorage, cachePath, 8388608, 1024))
            using (var cacheBlockStore = API.CreateCacheBlockStoreAPI(jobAPI, cacheLocalBlockStore, remoteBlockStore))
            using (var compressionBlockStore = API.CreateCompressBlockStoreAPI(cacheBlockStore, compressionRegistryAPI))
            using (var blockStore = API.CreateShareBlockStoreAPI(compressionBlockStore))
            using (var hashAPI = API.GetHashAPI(hashRegistry, targetVersionIndex.HashIdentifier))
            using (var versionDiff = API.CreateVersionDiff(hashAPI, currentVersionIndex, targetVersionIndex))
            using (var versionContentIndex = API.CreateContentIndexFromDiff(hashAPI, targetVersionIndex, versionDiff, 8388608, 1024))
            {

                using (var remoteContentIndex = await LongtailLib.API.RetargetContentIndex(blockStore, versionContentIndex))
                {
                    await ChangeVersion(
                        blockStore,
                        installStorage,
                        jobAPI,
                        hashAPI,
                        remoteContentIndex,
                        currentVersionIndex,
                        targetVersionIndex,
                        versionDiff,
                        installPath,
                        updateProgress,
                        cancellationToken).ConfigureAwait(false);

                    BlockStoreStats stats = API.BlockStoreGetStats(remoteBlockStore);
                    return stats;
                }
            }
        }

        private static async Task<VersionIndex> ScanVersion(
            StorageAPI localStorageAPI,
            string installPathInLocalStorageAPI,
            PathFilterFunc installFolderFilter,
            JobAPI jobAPI,
            HashAPI hashAPI,
            UInt32 targetChunkSize,
            ProgressFunc indexProgress,
            CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                using (var fileInfos = API.GetFilesRecursively(
                    localStorageAPI,
                    installFolderFilter,
                    cancellationToken,
                    installPathInLocalStorageAPI))
                {
                    VersionIndex currentVersionIndex = API.CreateVersionIndex(
                        localStorageAPI,
                        hashAPI,
                        jobAPI,
                        indexProgress,
                        cancellationToken,
                        installPathInLocalStorageAPI,
                        fileInfos,
                        null,
                        targetChunkSize);
                    return currentVersionIndex;
                }
            }).ConfigureAwait(false);
        }

        private static async Task ChangeVersion(
            BlockStoreAPI blockStore,
            StorageAPI localStorageAPI,
            JobAPI jobAPI,
            HashAPI hashAPI,
            ContentIndex remoteContentIndex,
            VersionIndex currentVersionIndex,
            VersionIndex targetVersionIndex,
            VersionDiff versionDiff,
            string installPathInLocalStorageAPI,
            ProgressFunc updateProgress,
            CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                API.SetLogLevel(API.LOG_LEVEL_DEBUG);
                API.ChangeVersion(
                        blockStore,
                        localStorageAPI,
                        hashAPI,
                        jobAPI,
                        updateProgress,
                        cancellationToken,
                        remoteContentIndex,
                        currentVersionIndex,
                        targetVersionIndex,
                        versionDiff,
                        installPathInLocalStorageAPI,
                        false); // On windows we don't care about retaining permissions
            }).ConfigureAwait(false);
        }
    }
}
