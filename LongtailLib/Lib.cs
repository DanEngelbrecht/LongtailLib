using System;
using System.Runtime.InteropServices;

namespace Longtail
{
    public interface IProgress
    {
        void OnProgress(uint total_count, uint done_count);
    };
    public interface IAsyncComplete
    {
        void OnComplete(int err);
    }

    public struct Longtail_API { }
    public struct Longtail_ProgressAPI { }
    public struct Longtail_AsyncCompleteAPI { }
    public struct Longtail_BlockStoreAPI { }

    public struct Longtail_StorageAPI { }

    public struct Longtail_HashAPI { }
    public struct Longtail_JobAPI { }

    public struct Longtail_Paths { }
    public struct Longtail_VersionIndex { }
    public struct Longtail_ContentIndex { }
    public struct Longtail_VersionDiff { }

    public struct Longtail_FileInfos { }
    public struct Longtail_CompressionAPI { }
    public struct Longtail_CompressionAPI_Settings { }
    public struct Longtail_CompressionRegistryAPI { }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate void DisposeFunc(Longtail_API* api);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void AssertCallback(string expression, string file, int line);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate void LogCallback(void* context, int level, string str);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate void ProgressCallback(Longtail_ProgressAPI* progress_api, uint total_count, uint done_count);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate void ASyncCompleteCallback(Longtail_AsyncCompleteAPI* async_complete_api, int err);

    public class Lib
    {
        public class ProgressHandle
        {
            public unsafe ProgressHandle(IProgress progress)
            {
                m_Progress = progress;
                ulong mem_size = Longtail_GetProgressAPISize();
                byte* mem = (byte*)Longtail_Alloc(mem_size);
                Longtail.DisposeFunc my_progress_dispose =
                    (Longtail.Longtail_API* api) =>
                    {
                        Longtail.Lib.Longtail_Free(m_ProgressAPI);
                    };
                Longtail.ProgressCallback my_progress_callback =
                    (Longtail.Longtail_ProgressAPI* progress_api, uint total_count, uint done_count) =>
                    {
                        m_Progress.OnProgress(total_count, done_count);
                    };
                m_ProgressAPI = Longtail.Lib.Longtail_MakeProgressAPI(mem, my_progress_dispose, my_progress_callback);
            }
            unsafe ~ProgressHandle()
            {
                Longtail_DisposeAPI((Longtail_API*)m_ProgressAPI);
            }
            unsafe public Longtail.Longtail_ProgressAPI* AsProgressAPI()
            {
                return m_ProgressAPI;
            }
            IProgress m_Progress;
            unsafe Longtail.Longtail_ProgressAPI* m_ProgressAPI;
        };

        public static unsafe void Progress_OnProgress(Longtail.Lib.ProgressHandle progress, uint total_count, uint done_count)
        {
            Longtail_Progress_OnProgress(progress.AsProgressAPI(), total_count, done_count);
        }

        public class ASyncCompleteHandle
        {
            public unsafe ASyncCompleteHandle(IAsyncComplete async_complete)
            {
                m_ASyncComplete = async_complete;
                ulong mem_size = Longtail_GetAsyncCompleteAPISize();
                byte* mem = (byte*)Longtail_Alloc(mem_size);
                Longtail.DisposeFunc my_async_complete_dispose =
                    (Longtail.Longtail_API* api) =>
                    {
                        Longtail.Lib.Longtail_Free(m_ASyncCompleteAPI);
                    };
                Longtail.ASyncCompleteCallback my_async_complete_callback =
                    (Longtail.Longtail_AsyncCompleteAPI* async_complete_api, int err) =>
                    {
                        m_ASyncComplete.OnComplete(err);
                    };
                m_ASyncCompleteAPI = Longtail.Lib.Longtail_MakeAsyncCompleteAPI(mem, my_async_complete_dispose, my_async_complete_callback);
            }
            unsafe ~ASyncCompleteHandle()
            {
                Longtail_DisposeAPI((Longtail_API*)m_ASyncCompleteAPI);
            }
            unsafe public Longtail_AsyncCompleteAPI* AsASyncCompleteAPI()
            {
                return m_ASyncCompleteAPI;
            }
            IAsyncComplete m_ASyncComplete;
            unsafe Longtail_AsyncCompleteAPI* m_ASyncCompleteAPI;
        };

        public static unsafe void ASyncComplete_OnComplete(Longtail.Lib.ASyncCompleteHandle async_complete, int err)
        {
            Longtail_AsyncComplete_OnComplete(async_complete.AsASyncCompleteAPI(), err);
        }

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern void Longtail_DisposeAPI(Longtail_API* api);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public static extern void Longtail_SetAssert([MarshalAs(UnmanagedType.FunctionPtr)] AssertCallback assertCallback);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern void Longtail_SetLog([MarshalAs(UnmanagedType.FunctionPtr)] LogCallback logCallback, void* context);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public static extern void Longtail_SetLogLevel(int level);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern void* Longtail_Alloc(ulong size);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern void Longtail_Free(void* data);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public static extern ulong Longtail_GetProgressAPISize();

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern Longtail.Longtail_ProgressAPI* Longtail_MakeProgressAPI(void* mem, [MarshalAs(UnmanagedType.FunctionPtr)] DisposeFunc disposeFunc, [MarshalAs(UnmanagedType.FunctionPtr)] ProgressCallback progressCallback);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern void Longtail_Progress_OnProgress(Longtail.Longtail_ProgressAPI* progressAPI, uint total_count, uint done_count);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public static extern ulong Longtail_GetAsyncCompleteAPISize();

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern Longtail_AsyncCompleteAPI* Longtail_MakeAsyncCompleteAPI(void* mem, [MarshalAs(UnmanagedType.FunctionPtr)] DisposeFunc disposeFunc, [MarshalAs(UnmanagedType.FunctionPtr)] ASyncCompleteCallback async_complete_callback);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern void Longtail_AsyncComplete_OnComplete(Longtail_AsyncCompleteAPI* aSyncCompleteAPI, int res);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern uint Longtail_Paths_GetCount(Longtail_Paths* paths);

#if (DEBUG)
    [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern Longtail_Paths* Longtail_FileInfos_GetPaths(Longtail_FileInfos * file_infos);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern ulong Longtail_FileInfos_GetSize(Longtail_FileInfos* file_infos, uint index);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern uint Longtail_FileInfos_GetPermissions(Longtail_FileInfos* file_infos, uint index);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern int Longtail_GetFilesRecursively(
            Longtail_StorageAPI* storage_api,
            string root_path,
            ref Longtail_FileInfos* out_file_infos);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern int Longtail_CreateVersionIndex(
            Longtail_StorageAPI* storage_api,
            Longtail_HashAPI* hash_api,
            Longtail_JobAPI* job_api,
            Longtail_ProgressAPI* progress_api,
            string root_path,
            Longtail_FileInfos* file_infos,
            uint[] asset_tags,
            uint max_chunk_size,
            ref Longtail_VersionIndex* out_version_index);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern int Longtail_ReadVersionIndexFromBuffer(void* buffer, ulong size, ref Longtail_VersionIndex* out_version_index);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern int Longtail_ReadVersionIndex(
            Longtail_StorageAPI* storage_api,
            string path,
            ref Longtail_VersionIndex* out_version_index);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern int Longtail_ReadContentIndexFromBuffer(
            void* buffer,
            ulong size,
            ref Longtail_ContentIndex* out_content_index);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern int Longtail_ReadContentIndex(
            Longtail_StorageAPI* storage_api,
            string path,
            ref Longtail_ContentIndex* out_content_index);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern int Longtail_CreateMissingContent(
            Longtail_HashAPI* hash_api,
            Longtail_ContentIndex* content_index,
            Longtail_VersionIndex* version,
            uint max_block_size,
            uint max_chunks_per_block,
            ref Longtail_ContentIndex* out_content_index);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern uint Longtail_ContentIndex_GetHashAPI(Longtail_ContentIndex* content_index);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern ulong Longtail_ContentIndex_GetBlockCount(Longtail_ContentIndex* content_index);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern ulong Longtail_ContentIndex_GetChunkCount(Longtail_ContentIndex* content_index);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern ulong Longtail_ContentIndex_BlockHashes(Longtail_ContentIndex* content_index);


#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern int Longtail_CreateVersionDiff(
            Longtail_VersionIndex* source_version,
            Longtail_VersionIndex* target_version,
            ref Longtail_VersionDiff* out_version_diff);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern int Longtail_ChangeVersion(
            Longtail_BlockStoreAPI* block_store_api,
            Longtail_StorageAPI* version_storage_api,
            Longtail_HashAPI* hash_api,
            Longtail_JobAPI* job_api,
            Longtail_ProgressAPI* progress_api,
            Longtail_ContentIndex* content_index,
            Longtail_VersionIndex* source_version,
            Longtail_VersionIndex* target_version,
            Longtail_VersionDiff* version_diff,
            string version_path,
            int retain_permissions);


#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern Longtail_StorageAPI* Longtail_CreateFSStorageAPI();

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern Longtail_StorageAPI* Longtail_CreateInMemStorageAPI();

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern uint Longtail_Hash_GetIdentifier(Longtail_HashAPI* hash_api);

#if (DEBUG)
    [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern Longtail_HashAPI* Longtail_CreateBlake2HashAPI();

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern uint Longtail_GetBlake2HashType();

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern Longtail_HashAPI* Longtail_CreateBlake3HashAPI();

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern uint Longtail_GetBlake3HashType();

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern Longtail_HashAPI* Longtail_CreateMeowHashAPI();

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern uint Longtail_GetMeowHashType();

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern Longtail_JobAPI* Longtail_CreateBikeshedJobAPI(ushort worker_count);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern uint Longtail_Job_GetWorkerCount(Longtail_JobAPI* job_api);


#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern Longtail_CompressionAPI* Longtail_CreateLizardCompressionAPI();

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern Longtail_CompressionAPI_Settings* Longtail_GetLizardMinQuality();
#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern Longtail_CompressionAPI_Settings* Longtail_GetLizardDefaultQuality();
#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern Longtail_CompressionAPI_Settings* Longtail_GetLizardMaxQuality();

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern Longtail_CompressionAPI* Longtail_CreateLZ4CompressionAPI();

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern Longtail_CompressionAPI_Settings* Longtail_GetLZ4DefaultQuality();


#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern Longtail_CompressionAPI* Longtail_CreateBrotliCompressionAPI();
#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern Longtail_CompressionAPI_Settings* Longtail_GetBrotliGenericMinQuality();
#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern Longtail_CompressionAPI_Settings* Longtail_GetBrotliGenericDefaultQuality();
#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern Longtail_CompressionAPI_Settings* Longtail_GetBrotliGenericMaxQuality();
#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern Longtail_CompressionAPI_Settings* Longtail_GetBrotliTextMinQuality();
#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern Longtail_CompressionAPI_Settings* Longtail_GetBrotliTextDefaultQuality();
#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern Longtail_CompressionAPI_Settings* Longtail_GetBrotliTextMaxQuality();

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern Longtail_CompressionAPI* Longtail_CreateZStdCompressionAPI();

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern Longtail_CompressionAPI_Settings* Longtail_GetZStdMinCompression();
#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern Longtail_CompressionAPI_Settings* Longtail_GetZStdDefaultQuality();

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern Longtail_CompressionAPI_Settings* Longtail_GetZStdMaxQuality();

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern Longtail_CompressionRegistryAPI* Longtail_CreateDefaultCompressionRegistry(
            uint compression_type_count,
            uint[] compression_types,
            Longtail_CompressionAPI*[] compression_apis,
            Longtail_CompressionAPI_Settings*[] compression_settings);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern Longtail_BlockStoreAPI* Longtail_CreateFSBlockStoreAPI(Longtail_StorageAPI* storage_api, string content_path);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern Longtail_BlockStoreAPI* Longtail_CreateCacheBlockStoreAPI(Longtail_BlockStoreAPI* local_block_store, Longtail_BlockStoreAPI* remote_block_store);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern Longtail_BlockStoreAPI* Longtail_CreateCompressBlockStoreAPI(Longtail_BlockStoreAPI* backing_block_store, Longtail_CompressionRegistryAPI* compression_registry);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern int Longtail_BlockStore_GetIndex(Longtail_BlockStoreAPI* block_store_api, Longtail_JobAPI* job_api, uint default_hash_api_identifier, Longtail_ProgressAPI* progress_api, ref Longtail_ContentIndex* out_content_index);
    }
}
