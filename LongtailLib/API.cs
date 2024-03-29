﻿using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LongtailLib
{
    public delegate void AssertFunc(string expression, string file, int line);
    public delegate void LogFunc(LogContext logContext, string message);
    public delegate bool PathFilterFunc(string rootPath, string assetPath, string assetName, bool isDir, ulong size, uint permissions);
    public delegate void ProgressFunc(UInt32 totalCount, UInt32 doneCount);

    public delegate void OnPutBlockComplete(Exception e);
    public delegate void OnPreflightStartedComplete(UInt64[] blockHashes, Exception e);
    public delegate void OnGetBlockComplete(StoredBlock storedBlock, Exception e);
    public delegate void OnGetExistingContentComplete(StoreIndex storeIndex, Exception e);
    public delegate void OnPruneComplete(UInt32 prunedBlockCount, Exception e);
    public delegate void OnFlushComplete(Exception e);

    public enum StatU64 : UInt32 {
        GetStoredBlock_Count = 0,
        GetStoredBlock_RetryCount = 1,
        GetStoredBlock_FailCount = 2,
        GetStoredBlock_Chunk_Count = 3,
        GetStoredBlock_Byte_Count = 4,

        PutStoredBlock_Count = 5,
        PutStoredBlock_RetryCount = 6,
        PutStoredBlock_FailCount = 7,
        PutStoredBlock_Chunk_Count = 8,
        PutStoredBlock_Byte_Count = 9,

        GetExistingContent_Count = 10,
        GetExistingContent_RetryCount = 11,
        GetExistingContent_FailCount = 12,

        PruneBlocks_Count = 13,
        PruneBlocks_RetryCount = 14,
        PruneBlocks_FailCount = 15,

        PreflightGet_Count = 16,
        PreflightGet_RetryCount = 17,
        PreflightGet_FailCount = 18,

        Flush_Count = 19,
        Flush_FailCount = 20,

        GetStats_Count = 21,
            Count = 22
    }

    public class BlockStoreStats
    {
        public BlockStoreStats()
        {
            m_StatU64 = new ulong[(int)StatU64.Count];
        }
        public ulong[] m_StatU64;
    }

    public sealed class IteratorEntryProperties
    {
        public string m_FileName;
        public ulong m_Size;
        public ushort m_Permissions;
        public bool m_IsDir;
    }

    public interface IBlockStore
    {
        void PutStoredBlock(StoredBlock storedBlock, OnPutBlockComplete completeCallback);
        void PreflightGet(UInt64[] blockHashes, OnPreflightStartedComplete completeCallback);
        void GetStoredBlock(UInt64 blockHash, OnGetBlockComplete completeCallback);
        void GetExistingContent(UInt64[] chunkHashes, UInt32 minBlockUsagePercent, OnGetExistingContentComplete completeCallback);
        void PruneBlocks(UInt64[] blockKeepHashes, OnPruneComplete completeCallback);
        BlockStoreStats GetStats();
        void Flush(OnFlushComplete completeCallback);
    }

    public interface IStorage
    {
        void OpenReadFile(string path, ref IntPtr outOpenFile);
        void GetSize(IntPtr f, ref UInt64 outSize);
        void Read(IntPtr f, UInt64 offset, UInt64 length, byte[] output);
        void OpenWriteFile(string path, UInt64 initialSize, ref IntPtr outOpenFile);
        void Write(IntPtr f, UInt64 offset, UInt64 length, byte[] input);
        void SetSize(IntPtr f, UInt64 length);
        void SetPermissions(string path, UInt16 permissions);
        UInt16 GetPermissions(string path);
        void CloseFile(IntPtr f);
        void CreateDir(string path);
        void RenameFile(string sourcePath, string targetPath);
        string ConcatPath(string rootPath, string subPath);
        bool IsDir(string path);
        bool IsFile(string path);
        void RemoveDir(string path);
        void RemoveFile(string path);
        bool StartFind(string path, ref IntPtr outIterator);
        bool FindNext(IntPtr iterator);
        void CloseFind(IntPtr iterator);
        IteratorEntryProperties GetEntryProperties(IntPtr iterator);
        void LockFile(string path, ref IntPtr outLockFile);
        void UnlockFile(IntPtr lockFile);
        string GetParentPath(string path);
    }

    public unsafe sealed class BlockStoreAPI : IDisposable
    {
        SafeNativeMethods.NativeBlockStoreAPI* _Native;
        internal BlockStoreAPI(SafeNativeMethods.NativeBlockStoreAPI* nativeBlockStoreAPI)
        {
            _Native = nativeBlockStoreAPI;
        }
        internal SafeNativeMethods.NativeBlockStoreAPI* Native
        {
            get { return this._Native; }
        }

        public void Dispose()
        {
            if (_Native != null)
            {
                SafeNativeMethods.Longtail_DisposeAPI((SafeNativeMethods.NativeAPI*)_Native);
                _Native = null;
            }
        }

        internal void Detach()
        {
            _Native = null;
        }
    }

    public unsafe sealed class StorageAPI : IDisposable
    {
        SafeNativeMethods.NativeStorageAPI* _Native;
        internal StorageAPI(SafeNativeMethods.NativeStorageAPI* nativeStorageAPI)
        {
            _Native = nativeStorageAPI;
        }
        internal SafeNativeMethods.NativeStorageAPI* Native
        {
            get { return this._Native; }
        }

        public void Dispose()
        {
            if (_Native != null)
            {
                SafeNativeMethods.Longtail_DisposeAPI((SafeNativeMethods.NativeAPI*)_Native);
                _Native = null;
            }
        }
    }

    public unsafe sealed class PathFilterAPI : IDisposable
    {
        SafeNativeMethods.NativePathFilterAPI* _Native;
        internal PathFilterAPI(SafeNativeMethods.NativePathFilterAPI* nativePathFilterAPI)
        {
            _Native = nativePathFilterAPI;
        }
        internal SafeNativeMethods.NativePathFilterAPI* Native
        {
            get { return this._Native; }
        }

        public void Dispose()
        {
            if (_Native != null)
            {
                SafeNativeMethods.Longtail_DisposeAPI((SafeNativeMethods.NativeAPI*)_Native);
                _Native = null;
            }
        }
    }

    public unsafe sealed class HashAPI : IDisposable
    {
        SafeNativeMethods.NativeHashAPI* _Native;
        bool _Owned;
        internal HashAPI(SafeNativeMethods.NativeHashAPI* nativeHashAPI, bool owned)
        {
            _Native = nativeHashAPI;
            _Owned = owned;
        }
        internal SafeNativeMethods.NativeHashAPI* Native
        {
            get { return this._Native; }
        }

        public void Dispose()
        {
            if (_Owned && _Native != null)
            {
                SafeNativeMethods.Longtail_DisposeAPI((SafeNativeMethods.NativeAPI*)_Native);
            }
            _Native = null;
        }
    }

    public unsafe sealed class JobAPI : IDisposable
    {
        SafeNativeMethods.NativeJobAPI* _Native;
        internal JobAPI(SafeNativeMethods.NativeJobAPI* nativeJobAPI)
        {
            _Native = nativeJobAPI;
        }
        internal SafeNativeMethods.NativeJobAPI* Native
        {
            get { return this._Native; }
        }

        public void Dispose()
        {
            if (_Native != null)
            {
                SafeNativeMethods.Longtail_DisposeAPI((SafeNativeMethods.NativeAPI*)_Native);
                _Native = null;
            }
        }
        internal void Detach()
        {
            _Native = null;
        }
    }

    public unsafe sealed class ChunkerAPI : IDisposable
    {
        SafeNativeMethods.NativeChunkerAPI* _Native;
        internal ChunkerAPI(SafeNativeMethods.NativeChunkerAPI* nativeChunkerAPI)
        {
            _Native = nativeChunkerAPI;
        }
        internal SafeNativeMethods.NativeChunkerAPI* Native
        {
            get { return this._Native; }
        }

        public void Dispose()
        {
            if (_Native != null)
            {
                SafeNativeMethods.Longtail_DisposeAPI((SafeNativeMethods.NativeAPI*)_Native);
                _Native = null;
            }
        }
        internal void Detach()
        {
            _Native = null;
        }
    }

    public unsafe sealed class CompressionRegistryAPI : IDisposable
    {
        SafeNativeMethods.NativeCompressionRegistryAPI* _Native;
        internal CompressionRegistryAPI(SafeNativeMethods.NativeCompressionRegistryAPI* nativeCompressionRegistryAPI)
        {
            _Native = nativeCompressionRegistryAPI;
        }
        internal SafeNativeMethods.NativeCompressionRegistryAPI* Native
        {
            get { return this._Native; }
        }

        public void Dispose()
        {
            if (_Native != null)
            {
                SafeNativeMethods.Longtail_DisposeAPI((SafeNativeMethods.NativeAPI*)_Native);
                _Native = null;
            }
        }
    }

    public unsafe sealed class HashRegistryAPI : IDisposable
    {
        SafeNativeMethods.NativeHashRegistryAPI* _Native;
        internal HashRegistryAPI(SafeNativeMethods.NativeHashRegistryAPI* nativeHashRegistryAPI)
        {
            _Native = nativeHashRegistryAPI;
        }
        internal SafeNativeMethods.NativeHashRegistryAPI* Native
        {
            get { return this._Native; }
        }

        public void Dispose()
        {
            if (_Native != null)
            {
                SafeNativeMethods.Longtail_DisposeAPI((SafeNativeMethods.NativeAPI*)_Native);
                _Native = null;
            }
        }
    }

    public unsafe sealed class FileInfos : IDisposable
    {
        SafeNativeMethods.NativeFileInfos* _Native;
        internal FileInfos(SafeNativeMethods.NativeFileInfos* nativeFileInfos)
        {
            _Native = nativeFileInfos;
        }
        internal SafeNativeMethods.NativeFileInfos* Native
        {
            get { return this._Native; }
        }

        public void Dispose()
        {
            if (_Native != null)
            {
                API.Free(_Native);
                _Native = null;
            }
        }
    }

    public unsafe sealed class VersionIndex : IDisposable
    {
        SafeNativeMethods.NativeVersionIndex* _Native;
        internal VersionIndex(SafeNativeMethods.NativeVersionIndex* NativeVersionIndex)
        {
            _Native = NativeVersionIndex;
        }
        internal SafeNativeMethods.NativeVersionIndex* Native
        {
            get { return this._Native; }
        }

        public void Dispose()
        {
            if (_Native != null)
            {
                API.Free(_Native);
                _Native = null;
            }
        }
        public unsafe UInt32 HashIdentifier
        {
            get { return _Native->GetHashIdentifier(); }
        }
        public unsafe UInt32 TargetChunkSize
        {
            get { return _Native->GetTargetChunkSize(); }
        }
        public unsafe UInt32 ChunkCount
        {
            get { return _Native->GetChunkCount(); }
        }
        public unsafe UInt64[] ChunkHashes
        {
            get { return _Native->GetChunkHashes(); }
        }
    }

    public unsafe sealed class StoreIndex : IDisposable
    {
        SafeNativeMethods.NativeStoreIndex* _Native;
        internal StoreIndex(SafeNativeMethods.NativeStoreIndex* NativeStoreIndex)
        {
            _Native = NativeStoreIndex;
        }
        internal SafeNativeMethods.NativeStoreIndex* Native
        {
            get { return this._Native; }
        }

        public void Dispose()
        {
            if (_Native != null)
            {
                API.Free(_Native);
                _Native = null;
            }
        }

        internal void Detach()
        {
            _Native = null;
        }

        public unsafe UInt32 HashIdentifier
        {
            get { return _Native->GetHashIdentifier(); }
        }
        public unsafe UInt64[] BlockHashes
        {
            get { return _Native->GetBlockHashes(); }
        }
        public unsafe UInt64[] CunkHashes
        {
            get { return _Native->GetChunkHashes(); }
        }
    }

    public unsafe sealed class LogContext : IDisposable
    {
        SafeNativeMethods.NativeLogContext* _Native;
        internal LogContext(SafeNativeMethods.NativeLogContext* NativeLogContext)
        {
            _Native = NativeLogContext;
        }
        internal SafeNativeMethods.NativeLogContext* Native
        {
            get { return this._Native; }
        }

        public void Dispose()
        {
            if (_Native != null)
            {
                API.Free(_Native);
                _Native = null;
            }
        }

        internal void Detach()
        {
            _Native = null;
        }

        public unsafe string File {
            get { return _Native->GetFile(); }
        }
        public unsafe string Function
        {
            get { return _Native->GetFunction(); }
        }
        public unsafe Int32 Line
        {
            get { return _Native->GetLine(); }
        }
        public unsafe Int32 Level
        {
            get { return _Native->GetLevel(); }
        }
        public unsafe Int32 FieldCount
        {
            get { return _Native->GetFieldCount(); }
        }
        public unsafe string FieldName(int fieldIndex)
        {
            return _Native->GetFieldName(fieldIndex);
        }
        public unsafe string FieldValue(int fieldIndex)
        {
            return _Native->GetFieldValue(fieldIndex);
        }
    }

    public unsafe sealed class VersionDiff : IDisposable
    {
        SafeNativeMethods.NativeVersionDiff* _Native;
        internal VersionDiff(SafeNativeMethods.NativeVersionDiff* NativeVersionDiff)
        {
            _Native = NativeVersionDiff;
        }
        internal SafeNativeMethods.NativeVersionDiff* Native
        {
            get { return this._Native; }
        }

        public unsafe int ChangeCount
        {
            get { return _Native->GetChangeCount(); }
        }

        public void Dispose()
        {
            if (_Native != null)
            {
                API.Free(_Native);
                _Native = null;
            }
        }
    }

    public unsafe sealed class StoredBlock : IDisposable
    {
        SafeNativeMethods.NativeStoredBlock* _Native;
        internal StoredBlock(SafeNativeMethods.NativeStoredBlock* NativeStoredBlock)
        {
            _Native = NativeStoredBlock;
        }
        internal SafeNativeMethods.NativeStoredBlock* Native
        {
            get { return this._Native; }
        }

        public void Dispose()
        {
            if (_Native != null)
            {
                SafeNativeMethods.Longtail_StoredBlock_Dispose(_Native);
                _Native = null;
            }
        }

        internal void Detach()
        {
            _Native = null;
        }
    }

    public sealed class LogHandle : IDisposable
    {
        unsafe public LogHandle(LogFunc logFunc)
        {
            m_LogFunc = logFunc;
            m_LogCallback =
                (SafeNativeMethods.NativeLogContext* log_context, IntPtr str) =>
                {
                    try
                    {
                        var logContext = new LogContext(log_context);
                        m_LogFunc(logContext, SafeNativeMethods.StringFromNativeUtf8(str));
                    }
                    catch (Exception)
                    {
                        // Eat exception, there is no way to report errors currently
                    }
                };
            SafeNativeMethods.Longtail_SetLog(m_LogCallback, null);
        }
        public unsafe void Dispose()
        {
            SafeNativeMethods.Longtail_SetLog(null, null);
        }
        SafeNativeMethods.LogCallback m_LogCallback;
        LogFunc m_LogFunc;
    };

    public sealed class AssertHandle : IDisposable
    {
        unsafe public AssertHandle(AssertFunc assertFunc)
        {
            m_AssertFunc = assertFunc;
            m_AssertCallback =
                (IntPtr expression, IntPtr file, int line) =>
                {
                    try
                    {
                        m_AssertFunc(SafeNativeMethods.StringFromNativeUtf8(expression), SafeNativeMethods.StringFromNativeUtf8(file), line);
                    }
                    catch (Exception)
                    {
                        // Eat exception, there is no way to report errors currently
                    }
                };
            SafeNativeMethods.Longtail_SetAssert(m_AssertCallback);
        }
        public unsafe void Dispose()
        {
            SafeNativeMethods.Longtail_SetAssert(null);
        }
        SafeNativeMethods.AssertCallback m_AssertCallback;
        AssertFunc m_AssertFunc;
    };



    public static class API
    {
        public const int LOG_LEVEL_DEBUG = 0;
        public const int LOG_LEVEL_INFO = 1;
        public const int LOG_LEVEL_WARNING = 2;
        public const int LOG_LEVEL_ERROR = 3;
        public const int LOG_LEVEL_OFF = 4;

        public static void ThrowExceptionFromErrno(string extraInfo, int errno, [CallerMemberName] string functionName = null)
        {
            switch (errno)
            {
                case SafeNativeMethods.EPERM:    /* Not super-user */
                case SafeNativeMethods.EACCES:  /* Permission denied */
                    throw new UnauthorizedAccessException(String.Format("{0} {1} failed with code `UnauthorizedAccessException` ({2})", functionName, extraInfo, errno));
                case SafeNativeMethods.ENOENT:   /* No such file or directory */
                case SafeNativeMethods.ESRCH:    /* No such process */
                case SafeNativeMethods.ENXIO:    /* No such device or address */
                    throw new EntryPointNotFoundException(String.Format("{0} {1} failed with code `EntryPointNotFoundException` ({2})", functionName, extraInfo, errno));
                case SafeNativeMethods.EINTR:    /* Interrupted system call */
                case SafeNativeMethods.EIO:      /* I/O error */
                case SafeNativeMethods.EBUSY:   /* Mount device busy */
                case SafeNativeMethods.EEXIST:  /* File exists */
                case SafeNativeMethods.EXDEV:   /* Cross-device link */
                case SafeNativeMethods.ENODEV:  /* No such device */
                case SafeNativeMethods.ENOTDIR: /* Not a directory */
                case SafeNativeMethods.EISDIR:  /* Is a directory */
                case SafeNativeMethods.ENFILE:  /* Too many open files in system */
                case SafeNativeMethods.EMFILE:  /* Too many open files */
                case SafeNativeMethods.EFBIG:   /* File too large */
                case SafeNativeMethods.ETXTBSY: /* Text file busy */
                case SafeNativeMethods.EROFS:   /* Read only file system */
                case SafeNativeMethods.EMLINK:  /* Too many links */
                    throw new IOException(String.Format("{0} {1} failed with code `IOException` ({2})", functionName, extraInfo, errno));
                case SafeNativeMethods.E2BIG:    /* Arg list too long */
                case SafeNativeMethods.ENOEXEC:  /* Exec format error */
                case SafeNativeMethods.EBADF:    /* Bad file number */
                case SafeNativeMethods.EINVAL:  /* Invalid argument */
                case SafeNativeMethods.ENOTBLK: /* Block device required */
                case SafeNativeMethods.ENOTTY:  /* Not a typewriter */
                case SafeNativeMethods.ESPIPE:  /* Illegal seek */
                case SafeNativeMethods.EPIPE:   /* Broken pipe */
                case SafeNativeMethods.ERANGE:  /* Math result not representable */
                case SafeNativeMethods.EDOM:    /* Math arg out of domain of func */
                    throw new ArgumentException(String.Format("{0} {1} failed with code `ArgumentException` ({2})", functionName, extraInfo, errno));
                case SafeNativeMethods.ECHILD:  /* No children */
                case SafeNativeMethods.EAGAIN:  /* No more processes */
                case SafeNativeMethods.ENOMEM:  /* Not enough core */
                    throw new OutOfMemoryException(String.Format("{0} {1} failed with code `OutOfMemoryException` ({2})", functionName, extraInfo, errno));
                case SafeNativeMethods.EFAULT:  /* Bad address */
                    throw new AccessViolationException(String.Format("{0} {1} failed with code `AccessViolationException` ({2})", functionName, extraInfo, errno));
                case SafeNativeMethods.ENOSPC:  /* No space left on device */
                    throw new IOException(String.Format("{0} {1} failed with code `NotSupportedException` ({2})", functionName, extraInfo, errno), unchecked((int)0x80070070));
                case SafeNativeMethods.ENOTSUP: /* Operation not supported (POSIX.1-2001). */
                    throw new NotSupportedException(String.Format("{0} {1} failed with code `NotSupportedException` ({2})", functionName, extraInfo, errno));
                case SafeNativeMethods.ECANCELED:
                    throw new TaskCanceledException();
                default:
                    throw new ArgumentException(String.Format("{0} {1} failed with code {2}", functionName, extraInfo, errno));
            }
        }

        public static int GetErrnoFromException(Exception ex, int defaultErrno)
        {
            if (ex == null)
            {
                return 0;
            }
            const int HR_ERROR_HANDLE_DISK_FULL = unchecked((int)0x80070027);
            const int HR_ERROR_DISK_FULL = unchecked((int)0x80070070);

            if (ex.HResult == HR_ERROR_HANDLE_DISK_FULL
                || ex.HResult == HR_ERROR_DISK_FULL)
            {
                return SafeNativeMethods.ENOSPC;
            }
            if (typeof(ArgumentException).Equals(ex.GetType()))
            {
                return SafeNativeMethods.EINVAL;
            }
            if (typeof(NotSupportedException).Equals(ex.GetType()))
            {
                return SafeNativeMethods.ENOTSUP;
            }
            if (typeof(NotImplementedException).Equals(ex.GetType()))
            {
                return SafeNativeMethods.ENOTSUP;
            }
            if (typeof(FileNotFoundException).Equals(ex.GetType()))
            {
                return SafeNativeMethods.ENOENT;
            }
            if (typeof(DirectoryNotFoundException).Equals(ex.GetType()))
            {
                return SafeNativeMethods.ENOENT;
            }
            if (typeof(DirectoryNotFoundException).Equals(ex.GetType()))
            {
                return SafeNativeMethods.ENOENT;
            }
            if (typeof(EntryPointNotFoundException).Equals(ex.GetType()))
            {
                return SafeNativeMethods.ENOENT;
            }
            if (typeof(OutOfMemoryException).Equals(ex.GetType()))
            {
                return SafeNativeMethods.ENOMEM;
            }
            if (typeof(UnauthorizedAccessException).Equals(ex.GetType()))
            {
                return SafeNativeMethods.EACCES;
            }
            if (typeof(IOException).Equals(ex.GetType()))
            {
                return SafeNativeMethods.EIO;
            }
            if (typeof(TaskCanceledException).Equals(ex.GetType()))
            {
                return SafeNativeMethods.ECANCELED;
            }
            if (typeof(OperationCanceledException).Equals(ex.GetType()))
            {
                return SafeNativeMethods.ECANCELED;
            }
            return defaultErrno;
        }

        public static void SetLogLevel(int level)
        {
            SafeNativeMethods.Longtail_SetLogLevel(level);
        }
        public unsafe static void* Alloc(string context, UInt64 size)
        {
            var nativeContext = SafeNativeMethods.NativeUtf8FromString(context);
            void* r = SafeNativeMethods.Longtail_Alloc(nativeContext, size);
            SafeNativeMethods.FreeNativeUtf8String(nativeContext);
            return r;
        }
        public unsafe static void Free(void* data)
        {
            SafeNativeMethods.Longtail_Free(data);
        }

        public unsafe static FileInfos GetFilesRecursively(
            StorageAPI storageAPI,
            PathFilterFunc pathFilterFunc,
            CancellationToken cancellationToken,
            string rootPath)
        {
            if (storageAPI == null)
            {
                throw new ArgumentException("GetFilesRecursively storageAPI is null");
            }
            PathFilterAPI pathFilterAPI = MakePathFilter(pathFilterFunc);
            CancelHandle cancelHandle = new CancelHandle(cancellationToken);
            IntPtr cRootPath = SafeNativeMethods.NativeUtf8FromString(rootPath);
            SafeNativeMethods.NativeFileInfos* nativeFileInfos = null;
            int err = SafeNativeMethods.Longtail_GetFilesRecursively(
                storageAPI.Native,
                pathFilterAPI != null ? pathFilterAPI.Native : null,
                cancelHandle.Native,
                (IntPtr)cancelHandle.Native,    // We don't have a dedicated token
                cRootPath,
                ref nativeFileInfos);
            SafeNativeMethods.FreeNativeUtf8String(cRootPath);
            cancelHandle.Dispose();
            pathFilterAPI?.Dispose();
            if (err == 0)
            {
                return new FileInfos(nativeFileInfos);
            }
            ThrowExceptionFromErrno(rootPath, err);
            return null;
        }

        public unsafe static UInt32 FileInfosGetCount(FileInfos fileInfos)
        {
            if (fileInfos == null) { return 0; }
            var cFileInfos = fileInfos.Native;
            return SafeNativeMethods.Longtail_FileInfos_GetCount(cFileInfos);
        }

        public unsafe static string FileInfosGetPath(FileInfos fileInfos, UInt32 index)
        {
            if (fileInfos == null) { return ""; }
            var cFileInfos = fileInfos.Native;
            IntPtr cString = SafeNativeMethods.Longtail_FileInfos_GetPath(cFileInfos, index);
            return SafeNativeMethods.StringFromNativeUtf8(cString);
        }

        public static BlockStoreAPI MakeBlockStore(IBlockStore blockStore)
        {
            if (blockStore == null) { throw new ArgumentException("MakeBlockStore blockStore is null"); }
            return new BlockStoreHandle(blockStore).Native;
        }

        public static StorageAPI MakeStorageAPI(IStorage storage)
        {
            if (storage == null) { throw new ArgumentException("MakeStorageAPI storage is null"); }
            return new StorageHandle(storage).Native;
        }

        public static PathFilterAPI MakePathFilter(PathFilterFunc pathFilterFunc)
        {
            return new PathFilterHandle(pathFilterFunc).Native;
        }

        private unsafe sealed class WrappedAsyncFlushAPI : IDisposable
        {
            public WrappedAsyncFlushAPI()
            {
                UInt64 mem_size = SafeNativeMethods.Longtail_GetAsyncFlushAPISize();
                byte* mem = (byte*)API.Alloc(nameof(WrappedAsyncFlushAPI), mem_size);
                if (mem == null)
                {
                    throw new OutOfMemoryException();
                }
                m_Dispose =
                        (SafeNativeMethods.NativeAPI* api) =>
                        {
                            SafeNativeMethods.Longtail_Free(_Native);
                            _Native = null;
                        };
                m_ASyncCallback =
                    (SafeNativeMethods.NativeAsyncFlushAPI* async_complete_api, int err) =>
                    {
                        this.err = err;
                        m_EventHandle.Set();
                        return 0;
                    };
                _Native = SafeNativeMethods.Longtail_MakeAsyncFlushAPI(mem, m_Dispose, m_ASyncCallback);
                m_EventHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            }
            public void Dispose()
            {
                if (_Native != null)
                {
                    SafeNativeMethods.Longtail_DisposeAPI((SafeNativeMethods.NativeAPI*)_Native);
                    _Native = null;
                }
            }
            internal SafeNativeMethods.NativeAsyncFlushAPI* Native
            {
                get { return this._Native; }
            }
            public int Err
            {
                get { m_EventHandle.WaitOne(); return this.err; }
            }

            SafeNativeMethods.Longtail_DisposeFunc m_Dispose;
            SafeNativeMethods.ASyncFlushCompleteCallback m_ASyncCallback;
            EventWaitHandle m_EventHandle;
            int err = -1;
            SafeNativeMethods.NativeAsyncFlushAPI* _Native;
        };

        private unsafe sealed class WrappedAsyncGetExistingContentAPI : IDisposable
        {
            public WrappedAsyncGetExistingContentAPI()
            {
                UInt64 mem_size = SafeNativeMethods.Longtail_GetAsyncGetExistingContentAPISize();
                byte* mem = (byte*)API.Alloc(nameof(WrappedAsyncGetExistingContentAPI), mem_size);
                if (mem == null)
                {
                    throw new OutOfMemoryException();
                }
                m_Dispose =
                        (SafeNativeMethods.NativeAPI* api) =>
                        {
                            SafeNativeMethods.Longtail_Free(_Native);
                            _Native = null;
                        };
                m_ASyncCallback =
                    (SafeNativeMethods.NativeAsyncGetExistingContentAPI* async_complete_api, SafeNativeMethods.NativeStoreIndex* store_index, int err) =>
                    {
                        this.err = err;
                        if (err == 0)
                        {
                            m_StoreIndex = new StoreIndex(store_index);
                        }
                        m_EventHandle.Set();
                        return 0;
                    };
                _Native = SafeNativeMethods.Longtail_MakeAsyncGetExistingContentAPI(mem, m_Dispose, m_ASyncCallback);
                m_EventHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
                m_StoreIndex = null;
            }
            public void Dispose()
            {
                if (_Native != null)
                {
                    SafeNativeMethods.Longtail_DisposeAPI((SafeNativeMethods.NativeAPI*)_Native);
                    _Native = null;
                }
            }
            internal SafeNativeMethods.NativeAsyncGetExistingContentAPI* Native
            {
                get { return this._Native; }
            }
            public StoreIndex Result
            {
                get { m_EventHandle.WaitOne();  return this.m_StoreIndex; }
            }
            public int Err
            {
                get { return this.err; }
            }

            SafeNativeMethods.Longtail_DisposeFunc m_Dispose;
            SafeNativeMethods.ASyncGetExistingContentCompleteCallback m_ASyncCallback;
            EventWaitHandle m_EventHandle;
            StoreIndex m_StoreIndex;
            int err = -1;
            SafeNativeMethods.NativeAsyncGetExistingContentAPI* _Native;
        };

        public unsafe static Task BlockStoreFlush(BlockStoreAPI blockStoreAPI)
        {
            if (blockStoreAPI == null) { throw new ArgumentException("BlockStoreFlush blockStoreAPI is null"); }
            return Task.Run(() =>
            {
                WrappedAsyncFlushAPI wrappedAsyncFlushAPI = new WrappedAsyncFlushAPI();
                int err = SafeNativeMethods.Longtail_BlockStore_Flush(
                    blockStoreAPI.Native,
                    wrappedAsyncFlushAPI.Native);
                if (err == 0)
                {
                    err = wrappedAsyncFlushAPI.Err;
                }
                wrappedAsyncFlushAPI.Dispose();
                if (err != 0)
                {
                    ThrowExceptionFromErrno("", err);
                }
            });
        }

        public unsafe static VersionIndex CreateVersionIndex(
            StorageAPI storageAPI,
            HashAPI hashAPI,
            ChunkerAPI chunkerAPI,
            JobAPI jobAPI,
            ProgressFunc progress,
            UInt32 progressPercentRateLimit,
            CancellationToken cancellationToken,
            string rootPath,
            FileInfos fileInfos,
            UInt32[] optional_assetTags,
            UInt32 maxChunkSize)
        {
            if (storageAPI == null) { throw new ArgumentException("CreateVersionIndex storageAPI is null"); }
            if (hashAPI == null) { throw new ArgumentException("CreateVersionIndex hashAPI is null"); }
            if (chunkerAPI == null) { throw new ArgumentException("CreateVersionIndex chunkerAPI is null"); }
            if (jobAPI == null) { throw new ArgumentException("CreateVersionIndex jobAPI is null"); }
            if (fileInfos == null) { throw new ArgumentException("CreateVersionIndex fileInfos is null"); }

            ProgressHandle progressHandle = new ProgressHandle(progress, progressPercentRateLimit);
            CancelHandle cancelHandle = new CancelHandle(cancellationToken);

            var cStorageAPI = storageAPI.Native;
            var cHashAPI = hashAPI.Native;
            var cChunkerAPI = chunkerAPI.Native;
            var cJobAPI = jobAPI.Native;
            var cProgressHandle = progressHandle.Native;
            var cCancelHandle = cancelHandle.Native;
            var cFileInfos = fileInfos.Native;
            IntPtr cRootPath = SafeNativeMethods.NativeUtf8FromString(rootPath);

            SafeNativeMethods.NativeVersionIndex* nativeVersionIndex = null;
            int err = SafeNativeMethods.Longtail_CreateVersionIndex(
                cStorageAPI,
                cHashAPI,
                cChunkerAPI,
                cJobAPI,
                cProgressHandle,
                cCancelHandle,
                (IntPtr)cCancelHandle,    // We don't have a dedicated token
                cRootPath,
                cFileInfos,
                optional_assetTags,
                maxChunkSize,
                ref nativeVersionIndex);
            SafeNativeMethods.FreeNativeUtf8String(cRootPath);
            cancelHandle.Dispose();
            progressHandle.Dispose();
            if (err == 0)
            {
                return new VersionIndex(nativeVersionIndex);
            }
            ThrowExceptionFromErrno(rootPath, err);
            return null;
        }
        public unsafe static VersionIndex ReadVersionIndexFromBuffer(byte[] buffer)
        {
            if (buffer == null) { throw new ArgumentException("ReadVersionIndexFromBuffer buffer is null"); }

            SafeNativeMethods.NativeVersionIndex* nativeVersionIndex = null;
            GCHandle pinnedArray = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            IntPtr bufferPtr = pinnedArray.AddrOfPinnedObject();
            int err = SafeNativeMethods.Longtail_ReadVersionIndexFromBuffer((void*)bufferPtr, (UInt64)buffer.Length, ref nativeVersionIndex);
            pinnedArray.Free();
            if (err == 0)
            {
                return new VersionIndex(nativeVersionIndex);
            }
            ThrowExceptionFromErrno("", err);
            return null;
        }
        public unsafe static VersionIndex ReadVersionIndex(StorageAPI storageAPI, string path)
        {
            if (storageAPI == null) { throw new ArgumentException("ReadVersionIndex storageAPI is null"); }
            if (path == null) { throw new ArgumentException("ReadVersionIndex path is null"); }

            var cStorageAPI = storageAPI.Native;
            IntPtr cPath = SafeNativeMethods.NativeUtf8FromString(path);
            SafeNativeMethods.NativeVersionIndex* nativeVersionIndex = null;
            int err = SafeNativeMethods.Longtail_ReadVersionIndex(
                cStorageAPI,
                cPath,
                ref nativeVersionIndex);
            SafeNativeMethods.FreeNativeUtf8String(cPath);
            if (err == 0)
            {
                return new VersionIndex(nativeVersionIndex);
            }
            ThrowExceptionFromErrno(path, err);
            return null;
        }
        public unsafe static StoreIndex ReadStoreIndexFromBuffer(byte[] buffer)
        {
            if (buffer == null) { throw new ArgumentException("ReadStoreIndexFromBuffer buffer is null"); }

            SafeNativeMethods.NativeStoreIndex* nativeStoreIndex = null;
            GCHandle pinnedArray = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            IntPtr bufferPtr = pinnedArray.AddrOfPinnedObject();
            int err = SafeNativeMethods.Longtail_ReadStoreIndexFromBuffer((void*)bufferPtr, (UInt64)buffer.Length, ref nativeStoreIndex);
            pinnedArray.Free();
            if (err == 0)
            {
                return new StoreIndex(nativeStoreIndex);
            }
            ThrowExceptionFromErrno("", err);
            return null;
        }
        public unsafe static StoreIndex ReadStoreIndex(StorageAPI storageAPI, string path)
        {
            if (storageAPI == null) { throw new ArgumentException("ReadStoreIndex storageAPI is null"); }
            if (path == null) { throw new ArgumentException("ReadStoreIndex path is null"); }

            var cStorageAPI = storageAPI.Native;
            IntPtr cPath = SafeNativeMethods.NativeUtf8FromString(path);
            SafeNativeMethods.NativeStoreIndex* nativeStoreIndex = null;
            int err = SafeNativeMethods.Longtail_ReadStoreIndex(
                cStorageAPI,
                cPath,
                ref nativeStoreIndex);
            SafeNativeMethods.FreeNativeUtf8String(cPath);
            if (err == 0)
            {
                return new StoreIndex(nativeStoreIndex);
            }
            ThrowExceptionFromErrno(path, err);
            return null;
        }
        public unsafe static StoreIndex CreateMissingContent(HashAPI hashAPI, StoreIndex storeIndex, VersionIndex version, UInt32 maxBlockSize, UInt32 maxChunksPerBlock)
        {
            if (hashAPI == null) { throw new ArgumentException("CreateMissingContent hashAPI is null"); }
            if (storeIndex == null) { throw new ArgumentException("CreateMissingContent storeIndex is null"); }
            if (version == null) { throw new ArgumentException("CreateMissingContent version is null"); }

            var cHashAPI = hashAPI.Native;
            var cStoreIndex = storeIndex.Native;
            var cVersion = version.Native;
            SafeNativeMethods.NativeStoreIndex* nativeStoreIndex = null;
            int err = SafeNativeMethods.Longtail_CreateMissingContent(
                cHashAPI,
                cStoreIndex,
                cVersion,
                maxBlockSize,
                maxChunksPerBlock,
                ref nativeStoreIndex);
            if (err == 0)
            {
                return new StoreIndex(nativeStoreIndex);
            }
            ThrowExceptionFromErrno("", err);
            return null;
        }

        public unsafe static UInt64[] GetRequiredChunkHashes(
            VersionIndex versionIndex,
            VersionDiff versionDiff)
        {
            if (versionIndex == null) { throw new ArgumentException("GetRequiredChunkHashes versionIndex is null"); }
            if (versionDiff == null) { throw new ArgumentException("GetRequiredChunkHashes versionDiff is null"); }

            var cVersionIndex = versionIndex.Native;
            var cVersionDiff = versionDiff.Native;
            UInt32 chunkCount = 0;
            var chunkHashes = new UInt64[versionIndex.ChunkCount];
            GCHandle pinnedArray = GCHandle.Alloc(chunkHashes, GCHandleType.Pinned);
            IntPtr chunkHashesPtr = pinnedArray.AddrOfPinnedObject();
            int err = SafeNativeMethods.Longtail_GetRequiredChunkHashes(
                cVersionIndex,
                cVersionDiff,
                ref chunkCount,
                (UInt64*)chunkHashesPtr);
            pinnedArray.Free();
            if (err == 0)
            {
                var result = new UInt64[chunkCount];
                Array.Copy(chunkHashes, result, (int)chunkCount);
                return result;
            }
            ThrowExceptionFromErrno("", err);
            return null;
        }

        public unsafe static StoreIndex GetExistingStoreIndex(
            StoreIndex storeIndex,
            UInt64[] chunkHashes,
            UInt32 minBlockUsagePercent)
        {
            if (storeIndex == null) { throw new ArgumentException("GetExistingStoreIndex storeIndex is null"); }

            var cStoreIndex = storeIndex.Native;
            GCHandle pinnedArray = GCHandle.Alloc(chunkHashes, GCHandleType.Pinned);
            IntPtr chunkHashesPtr = pinnedArray.AddrOfPinnedObject();

            SafeNativeMethods.NativeStoreIndex* nativeStoreIndex = null;
            int err = SafeNativeMethods.Longtail_GetExistingStoreIndex(
                cStoreIndex,
                (UInt32)chunkHashes.Length,
                (UInt64*)chunkHashesPtr,
                minBlockUsagePercent,
                ref nativeStoreIndex);
            pinnedArray.Free();
            if (err == 0)
            {
                return new StoreIndex(nativeStoreIndex);
            }
            ThrowExceptionFromErrno("", err);
            return null;
        }
        /*        public unsafe static ContentIndex GetExistingContent(ContentIndex referenceContentIndex, ContentIndex contentIndex)
                {
                    if (referenceContentIndex == null) { throw new ArgumentException("GetExistingContent referenceContentIndex is null"); }
                    if (contentIndex == null) { throw new ArgumentException("GetExistingContent contentIndex is null"); }

                    var cReferenceContentIndex = referenceContentIndex.Native;
                    var cContentIndex = contentIndex.Native;

                    SafeNativeMethods.NativeContentIndex* nativeContentIndex = null;
                    int err = SafeNativeMethods.Longtail_GetExistingContent(
                        cReferenceContentIndex,
                        cContentIndex,
                        ref nativeContentIndex);
                    if (err == 0)
                    {
                        return new ContentIndex(nativeContentIndex);
                    }
                    ThrowExceptionFromErrno("", err);
                    return null;
                }
                public unsafe static ContentIndex MergeContentIndex(
                    JobAPI jobAPI,
                    ContentIndex localContentIndex,
                    ContentIndex newContentIndex)
                {
                    if (jobAPI == null) { throw new ArgumentException("MergeContentIndex jobAPI is null"); }
                    if (localContentIndex == null) { throw new ArgumentException("MergeContentIndex localContentIndex is null"); }
                    if (newContentIndex == null) { throw new ArgumentException("MergeContentIndex newContentIndex is null"); }

                    var cLocalContentIndex = localContentIndex.Native;
                    var cNewIndex = newContentIndex.Native;
                    var cJobAPI = jobAPI.Native;

                    SafeNativeMethods.NativeContentIndex* nativeContentIndex = null;
                    int err = SafeNativeMethods.Longtail_MergeContentIndex(
                        cJobAPI,
                        cLocalContentIndex,
                        cNewIndex,
                        ref nativeContentIndex);
                    if (err == 0)
                    {
                        return new ContentIndex(nativeContentIndex);
                    }
                    ThrowExceptionFromErrno("", err);
                    return null;
                }*/

        public unsafe static Task<StoreIndex> GetExistingContent(BlockStoreAPI blockStoreAPI, UInt64[] chunkHashes, UInt32 minBlockUsagePercent)
        {
            if (blockStoreAPI == null) { throw new ArgumentException("GetExistingContent blockStoreAPI is null"); }
            return Task<StoreIndex>.Run(() =>
            {
                GCHandle pinnedArray = GCHandle.Alloc(chunkHashes, GCHandleType.Pinned);
                IntPtr chunkHashesPtr = pinnedArray.AddrOfPinnedObject();

                WrappedAsyncGetExistingContentAPI wrappedAsyncGetExistingContentAPI = new WrappedAsyncGetExistingContentAPI();
                int err = SafeNativeMethods.Longtail_BlockStore_GetExistingContent(
                    blockStoreAPI.Native,
                    (UInt32)chunkHashes.Length,
                    (UInt64*)chunkHashesPtr,
                    minBlockUsagePercent,
                    wrappedAsyncGetExistingContentAPI.Native);
                pinnedArray.Free();
                if (err != 0)
                {
                    wrappedAsyncGetExistingContentAPI.Dispose();
                    ThrowExceptionFromErrno("", err);
                }
                StoreIndex result = wrappedAsyncGetExistingContentAPI.Result;
                wrappedAsyncGetExistingContentAPI.Dispose();
                if (wrappedAsyncGetExistingContentAPI.Err != 0)
                {
                    ThrowExceptionFromErrno("", wrappedAsyncGetExistingContentAPI.Err);
                }
                return result;
            });
        }
        public unsafe static void ValidateStore(StoreIndex storeIndex, VersionIndex versionIndex)
        {
            if (storeIndex == null) { throw new ArgumentException("ValidateStore contentIndex is null"); }
            if (versionIndex == null) { throw new ArgumentException("ValidateStore versionIndex is null"); }
            int err = SafeNativeMethods.Longtail_ValidateStore(storeIndex.Native, versionIndex.Native);
            if (err != 0)
            {
                ThrowExceptionFromErrno("", err);
            }
        }
        public unsafe static VersionDiff CreateVersionDiff(
            HashAPI hashAPI,
            VersionIndex sourceVersion,
            VersionIndex targetVersion)
        {
            if (hashAPI == null) { throw new ArgumentException("CreateVersionDiff hashAPI is null"); }
            if (sourceVersion == null) { throw new ArgumentException("CreateVersionDiff sourceVersion is null"); }
            if (targetVersion == null) { throw new ArgumentException("CreateVersionDiff targetVersion is null"); }

            var cSourceVersion = sourceVersion.Native;
            var cTargetVersion = targetVersion.Native;
            var cHashAPI = hashAPI.Native;
            SafeNativeMethods.NativeVersionDiff* nativeVersionDiff = null;
            int err = SafeNativeMethods.Longtail_CreateVersionDiff(
                cHashAPI,
                cSourceVersion,
                cTargetVersion,
                ref nativeVersionDiff);
            if (err == 0)
            {
                return new VersionDiff(nativeVersionDiff);
            }
            ThrowExceptionFromErrno("", err);
            return null;
        }
        public unsafe static void ChangeVersion(
            BlockStoreAPI blockStoreAPI,
            StorageAPI versionStorageAPI,
            HashAPI hashAPI,
            JobAPI jobAPI,
            ProgressFunc progress,
            UInt32 progressPercentRateLimit,
            CancellationToken cancellationToken,
            StoreIndex storeIndex,
            VersionIndex sourceVersion,
            VersionIndex targetVersion,
            VersionDiff versionDiff,
            string versionPath,
            bool retainPermissions)
        {
            if (blockStoreAPI == null) { throw new ArgumentException("ChangeVersion blockStoreAPI is null"); }
            if (versionStorageAPI == null) { throw new ArgumentException("ChangeVersion versionStorageAPI is null"); }
            if (hashAPI == null) { throw new ArgumentException("ChangeVersion hashAPI is null"); }
            if (jobAPI == null) { throw new ArgumentException("ChangeVersion jobAPI is null"); }
            if (storeIndex == null) { throw new ArgumentException("ChangeVersion storeIndex is null"); }
            if (sourceVersion == null) { throw new ArgumentException("ChangeVersion sourceVersion is null"); }
            if (targetVersion == null) { throw new ArgumentException("ChangeVersion targetVersion is null"); }
            if (versionDiff == null) { throw new ArgumentException("ChangeVersion versionDiff is null"); }

            ProgressHandle progressHandle = new ProgressHandle(progress, progressPercentRateLimit);
            CancelHandle cancelHandle = new CancelHandle(cancellationToken);

            var cBlockStoreAPI = blockStoreAPI.Native;
            var cVersionStorageAPI = versionStorageAPI.Native;
            var cHashAPI = hashAPI.Native;
            var cJobAPI = jobAPI.Native;
            var cProgressHandle = progressHandle.Native;
            var cCancelHandle = cancelHandle.Native;
            var cStoreIndex = storeIndex.Native;
            var cSourceVersion = sourceVersion.Native;
            var cTargetVersion = targetVersion.Native;
            var cVersionDiff = versionDiff.Native;
            var cVersionPath = SafeNativeMethods.NativeUtf8FromString(versionPath);

            int err = SafeNativeMethods.Longtail_ChangeVersion(
                cBlockStoreAPI,
                cVersionStorageAPI,
                cHashAPI,
                cJobAPI,
                cProgressHandle,
                cCancelHandle,
                (IntPtr)cCancelHandle,    // We don't have a dedicated token
                cStoreIndex,
                cSourceVersion,
                cTargetVersion,
                cVersionDiff,
                cVersionPath,
                retainPermissions ? 1 : 0);
            SafeNativeMethods.FreeNativeUtf8String(cVersionPath);
            cancelHandle.Dispose();
            progressHandle.Dispose();
            if (err == 0)
            {
                return;
            }
            ThrowExceptionFromErrno(versionPath, err);
        }
        public unsafe static StorageAPI CreateFSStorageAPI()
        {
            return new StorageAPI(SafeNativeMethods.Longtail_CreateFSStorageAPI());
        }
        public unsafe static StorageAPI CreateInMemStorageAPI()
        {
            return new StorageAPI(SafeNativeMethods.Longtail_CreateInMemStorageAPI());
        }
        public unsafe static UInt32 HashGetIdentifier(HashAPI hashAPI)
        {
            return SafeNativeMethods.Longtail_Hash_GetIdentifier(hashAPI.Native);
        }
        public unsafe static HashAPI CreateBlake2HashAPI()
        {
            return new HashAPI(SafeNativeMethods.Longtail_CreateBlake2HashAPI(), true);
        }
        public unsafe static UInt32 GetBlake2HashType()
        {
            return SafeNativeMethods.Longtail_GetBlake2HashType();
        }
        public unsafe static HashAPI CreateBlake3HashAPI()
        {
            return new HashAPI(SafeNativeMethods.Longtail_CreateBlake3HashAPI(), true);
        }
        public unsafe static UInt32 GetBlake3HashType()
        {
            return SafeNativeMethods.Longtail_GetBlake3HashType();
        }
        public unsafe static HashAPI CreateMeowHashAPI()
        {
            return new HashAPI(SafeNativeMethods.Longtail_CreateMeowHashAPI(), true);
        }
        public unsafe static UInt32 GetMeowHashType()
        {
            return SafeNativeMethods.Longtail_GetMeowHashType();
        }
        public unsafe static JobAPI CreateBikeshedJobAPI(UInt32 workerCount, int workerPriority)
        {
            return new JobAPI(SafeNativeMethods.Longtail_CreateBikeshedJobAPI(workerCount, workerPriority));
        }
        public unsafe static ChunkerAPI CreateHPCDCChunkerAPI()
        {
            return new ChunkerAPI(SafeNativeMethods.Longtail_CreateHPCDCChunkerAPI());
        }
        public unsafe static HashRegistryAPI CreateFullHashRegistry()
        {
            return new HashRegistryAPI(SafeNativeMethods.Longtail_CreateFullHashRegistry());
        }
        public unsafe static HashAPI GetHashAPI(HashRegistryAPI hashRegistry, UInt32 hashIdentifier)
        {
            if (hashRegistry == null) { throw new ArgumentException("GetHashAPI hashRegistry is null"); }

            var cHashRegistry = hashRegistry.Native;
            SafeNativeMethods.NativeHashAPI* hashAPI = null;
            int err = SafeNativeMethods.Longtail_GetHashRegistry_GetHashAPI(
                cHashRegistry,
                hashIdentifier,
                ref hashAPI);
            if (err == 0)
            {
                return new HashAPI(hashAPI, false);
            }
            ThrowExceptionFromErrno(hashIdentifier.ToString(), err);
            return null;
        }
        public unsafe static CompressionRegistryAPI CreateFullCompressionRegistry()
        {
            return new CompressionRegistryAPI(SafeNativeMethods.Longtail_CreateFullCompressionRegistry());
        }
        public unsafe static BlockStoreAPI CreateFSBlockStoreAPI(
            JobAPI jobAPI,
            StorageAPI storageAPI,
            string contentPath,
            UInt32 default_max_block_size,
            UInt32 default_max_chunks_per_block)
        {
            if (jobAPI == null) { throw new ArgumentException("CreateFSBlockStoreAPI jobAPI is null"); }
            if (storageAPI == null) { throw new ArgumentException("CreateFSBlockStoreAPI storageAPI is null"); }
            if (contentPath == null) { throw new ArgumentException("CreateFSBlockStoreAPI contentPath is null"); }

            var cStorageAPI = storageAPI.Native;
            var cJobAPI = jobAPI.Native;
            var cContentPath = SafeNativeMethods.NativeUtf8FromString(contentPath);

            var store = new BlockStoreAPI(SafeNativeMethods.Longtail_CreateFSBlockStoreAPI(
                cJobAPI,
                cStorageAPI,
                cContentPath,
                IntPtr.Zero));
            SafeNativeMethods.FreeNativeUtf8String(cContentPath);
            return store;
        }
        public unsafe static BlockStoreAPI CreateCacheBlockStoreAPI(
            JobAPI jobAPI,
            BlockStoreAPI localBlockStore,
            BlockStoreAPI remoteBlockStore)
        {
            if (jobAPI == null) { throw new ArgumentException("CreateCacheBlockStoreAPI jobAPI is null"); }
            if (localBlockStore == null) { throw new ArgumentException("CreateCacheBlockStoreAPI localBlockStore is null"); }
            if (remoteBlockStore == null) { throw new ArgumentException("CreateCacheBlockStoreAPI remoteBlockStore is null"); }

            var cLocalBlockStore = localBlockStore.Native;
            var cRemoteBlockStore = remoteBlockStore.Native;
            var cJobAPI = jobAPI.Native;

            return new BlockStoreAPI(SafeNativeMethods.Longtail_CreateCacheBlockStoreAPI(
                cJobAPI,
                cLocalBlockStore,
                cRemoteBlockStore));
        }
        public unsafe static BlockStoreAPI CreateCompressBlockStoreAPI(BlockStoreAPI backingBlockStore, CompressionRegistryAPI compressionRegistry)
        {
            if (backingBlockStore == null) { throw new ArgumentException("CreateCompressBlockStoreAPI backingBlockStore is null"); }
            if (compressionRegistry == null) { throw new ArgumentException("CreateCompressBlockStoreAPI compressionRegistry is null"); }

            var cBackingBlockStore = backingBlockStore.Native;
            var cCompressionRegistry = compressionRegistry.Native;
            return new BlockStoreAPI(SafeNativeMethods.Longtail_CreateCompressBlockStoreAPI(
                cBackingBlockStore,
                cCompressionRegistry));
        }
        public unsafe static BlockStoreAPI CreateLRUBlockStoreAPI(BlockStoreAPI backingBlockStore, UInt32 maxLRUBlockCount)
        {
            if (backingBlockStore == null) { throw new ArgumentException("CreateLRUBlockStoreAPI backingBlockStore is null"); }

            var cBackingBlockStore = backingBlockStore.Native;
            return new BlockStoreAPI(SafeNativeMethods.Longtail_CreateLRUBlockStoreAPI(
                cBackingBlockStore,
                maxLRUBlockCount));
        }
        public unsafe static BlockStoreAPI CreateShareBlockStoreAPI(BlockStoreAPI backingBlockStore)
        {
            if (backingBlockStore == null) { throw new ArgumentException("CreateShareBlockStoreAPI backingBlockStore is null"); }

            var cBackingBlockStore = backingBlockStore.Native;
            return new BlockStoreAPI(SafeNativeMethods.Longtail_CreateShareBlockStoreAPI(cBackingBlockStore));
        }

        public unsafe static BlockStoreStats BlockStoreGetStats(BlockStoreAPI blockStoreAPI)
        {
            if (blockStoreAPI == null) { throw new ArgumentException("BlockStoreGetStats blockStoreAPI is null"); }

            var cBlockStoreAPI = blockStoreAPI.Native;

            SafeNativeMethods.NativeBlockStoreStats nativeStats = new SafeNativeMethods.NativeBlockStoreStats { };
            int err = SafeNativeMethods.Longtail_BlockStore_GetStats(
                cBlockStoreAPI,
                ref nativeStats);
            if (err == 0)
            {
                BlockStoreStats outStats = new BlockStoreStats();
                for (var s = 0; s < (int)StatU64.Count; ++s)
                {
                    outStats.m_StatU64[s] = nativeStats.m_StatU64[s];
                }
                return outStats;
            }
            ThrowExceptionFromErrno("", err);
            return new BlockStoreStats();
        }

        public unsafe static StoredBlock ReadStoredBlockFromBuffer(byte[] buffer)
        {
            if (buffer == null) { throw new ArgumentException("ReadStoredBlockFromBuffer buffer is null"); }
            SafeNativeMethods.NativeStoredBlock* nativeStoredBlock = null;

            GCHandle pinnedArray = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            IntPtr bufferPtr = pinnedArray.AddrOfPinnedObject();
            int err = SafeNativeMethods.Longtail_ReadStoredBlockFromBuffer(
                (void*)bufferPtr,
                (UInt64)buffer.Length,
                ref nativeStoredBlock);
            pinnedArray.Free();
            if (err == 0)
            {
                return new StoredBlock(nativeStoredBlock);
            }
            ThrowExceptionFromErrno("", err);
            return new StoredBlock(null);
        }

        public unsafe static StoredBlock ReadStoredBlock(StorageAPI storageAPI, string path)
        {
            if (storageAPI == null) { throw new ArgumentException("ReadStoredBlock storageAPI is null"); }
            if (path == null) { throw new ArgumentException("ReadStoredBlock path is null"); }

            var cStorageAPI = storageAPI.Native;
            var cPath = SafeNativeMethods.NativeUtf8FromString(path);
            SafeNativeMethods.NativeStoredBlock* nativeStoredBlock = null;
            int err = SafeNativeMethods.Longtail_ReadStoredBlock(
                cStorageAPI,
                cPath,
                ref nativeStoredBlock);
            SafeNativeMethods.FreeNativeUtf8String(cPath);
            if (err == 0)
            {
                return new StoredBlock(nativeStoredBlock);
            }
            ThrowExceptionFromErrno(path, err);
            return new StoredBlock(null);
        }

        public unsafe static int WriteStoredBlock(StorageAPI storageAPI, StoredBlock storedBlock, string path)
        {
            if (storageAPI == null) { throw new ArgumentException("WriteStoredBlock storageAPI is null"); }
            if (storedBlock == null) { throw new ArgumentException("WriteStoredBlock storedBlock is null"); }
            if (path == null) { throw new ArgumentException("WriteStoredBlock path is null"); }

            var cStorageAPI = storageAPI.Native;
            var cStoredBlock = storedBlock.Native;
            var cPath = SafeNativeMethods.NativeUtf8FromString(path);
            var block = SafeNativeMethods.Longtail_WriteStoredBlock(cStorageAPI, cStoredBlock, cPath);
            SafeNativeMethods.FreeNativeUtf8String(cPath);
            return block;
        }

        public unsafe static byte[] WriteStoredBlockToBuffer(StoredBlock storedBlock)
        {
            if (storedBlock == null) { throw new ArgumentException("WriteStoredBlockToBuffer storedBlock is null"); }

            var cStoredBlock = storedBlock.Native;

            void* buffer = null;
            UInt64 size = 0;
            int err = SafeNativeMethods.Longtail_WriteStoredBlockToBuffer(cStoredBlock, ref buffer, ref size);
            if (err != 0)
            {
                ThrowExceptionFromErrno("", err);
            }
            byte[] result = new byte[size];
            Marshal.Copy((IntPtr)buffer, result, 0, (int)size);
            Free(buffer);
            return result;
        }

        private unsafe sealed class BlockStoreHandle
        {
            public BlockStoreHandle(IBlockStore blockStore)
            {
                m_BlockStore = blockStore;
                UInt64 mem_size = SafeNativeMethods.Longtail_GetBlockStoreAPISize();
                byte* mem = (byte*)API.Alloc(nameof(BlockStoreHandle), mem_size);
                if (mem == null)
                {
                    throw new OutOfMemoryException();
                }
                m_Pinned = GCHandle.Alloc(this, GCHandleType.Normal);
                m_Dispose =
                        (SafeNativeMethods.NativeAPI* api) =>
                        {
                            SafeNativeMethods.Longtail_Free(_Native.Native);
                            _Native = null;
                            m_Pinned.Free();
                        };
                m_BlockStorePutStoredBlock =
                        (SafeNativeMethods.NativeBlockStoreAPI* block_store_api, SafeNativeMethods.NativeStoredBlock* stored_block, SafeNativeMethods.NativeAsyncPutStoredBlockAPI* async_complete_api) =>
                        {
                            try
                            {
                                StoredBlock wrapped_stored_block = new StoredBlock(stored_block);
                                m_BlockStore.PutStoredBlock(wrapped_stored_block, (Exception e) =>
                                {
                                    SafeNativeMethods.Longtail_AsyncPutStoredBlock_OnComplete(async_complete_api, API.GetErrnoFromException(e, SafeNativeMethods.EIO));
                                });
                            }
                            catch (Exception e)
                            {
                                int errno = API.GetErrnoFromException(e, SafeNativeMethods.EIO);
                                SafeNativeMethods.Longtail_AsyncPutStoredBlock_OnComplete(async_complete_api, errno);
                            }
                            return 0;
                        };

                m_BlockStorePreflightGet =
                        (SafeNativeMethods.NativeBlockStoreAPI* block_store_api, UInt32 blockCount, UInt64* blockHashes, SafeNativeMethods.NativeAsyncPreflightStartedAPI* async_complete_api) =>
                        {
                            try
                            {
                                var hashes = new UInt64[blockCount];
                                for (UInt32 i = 0; i < blockCount; i++)
                                {
                                    hashes[i] = blockHashes[i];
                                }
                                m_BlockStore.PreflightGet(hashes, (UInt64[] fetchingBlockHashes, Exception e) =>
                                {
                                    if (async_complete_api == null)
                                    {
                                        return;
                                    }
                                    GCHandle pinnedArray = GCHandle.Alloc(fetchingBlockHashes, GCHandleType.Pinned);
                                    IntPtr blockHashesPtr = pinnedArray.AddrOfPinnedObject();

                                    SafeNativeMethods.Longtail_AsyncPreflightStarted_OnComplete(
                                        async_complete_api,
                                        (UInt32)fetchingBlockHashes.Length,
                                        (UInt64*)blockHashesPtr,
                                        API.GetErrnoFromException(e, SafeNativeMethods.EIO));
                                    pinnedArray.Free();
                                });
                                return 0;
                            }
                            catch (Exception e)
                            {
                                return API.GetErrnoFromException(e, SafeNativeMethods.EIO);
                            }
                        };

                m_BlockStoreGetStoredBlock =
                        (SafeNativeMethods.NativeBlockStoreAPI* block_store_api, UInt64 block_hash, SafeNativeMethods.NativeAsyncGetStoredBlockAPI* async_complete_api) =>
                        {
                            try
                            {
                                m_BlockStore.GetStoredBlock(block_hash, (StoredBlock storedBlock, Exception e) =>
                                {
                                    SafeNativeMethods.Longtail_AsyncGetStoredBlock_OnComplete(async_complete_api, storedBlock == null ? null : storedBlock.Native, API.GetErrnoFromException(e, SafeNativeMethods.EIO));
                                });
                            }
                            catch (Exception e)
                            {
                                int errno = API.GetErrnoFromException(e, SafeNativeMethods.EIO);
                                SafeNativeMethods.Longtail_AsyncGetStoredBlock_OnComplete(async_complete_api, null, errno);
                            }
                            return 0;
                        };

                m_PruneBlocks =
                        (SafeNativeMethods.NativeBlockStoreAPI* block_store_api, UInt32 blockKeepCount, UInt64* blockKeepHashes, SafeNativeMethods.NativeAsyncPruneBlocksAPI* async_complete_api) =>
                        {
                            try
                            {
                                var hashes = new UInt64[blockKeepCount];
                                for (UInt32 i = 0; i < blockKeepCount; i++)
                                {
                                    hashes[i] = blockKeepHashes[i];
                                }
                                m_BlockStore.PruneBlocks(hashes, (UInt32 prunedBlockCount, Exception e) =>
                                {
                                    SafeNativeMethods.Longtail_AsyncPruneBlocks_OnComplete(async_complete_api, prunedBlockCount, API.GetErrnoFromException(e, SafeNativeMethods.EIO));
                                });
                            }
                            catch (Exception e)
                            {
                                int errno = API.GetErrnoFromException(e, SafeNativeMethods.EIO);
                                SafeNativeMethods.Longtail_AsyncPruneBlocks_OnComplete(async_complete_api, 0, errno);
                            }
                            return 0;
                        };

                m_GetExistingContent =
                        (SafeNativeMethods.NativeBlockStoreAPI* block_store_api, UInt32 chunkCount, UInt64* chunkHashes, UInt32 minBlockUsagePercent, SafeNativeMethods.NativeAsyncGetExistingContentAPI* async_complete_api) =>
                        {
                            try
                            {
                                var hashes = new UInt64[chunkCount];
                                for (UInt32 i = 0; i < chunkCount; i++)
                                {
                                    hashes[i] = chunkHashes[i];
                                }
                                m_BlockStore.GetExistingContent(hashes, minBlockUsagePercent, (StoreIndex storeIndex, Exception e) =>
                                {
                                    SafeNativeMethods.Longtail_AsyncGetExistingContent_OnComplete(async_complete_api, storeIndex == null ? null : storeIndex.Native, API.GetErrnoFromException(e, SafeNativeMethods.EIO));
                                });
                            }
                            catch (Exception e)
                            {
                                int errno = API.GetErrnoFromException(e, SafeNativeMethods.EIO);
                                SafeNativeMethods.Longtail_AsyncGetExistingContent_OnComplete(async_complete_api, null, errno);
                            }
                            return 0;
                        };

                m_BlockStoreGetStats =
                        (SafeNativeMethods.NativeBlockStoreAPI* block_store_api, ref SafeNativeMethods.NativeBlockStoreStats out_stats) =>
                        {
                            try
                            {
                                BlockStoreStats stats = m_BlockStore.GetStats();
                                for (var s = 0; s < (int)StatU64.Count; ++s)
                                {
                                    out_stats.m_StatU64[s] = stats.m_StatU64[s];
                                }
                                return 0;
                            }
                            catch (Exception e)
                            {
                                return API.GetErrnoFromException(e, SafeNativeMethods.ENOMEM);
                            }
                        };

                m_BlockStoreFlush =
                        (SafeNativeMethods.NativeBlockStoreAPI* block_store_api, SafeNativeMethods.NativeAsyncFlushAPI* async_complete_api) =>
                        {
                            try
                            {
                                m_BlockStore.Flush((Exception e) =>
                                {
                                    SafeNativeMethods.Longtail_AsyncFlush_OnComplete(async_complete_api, API.GetErrnoFromException(e, SafeNativeMethods.EIO));
                                });
                            }
                            catch (Exception e)
                            {
                                int errno = API.GetErrnoFromException(e, SafeNativeMethods.EIO);
                                SafeNativeMethods.Longtail_AsyncFlush_OnComplete(async_complete_api, errno);
                            }
                            return 0;
                        };

                _Native = new BlockStoreAPI(
                    SafeNativeMethods.Longtail_MakeBlockStoreAPI(
                        mem,
                        m_Dispose,
                        m_BlockStorePutStoredBlock,
                        m_BlockStorePreflightGet,
                        m_BlockStoreGetStoredBlock,
                        m_GetExistingContent,
                        m_PruneBlocks,
                        m_BlockStoreGetStats,
                        m_BlockStoreFlush));
            }
            SafeNativeMethods.Longtail_DisposeFunc m_Dispose;
            SafeNativeMethods.BlockStore_PutStoredBlockCallback m_BlockStorePutStoredBlock;
            SafeNativeMethods.BlockStore_PreflightGetCallback m_BlockStorePreflightGet;
            SafeNativeMethods.BlockStore_GetStoredBlockCallback m_BlockStoreGetStoredBlock;
            SafeNativeMethods.BlockStore_GetExistingContentCallback m_GetExistingContent;
            SafeNativeMethods.BlockStore_PruneBlocksCallback m_PruneBlocks;
            SafeNativeMethods.BlockStore_GetStatsCallback m_BlockStoreGetStats;
            SafeNativeMethods.BlockStore_FlushCallback m_BlockStoreFlush;

            IBlockStore m_BlockStore;
            GCHandle m_Pinned;
            BlockStoreAPI _Native;

            public BlockStoreAPI Native
            {
                get { return this._Native; }
            }
        };

        private unsafe sealed class StorageHandle
        {
            public StorageHandle(IStorage storage)
            {
                m_Storage = storage;
                m_AllocatedStrings = new ConcurrentDictionary<IntPtr, IntPtr>(2, 2);
                UInt64 mem_size = SafeNativeMethods.Longtail_GetStorageAPISize();
                byte* mem = (byte*)API.Alloc(nameof(StorageHandle), mem_size);
                if (mem == null)
                {
                    throw new OutOfMemoryException();
                }
                m_Pinned = GCHandle.Alloc(this, GCHandleType.Normal);
                m_Dispose =
                        (SafeNativeMethods.NativeAPI* api) =>
                        {
                            SafeNativeMethods.Longtail_Free(_Native.Native);
                            _Native = null;
                            m_Pinned.Free();
                        };

                m_OpenReadFileFunc =
                    (SafeNativeMethods.NativeStorageAPI* storage_api, IntPtr path, ref IntPtr outOpenFile) =>
                    {
                        try
                        {
                            m_Storage.OpenReadFile(SafeNativeMethods.StringFromNativeUtf8(path), ref outOpenFile);
                            return 0;
                        }
                        catch (Exception e)
                        {
                            int errno = API.GetErrnoFromException(e, SafeNativeMethods.EIO);
                            return errno;
                        }
                    };
                m_GetSizeFunc =
                    (SafeNativeMethods.NativeStorageAPI* storage_api, IntPtr f, ref UInt64 outSize) =>
                    {
                        try
                        {
                            m_Storage.GetSize(f, ref outSize);
                            return 0;
                        }
                        catch (Exception e)
                        {
                            int errno = API.GetErrnoFromException(e, SafeNativeMethods.EIO);
                            return errno;
                        }
                    };
                m_ReadFunc =
                    (SafeNativeMethods.NativeStorageAPI* storage_api, IntPtr f, UInt64 offset, UInt64 length, void* output) =>
                    {
                        try
                        {
                            byte[] o = new byte[length];
                            m_Storage.Read(f, offset, length, o);
                            Marshal.Copy(o, 0, (IntPtr)output, (int)length);
                            return 0;
                        }
                        catch (Exception e)
                        {
                            int errno = API.GetErrnoFromException(e, SafeNativeMethods.EIO);
                            return errno;
                        }
                    };
                m_OpenWriteFileFunc =
                    (SafeNativeMethods.NativeStorageAPI* storage_api, IntPtr path, UInt64 initialSize, ref IntPtr outOpenFile) =>
                    {
                        try
                        {
                            m_Storage.OpenWriteFile(SafeNativeMethods.StringFromNativeUtf8(path), initialSize, ref outOpenFile);
                            return 0;
                        }
                        catch (Exception e)
                        {
                            int errno = API.GetErrnoFromException(e, SafeNativeMethods.EIO);
                            return errno;
                        }
                    };
                m_WriteFunc =
                    (SafeNativeMethods.NativeStorageAPI* storage_api, IntPtr f, UInt64 offset, UInt64 length, void* input) =>
                    {
                        try
                        {
                            byte[] o = new byte[length];
                            Marshal.Copy((IntPtr)input, o, 0, (int)length);
                            m_Storage.Write(f, offset, length, o);
                            return 0;
                        }
                        catch (Exception e)
                        {
                            int errno = API.GetErrnoFromException(e, SafeNativeMethods.EIO);
                            return errno;
                        }
                    };
                m_SetSizeFunc =
                    (SafeNativeMethods.NativeStorageAPI* storage_api, IntPtr f, UInt64 length) =>
                    {
                        try
                        {
                            m_Storage.GetSize(f, ref length);
                            return 0;
                        }
                        catch (Exception e)
                        {
                            int errno = API.GetErrnoFromException(e, SafeNativeMethods.EIO);
                            return errno;
                        }
                    };
                m_SetPermissionsFunc =
                    (SafeNativeMethods.NativeStorageAPI* storage_api, IntPtr path, UInt16 permissions) =>
                    {
                        try
                        {
                            m_Storage.SetPermissions(SafeNativeMethods.StringFromNativeUtf8(path), permissions);
                            return 0;
                        }
                        catch (Exception e)
                        {
                            int errno = API.GetErrnoFromException(e, SafeNativeMethods.EIO);
                            return errno;
                        }
                    };
                m_GetPermissionsFunc =
                    (SafeNativeMethods.NativeStorageAPI* storage_api, IntPtr path, ref UInt16 out_permissions) =>
                    {
                        try
                        {
                            out_permissions = m_Storage.GetPermissions(SafeNativeMethods.StringFromNativeUtf8(path));
                            return 0;
                        }
                        catch (Exception e)
                        {
                            int errno = API.GetErrnoFromException(e, SafeNativeMethods.EIO);
                            return errno;
                        }
                    };

                m_CloseFileFunc =
                    (SafeNativeMethods.NativeStorageAPI* storage_api, IntPtr f) =>
                    {
                        try
                        {
                            m_Storage.CloseFile(f);
                        }
                        catch (Exception /*e*/)
                        {
//                            Log.Information("StorageAPI::CloseFile failed with {@e}", e);
                        }
                    };
                m_CreateDirFunc =
                    (SafeNativeMethods.NativeStorageAPI* storage_api, IntPtr path) =>
                    {
                        try
                        {
                            m_Storage.CreateDir(SafeNativeMethods.StringFromNativeUtf8(path));
                            return 0;
                        }
                        catch (Exception e)
                        {
                            int errno = API.GetErrnoFromException(e, SafeNativeMethods.EIO);
                            return errno;
                        }
                    };
                m_RenameFileFunc =
                    (SafeNativeMethods.NativeStorageAPI* storage_api, IntPtr sourcePath, IntPtr targetPath) =>
                    {
                        try
                        {
                            m_Storage.RenameFile(SafeNativeMethods.StringFromNativeUtf8(sourcePath), SafeNativeMethods.StringFromNativeUtf8(targetPath));
                            return 0;
                        }
                        catch (Exception e)
                        {
                            int errno = API.GetErrnoFromException(e, SafeNativeMethods.EIO);
                            return errno;
                        }
                    };
                m_ConcatPathFunc =
                    (SafeNativeMethods.NativeStorageAPI* storage_api, IntPtr rootPath, IntPtr subPath) =>
                    {
                        try
                        {
                            string path = m_Storage.ConcatPath(SafeNativeMethods.StringFromNativeUtf8(rootPath), SafeNativeMethods.StringFromNativeUtf8(subPath));
                            IntPtr nativeString = SafeNativeMethods.NativeUtf8FromString(path);
                            var result = SafeNativeMethods.Longtail_Strdup(nativeString);
                            SafeNativeMethods.FreeNativeUtf8String(nativeString);
                            return result;
                        }
                        catch (Exception /*e*/)
                        {
//                            Log.Information("StorageAPI::ConcatPath failed with {@e}", e);
                            return IntPtr.Zero;
                        }
                    };
                m_IsDirFunc =
                    (SafeNativeMethods.NativeStorageAPI* storage_api, IntPtr path) =>
                    {
                        try
                        {
                            bool isDir = m_Storage.IsDir(SafeNativeMethods.StringFromNativeUtf8(path));
                            return isDir ? 1 : 0;
                        }
                        catch (Exception e)
                        {
                            int errno = API.GetErrnoFromException(e, SafeNativeMethods.EIO);
                            return errno;
                        }
                    };
                m_IsFileFunc =
                    (SafeNativeMethods.NativeStorageAPI* storage_api, IntPtr path) =>
                    {
                        try
                        {
                            bool isFile = m_Storage.IsFile(SafeNativeMethods.StringFromNativeUtf8(path));
                            return isFile ? 1 : 0;
                        }
                        catch (Exception e)
                        {
                            int errno = API.GetErrnoFromException(e, SafeNativeMethods.EIO);
                            return errno;
                        }
                    };
                m_RemoveDirFunc =
                    (SafeNativeMethods.NativeStorageAPI* storage_api, IntPtr path) =>
                    {
                        try
                        {
                            m_Storage.RemoveDir(SafeNativeMethods.StringFromNativeUtf8(path));
                            return 0;
                        }
                        catch (Exception e)
                        {
                            int errno = API.GetErrnoFromException(e, SafeNativeMethods.EIO);
                            return errno;
                        }
                    };
                m_RemoveFileFunc =
                    (SafeNativeMethods.NativeStorageAPI* storage_api, IntPtr path) =>
                    {
                        try
                        {
                            m_Storage.RemoveFile(SafeNativeMethods.StringFromNativeUtf8(path));
                            return 0;
                        }
                        catch (Exception e)
                        {
                            int errno = API.GetErrnoFromException(e, SafeNativeMethods.EIO);
                            return errno;
                        }
                    };
                m_StartFindFunc =
                    (SafeNativeMethods.NativeStorageAPI* storage_api, IntPtr path, ref IntPtr outIterator) =>
                    {
                        try
                        {
                            bool hasEntries = m_Storage.StartFind(SafeNativeMethods.StringFromNativeUtf8(path), ref outIterator);
                            if (hasEntries)
                            {
                                return 0;
                            }
                            return SafeNativeMethods.ENOENT;
                        }
                        catch (Exception e)
                        {
                            int errno = API.GetErrnoFromException(e, SafeNativeMethods.EIO);
                            return errno;
                        }
                    };
                m_FindNextFunc =
                    (SafeNativeMethods.NativeStorageAPI* storage_api, IntPtr iterator) =>
                    {
                        try
                        {
                            IntPtr oldNativeString;
                            if (m_AllocatedStrings.TryRemove(iterator, out oldNativeString))
                            {
                                SafeNativeMethods.FreeNativeUtf8String(oldNativeString);
                            }
                            bool hasMore = m_Storage.FindNext(iterator);
                            return hasMore ? 0 : SafeNativeMethods.ENOENT;
                        }
                        catch (Exception e)
                        {
                            int errno = API.GetErrnoFromException(e, SafeNativeMethods.EIO);
                            return errno;
                        }
                    };
                m_CloseFindFunc =
                    (SafeNativeMethods.NativeStorageAPI* storage_api, IntPtr iterator) =>
                    {
                        try
                        {
                            IntPtr oldNativeString;
                            if (m_AllocatedStrings.TryRemove(iterator, out oldNativeString))
                            {
                                SafeNativeMethods.FreeNativeUtf8String(oldNativeString);
                            }
                            m_Storage.CloseFind(iterator);
                        }
                        catch (Exception /*e*/)
                        {
//                            Log.Information("StorageAPI::CloseFind failed with {@e}", e);
                        }
                    };
                m_GetEntryPropertiesFunc =
                    (SafeNativeMethods.NativeStorageAPI* storage_api, IntPtr iterator, ref SafeNativeMethods.NativeStorageAPIProperties out_properties) =>
                    {
                        try
                        {
                            IntPtr oldNativeString;
                            if (m_AllocatedStrings.TryRemove(iterator, out oldNativeString))
                            {
                                SafeNativeMethods.FreeNativeUtf8String(oldNativeString);
                            }
                            var properties = m_Storage.GetEntryProperties(iterator);
                            IntPtr nativeString = SafeNativeMethods.NativeUtf8FromString(properties.m_FileName);
                            out_properties.m_FileName = nativeString;
                            out_properties.m_Size = properties.m_Size;
                            out_properties.m_Permissions = properties.m_Permissions;
                            out_properties.m_IsDir = properties.m_IsDir ? 1 : 0;
                            m_AllocatedStrings.TryAdd(iterator, nativeString);
                            return 0;
                        }
                        catch (Exception /*e*/)
                        {
//                            Log.Information("StorageAPI::GetFileName failed with {@e}", e);
                            return 0;
                        }
                    };
                m_LockFileFunc =
                    (SafeNativeMethods.NativeStorageAPI* storage_api, IntPtr path, ref IntPtr outOpenFile) =>
                    {
                        try
                        {
                            m_Storage.LockFile(SafeNativeMethods.StringFromNativeUtf8(path), ref outOpenFile);
                            return 0;
                        }
                        catch (Exception e)
                        {
                            int errno = API.GetErrnoFromException(e, SafeNativeMethods.EIO);
                            return errno;
                        }
                    };
                m_UnlockFileFunc =
                    (SafeNativeMethods.NativeStorageAPI* storage_api, IntPtr outOpenFile) =>
                    {
                        try
                        {
                            m_Storage.UnlockFile(outOpenFile);
                            return 0;
                        }
                        catch (Exception e)
                        {
                            int errno = API.GetErrnoFromException(e, SafeNativeMethods.EIO);
                            return errno;
                        }
                    };
                m_GetParentPathFunc =
                    (SafeNativeMethods.NativeStorageAPI* storage_api, IntPtr path) =>
                    {
                        try
                        {
                            string parentPath = m_Storage.GetParentPath(SafeNativeMethods.StringFromNativeUtf8(path));
                            if (parentPath == null)
                            {
                                return IntPtr.Zero;
                            }
                            IntPtr nativeString = SafeNativeMethods.NativeUtf8FromString(parentPath);
                            var result = SafeNativeMethods.Longtail_Strdup(nativeString);
                            SafeNativeMethods.FreeNativeUtf8String(nativeString);
                            return result;
                        }
                        catch (Exception /*e*/)
                        {
                            //                            Log.Information("StorageAPI::GetParentPath failed with {@e}", e);
                            return IntPtr.Zero;
                        }
                    };
                _Native = new StorageAPI(SafeNativeMethods.Longtail_MakeStorageAPI(
                    mem,
                    m_Dispose,
                    m_OpenReadFileFunc,
                    m_GetSizeFunc,
                    m_ReadFunc,
                    m_OpenWriteFileFunc,
                    m_WriteFunc,
                    m_SetSizeFunc,
                    m_SetPermissionsFunc,
                    m_GetPermissionsFunc,
                    m_CloseFileFunc,
                    m_CreateDirFunc,
                    m_RenameFileFunc,
                    m_ConcatPathFunc,
                    m_IsDirFunc,
                    m_IsFileFunc,
                    m_RemoveDirFunc,
                    m_RemoveFileFunc,
                    m_StartFindFunc,
                    m_FindNextFunc,
                    m_CloseFindFunc,
                    m_GetEntryPropertiesFunc,
                    m_LockFileFunc,
                    m_UnlockFileFunc,
                    m_GetParentPathFunc));
            }
            SafeNativeMethods.Longtail_DisposeFunc m_Dispose;
            SafeNativeMethods.Longtail_Storage_OpenReadFileFunc m_OpenReadFileFunc;
            SafeNativeMethods.Longtail_Storage_GetSizeFunc m_GetSizeFunc;
            SafeNativeMethods.Longtail_Storage_ReadFunc m_ReadFunc;
            SafeNativeMethods.Longtail_Storage_OpenWriteFileFunc m_OpenWriteFileFunc;
            SafeNativeMethods.Longtail_Storage_WriteFunc m_WriteFunc;
            SafeNativeMethods.Longtail_Storage_SetSizeFunc m_SetSizeFunc;
            SafeNativeMethods.Longtail_Storage_SetPermissionsFunc m_SetPermissionsFunc;
            SafeNativeMethods.Longtail_Storage_GetPermissionsFunc m_GetPermissionsFunc;
            SafeNativeMethods.Longtail_Storage_CloseFileFunc m_CloseFileFunc;
            SafeNativeMethods.Longtail_Storage_CreateDirFunc m_CreateDirFunc;
            SafeNativeMethods.Longtail_Storage_RenameFileFunc m_RenameFileFunc;
            SafeNativeMethods.Longtail_Storage_ConcatPathFunc m_ConcatPathFunc;
            SafeNativeMethods.Longtail_Storage_IsDirFunc m_IsDirFunc;
            SafeNativeMethods.Longtail_Storage_IsFileFunc m_IsFileFunc;
            SafeNativeMethods.Longtail_Storage_RemoveDirFunc m_RemoveDirFunc;
            SafeNativeMethods.Longtail_Storage_RemoveFileFunc m_RemoveFileFunc;
            SafeNativeMethods.Longtail_Storage_StartFindFunc m_StartFindFunc;
            SafeNativeMethods.Longtail_Storage_FindNextFunc m_FindNextFunc;
            SafeNativeMethods.Longtail_Storage_CloseFindFunc m_CloseFindFunc;
            SafeNativeMethods.Longtail_Storage_GetEntryPropertiesFunc m_GetEntryPropertiesFunc;
            SafeNativeMethods.Longtail_Storage_LockFileFunc m_LockFileFunc;
            SafeNativeMethods.Longtail_Storage_UnlockFileFunc m_UnlockFileFunc;
            SafeNativeMethods.Longtail_Storage_GetParentPathFunc m_GetParentPathFunc;
            ConcurrentDictionary<IntPtr, IntPtr> m_AllocatedStrings;

            IStorage m_Storage;
            GCHandle m_Pinned;
            StorageAPI _Native;

            public StorageAPI Native
            {
                get { return this._Native; }
            }
        };

        private unsafe sealed class ProgressHandle : IDisposable
        {
            public ProgressHandle(ProgressFunc progressFunc, UInt32 progressPercentRateLimit)
            {
                if (progressFunc == null)
                {
                    return;
                }
                m_ProgressFunc = progressFunc;
                UInt64 mem_size = SafeNativeMethods.Longtail_GetProgressAPISize();
                m_ProgressMem = (byte*)API.Alloc(nameof(ProgressHandle), mem_size);
                if (m_ProgressMem == null)
                {
                    throw new OutOfMemoryException();
                }
                m_Dispose =
                        (SafeNativeMethods.NativeAPI* api) =>
                        {
                            SafeNativeMethods.Longtail_Free(m_ProgressMem);
                            m_ProgressMem = null;
                        };
                m_ProgressCallback =
                    (SafeNativeMethods.NativeProgressAPI* progress_api, UInt32 total_count, UInt32 done_count) =>
                    {
                        try
                        {
                            m_ProgressFunc(total_count, done_count);
                        }
                        catch (Exception)
                        {
                            // Eat exception, there is no way to report errors currently
                        }
                    };
                if (progressPercentRateLimit > 0)
                {
                    var progress = SafeNativeMethods.Longtail_MakeProgressAPI(m_ProgressMem, m_Dispose, m_ProgressCallback);
                    // Limit callbacks for progress to every 1%
                    _Native = SafeNativeMethods.Longtail_CreateRateLimitedProgress(progress, progressPercentRateLimit);
                }
                else
                {
                    _Native = SafeNativeMethods.Longtail_MakeProgressAPI(m_ProgressMem, m_Dispose, m_ProgressCallback);
                }
            }
            public void Dispose()
            {
                if (_Native != null)
                {
                    SafeNativeMethods.Longtail_DisposeAPI((SafeNativeMethods.NativeAPI*)_Native);
                    _Native = null;
                }
            }
            internal SafeNativeMethods.NativeProgressAPI* Native
            {
                get { return this._Native; }
            }

            SafeNativeMethods.Longtail_DisposeFunc m_Dispose;
            SafeNativeMethods.ProgressCallback m_ProgressCallback;
            ProgressFunc m_ProgressFunc;
            byte* m_ProgressMem;
            SafeNativeMethods.NativeProgressAPI* _Native;
        };

        private unsafe sealed class CancelHandle : IDisposable
        {
            public CancelHandle(CancellationToken cancellationToken)
            {
                if (cancellationToken == null)
                {
                    return;
                }
                m_CancellationToken = cancellationToken;
                UInt64 mem_size = SafeNativeMethods.Longtail_GetCancelAPISize();
                byte* mem = (byte*)API.Alloc(nameof(CancelHandle), mem_size);
                if (mem == null)
                {
                    throw new OutOfMemoryException();
                }
                m_Dispose =
                        (SafeNativeMethods.NativeAPI* api) =>
                        {
                            SafeNativeMethods.Longtail_Free(_Native);
                            _Native = null;
                        };

                m_CreateTokenFunc =
                    (SafeNativeMethods.NativeCancelAPI* cancel_api, ref IntPtr out_token) =>
                    {
                        return SafeNativeMethods.ENOTSUP;
                    };

                m_CancelFunc =
                    (SafeNativeMethods.NativeCancelAPI* cancel_api, IntPtr token) =>
                    {
                        return SafeNativeMethods.ENOTSUP;
                    };

                m_IsCancelledFunc =
                    (SafeNativeMethods.NativeCancelAPI* cancel_api, IntPtr token) =>
                    {
                        if (token != (IntPtr)_Native)
                        {
                            return SafeNativeMethods.EINVAL;
                        }
                        if (m_CancellationToken.IsCancellationRequested)
                        {
                            return SafeNativeMethods.ECANCELED;
                        }
                        return 0;
                    };

                m_DisposeTokenFunc =
                    (SafeNativeMethods.NativeCancelAPI* cancel_api, IntPtr token) =>
                    {
                        return SafeNativeMethods.ENOTSUP;
                    };
                _Native = SafeNativeMethods.Longtail_MakeCancelAPI(
                    mem,
                    m_Dispose,
                    m_CreateTokenFunc,
                    m_CancelFunc,
                    m_IsCancelledFunc,
                    m_DisposeTokenFunc);
            }
            public void Dispose()
            {
                if (_Native != null)
                {
                    SafeNativeMethods.Longtail_DisposeAPI((SafeNativeMethods.NativeAPI*)_Native);
                    _Native = null;
                }
            }
            internal SafeNativeMethods.NativeCancelAPI* Native
            {
                get { return this._Native; }
            }

            SafeNativeMethods.Longtail_DisposeFunc m_Dispose;
            CancellationToken m_CancellationToken;
            SafeNativeMethods.NativeCancelAPI* _Native;

            SafeNativeMethods.Longtail_CancelAPI_CreateTokenFunc m_CreateTokenFunc;
            SafeNativeMethods.Longtail_CancelAPI_CancelFunc m_CancelFunc;
            SafeNativeMethods.Longtail_CancelAPI_IsCancelledFunc m_IsCancelledFunc;
            SafeNativeMethods.Longtail_CancelAPI_DisposeTokenFunc m_DisposeTokenFunc;
        };

        private unsafe sealed class PathFilterHandle
        {
            public PathFilterHandle(PathFilterFunc pathFilterFunc)
            {
                if (pathFilterFunc == null)
                {
                    return;
                }
                m_PathFilterFunc = pathFilterFunc;
                UInt64 mem_size = SafeNativeMethods.Longtail_GetPathFilterAPISize();
                byte* mem = (byte*)API.Alloc(nameof(PathFilterHandle), mem_size);
                if (mem == null)
                {
                    throw new OutOfMemoryException();
                }
                m_Pinned = GCHandle.Alloc(this, GCHandleType.Normal);
                m_Dispose =
                        (SafeNativeMethods.NativeAPI* api) =>
                        {
                            SafeNativeMethods.Longtail_Free(_Native.Native);
                            _Native = null;
                            m_Pinned.Free();
                        };

                m_PathFilterCallback =
                    (SafeNativeMethods.NativePathFilterAPI* path_filter_api, IntPtr root_path, IntPtr asset_path, IntPtr asset_name, int is_dir, UInt64 size, UInt16 permissions) =>
                    {
                        try
                        {
                            bool result = m_PathFilterFunc(SafeNativeMethods.StringFromNativeUtf8(root_path), SafeNativeMethods.StringFromNativeUtf8(asset_path), SafeNativeMethods.StringFromNativeUtf8(asset_name), is_dir != 0, (ulong)size, (uint)permissions);
                            return result ? 1 : 0;
                        }
                        catch (Exception)
                        {
                            // Eat exceptions and assume that we should include the file
                            return 1;
                        }
                    };
                _Native = new PathFilterAPI(SafeNativeMethods.Longtail_MakePathFilterAPI(mem, m_Dispose, m_PathFilterCallback));

            }

            internal PathFilterAPI Native
            {
                get { return this._Native; }
            }

            SafeNativeMethods.Longtail_DisposeFunc m_Dispose;
            SafeNativeMethods.PathFilterCallback m_PathFilterCallback;
            PathFilterFunc m_PathFilterFunc;
            GCHandle m_Pinned;

            PathFilterAPI _Native;
        };

    };






    internal static class SafeNativeMethods
    {
#if DEBUG
        const string LongtailDLLName = "longtail_win32_x64_debug.dll";
#else
        const string LongtailDLLName = "longtail_win32_x64.dll";
#endif
        public const int EPERM = 1;    /* Not super-user */
        public const int ENOENT = 2;   /* No such file or directory */
        public const int ESRCH = 3;    /* No such process */
        public const int EINTR = 4;    /* Interrupted system call */
        public const int EIO = 5;      /* I/O error */
        public const int ENXIO = 6;    /* No such device or address */
        public const int E2BIG = 7;    /* Arg list too long */
        public const int ENOEXEC = 8;  /* Exec format error */
        public const int EBADF = 9;    /* Bad file number */
        public const int ECHILD = 10;  /* No children */
        public const int EAGAIN = 11;  /* No more processes */
        public const int ENOMEM = 12;  /* Not enough core */
        public const int EACCES = 13;  /* Permission denied */
        public const int EFAULT = 14;  /* Bad address */
        public const int ENOTBLK = 15; /* Block device required */
        public const int EBUSY = 16;   /* Mount device busy */
        public const int EEXIST = 17;  /* File exists */
        public const int EXDEV = 18;   /* Cross-device link */
        public const int ENODEV = 19;  /* No such device */
        public const int ENOTDIR = 20; /* Not a directory */
        public const int EISDIR = 21;  /* Is a directory */
        public const int EINVAL = 22;  /* Invalid argument */
        public const int ENFILE = 23;  /* Too many open files in system */
        public const int EMFILE = 24;  /* Too many open files */
        public const int ENOTTY = 25;  /* Not a typewriter */
        public const int ETXTBSY = 26; /* Text file busy */
        public const int EFBIG = 27;   /* File too large */
        public const int ENOSPC = 28;  /* No space left on device */
        public const int ESPIPE = 29;  /* Illegal seek */
        public const int EROFS = 30;   /* Read only file system */
        public const int EMLINK = 31;  /* Too many links */
        public const int EPIPE = 32;   /* Broken pipe */
        public const int EDOM = 33;    /* Math arg out of domain of func */
        public const int ERANGE = 34;  /* Math result not representable */
        public const int ENOTSUP = 129;/* Operation not supported (POSIX.1-2001). */
        public const int ECANCELED = 105; /* Operation was cancelled */

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void AssertCallback(IntPtr expression, IntPtr file, int line);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate void LogCallback(SafeNativeMethods.NativeLogContext* context, IntPtr str);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate void Longtail_DisposeFunc(NativeAPI* api);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate void ProgressCallback(NativeProgressAPI* progress_api, UInt32 totalCount, UInt32 doneCount);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate int PathFilterCallback(NativePathFilterAPI* path_filter_api, IntPtr root_path, IntPtr asset_path, IntPtr asset_name, int is_dir, UInt64 size, UInt16 permissions);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate int ASyncPutStoredBlockCompleteCallback(NativeAsyncPutStoredBlockAPI* asyncCompleteAPI, int err);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate int ASyncGetStoredBlockCompleteCallback(NativeAsyncGetStoredBlockAPI* asyncCompleteAPI, NativeStoredBlock* stored_block, int err);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate int ASyncFlushCompleteCallback(NativeAsyncFlushAPI* asyncCompleteAPI, int err);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate int ASyncGetExistingContentCompleteCallback(NativeAsyncGetExistingContentAPI* asyncCompleteAPI, NativeStoreIndex* store_index, int err);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate int BlockStore_PutStoredBlockCallback(NativeBlockStoreAPI* block_store_api, NativeStoredBlock* stored_block, NativeAsyncPutStoredBlockAPI* async_complete_api);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate int BlockStore_PreflightGetCallback(SafeNativeMethods.NativeBlockStoreAPI* block_store_api, UInt32 chunkCount, UInt64* chunkHashes, NativeAsyncPreflightStartedAPI* async_complete_api);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate int BlockStore_GetStoredBlockCallback(NativeBlockStoreAPI* block_store_api, UInt64 block_hash, NativeAsyncGetStoredBlockAPI* async_complete_api);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate int BlockStore_FlushCallback(NativeBlockStoreAPI* block_store_api, NativeAsyncFlushAPI* async_complete_api);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate int BlockStore_GetExistingContentCallback(NativeBlockStoreAPI* block_store_api, UInt32 chunkCount, UInt64* chunkHashes, UInt32 minBlockUsagePercent, NativeAsyncGetExistingContentAPI* async_complete_api);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate int BlockStore_PruneBlocksCallback(NativeBlockStoreAPI* block_store_api, UInt32 keepBlockCount, UInt64* blockHashes, NativeAsyncPruneBlocksAPI* async_complete_api);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate int BlockStore_GetStatsCallback(NativeBlockStoreAPI* block_store_api, ref NativeBlockStoreStats out_stats);

        public unsafe delegate int Longtail_Storage_OpenReadFileFunc(NativeStorageAPI* storage_api, IntPtr path, ref IntPtr out_open_file);
        public unsafe delegate int Longtail_Storage_GetSizeFunc(NativeStorageAPI* storage_api, IntPtr f, ref UInt64 out_size);
        public unsafe delegate int Longtail_Storage_ReadFunc(NativeStorageAPI* storage_api, IntPtr f, UInt64 offset, UInt64 length, void* output);
        public unsafe delegate int Longtail_Storage_OpenWriteFileFunc(NativeStorageAPI* storage_api, IntPtr path, UInt64 initial_size, ref IntPtr out_open_file);
        public unsafe delegate int Longtail_Storage_WriteFunc(NativeStorageAPI* storage_api, IntPtr f, UInt64 offset, UInt64 length, void* input);
        public unsafe delegate int Longtail_Storage_SetSizeFunc(NativeStorageAPI* storage_api, IntPtr f, UInt64 length);
        public unsafe delegate int Longtail_Storage_SetPermissionsFunc(NativeStorageAPI* storage_api, IntPtr path, UInt16 permissions);
        public unsafe delegate int Longtail_Storage_GetPermissionsFunc(NativeStorageAPI* storage_api, IntPtr path, ref UInt16 out_permissions);
        public unsafe delegate void Longtail_Storage_CloseFileFunc(NativeStorageAPI* storage_api, IntPtr f);
        public unsafe delegate int Longtail_Storage_CreateDirFunc(NativeStorageAPI* storage_api, IntPtr path);
        public unsafe delegate int Longtail_Storage_RenameFileFunc(NativeStorageAPI* storage_api, IntPtr source_path, IntPtr target_path);
        public unsafe delegate IntPtr Longtail_Storage_ConcatPathFunc(NativeStorageAPI* storage_api, IntPtr root_path, IntPtr sub_path);
        public unsafe delegate int Longtail_Storage_IsDirFunc(NativeStorageAPI* storage_api, IntPtr path);
        public unsafe delegate int Longtail_Storage_IsFileFunc(NativeStorageAPI* storage_api, IntPtr path);
        public unsafe delegate int Longtail_Storage_RemoveDirFunc(NativeStorageAPI* storage_api, IntPtr path);
        public unsafe delegate int Longtail_Storage_RemoveFileFunc(NativeStorageAPI* storage_api, IntPtr path);
        public unsafe delegate int Longtail_Storage_StartFindFunc(NativeStorageAPI* storage_api, IntPtr path, ref IntPtr out_iterator);
        public unsafe delegate int Longtail_Storage_FindNextFunc(NativeStorageAPI* storage_api, IntPtr iterator);
        public unsafe delegate void Longtail_Storage_CloseFindFunc(NativeStorageAPI* storage_api, IntPtr iterator);
        public unsafe delegate int Longtail_Storage_GetEntryPropertiesFunc(NativeStorageAPI* storage_api, IntPtr iterator, ref NativeStorageAPIProperties out_properties);
        public unsafe delegate int Longtail_Storage_LockFileFunc(NativeStorageAPI* storage_api, IntPtr path, ref IntPtr out_lock_file);
        public unsafe delegate int Longtail_Storage_UnlockFileFunc(NativeStorageAPI* storage_api, IntPtr lock_file);
        public unsafe delegate IntPtr Longtail_Storage_GetParentPathFunc(NativeStorageAPI* storage_api, IntPtr path);

        public unsafe delegate int Longtail_CancelAPI_CreateTokenFunc(NativeCancelAPI* cancel_api, ref IntPtr out_token);
        public unsafe delegate int Longtail_CancelAPI_CancelFunc(NativeCancelAPI* cancel_api, IntPtr token);
        public unsafe delegate int Longtail_CancelAPI_IsCancelledFunc(NativeCancelAPI* cancel_api, IntPtr token);
        public unsafe delegate int Longtail_CancelAPI_DisposeTokenFunc(NativeCancelAPI* cancel_api, IntPtr token);

        [DllImport(LongtailDLLName)]
        internal unsafe static extern UInt64 Longtail_GetStorageAPISize();

        [DllImport(LongtailDLLName)]
        internal unsafe static extern NativeStorageAPI* Longtail_MakeStorageAPI(
            void* mem,
            Longtail_DisposeFunc dispose_func,
            Longtail_Storage_OpenReadFileFunc open_read_file_func,
            Longtail_Storage_GetSizeFunc get_size_func,
            Longtail_Storage_ReadFunc read_func,
            Longtail_Storage_OpenWriteFileFunc open_write_file_func,
            Longtail_Storage_WriteFunc write_func,
            Longtail_Storage_SetSizeFunc set_size_func,
            Longtail_Storage_SetPermissionsFunc set_permissions_func,
            Longtail_Storage_GetPermissionsFunc get_permissions_func,
            Longtail_Storage_CloseFileFunc close_file_func,
            Longtail_Storage_CreateDirFunc create_dir_func,
            Longtail_Storage_RenameFileFunc rename_file_func,
            Longtail_Storage_ConcatPathFunc concat_path_func,
            Longtail_Storage_IsDirFunc is_dir_func,
            Longtail_Storage_IsFileFunc is_file_func,
            Longtail_Storage_RemoveDirFunc remove_dir_func,
            Longtail_Storage_RemoveFileFunc remove_file_func,
            Longtail_Storage_StartFindFunc start_find_func,
            Longtail_Storage_FindNextFunc find_next_func,
            Longtail_Storage_CloseFindFunc close_find_func,
            Longtail_Storage_GetEntryPropertiesFunc get_entry_properties_func,
            Longtail_Storage_LockFileFunc lock_file_func,
            Longtail_Storage_UnlockFileFunc unlock_file_func,
            Longtail_Storage_GetParentPathFunc get_parent_path_func);

        internal unsafe struct NativeAPI { }
        internal unsafe struct NativeProgressAPI { }
        internal unsafe struct NativePathFilterAPI { }
        internal unsafe struct NativeAsyncPutStoredBlockAPI { }
        internal unsafe struct NativeAsyncPreflightStartedAPI { }
        internal unsafe struct NativeAsyncGetStoredBlockAPI { }
        internal unsafe struct NativeAsyncPruneBlocksAPI { }
        internal unsafe struct NativeAsyncFlushAPI { }
        internal unsafe struct NativeAsyncGetExistingContentAPI { }

        internal unsafe struct NativeBlockStoreAPI { }
        internal unsafe struct NativeStorageAPI { }
        internal unsafe struct NativeHashAPI { }
        internal unsafe struct NativeJobAPI { }
        internal unsafe struct NativeChunkerAPI { }
        internal unsafe struct NativeHashRegistryAPI { }
        internal unsafe struct NativeCompressionRegistryAPI { }
        internal unsafe struct NativeCancelAPI { }

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct NativeStoreIndex
        {
            UInt32* m_Version;
            UInt32* m_HashIdentifier;
            UInt32* m_BlockCount;
            UInt32* m_ChunkCount;
            UInt64* m_BlockHashes;
            UInt64* m_ChunkHashes;
            UInt32* m_BlockChunksOffsets;
            UInt32* m_BlockChunkCounts;
            UInt32* m_BlockTags;
            UInt32* m_ChunkSizes;

            public unsafe UInt32 GetHashIdentifier() { return *m_HashIdentifier; }
            public unsafe UInt64[] GetBlockHashes()
            {
                UInt32 blockCount = *m_BlockCount;
                var blockHashes = new UInt64[blockCount];
                for (UInt32 b = 0; b < blockCount; b++)
                {
                    blockHashes[b] = m_BlockHashes[b];
                }
                return blockHashes;
            }
            public unsafe UInt64[] GetChunkHashes()
            {
                UInt32 chunkCount = *m_ChunkCount;
                var chunkHashes = new UInt64[chunkCount];
                for (UInt32 b = 0; b < chunkCount; b++)
                {
                    chunkHashes[b] = m_ChunkHashes[b];
                }
                return chunkHashes;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct NativeStorageAPIProperties
        {
            public IntPtr m_FileName;
            public UInt64 m_Size;
            public UInt16 m_Permissions;
            public Int32 m_IsDir;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct NativeFileInfos
        {
            UInt32 m_Count;
            UInt32 m_PathDataSize;
            UInt64* m_Sizes;
            UInt32* m_PathStartOffsets;
            UInt16* m_Permissions;
            IntPtr m_PathData;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct NativeVersionIndex
        {
            UInt32* m_Version;
            UInt32* m_HashIdentifier;
            UInt32* m_TargetChunkSize;
            UInt32* m_AssetCount;
            UInt32* m_ChunkCount;
            UInt32* m_AssetChunkIndexCount;
            UInt64* m_PathHashes;
            UInt64* m_ContentHashes;
            UInt64* m_AssetSizes;
            UInt32* m_AssetChunkCounts;
            UInt32* m_AssetChunkIndexStarts;
            UInt32* m_AssetChunkIndexes;
            UInt64* m_ChunkHashes;
            UInt32* m_ChunkSizes;
            UInt32* m_ChunkTags;
            UInt32* m_NameOffsets;
            UInt32 m_NameDataSize;
            UInt16* m_Permissions;
            IntPtr m_NameData;

            public unsafe UInt32 GetHashIdentifier() { return *m_HashIdentifier; }
            public unsafe UInt32 GetTargetChunkSize() { return *m_TargetChunkSize; }
            public unsafe UInt32 GetChunkCount() { return *m_ChunkCount; }
            public unsafe UInt64[] GetChunkHashes()
            {
                UInt32 chunkCount = *m_ChunkCount;
                var chunkHashes = new UInt64[chunkCount];
                for (UInt32 b = 0; b < chunkCount; b++)
                {
                    chunkHashes[b] = m_ChunkHashes[b];
                }
                return chunkHashes;
            }
        }

        public static IntPtr NativeUtf8FromString(string managedString)
        {
            int len = Encoding.UTF8.GetByteCount(managedString);
            byte[] buffer = new byte[len + 1];
            Encoding.UTF8.GetBytes(managedString, 0, managedString.Length, buffer, 0);
            IntPtr nativeUtf8 = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, nativeUtf8, buffer.Length);
            return nativeUtf8;
        }

        public static void FreeNativeUtf8String(IntPtr nativeString)
        {
            Marshal.FreeHGlobal(nativeString);
        }

        public static string StringFromNativeUtf8(IntPtr nativeUtf8)
        {
            int len = 0;
            while (Marshal.ReadByte(nativeUtf8, len) != 0) ++len;
            byte[] buffer = new byte[len];
            Marshal.Copy(nativeUtf8, buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer);
        }

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct NativeLogContext
        {
            void* m_Context;
            IntPtr m_File;
            IntPtr m_Function;
            IntPtr* m_Fields;
            Int32 m_FieldCount;
            Int32 m_Line;
            Int32 m_Level;

            public unsafe string GetFile() { return StringFromNativeUtf8(m_File);}
            public unsafe string GetFunction() { return StringFromNativeUtf8(m_Function); }
            public unsafe int GetFieldCount() { return (int)m_FieldCount; }
            public unsafe string GetFieldName(int fieldIndex) { return StringFromNativeUtf8(m_Fields[fieldIndex * 2 + 0]); }
            public unsafe string GetFieldValue(int fieldIndex) { return StringFromNativeUtf8(m_Fields[fieldIndex * 2 + 1]); }
            public unsafe Int32 GetLine() { return m_Line; }
            public unsafe Int32 GetLevel() { return m_Level; }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct NativeVersionDiff
        {
            UInt32* m_SourceRemovedCount;
            UInt32* m_TargetAddedCount;
            UInt32* m_ModifiedContentCount;
            UInt32* m_ModifiedPermissionsCount;
            UInt32* m_SourceRemovedAssetIndexes;
            UInt32* m_TargetAddedAssetIndexes;
            UInt32* m_SourceContentModifiedAssetIndexes;
            UInt32* m_TargetContentModifiedAssetIndexes;
            UInt32* m_SourcePermissionsModifiedAssetIndexes;
            UInt32* m_TargetPermissionsModifiedAssetIndexes;

            public unsafe int GetChangeCount() { return (int)(*m_SourceRemovedCount + *m_ModifiedContentCount + *m_TargetAddedCount); }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct NativeBlockStoreStats
        {
            internal fixed UInt64 m_StatU64[(int)StatU64.Count];
        }

        internal struct NativeStoredBlock { }

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern void Longtail_DisposeAPI(NativeAPI* api);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void Longtail_SetAssert([MarshalAs(UnmanagedType.FunctionPtr)] AssertCallback assertCallback);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern void Longtail_SetLog([MarshalAs(UnmanagedType.FunctionPtr)] LogCallback logCallback, void* context);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void Longtail_SetLogLevel(int level);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern void* Longtail_Alloc(IntPtr context, UInt64 size);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern void Longtail_Free(void* data);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern IntPtr Longtail_Strdup(IntPtr str);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern UInt64 Longtail_GetProgressAPISize();

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern NativeProgressAPI* Longtail_MakeProgressAPI(void* mem, [MarshalAs(UnmanagedType.FunctionPtr)] Longtail_DisposeFunc disposeFunc, [MarshalAs(UnmanagedType.FunctionPtr)] ProgressCallback progressCallback);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern UInt64 Longtail_GetPathFilterAPISize();

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern NativePathFilterAPI* Longtail_MakePathFilterAPI(void* mem, [MarshalAs(UnmanagedType.FunctionPtr)] Longtail_DisposeFunc disposeFunc, [MarshalAs(UnmanagedType.FunctionPtr)] PathFilterCallback includeCallback);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern void Longtail_Progress_OnProgress(NativeProgressAPI* progressAPI, UInt32 total_count, UInt32 done_count);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern UInt64 Longtail_GetAsyncPutStoredBlockAPISize();

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern NativeAsyncPutStoredBlockAPI* Longtail_MakeAsyncPutStoredBlockAPI(void* mem, [MarshalAs(UnmanagedType.FunctionPtr)] Longtail_DisposeFunc disposeFunc, [MarshalAs(UnmanagedType.FunctionPtr)] ASyncPutStoredBlockCompleteCallback asyncComplete_callback);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern void Longtail_AsyncPutStoredBlock_OnComplete(NativeAsyncPutStoredBlockAPI* aSyncCompleteAPI, int res);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern UInt64 Longtail_GetAsyncGetStoredBlockAPISize();

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern NativeAsyncGetStoredBlockAPI* Longtail_MakeAsyncGetStoredBlockAPI(void* mem, [MarshalAs(UnmanagedType.FunctionPtr)] Longtail_DisposeFunc disposeFunc, [MarshalAs(UnmanagedType.FunctionPtr)] ASyncGetStoredBlockCompleteCallback asyncComplete_callback);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern void Longtail_AsyncGetStoredBlock_OnComplete(NativeAsyncGetStoredBlockAPI* aSyncCompleteAPI, NativeStoredBlock* storedBlock, int res);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern UInt64 Longtail_GetAsyncGetExistingContentAPISize();

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern NativeAsyncGetExistingContentAPI* Longtail_MakeAsyncGetExistingContentAPI(
            void* mem,
            [MarshalAs(UnmanagedType.FunctionPtr)] Longtail_DisposeFunc disposeFunc,
            [MarshalAs(UnmanagedType.FunctionPtr)] ASyncGetExistingContentCompleteCallback asyncComplete_callback);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern void Longtail_AsyncGetExistingContent_OnComplete(NativeAsyncGetExistingContentAPI* aSyncCompleteAPI, NativeStoreIndex* storeIndex, int res);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern void Longtail_AsyncPruneBlocks_OnComplete(NativeAsyncPruneBlocksAPI* aSyncCompleteAPI, UInt32 prunedBlockCount, int res);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern void Longtail_AsyncPreflightStarted_OnComplete(NativeAsyncPreflightStartedAPI* aSyncCompleteAPI, UInt32 blockCount, UInt64* blockHashes, int res);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern UInt64 Longtail_GetAsyncFlushAPISize();

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern NativeAsyncFlushAPI* Longtail_MakeAsyncFlushAPI(
            void* mem,
            [MarshalAs(UnmanagedType.FunctionPtr)] Longtail_DisposeFunc disposeFunc,
            [MarshalAs(UnmanagedType.FunctionPtr)] ASyncFlushCompleteCallback asyncComplete_callback);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern void Longtail_AsyncFlush_OnComplete(NativeAsyncFlushAPI* aSyncCompleteAPI, int res);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern UInt32 Longtail_FileInfos_GetCount(NativeFileInfos* file_infos);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern IntPtr Longtail_FileInfos_GetPath(NativeFileInfos* file_infos, UInt32 index);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern UInt32 Longtail_FileInfos_GetPermissions(NativeFileInfos* file_infos, UInt32 index);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern int Longtail_GetFilesRecursively(
            NativeStorageAPI* storage_api,
            NativePathFilterAPI* path_filter_api,
            NativeCancelAPI* cancel_api,
            IntPtr cancel_token,
            IntPtr root_path,
            ref NativeFileInfos* out_file_infos);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern int Longtail_CreateVersionIndex(
            NativeStorageAPI* storage_api,
            NativeHashAPI* hash_api,
            NativeChunkerAPI* chunker_api,
            NativeJobAPI* job_api,
            NativeProgressAPI* progress_api,
            NativeCancelAPI* cancel_api,
            IntPtr cancel_token,
            IntPtr root_path,
            NativeFileInfos* file_infos,
            UInt32[] asset_tags,
            UInt32 max_chunk_size,
            ref NativeVersionIndex* out_version_index);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern int Longtail_ReadVersionIndexFromBuffer(void* buffer, UInt64 size, ref NativeVersionIndex* out_version_index);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern int Longtail_ReadVersionIndex(
            NativeStorageAPI* storage_api,
            IntPtr path,
            ref NativeVersionIndex* out_version_index);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern int Longtail_ReadStoreIndexFromBuffer(
            void* buffer,
            UInt64 size,
            ref NativeStoreIndex* out_store_index);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern int Longtail_ReadStoreIndex(
            NativeStorageAPI* storage_api,
            IntPtr path,
            ref NativeStoreIndex* out_store_index);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern int Longtail_CreateMissingContent(
            NativeHashAPI* hash_api,
            NativeStoreIndex* store_index,
            NativeVersionIndex* version,
            UInt32 max_block_size,
            UInt32 max_chunks_per_block,
            ref NativeStoreIndex* out_store_index);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern int Longtail_GetExistingStoreIndex(
            NativeStoreIndex* store_index,
            UInt32 chunk_count,
            UInt64* chunk_hashes,
            UInt32 minBlockUsagePercent,
            ref NativeStoreIndex* out_store_index);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern int Longtail_CreateVersionDiff(
            NativeHashAPI* hash_api,
            NativeVersionIndex* source_version,
            NativeVersionIndex* target_version,
            ref NativeVersionDiff* out_version_diff);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern int Longtail_ChangeVersion(
            NativeBlockStoreAPI* block_store_api,
            NativeStorageAPI* version_storage_api,
            NativeHashAPI* hash_api,
            NativeJobAPI* job_api,
            NativeProgressAPI* progress_api,
            NativeCancelAPI* cancel_api,
            IntPtr cancel_token,
            NativeStoreIndex* store_index,
            NativeVersionIndex* source_version,
            NativeVersionIndex* target_version,
            NativeVersionDiff* version_diff,
            IntPtr version_path,
            int retain_permissions);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern int Longtail_GetRequiredChunkHashes(
            NativeVersionIndex* version_index,
            NativeVersionDiff* version_diff,
            ref UInt32 chunk_count,
            UInt64* out_chunk_hashes);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern NativeStorageAPI* Longtail_CreateFSStorageAPI();

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern NativeStorageAPI* Longtail_CreateInMemStorageAPI();

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern UInt32 Longtail_Hash_GetIdentifier(NativeHashAPI* hash_api);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern NativeHashAPI* Longtail_CreateBlake2HashAPI();

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern UInt32 Longtail_GetBlake2HashType();

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern NativeHashAPI* Longtail_CreateBlake3HashAPI();

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern UInt32 Longtail_GetBlake3HashType();

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern NativeHashAPI* Longtail_CreateMeowHashAPI();

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern UInt32 Longtail_GetMeowHashType();

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern NativeJobAPI* Longtail_CreateBikeshedJobAPI(UInt32 worker_count, int worker_priority);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern NativeChunkerAPI* Longtail_CreateHPCDCChunkerAPI();

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern UInt32 Longtail_Job_GetWorkerCount(NativeJobAPI* job_api);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern NativeHashRegistryAPI* Longtail_CreateFullHashRegistry();

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern int Longtail_GetHashRegistry_GetHashAPI(NativeHashRegistryAPI* hash_registry, UInt32 hash_type, ref NativeHashAPI* out_hash_api);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern NativeCompressionRegistryAPI* Longtail_CreateFullCompressionRegistry();

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern NativeBlockStoreAPI* Longtail_CreateFSBlockStoreAPI(NativeJobAPI* job_api, NativeStorageAPI* storage_api, IntPtr content_path, IntPtr optional_block_extension);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern NativeBlockStoreAPI* Longtail_CreateCacheBlockStoreAPI(NativeJobAPI* job_api, NativeBlockStoreAPI* local_block_store, NativeBlockStoreAPI* remote_block_store);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern NativeBlockStoreAPI* Longtail_CreateCompressBlockStoreAPI(NativeBlockStoreAPI* backing_block_store, NativeCompressionRegistryAPI* compression_registry);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern NativeBlockStoreAPI* Longtail_CreateLRUBlockStoreAPI(NativeBlockStoreAPI* backing_block_store, UInt32 maxLRUCount);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern NativeBlockStoreAPI* Longtail_CreateShareBlockStoreAPI(NativeBlockStoreAPI* backing_block_store);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern int Longtail_BlockStore_PutStoredBlock(NativeBlockStoreAPI* block_store_api, NativeStoredBlock* stored_block, NativeAsyncPutStoredBlockAPI* async_complete_api);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe extern static int Longtail_BlockStore_GetStoredBlock(NativeBlockStoreAPI* block_store_api, UInt64 block_hash, NativeAsyncGetStoredBlockAPI* async_complete_api);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern int Longtail_BlockStore_Flush(NativeBlockStoreAPI* block_store_api, NativeAsyncFlushAPI* async_complete_api);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern int Longtail_BlockStore_GetExistingContent(NativeBlockStoreAPI* block_store_api, UInt32 chunk_count, UInt64* chunk_hashes, UInt32 minBlockUsagePercent, NativeAsyncGetExistingContentAPI* async_complete_api);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern int Longtail_BlockStore_GetStats(NativeBlockStoreAPI* block_store_api, ref NativeBlockStoreStats out_stats);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern int Longtail_ReadStoredBlockFromBuffer(void* buffer, UInt64 size, ref NativeStoredBlock* out_stored_block);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern int Longtail_ReadStoredBlock(NativeStorageAPI* storage_api, IntPtr path, ref NativeStoredBlock* out_stored_block);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern int Longtail_ValidateStore(NativeStoreIndex* store_index, NativeVersionIndex* version_index);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern int Longtail_WriteStoredBlock(NativeStorageAPI* storage_api, NativeStoredBlock* stored_block, IntPtr path);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern int Longtail_WriteStoredBlockToBuffer(NativeStoredBlock* stored_block, ref void* out_buffer, ref UInt64 out_size);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern void Longtail_StoredBlock_Dispose(NativeStoredBlock* stored_block);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern UInt64 Longtail_GetBlockStoreAPISize();

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern NativeBlockStoreAPI* Longtail_MakeBlockStoreAPI(
            void* mem,
            [MarshalAs(UnmanagedType.FunctionPtr)] Longtail_DisposeFunc dispose_func,
            [MarshalAs(UnmanagedType.FunctionPtr)] BlockStore_PutStoredBlockCallback put_stored_block_func,
            [MarshalAs(UnmanagedType.FunctionPtr)] BlockStore_PreflightGetCallback preflight_get_func,
            [MarshalAs(UnmanagedType.FunctionPtr)] BlockStore_GetStoredBlockCallback get_stored_block_func,
            [MarshalAs(UnmanagedType.FunctionPtr)] BlockStore_GetExistingContentCallback get_existing_content_func,
            [MarshalAs(UnmanagedType.FunctionPtr)] BlockStore_PruneBlocksCallback prune_blocks_func,
            [MarshalAs(UnmanagedType.FunctionPtr)] BlockStore_GetStatsCallback get_stats_func,
            [MarshalAs(UnmanagedType.FunctionPtr)] BlockStore_FlushCallback flush_func);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern UInt64 Longtail_GetCancelAPISize();

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern NativeCancelAPI* Longtail_MakeCancelAPI(
            void* mem,
            [MarshalAs(UnmanagedType.FunctionPtr)] Longtail_DisposeFunc dispose_func,
            [MarshalAs(UnmanagedType.FunctionPtr)] Longtail_CancelAPI_CreateTokenFunc create_token_func,
            [MarshalAs(UnmanagedType.FunctionPtr)] Longtail_CancelAPI_CancelFunc cancel_func,
            [MarshalAs(UnmanagedType.FunctionPtr)] Longtail_CancelAPI_IsCancelledFunc is_cancelled_func,
            [MarshalAs(UnmanagedType.FunctionPtr)] Longtail_CancelAPI_DisposeTokenFunc dispose_token_func);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern NativeProgressAPI* Longtail_CreateRateLimitedProgress(NativeProgressAPI* backing_block_store, UInt32 percent_rate_limit);
    }
}
