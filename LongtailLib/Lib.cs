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

    public struct Longtail_BlockIndex { }
    public struct Longtail_Paths { }
    public struct Longtail_VersionIndex { }
    public struct Longtail_StoredBlock { }
    public struct Longtail_ContentIndex { }
    public struct Longtail_VersionDiff { }

    public struct Longtail_FileInfos { }

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
        public unsafe static extern uint Longtail_FileInfos_GetPathCount(Longtail_FileInfos* file_infos);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern string Longtail_FileInfos_GetPath(Longtail_FileInfos * file_infos, uint index);

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
            Longtail_Paths* paths,
            ulong[] asset_sizes,
            uint[] asset_permissions,
            uint[] asset_tags,
            uint max_chunk_size,
            ref Longtail_VersionIndex* out_version_index);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern int Longtail_CreateVersionIndexUtil(
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
        public unsafe static extern int Longtail_CreateVersionIndexFromBlocks(
            Longtail_StorageAPI* storage_api,
            Longtail_HashAPI* hash_api,
            Longtail_JobAPI* job_api,
            Longtail_ProgressAPI* progress_api,
            string root_path,
            Longtail_Paths* paths,
            ulong[] asset_sizes,
            uint[] asset_permissions,
            uint[] asset_tags,
            uint max_chunk_size,
            ref Longtail_VersionIndex* out_version_index);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern int Longtail_WriteVersionIndexToBuffer(
            Longtail_VersionIndex* version_index,
            ref void* out_buffer,
            ref ulong out_size);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern int Longtail_ReadVersionIndexFromBuffer(
            void* buffer,
            ulong size,
            ref Longtail_VersionIndex* out_version_index);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern int Longtail_WriteVersionIndex(
            Longtail_StorageAPI* storage_api,
            Longtail_VersionIndex* version_index,
            string path);

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
        public unsafe static extern ulong Longtail_GetContentIndexDataSize(
            ulong block_count,
            ulong chunk_count);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern ulong Longtail_GetContentIndexSize(
            ulong block_count,
            ulong chunk_count);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern int Longtail_InitContentIndexFromData(
            Longtail_ContentIndex* content_index,
            void* data,
            ulong data_size);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern int Longtail_InitContentIndex(
            Longtail_ContentIndex* content_index,
            void* data,
            ulong data_size,
            uint hash_api,
            ulong block_count,
            ulong chunk_count);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern int Longtail_CreateContentIndexFromBlocks(
            uint hash_identifier,
            ulong block_count,
            ref Longtail_BlockIndex* block_indexes,
            ref Longtail_ContentIndex* out_content_index);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern int Longtail_CreateContentIndex(
            Longtail_HashAPI* hash_api,
            ulong chunk_count,
            ulong[] chunk_hashes,
            uint[] chunk_sizes,
            uint[] chunk_tags,
            uint max_block_size,
            uint max_chunks_per_block,
            ref Longtail_ContentIndex* out_content_index);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern int Longtail_CreateContentIndexUtil(
            Longtail_HashAPI* hash_api,
            Longtail_VersionIndex* version_index,
            uint max_block_size,
            uint max_chunks_per_block,
            ref Longtail_ContentIndex* out_content_index);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern int Longtail_WriteContentIndexToBuffer(
            Longtail_ContentIndex* content_index,
            void** out_buffer,
            ulong* out_size);

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
        public unsafe static extern int Longtail_WriteContentIndex(
            Longtail_StorageAPI* storage_api,
            Longtail_ContentIndex* content_index,
            string path);

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
        public unsafe static extern int Longtail_WriteContent(
            Longtail_StorageAPI* source_storage_api,
            Longtail_BlockStoreAPI* block_store_api,
            Longtail_JobAPI* job_api,
            Longtail_ProgressAPI* progress_api,
            Longtail_ContentIndex* block_store_content_index,
            Longtail_ContentIndex* version_content_index,
            Longtail_VersionIndex* version_index,
            string assets_folder);

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
        public unsafe static extern int Longtail_RetargetContent(
            Longtail_ContentIndex* reference_content_index,
            Longtail_ContentIndex* content_index,
            Longtail_ContentIndex** out_content_index);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern int Longtail_MergeContentIndex(
            Longtail_ContentIndex* local_content_index,
            Longtail_ContentIndex* remote_content_index,
            ref Longtail_ContentIndex* out_content_index);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern int Longtail_WriteVersion(
            Longtail_BlockStoreAPI* block_storage_api,
            Longtail_StorageAPI* version_storage_api,
            Longtail_JobAPI* job_api,
            Longtail_ProgressAPI* progress_api,
            Longtail_ContentIndex* content_index,
            Longtail_VersionIndex* version_index,
            string version_path,
            int retain_permissions);

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
        public unsafe static extern ulong Longtail_GetBlockIndexSize(uint chunk_count);
#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern ulong Longtail_GetBlockIndexDataSize(uint chunk_count);
#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern Longtail_BlockIndex* Longtail_InitBlockIndex(void* mem, uint chunk_count);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern int Longtail_InitBlockIndexFromData(
            Longtail_BlockIndex* block_index,
            void* data,
            ulong data_size);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern int Longtail_CreateBlockIndex(
            Longtail_HashAPI* hash_api,
            uint tag,
            uint chunk_count,
            ulong* chunk_indexes,
            ulong[] chunk_hashes,
            uint[] chunk_sizes,
            ref Longtail_BlockIndex** out_block_index);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern int Longtail_WriteBlockIndexToBuffer(
            Longtail_BlockIndex* block_index,
            void** out_buffer,
            ulong* out_size);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern int Longtail_ReadBlockIndexFromBuffer(
            void* buffer,
            ulong size,
            ref Longtail_BlockIndex* out_block_index);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern int Longtail_WriteBlockIndex(
            Longtail_StorageAPI* storage_api,
            Longtail_BlockIndex* block_index,
            string path);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern int Longtail_ReadBlockIndex(
            Longtail_StorageAPI* storage_api,
            string path,
            ref Longtail_BlockIndex* out_block_index);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern ulong Longtail_GetStoredBlockSize(ulong block_data_size);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern int Longtail_InitStoredBlockFromData(
            Longtail_StoredBlock* stored_block,
            void* block_data,
            ulong block_data_size);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern int Longtail_CreateStoredBlock(
            ulong block_hash,
            uint chunk_count,
            uint tag,
            ulong[] chunk_hashes,
            uint[] chunk_sizes,
            uint block_data_size,
            ref Longtail_StoredBlock* out_stored_block);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern int Longtail_ValidateContent(
            Longtail_ContentIndex* content_index,
            Longtail_VersionIndex* version_index);

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern int Longtail_ValidateVersion(
            Longtail_ContentIndex* content_index,
            Longtail_VersionIndex* version_index);


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
//#if (DEBUG)
//        [DllImport("longtail_debug.dll")]
//#else
//        [DllImport("longtail.dll")]
//#endif
//        public unsafe static extern uint GET_LONGTAIL_BLAKE2_HASH_TYPE();

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern Longtail_HashAPI* Longtail_CreateBlake3HashAPI();
//#if (DEBUG)
//        [DllImport("longtail_debug.dll")]
//#else
//        [DllImport("longtail.dll")]
//#endif
//        public unsafe static extern uint GET_LONGTAIL_BLAKE3_HASH_TYPE();
#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern Longtail_HashAPI* Longtail_CreateMeowHashAPI();
//#if (DEBUG)
//        [DllImport("longtail_debug.dll")]
//#else
//        [DllImport("longtail.dll")]
//#endif
//        public unsafe static extern uint GET_LONGTAIL_MEOW_HASH_TYPE();

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
        public unsafe static extern Longtail_StorageAPI* Longtail_CreateFSStorageAPI();

#if (DEBUG)
        [DllImport("longtail_debug.dll")]
#else
        [DllImport("longtail.dll")]
#endif
        public unsafe static extern Longtail_StorageAPI* Longtail_CreateInMemStorageAPI();
    }
}
