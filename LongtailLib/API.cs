using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace LongtailLib
{
    public delegate void AssertFunc(string expression, string file, int line);
    public delegate void LogFunc(int level, string message);
    public delegate bool PathFilterFunc(string rootPath, string assetPath, string assetName, bool isDir, ulong size, uint permissions);
    public delegate void ProgressFunc(UInt32 totalCount, UInt32 doneCount);

    public delegate void OnPutBlockComplete(Exception e);
    public delegate void OnGetBlockComplete(StoredBlock storedBlock, Exception e);
    public delegate void OnGetIndexComplete(ContentIndex contentIndex, Exception e);
    public delegate void OnRetargetContentComplete(ContentIndex contentIndex, Exception e);

    public struct BlockStoreStats
    {
        public ulong m_IndexGetCount;
        public ulong m_BlocksGetCount;
        public ulong m_BlocksPutCount;
        public ulong m_ChunksGetCount;
        public ulong m_ChunksPutCount;
        public ulong m_BytesGetCount;
        public ulong m_BytesPutCount;
        public ulong m_IndexGetRetryCount;
        public ulong m_BlockGetRetryCount;
        public ulong m_BlockPutRetryCount;
        public ulong m_IndexGetFailCount;
        public ulong m_BlockGetFailCount;
        public ulong m_BlockPutFailCount;
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
        void PreflightGet(UInt64 blockCount, UInt64[] blockHashes, uint[] blockRefCounts);
        void GetStoredBlock(UInt64 blockHash, OnGetBlockComplete completeCallback);
        void GetIndex(OnGetIndexComplete completeCallback);
        void RetargetContent(ContentIndex contentIndex, OnRetargetContentComplete completeCallback);
        BlockStoreStats GetStats();
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
    }

    public unsafe sealed class ContentIndex : IDisposable
    {
        SafeNativeMethods.NativeContentIndex* _Native;
        internal ContentIndex(SafeNativeMethods.NativeContentIndex* NativeContentIndex)
        {
            _Native = NativeContentIndex;
        }
        internal SafeNativeMethods.NativeContentIndex* Native
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
        public unsafe UInt32 MaxBlockSize
        {
            get { return _Native->GetMaxBlockSize(); }
        }
        public unsafe UInt32 MaxChunksPerBlock
        {
            get { return _Native->GetMaxChunksPerBlock(); }
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
                (void* context, int level, string str) =>
                {
                    try
                    {
                        m_LogFunc(level, str);
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
                (string expression, string file, int line) =>
                {
                    try
                    {
                        m_AssertFunc(expression, file, line);
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

        public static void ThrowExceptionFromErrno(string functionName, string extraInfo, int errno)
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
        public unsafe static void* Alloc(UInt64 size)
        {
            return SafeNativeMethods.Longtail_Alloc(size);
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
            SafeNativeMethods.NativeFileInfos* nativeFileInfos = null;
            int err = SafeNativeMethods.Longtail_GetFilesRecursively(
                storageAPI.Native,
                pathFilterAPI.Native,
                cancelHandle.Native,
                (IntPtr)cancelHandle.Native,    // We don't have a dedicated token
                rootPath,
                ref nativeFileInfos);
            cancelHandle.Dispose();
            pathFilterAPI.Dispose();
            if (err == 0)
            {
                return new FileInfos(nativeFileInfos);
            }
            ThrowExceptionFromErrno("GetFilesRecursively", rootPath, err);
            return null;
        }

        public unsafe static UInt32 FileInfosGetCount(FileInfos fileInfos)
        {
            if (fileInfos == null) { return 0; }
            var cFileInfos = fileInfos.Native;
            return SafeNativeMethods.Longtail_FileInfos_GetCount(cFileInfos);
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

        private unsafe sealed class WrappedAsyncGetIndexAPI : IDisposable
        {
            public WrappedAsyncGetIndexAPI()
            {
                UInt64 mem_size = SafeNativeMethods.Longtail_GetAsyncGetIndexAPISize();
                byte* mem = (byte*)API.Alloc(mem_size);
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
                    (SafeNativeMethods.NativeAsyncGetIndexAPI* async_complete_api, SafeNativeMethods.NativeContentIndex* content_index, int err) =>
                    {
                        this.err = err;
                        if (err == 0)
                        {
                            m_ContentIndex = new ContentIndex(content_index);
                        }
                        m_EventHandle.Set();
                        return 0;
                    };
                _Native = SafeNativeMethods.Longtail_MakeAsyncGetIndexAPI(mem, m_Dispose, m_ASyncCallback);
                m_EventHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
                m_ContentIndex = null;
            }
            public void Dispose()
            {
                if (_Native != null)
                {
                    SafeNativeMethods.Longtail_DisposeAPI((SafeNativeMethods.NativeAPI*)_Native);
                    _Native = null;
                }
            }
            internal SafeNativeMethods.NativeAsyncGetIndexAPI* Native
            {
                get { return this._Native; }
            }
            public ContentIndex Result
            {
                get { m_EventHandle.WaitOne();  return this.m_ContentIndex; }
            }
            public int Err
            {
                get { return this.err; }
            }

            SafeNativeMethods.Longtail_DisposeFunc m_Dispose;
            SafeNativeMethods.ASyncGetIndexCompleteCallback m_ASyncCallback;
            EventWaitHandle m_EventHandle;
            ContentIndex m_ContentIndex;
            int err = -1;
            SafeNativeMethods.NativeAsyncGetIndexAPI* _Native;
        };

        private unsafe sealed class WrappedAsyncRetargetContentAPI : IDisposable
        {
            public WrappedAsyncRetargetContentAPI()
            {
                UInt64 mem_size = SafeNativeMethods.Longtail_GetAsyncRetargetContentAPISize();
                byte* mem = (byte*)API.Alloc(mem_size);
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
                    (SafeNativeMethods.NativeAsyncRetargetContentAPI* async_complete_api, SafeNativeMethods.NativeContentIndex* content_index, int err) =>
                    {
                        this.err = err;
                        if (err == 0)
                        {
                            m_ContentIndex = new ContentIndex(content_index);
                        }
                        m_EventHandle.Set();
                        return 0;
                    };
                _Native = SafeNativeMethods.Longtail_MakeAsyncRetargetContentAPI(mem, m_Dispose, m_ASyncCallback);
                m_EventHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
                m_ContentIndex = null;
            }
            public void Dispose()
            {
                if (_Native != null)
                {
                    SafeNativeMethods.Longtail_DisposeAPI((SafeNativeMethods.NativeAPI*)_Native);
                    _Native = null;
                }
            }
            internal SafeNativeMethods.NativeAsyncRetargetContentAPI* Native
            {
                get { return this._Native; }
            }
            public ContentIndex Result
            {
                get { m_EventHandle.WaitOne();  return this.m_ContentIndex; }
            }
            public int Err
            {
                get { return this.err; }
            }

            SafeNativeMethods.Longtail_DisposeFunc m_Dispose;
            SafeNativeMethods.ASyncRetargetContentCompleteCallback m_ASyncCallback;
            EventWaitHandle m_EventHandle;
            ContentIndex m_ContentIndex;
            int err = -1;
            SafeNativeMethods.NativeAsyncRetargetContentAPI* _Native;
        };

        public unsafe static Task<ContentIndex> BlockStoreGetIndex(BlockStoreAPI blockStoreAPI)
        {
            if (blockStoreAPI == null) { throw new ArgumentException("BlockStoreGetIndex blockStoreAPI is null"); }
            return Task<ContentIndex>.Run(() =>
            {
                WrappedAsyncGetIndexAPI wrappedAsyncGetIndexAPI = new WrappedAsyncGetIndexAPI();
                int err = SafeNativeMethods.Longtail_BlockStore_GetIndex(
                    blockStoreAPI.Native,
                    wrappedAsyncGetIndexAPI.Native);
                ContentIndex contentIndex = wrappedAsyncGetIndexAPI.Result;
                if (err == 0)
                {
                    err = wrappedAsyncGetIndexAPI.Err;
                }
                wrappedAsyncGetIndexAPI.Dispose();
                if (err != 0)
                {
                    ThrowExceptionFromErrno("BlockStoreGetIndex", "", err);
                }
                return contentIndex;
            });
        }

        public unsafe static VersionIndex CreateVersionIndex(
            StorageAPI storageAPI,
            HashAPI hashAPI,
            JobAPI jobAPI,
            ProgressFunc progress,
            CancellationToken cancellationToken,
            string rootPath,
            FileInfos fileInfos,
            UInt32[] optional_assetTags,
            UInt32 maxChunkSize)
        {
            if (storageAPI == null) { throw new ArgumentException("CreateVersionIndex storageAPI is null"); }
            if (hashAPI == null) { throw new ArgumentException("CreateVersionIndex hashAPI is null"); }
            if (jobAPI == null) { throw new ArgumentException("CreateVersionIndex jobAPI is null"); }
            if (fileInfos == null) { throw new ArgumentException("CreateVersionIndex fileInfos is null"); }

            ProgressHandle progressHandle = new ProgressHandle(progress);
            CancelHandle cancelHandle = new CancelHandle(cancellationToken);

            var cStorageAPI = storageAPI.Native;
            var cHashAPI = hashAPI.Native;
            var cJobAPI = jobAPI.Native;
            var cProgressHandle = progressHandle.Native;
            var cCancelHandle = cancelHandle.Native;
            var cFileInfos = fileInfos.Native;

            SafeNativeMethods.NativeVersionIndex* nativeVersionIndex = null;
            int err = SafeNativeMethods.Longtail_CreateVersionIndex(
                cStorageAPI,
                cHashAPI,
                cJobAPI,
                cProgressHandle,
                cCancelHandle,
                (IntPtr)cCancelHandle,    // We don't have a dedicated token
                rootPath,
                cFileInfos,
                optional_assetTags,
                maxChunkSize,
                ref nativeVersionIndex);
            cancelHandle.Dispose();
            progressHandle.Dispose();
            if (err == 0)
            {
                return new VersionIndex(nativeVersionIndex);
            }
            ThrowExceptionFromErrno("CreateVersionIndex", rootPath, err);
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
            ThrowExceptionFromErrno("ReadVersionIndexFromBuffer", "", err);
            return null;
        }
        public unsafe static VersionIndex ReadVersionIndex(StorageAPI storageAPI, string path)
        {
            if (storageAPI == null) { throw new ArgumentException("ReadVersionIndex storageAPI is null"); }
            if (path == null) { throw new ArgumentException("ReadVersionIndex path is null"); }

            var cStorageAPI = storageAPI.Native;
            SafeNativeMethods.NativeVersionIndex* nativeVersionIndex = null;
            int err = SafeNativeMethods.Longtail_ReadVersionIndex(
                cStorageAPI,
                path,
                ref nativeVersionIndex);
            if (err == 0)
            {
                return new VersionIndex(nativeVersionIndex);
            }
            ThrowExceptionFromErrno("ReadVersionIndex", path, err);
            return null;
        }
        public unsafe static ContentIndex ReadContentIndexFromBuffer(byte[] buffer)
        {
            if (buffer == null) { throw new ArgumentException("ReadContentIndexFromBuffer buffer is null"); }

            SafeNativeMethods.NativeContentIndex* nativeContentIndex = null;
            GCHandle pinnedArray = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            IntPtr bufferPtr = pinnedArray.AddrOfPinnedObject();
            int err = SafeNativeMethods.Longtail_ReadContentIndexFromBuffer((void*)bufferPtr, (UInt64)buffer.Length, ref nativeContentIndex);
            pinnedArray.Free();
            if (err == 0)
            {
                return new ContentIndex(nativeContentIndex);
            }
            ThrowExceptionFromErrno("ReadContentIndexFromBuffer", "", err);
            return null;
        }
        public unsafe static ContentIndex ReadContentIndex(StorageAPI storageAPI, string path)
        {
            if (storageAPI == null) { throw new ArgumentException("ReadContentIndex storageAPI is null"); }
            if (path == null) { throw new ArgumentException("ReadContentIndex path is null"); }

            var cStorageAPI = storageAPI.Native;
            SafeNativeMethods.NativeContentIndex* nativeContentIndex = null;
            int err = SafeNativeMethods.Longtail_ReadContentIndex(
                cStorageAPI,
                path,
                ref nativeContentIndex);
            if (err == 0)
            {
                return new ContentIndex(nativeContentIndex);
            }
            ThrowExceptionFromErrno("ReadContentIndex", path, err);
            return null;
        }
        public unsafe static ContentIndex CreateMissingContent(HashAPI hashAPI, ContentIndex contentIndex, VersionIndex version, UInt32 maxBlockSize, UInt32 maxChunksPerBlock)
        {
            if (hashAPI == null) { throw new ArgumentException("CreateMissingContent hashAPI is null"); }
            if (contentIndex == null) { throw new ArgumentException("CreateMissingContent contentIndex is null"); }
            if (version == null) { throw new ArgumentException("CreateMissingContent version is null"); }

            var cHashAPI = hashAPI.Native;
            var cContentIndex = contentIndex.Native;
            var cVersion = version.Native;
            SafeNativeMethods.NativeContentIndex* nativeContentIndex = null;
            int err = SafeNativeMethods.Longtail_CreateMissingContent(
                cHashAPI,
                cContentIndex,
                cVersion,
                maxBlockSize,
                maxChunksPerBlock,
                ref nativeContentIndex);
            if (err == 0)
            {
                return new ContentIndex(nativeContentIndex);
            }
            ThrowExceptionFromErrno("CreateMissingContent", "", err);
            return null;
        }
        public unsafe static ContentIndex GetMissingContent(
            HashAPI hashAPI,
            ContentIndex referenceContentIndex,
            ContentIndex contentIndex)
        {
            if (hashAPI == null) { throw new ArgumentException("CreateMissingContent hashAPI is null"); }
            if (referenceContentIndex == null) { throw new ArgumentException("CreateMissingContent reference_content_index is null"); }
            if (contentIndex == null) { throw new ArgumentException("CreateMissingContent content_index is null"); }

            var cHashAPI = hashAPI.Native;
            var cReferenceContentIndex = referenceContentIndex.Native;
            var cContentIndex = contentIndex.Native;
            SafeNativeMethods.NativeContentIndex* nativeContentIndex = null;
            int err = SafeNativeMethods.Longtail_GetMissingContent(
                cHashAPI,
                cReferenceContentIndex,
                cContentIndex,
                ref nativeContentIndex);
            if (err == 0)
            {
                return new ContentIndex(nativeContentIndex);
            }
            ThrowExceptionFromErrno("GetMissingContent", "", err);
            return null;
        }

        public unsafe static ContentIndex CreateContentIndex(
            HashAPI hashAPI,
            VersionIndex versionIndex,
            UInt32 maxBlockSize,
            UInt32 maxChunksPerBlock)
        {
            if (hashAPI == null) { throw new ArgumentException("CreateContentIndex hashAPI is null"); }
            if (versionIndex == null) { throw new ArgumentException("CreateContentIndex versionIndex is null"); }
            if (maxBlockSize == 0) { throw new ArgumentException("CreateContentIndex maxBlockSize is 0"); }
            if (maxChunksPerBlock == 0) { throw new ArgumentException("CreateContentIndex maxChunksPerBlock is 0"); }

            var cHashAPI = hashAPI.Native;
            var cVersionIndex = versionIndex.Native;
            SafeNativeMethods.NativeContentIndex* nativeContentIndex = null;
            int err = SafeNativeMethods.Longtail_CreateContentIndex(
                cHashAPI,
                cVersionIndex,
                maxBlockSize,
                maxChunksPerBlock,
                ref nativeContentIndex);
            if (err == 0)
            {
                return new ContentIndex(nativeContentIndex);
            }
            ThrowExceptionFromErrno("CreateContentIndex", "", err);
            return null;
        }

        public unsafe static ContentIndex RetargetContent(ContentIndex referenceContentIndex, ContentIndex contentIndex)
        {
            if (referenceContentIndex == null) { throw new ArgumentException("RetargetContent referenceContentIndex is null"); }
            if (contentIndex == null) { throw new ArgumentException("RetargetContent contentIndex is null"); }

            var cReferenceContentIndex = referenceContentIndex.Native;
            var cContentIndex = contentIndex.Native;

            SafeNativeMethods.NativeContentIndex* nativeContentIndex = null;
            int err = SafeNativeMethods.Longtail_RetargetContent(
                cReferenceContentIndex,
                cContentIndex,
                ref nativeContentIndex);
            if (err == 0)
            {
                return new ContentIndex(nativeContentIndex);
            }
            ThrowExceptionFromErrno("RetargetContent", "", err);
            return null;
        }
        public unsafe static ContentIndex MergeContentIndex(ContentIndex localContentIndex, ContentIndex newContentIndex)
        {
            if (localContentIndex == null) { throw new ArgumentException("MergeContentIndex localContentIndex is null"); }
            if (newContentIndex == null) { throw new ArgumentException("MergeContentIndex newContentIndex is null"); }

            var cLocalContentIndex = localContentIndex.Native;
            var cNewIndex = newContentIndex.Native;

            SafeNativeMethods.NativeContentIndex* nativeContentIndex = null;
            int err = SafeNativeMethods.Longtail_MergeContentIndex(
                cLocalContentIndex,
                cNewIndex,
                ref nativeContentIndex);
            if (err == 0)
            {
                return new ContentIndex(nativeContentIndex);
            }
            ThrowExceptionFromErrno("MergeContentIndex", "", err);
            return null;
        }

        public unsafe static Task<ContentIndex> RetargetContentIndex(BlockStoreAPI blockStoreAPI, ContentIndex contentIndex)
        {
            if (blockStoreAPI == null) { throw new ArgumentException("RetargetContentIndex blockStoreAPI is null"); }
            if (contentIndex == null) { throw new ArgumentException("RetargetContentIndex contentIndex is null"); }
            return Task<ContentIndex>.Run(() =>
            {
                WrappedAsyncRetargetContentAPI wrappedAsyncRetargetContentAPI = new WrappedAsyncRetargetContentAPI();
                int err = SafeNativeMethods.Longtail_BlockStore_RetargetContent(
                    blockStoreAPI.Native,
                    contentIndex.Native,
                    wrappedAsyncRetargetContentAPI.Native);
                ContentIndex result = wrappedAsyncRetargetContentAPI.Result;
                if (err == 0)
                {
                    err = wrappedAsyncRetargetContentAPI.Err;
                }
                wrappedAsyncRetargetContentAPI.Dispose();
                if (err != 0)
                {
                    ThrowExceptionFromErrno("BlockStoreRetargetContent", "", err);
                }
                return result;
            });
        }
        public unsafe static UInt32 ContentIndexGetHashAPI(ContentIndex contentIndex)
        {
            if (contentIndex == null) { throw new ArgumentException("ContentIndexGetHashAPI contentIndex is null"); }

            var cContentIndex = contentIndex.Native;
            return SafeNativeMethods.Longtail_ContentIndex_GetHashAPI(cContentIndex);
        }
        public unsafe static UInt64 ContentIndexGetBlockCount(ContentIndex contentIndex)
        {
            if (contentIndex == null) { throw new ArgumentException("ContentIndexGetBlockCount contentIndex is null"); }

            var cContentIndex = contentIndex.Native;
            return SafeNativeMethods.Longtail_ContentIndex_GetBlockCount(cContentIndex);
        }
        public unsafe static UInt64[] ContentIndexBlockHashes(ContentIndex contentIndex)
        {
            if (contentIndex == null) { throw new ArgumentException("ContentIndexBlockHashes contentIndex is null"); }

            var cContentIndex = contentIndex.Native;
            UInt64 blockCount = ContentIndexGetBlockCount(contentIndex);
            UInt64* blockHashes = SafeNativeMethods.Longtail_ContentIndex_BlockHashes(cContentIndex);
            UInt64[] result = new UInt64[blockCount];
            for (UInt64 b = 0; b < blockCount; ++b)
            {
                result[b] = blockHashes[b];
            }
            return result;
        }
        public unsafe static VersionDiff CreateVersionDiff(VersionIndex sourceVersion, VersionIndex targetVersion)
        {
            if (sourceVersion == null) { throw new ArgumentException("CreateVersionDiff sourceVersion is null"); }
            if (targetVersion == null) { throw new ArgumentException("CreateVersionDiff targetVersion is null"); }

            var cSourceVersion = sourceVersion.Native;
            var cTargetVersion = targetVersion.Native;
            SafeNativeMethods.NativeVersionDiff* nativeVersionDiff = null;
            int err = SafeNativeMethods.Longtail_CreateVersionDiff(
                cSourceVersion,
                cTargetVersion,
                ref nativeVersionDiff);
            if (err == 0)
            {
                return new VersionDiff(nativeVersionDiff);
            }
            ThrowExceptionFromErrno("CreateVersionDiff", "", err);
            return null;
        }
        public unsafe static void ChangeVersion(
            BlockStoreAPI blockStoreAPI,
            StorageAPI versionStorageAPI,
            HashAPI hashAPI,
            JobAPI jobAPI,
            ProgressFunc progress,
            CancellationToken cancellationToken,
            ContentIndex contentIndex,
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
            if (contentIndex == null) { throw new ArgumentException("ChangeVersion contentIndex is null"); }
            if (sourceVersion == null) { throw new ArgumentException("ChangeVersion sourceVersion is null"); }
            if (targetVersion == null) { throw new ArgumentException("ChangeVersion targetVersion is null"); }
            if (versionDiff == null) { throw new ArgumentException("ChangeVersion versionDiff is null"); }

            ProgressHandle progressHandle = new ProgressHandle(progress);
            CancelHandle cancelHandle = new CancelHandle(cancellationToken);

            var cBlockStoreAPI = blockStoreAPI.Native;
            var cVersionStorageAPI = versionStorageAPI.Native;
            var cHashAPI = hashAPI.Native;
            var cJobAPI = jobAPI.Native;
            var cProgressHandle = progressHandle.Native;
            var cCancelHandle = cancelHandle.Native;
            var cContentIndex = contentIndex.Native;
            var cSourceVersion = sourceVersion.Native;
            var cTargetVersion = targetVersion.Native;
            var cVersionDiff = versionDiff.Native;

            int err = SafeNativeMethods.Longtail_ChangeVersion(
                cBlockStoreAPI,
                cVersionStorageAPI,
                cHashAPI,
                cJobAPI,
                cProgressHandle,
                cCancelHandle,
                (IntPtr)cCancelHandle,    // We don't have a dedicated token
                cContentIndex,
                cSourceVersion,
                cTargetVersion,
                cVersionDiff,
                versionPath,
                retainPermissions ? 1 : 0);
            cancelHandle.Dispose();
            progressHandle.Dispose();
            if (err == 0)
            {
                return;
            }
            ThrowExceptionFromErrno("ChangeVersion", versionPath, err);
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
            ThrowExceptionFromErrno("GetHashAPI", hashIdentifier.ToString(), err);
            return null;
        }
        public unsafe static CompressionRegistryAPI CreateFullCompressionRegistry()
        {
            return new CompressionRegistryAPI(SafeNativeMethods.Longtail_CreateFullCompressionRegistry());
        }
        public unsafe static BlockStoreAPI CreateFSBlockStoreAPI(StorageAPI storageAPI, string contentPath, UInt32 default_max_block_size, UInt32 default_max_chunks_per_block)
        {
            if (storageAPI == null) { throw new ArgumentException("CreateFSBlockStoreAPI storageAPI is null"); }
            if (contentPath == null) { throw new ArgumentException("CreateFSBlockStoreAPI contentPath is null"); }

            var cStorageAPI = storageAPI.Native;
            return new BlockStoreAPI(SafeNativeMethods.Longtail_CreateFSBlockStoreAPI(cStorageAPI, contentPath, default_max_block_size, default_max_chunks_per_block, null));
        }
        public unsafe static BlockStoreAPI CreateCacheBlockStoreAPI(BlockStoreAPI localBlockStore, BlockStoreAPI remoteBlockStore)
        {
            if (localBlockStore == null) { throw new ArgumentException("CreateCacheBlockStoreAPI localBlockStore is null"); }
            if (remoteBlockStore == null) { throw new ArgumentException("CreateCacheBlockStoreAPI remoteBlockStore is null"); }

            var cLocalBlockStore = localBlockStore.Native;
            var cRemoteBlockStore = remoteBlockStore.Native;
            return new BlockStoreAPI(SafeNativeMethods.Longtail_CreateCacheBlockStoreAPI(
                cLocalBlockStore,
                cRemoteBlockStore));
        }
        public unsafe static BlockStoreAPI CreateCompressBlockStoreAPI(BlockStoreAPI backingBlockStore, CompressionRegistryAPI compressionRegistry)
        {
            if (backingBlockStore == null) { throw new ArgumentException("CreateCompressBlockStoreAPI compressionRegistry is null"); }
            if (compressionRegistry == null) { throw new ArgumentException("CreateCompressBlockStoreAPI compressionRegistry is null"); }

            var cBackingBlockStore = backingBlockStore.Native;
            var cCompressionRegistry = compressionRegistry.Native;
            return new BlockStoreAPI(SafeNativeMethods.Longtail_CreateCompressBlockStoreAPI(
                cBackingBlockStore,
                cCompressionRegistry));
        }
        public unsafe static BlockStoreAPI CreateShareBlockStoreAPI(BlockStoreAPI backingBlockStore)
        {
            if (backingBlockStore == null) { throw new ArgumentException("CreateShareBlockStoreAPI compressionRegistry is null"); }

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
                BlockStoreStats outStats = new BlockStoreStats
                {
                    m_IndexGetCount = nativeStats.m_IndexGetCount,
                    m_BlocksGetCount = nativeStats.m_BlocksGetCount,
                    m_BlocksPutCount = nativeStats.m_BlocksPutCount,
                    m_ChunksGetCount = nativeStats.m_ChunksGetCount,
                    m_ChunksPutCount = nativeStats.m_ChunksPutCount,
                    m_BytesGetCount = nativeStats.m_BytesGetCount,
                    m_BytesPutCount = nativeStats.m_BytesPutCount,
                    m_IndexGetRetryCount = nativeStats.m_IndexGetRetryCount,
                    m_BlockGetRetryCount = nativeStats.m_BlockGetRetryCount,
                    m_BlockPutRetryCount = nativeStats.m_BlockPutRetryCount,
                    m_IndexGetFailCount = nativeStats.m_IndexGetFailCount,
                    m_BlockGetFailCount = nativeStats.m_BlockGetFailCount,
                    m_BlockPutFailCount = nativeStats.m_BlockPutFailCount
                };
                return outStats;
            }
            ThrowExceptionFromErrno("BlockStoreGetStats", "", err);
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
            ThrowExceptionFromErrno("ReadStoredBlockFromBuffer", "", err);
            return new StoredBlock(null);
        }

        public unsafe static StoredBlock ReadStoredBlock(StorageAPI storageAPI, string path)
        {
            if (storageAPI == null) { throw new ArgumentException("ReadStoredBlock storageAPI is null"); }
            if (path == null) { throw new ArgumentException("ReadStoredBlock path is null"); }

            var cStorageAPI = storageAPI.Native;
            SafeNativeMethods.NativeStoredBlock* nativeStoredBlock = null;
            int err = SafeNativeMethods.Longtail_ReadStoredBlock(
                cStorageAPI,
                path,
                ref nativeStoredBlock);
            if (err == 0)
            {
                return new StoredBlock(nativeStoredBlock);
            }
            ThrowExceptionFromErrno("ReadStoredBlockFromBuffer", path, err);
            return new StoredBlock(null);
        }

        public unsafe static int WriteStoredBlock(StorageAPI storageAPI, StoredBlock storedBlock, string path)
        {
            if (storageAPI == null) { throw new ArgumentException("WriteStoredBlock storageAPI is null"); }
            if (storedBlock == null) { throw new ArgumentException("WriteStoredBlock storedBlock is null"); }
            if (path == null) { throw new ArgumentException("WriteStoredBlock path is null"); }

            var cStorageAPI = storageAPI.Native;
            var cStoredBlock = storedBlock.Native;
            return SafeNativeMethods.Longtail_WriteStoredBlock(cStorageAPI, cStoredBlock, path);
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
                ThrowExceptionFromErrno("WriteStoredBlockToBuffer", "", err);
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
                byte* mem = (byte*)API.Alloc(mem_size);
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
                        (SafeNativeMethods.NativeBlockStoreAPI* block_store_api, UInt64 block_count, UInt64* block_hashes, UInt32* block_ref_counts) =>
                        {
                            try
                            {
                                UInt64[] blockHashes = new UInt64[block_count];
                                uint[] blockRefCounts = new uint[block_count];
                                for (UInt32 b = 0; b < block_count; b++)
                                {
                                    blockHashes[b] = block_hashes[b];
                                    blockRefCounts[b] = block_ref_counts[b];
                                }
                                m_BlockStore.PreflightGet(block_count, blockHashes, blockRefCounts);
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

                m_BlockStoreGetIndex =
                        (SafeNativeMethods.NativeBlockStoreAPI* block_store_api, SafeNativeMethods.NativeAsyncGetIndexAPI* async_complete_api) =>
                        {
                            try
                            {
                                m_BlockStore.GetIndex((ContentIndex contentIndex, Exception e) =>
                                {
                                    SafeNativeMethods.Longtail_AsyncGetIndex_OnComplete(async_complete_api, contentIndex.Native == null ? null : contentIndex.Native, API.GetErrnoFromException(e, SafeNativeMethods.EIO));
                                });
                            }
                            catch (Exception e)
                            {
                                int errno = API.GetErrnoFromException(e, SafeNativeMethods.EIO);
                                SafeNativeMethods.Longtail_AsyncGetIndex_OnComplete(async_complete_api, null, errno);
                            }
                            return 0;
                        };

                m_RetargetContent =
                        (SafeNativeMethods.NativeBlockStoreAPI* block_store_api, SafeNativeMethods.NativeContentIndex* native_content_index, SafeNativeMethods.NativeAsyncRetargetContentAPI* async_complete_api) =>
                        {
                            var content_index = new ContentIndex(native_content_index);
                            try
                            {
                                m_BlockStore.RetargetContent(content_index, (ContentIndex contentIndex, Exception e) =>
                                {
                                    SafeNativeMethods.Longtail_AsyncRetargetContent_OnComplete(async_complete_api, contentIndex.Native == null ? null : contentIndex.Native, API.GetErrnoFromException(e, SafeNativeMethods.EIO));
                                });
                            }
                            catch (Exception e)
                            {
                                int errno = API.GetErrnoFromException(e, SafeNativeMethods.EIO);
                                SafeNativeMethods.Longtail_AsyncRetargetContent_OnComplete(async_complete_api, null, errno);
                            }
                            finally
                            {
                                content_index.Detach();
                            }
                            return 0;
                        };

                m_BlockStoreGetStats =
                        (SafeNativeMethods.NativeBlockStoreAPI* block_store_api, ref SafeNativeMethods.NativeBlockStoreStats out_stats) =>
                        {
                            try
                            {
                                BlockStoreStats stats = m_BlockStore.GetStats();
                                out_stats.m_IndexGetCount = stats.m_IndexGetCount;
                                out_stats.m_BlocksGetCount = stats.m_BlocksGetCount;
                                out_stats.m_BlocksPutCount = stats.m_BlocksPutCount;
                                out_stats.m_ChunksGetCount = stats.m_ChunksGetCount;
                                out_stats.m_ChunksPutCount = stats.m_ChunksPutCount;
                                out_stats.m_BytesGetCount = stats.m_BytesGetCount;
                                out_stats.m_BytesPutCount = stats.m_BytesPutCount;
                                out_stats.m_IndexGetRetryCount = stats.m_IndexGetRetryCount;
                                out_stats.m_BlockGetRetryCount = stats.m_BlockGetRetryCount;
                                out_stats.m_BlockPutRetryCount = stats.m_BlockPutRetryCount;
                                out_stats.m_IndexGetFailCount = stats.m_IndexGetFailCount;
                                out_stats.m_BlockGetFailCount = stats.m_BlockGetFailCount;
                                out_stats.m_BlockPutFailCount = stats.m_BlockPutFailCount;
                                return 0;
                            }
                            catch (Exception e)
                            {
                                return API.GetErrnoFromException(e, SafeNativeMethods.ENOMEM);
                            }
                        };
                _Native = new BlockStoreAPI(
                    SafeNativeMethods.Longtail_MakeBlockStoreAPI(
                        mem,
                        m_Dispose,
                        m_BlockStorePutStoredBlock,
                        m_BlockStorePreflightGet,
                        m_BlockStoreGetStoredBlock,
                        m_BlockStoreGetIndex,
                        m_RetargetContent,
                        m_BlockStoreGetStats));
            }
            SafeNativeMethods.Longtail_DisposeFunc m_Dispose;
            SafeNativeMethods.BlockStore_PutStoredBlockCallback m_BlockStorePutStoredBlock;
            SafeNativeMethods.BlockStore_PreflightGetCallback m_BlockStorePreflightGet;
            SafeNativeMethods.BlockStore_GetStoredBlockCallback m_BlockStoreGetStoredBlock;
            SafeNativeMethods.BlockStore_GetIndexCallback m_BlockStoreGetIndex;
            SafeNativeMethods.BlockStore_RetargetContentCallback m_RetargetContent;
            SafeNativeMethods.BlockStore_GetStatsCallback m_BlockStoreGetStats;

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
                byte* mem = (byte*)API.Alloc(mem_size);
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
                    (SafeNativeMethods.NativeStorageAPI* storage_api, string path, ref IntPtr outOpenFile) =>
                    {
                        try
                        {
                            m_Storage.OpenReadFile(path, ref outOpenFile);
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
                    (SafeNativeMethods.NativeStorageAPI* storage_api, string path, UInt64 initialSize, ref IntPtr outOpenFile) =>
                    {
                        try
                        {
                            m_Storage.OpenWriteFile(path, initialSize, ref outOpenFile);
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
                    (SafeNativeMethods.NativeStorageAPI* storage_api, string path, UInt16 permissions) =>
                    {
                        try
                        {
                            m_Storage.SetPermissions(path, permissions);
                            return 0;
                        }
                        catch (Exception e)
                        {
                            int errno = API.GetErrnoFromException(e, SafeNativeMethods.EIO);
                            return errno;
                        }
                    };
                m_GetPermissionsFunc =
                    (SafeNativeMethods.NativeStorageAPI* storage_api, string path, ref UInt16 out_permissions) =>
                    {
                        try
                        {
                            out_permissions = m_Storage.GetPermissions(path);
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
                        catch (Exception e)
                        {
//                            Log.Information("StorageAPI::CloseFile failed with {@e}", e);
                        }
                    };
                m_CreateDirFunc =
                    (SafeNativeMethods.NativeStorageAPI* storage_api, string path) =>
                    {
                        try
                        {
                            m_Storage.CreateDir(path);
                            return 0;
                        }
                        catch (Exception e)
                        {
                            int errno = API.GetErrnoFromException(e, SafeNativeMethods.EIO);
                            return errno;
                        }
                    };
                m_RenameFileFunc =
                    (SafeNativeMethods.NativeStorageAPI* storage_api, string sourcePath, string targetPath) =>
                    {
                        try
                        {
                            m_Storage.RenameFile(sourcePath, targetPath);
                            return 0;
                        }
                        catch (Exception e)
                        {
                            int errno = API.GetErrnoFromException(e, SafeNativeMethods.EIO);
                            return errno;
                        }
                    };
                m_ConcatPathFunc =
                    (SafeNativeMethods.NativeStorageAPI* storage_api, string rootPath, string subPath) =>
                    {
                        try
                        {
                            string path = m_Storage.ConcatPath(rootPath, subPath);
                            return SafeNativeMethods.Longtail_Strdup(path);
                        }
                        catch (Exception e)
                        {
//                            Log.Information("StorageAPI::ConcatPath failed with {@e}", e);
                            return null;
                        }
                    };
                m_IsDirFunc =
                    (SafeNativeMethods.NativeStorageAPI* storage_api, string path) =>
                    {
                        try
                        {
                            bool isDir = m_Storage.IsDir(path);
                            return isDir ? 1 : 0;
                        }
                        catch (Exception e)
                        {
                            int errno = API.GetErrnoFromException(e, SafeNativeMethods.EIO);
                            return errno;
                        }
                    };
                m_IsFileFunc =
                    (SafeNativeMethods.NativeStorageAPI* storage_api, string path) =>
                    {
                        try
                        {
                            bool isFile = m_Storage.IsFile(path);
                            return isFile ? 1 : 0;
                        }
                        catch (Exception e)
                        {
                            int errno = API.GetErrnoFromException(e, SafeNativeMethods.EIO);
                            return errno;
                        }
                    };
                m_RemoveDirFunc =
                    (SafeNativeMethods.NativeStorageAPI* storage_api, string path) =>
                    {
                        try
                        {
                            m_Storage.RemoveDir(path);
                            return 0;
                        }
                        catch (Exception e)
                        {
                            int errno = API.GetErrnoFromException(e, SafeNativeMethods.EIO);
                            return errno;
                        }
                    };
                m_RemoveFileFunc =
                    (SafeNativeMethods.NativeStorageAPI* storage_api, string path) =>
                    {
                        try
                        {
                            m_Storage.RemoveFile(path);
                            return 0;
                        }
                        catch (Exception e)
                        {
                            int errno = API.GetErrnoFromException(e, SafeNativeMethods.EIO);
                            return errno;
                        }
                    };
                m_StartFindFunc =
                    (SafeNativeMethods.NativeStorageAPI* storage_api, string path, ref IntPtr outIterator) =>
                    {
                        try
                        {
                            bool hasEntries = m_Storage.StartFind(path, ref outIterator);
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
                                SafeNativeMethods.Longtail_Free((void*)oldNativeString);
                            }
                            m_Storage.CloseFind(iterator);
                        }
                        catch (Exception e)
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
                                SafeNativeMethods.Longtail_Free((void*)oldNativeString);
                            }
                            var properties = m_Storage.GetEntryProperties(iterator);
                            char* nativeString = SafeNativeMethods.Longtail_Strdup(properties.m_FileName);
                            IntPtr charPtr = (IntPtr)nativeString;
                            out_properties.m_FileName = nativeString;
                            out_properties.m_Size = properties.m_Size;
                            out_properties.m_Permissions = properties.m_Permissions;
                            out_properties.m_IsDir = properties.m_IsDir ? 1 : 0;
                            m_AllocatedStrings.TryAdd(iterator, charPtr);
                            return 0;
                        }
                        catch (Exception e)
                        {
//                            Log.Information("StorageAPI::GetFileName failed with {@e}", e);
                            return 0;
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
                    m_GetEntryPropertiesFunc));
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
            public ProgressHandle(ProgressFunc progressFunc)
            {
                if (progressFunc == null)
                {
                    return;
                }
                m_ProgressFunc = progressFunc;
                UInt64 mem_size = SafeNativeMethods.Longtail_GetProgressAPISize();
                byte* mem = (byte*)API.Alloc(mem_size);
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
                _Native = SafeNativeMethods.Longtail_MakeProgressAPI(mem, m_Dispose, m_ProgressCallback);

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
                byte* mem = (byte*)API.Alloc(mem_size);
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
                byte* mem = (byte*)API.Alloc(mem_size);
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
                    (SafeNativeMethods.NativePathFilterAPI* path_filter_api, string root_path, string asset_path, string asset_name, int is_dir, UInt64 size, UInt16 permissions) =>
                    {
                        try
                        {
                            bool result = m_PathFilterFunc(root_path, asset_path, asset_name, is_dir != 0, (ulong)size, (uint)permissions);
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
        public delegate void AssertCallback(string expression, string file, int line);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate void LogCallback(void* context, int level, string str);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate void Longtail_DisposeFunc(NativeAPI* api);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate void ProgressCallback(NativeProgressAPI* progress_api, UInt32 totalCount, UInt32 doneCount);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate int PathFilterCallback(NativePathFilterAPI* path_filter_api, string root_path, string asset_path, string asset_name, int is_dir, UInt64 size, UInt16 permissions);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate int ASyncPutStoredBlockCompleteCallback(NativeAsyncPutStoredBlockAPI* asyncCompleteAPI, int err);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate int ASyncGetStoredBlockCompleteCallback(NativeAsyncGetStoredBlockAPI* asyncCompleteAPI, NativeStoredBlock* stored_block, int err);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate int ASyncGetIndexCompleteCallback(NativeAsyncGetIndexAPI* asyncCompleteAPI, NativeContentIndex* content_index, int err);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate int ASyncRetargetContentCompleteCallback(NativeAsyncRetargetContentAPI* asyncCompleteAPI, NativeContentIndex* content_index, int err);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate int BlockStore_PutStoredBlockCallback(NativeBlockStoreAPI* block_store_api, NativeStoredBlock* stored_block, NativeAsyncPutStoredBlockAPI* async_complete_api);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate int BlockStore_PreflightGetCallback(SafeNativeMethods.NativeBlockStoreAPI* block_store_api, UInt64 block_count, UInt64* block_hashes, UInt32* block_ref_counts);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate int BlockStore_GetStoredBlockCallback(NativeBlockStoreAPI* block_store_api, UInt64 block_hash, NativeAsyncGetStoredBlockAPI* async_complete_api);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate int BlockStore_GetIndexCallback(NativeBlockStoreAPI* block_store_api, NativeAsyncGetIndexAPI* async_complete_api);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate int BlockStore_RetargetContentCallback(NativeBlockStoreAPI* block_store_api, NativeContentIndex* content_index, NativeAsyncRetargetContentAPI* async_complete_api);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate int BlockStore_GetStatsCallback(NativeBlockStoreAPI* block_store_api, ref NativeBlockStoreStats out_stats);

        public unsafe delegate int Longtail_Storage_OpenReadFileFunc(NativeStorageAPI* storage_api, string path, ref IntPtr out_open_file);
        public unsafe delegate int Longtail_Storage_GetSizeFunc(NativeStorageAPI* storage_api, IntPtr f, ref UInt64 out_size);
        public unsafe delegate int Longtail_Storage_ReadFunc(NativeStorageAPI* storage_api, IntPtr f, UInt64 offset, UInt64 length, void* output);
        public unsafe delegate int Longtail_Storage_OpenWriteFileFunc(NativeStorageAPI* storage_api, string path, UInt64 initial_size, ref IntPtr out_open_file);
        public unsafe delegate int Longtail_Storage_WriteFunc(NativeStorageAPI* storage_api, IntPtr f, UInt64 offset, UInt64 length, void* input);
        public unsafe delegate int Longtail_Storage_SetSizeFunc(NativeStorageAPI* storage_api, IntPtr f, UInt64 length);
        public unsafe delegate int Longtail_Storage_SetPermissionsFunc(NativeStorageAPI* storage_api, string path, UInt16 permissions);
        public unsafe delegate int Longtail_Storage_GetPermissionsFunc(NativeStorageAPI* storage_api, string path, ref UInt16 out_permissions);
        public unsafe delegate void Longtail_Storage_CloseFileFunc(NativeStorageAPI* storage_api, IntPtr f);
        public unsafe delegate int Longtail_Storage_CreateDirFunc(NativeStorageAPI* storage_api, string path);
        public unsafe delegate int Longtail_Storage_RenameFileFunc(NativeStorageAPI* storage_api, string source_path, string target_path);
        public unsafe delegate char* Longtail_Storage_ConcatPathFunc(NativeStorageAPI* storage_api, string root_path, string sub_path);
        public unsafe delegate int Longtail_Storage_IsDirFunc(NativeStorageAPI* storage_api, string path);
        public unsafe delegate int Longtail_Storage_IsFileFunc(NativeStorageAPI* storage_api, string path);
        public unsafe delegate int Longtail_Storage_RemoveDirFunc(NativeStorageAPI* storage_api, string path);
        public unsafe delegate int Longtail_Storage_RemoveFileFunc(NativeStorageAPI* storage_api, string path);
        public unsafe delegate int Longtail_Storage_StartFindFunc(NativeStorageAPI* storage_api, string path, ref IntPtr out_iterator);
        public unsafe delegate int Longtail_Storage_FindNextFunc(NativeStorageAPI* storage_api, IntPtr iterator);
        public unsafe delegate void Longtail_Storage_CloseFindFunc(NativeStorageAPI* storage_api, IntPtr iterator);
        public unsafe delegate int Longtail_Storage_GetEntryPropertiesFunc(NativeStorageAPI* storage_api, IntPtr iterator, ref NativeStorageAPIProperties out_properties);

        public unsafe delegate int Longtail_CancelAPI_CreateTokenFunc(NativeCancelAPI* cancel_api, ref IntPtr out_token);
        public unsafe delegate int Longtail_CancelAPI_CancelFunc(NativeCancelAPI* cancel_api, IntPtr token);
        public unsafe delegate int Longtail_CancelAPI_IsCancelledFunc(NativeCancelAPI* cancel_api, IntPtr token);
        public unsafe delegate int Longtail_CancelAPI_DisposeTokenFunc(NativeCancelAPI* cancel_api, IntPtr token);

        [DllImport(LongtailDLLName, CharSet = CharSet.Ansi)]
        internal unsafe static extern UInt64 Longtail_GetStorageAPISize();

        [DllImport(LongtailDLLName, CharSet = CharSet.Ansi)]
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
            Longtail_Storage_GetEntryPropertiesFunc get_entry_properties_func);

        internal unsafe struct NativeAPI { }
        internal unsafe struct NativeProgressAPI { }
        internal unsafe struct NativePathFilterAPI { }
        internal unsafe struct NativeAsyncPutStoredBlockAPI { }
        internal unsafe struct NativeAsyncGetStoredBlockAPI { }
        internal unsafe struct NativeAsyncGetIndexAPI { }
        internal unsafe struct NativeAsyncRetargetContentAPI { }

        internal unsafe struct NativeBlockStoreAPI { }
        internal unsafe struct NativeStorageAPI { }
        internal unsafe struct NativeHashAPI { }
        internal unsafe struct NativeJobAPI { }
        internal unsafe struct NativeHashRegistryAPI { }
        internal unsafe struct NativeCompressionRegistryAPI { }
        internal unsafe struct NativeCancelAPI { }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        internal unsafe struct NativeStorageAPIProperties
        {
            public char* m_FileName;
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
            char* m_PathData;
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
            char* m_NameData;

            public unsafe UInt32 GetHashIdentifier() { return *m_HashIdentifier; }
            public unsafe UInt32 GetTargetChunkSize() { return *m_TargetChunkSize; }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct NativeContentIndex
        {
            UInt32* m_Version;
            UInt32* m_HashIdentifier;
            UInt32* m_MaxBlockSize;
            UInt32* m_MaxChunksPerBlock;
            UInt64* m_BlockCount;
            UInt64* m_ChunkCount;
            UInt64* m_BlockHashes;
            UInt64* m_ChunkHashes;
            UInt64* m_ChunkBlockIndexes;

            public unsafe UInt32 GetHashIdentifier() { return *m_HashIdentifier; }
            public unsafe UInt32 GetMaxBlockSize() { return *m_MaxBlockSize; }
            public unsafe UInt32 GetMaxChunksPerBlock() { return *m_MaxChunksPerBlock; }
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
            internal UInt64 m_IndexGetCount;
            internal UInt64 m_BlocksGetCount;
            internal UInt64 m_BlocksPutCount;
            internal UInt64 m_ChunksGetCount;
            internal UInt64 m_ChunksPutCount;
            internal UInt64 m_BytesGetCount;
            internal UInt64 m_BytesPutCount;
            internal UInt64 m_IndexGetRetryCount;
            internal UInt64 m_BlockGetRetryCount;
            internal UInt64 m_BlockPutRetryCount;
            internal UInt64 m_IndexGetFailCount;
            internal UInt64 m_BlockGetFailCount;
            internal UInt64 m_BlockPutFailCount;
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
        internal unsafe static extern void* Longtail_Alloc(UInt64 size);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern void Longtail_Free(void* data);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal unsafe static extern char* Longtail_Strdup(string str);

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
        internal static extern UInt64 Longtail_GetAsyncRetargetContentAPISize();

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern NativeAsyncRetargetContentAPI* Longtail_MakeAsyncRetargetContentAPI(
            void* mem,
            [MarshalAs(UnmanagedType.FunctionPtr)] Longtail_DisposeFunc disposeFunc,
            [MarshalAs(UnmanagedType.FunctionPtr)] ASyncRetargetContentCompleteCallback asyncComplete_callback);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern void Longtail_AsyncRetargetContent_OnComplete(NativeAsyncRetargetContentAPI* aSyncCompleteAPI, NativeContentIndex* contentIndex, int res);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern UInt64 Longtail_GetAsyncGetIndexAPISize();

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern NativeAsyncGetIndexAPI* Longtail_MakeAsyncGetIndexAPI(
            void* mem,
            [MarshalAs(UnmanagedType.FunctionPtr)] Longtail_DisposeFunc disposeFunc,
            [MarshalAs(UnmanagedType.FunctionPtr)] ASyncGetIndexCompleteCallback asyncComplete_callback);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern void Longtail_AsyncGetIndex_OnComplete(NativeAsyncGetIndexAPI* aSyncCompleteAPI, NativeContentIndex* contentIndex, int res);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern UInt32 Longtail_FileInfos_GetCount(NativeFileInfos* file_infos);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern UInt64 Longtail_FileInfos_GetSize(NativeFileInfos* file_infos, UInt32 index);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern UInt32 Longtail_FileInfos_GetPermissions(NativeFileInfos* file_infos, UInt32 index);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal unsafe static extern int Longtail_GetFilesRecursively(
            NativeStorageAPI* storage_api,
            NativePathFilterAPI* path_filter_api,
            NativeCancelAPI* cancel_api,
            IntPtr cancel_token,
            [MarshalAs(UnmanagedType.LPStr)] string root_path,
            ref NativeFileInfos* out_file_infos);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal unsafe static extern int Longtail_CreateVersionIndex(
            NativeStorageAPI* storage_api,
            NativeHashAPI* hash_api,
            NativeJobAPI* job_api,
            NativeProgressAPI* progress_api,
            NativeCancelAPI* cancel_api,
            IntPtr cancel_token,
            [MarshalAs(UnmanagedType.LPStr)] string root_path,
            NativeFileInfos* file_infos,
            UInt32[] asset_tags,
            UInt32 max_chunk_size,
            ref NativeVersionIndex* out_version_index);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern int Longtail_ReadVersionIndexFromBuffer(void* buffer, UInt64 size, ref NativeVersionIndex* out_version_index);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal unsafe static extern int Longtail_ReadVersionIndex(
            NativeStorageAPI* storage_api,
            [MarshalAs(UnmanagedType.LPStr)] string path,
            ref NativeVersionIndex* out_version_index);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern int Longtail_ReadContentIndexFromBuffer(
            void* buffer,
            UInt64 size,
            ref NativeContentIndex* out_content_index);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal unsafe static extern int Longtail_ReadContentIndex(
            NativeStorageAPI* storage_api,
            [MarshalAs(UnmanagedType.LPStr)] string path,
            ref NativeContentIndex* out_content_index);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern int Longtail_CreateMissingContent(
            NativeHashAPI* hash_api,
            NativeContentIndex* content_index,
            NativeVersionIndex* version,
            UInt32 max_block_size,
            UInt32 max_chunks_per_block,
            ref NativeContentIndex* out_content_index);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern int Longtail_GetMissingContent(
            NativeHashAPI* hash_api,
            NativeContentIndex* reference_content_index,
            NativeContentIndex* content_index,
            ref NativeContentIndex* out_content_index);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern int Longtail_CreateContentIndex(
            NativeHashAPI* hash_api,
            NativeVersionIndex* version_index,
            UInt32 maxBlockSize,
            UInt32 maxChunksPerBlock,
            ref NativeContentIndex* out_content_index);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern int Longtail_RetargetContent(NativeContentIndex* reference_content_index, NativeContentIndex* content_index, ref NativeContentIndex* out_content_index);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern int Longtail_MergeContentIndex(NativeContentIndex* local_content_index, NativeContentIndex* new_content_index, ref NativeContentIndex* out_content_index);


        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern UInt32 Longtail_ContentIndex_GetHashAPI(NativeContentIndex* content_index);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern UInt64 Longtail_ContentIndex_GetBlockCount(NativeContentIndex* content_index);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern UInt64 Longtail_ContentIndex_GetChunkCount(NativeContentIndex* content_index);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern UInt64* Longtail_ContentIndex_BlockHashes(NativeContentIndex* content_index);


        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern int Longtail_CreateVersionDiff(
            NativeVersionIndex* source_version,
            NativeVersionIndex* target_version,
            ref NativeVersionDiff* out_version_diff);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal unsafe static extern int Longtail_ChangeVersion(
            NativeBlockStoreAPI* block_store_api,
            NativeStorageAPI* version_storage_api,
            NativeHashAPI* hash_api,
            NativeJobAPI* job_api,
            NativeProgressAPI* progress_api,
            NativeCancelAPI* cancel_api,
            IntPtr cancel_token,
            NativeContentIndex* content_index,
            NativeVersionIndex* source_version,
            NativeVersionIndex* target_version,
            NativeVersionDiff* version_diff,
            [MarshalAs(UnmanagedType.LPStr)] string version_path,
            int retain_permissions);


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
        internal unsafe static extern UInt32 Longtail_Job_GetWorkerCount(NativeJobAPI* job_api);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern NativeHashRegistryAPI* Longtail_CreateFullHashRegistry();

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern int Longtail_GetHashRegistry_GetHashAPI(NativeHashRegistryAPI* hash_registry, UInt32 hash_type, ref NativeHashAPI* out_hash_api);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern NativeCompressionRegistryAPI* Longtail_CreateFullCompressionRegistry();

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal unsafe static extern NativeBlockStoreAPI* Longtail_CreateFSBlockStoreAPI(NativeStorageAPI* storage_api, [MarshalAs(UnmanagedType.LPStr)] string content_path, UInt32 default_max_block_size, UInt32 default_max_chunks_per_block, [MarshalAs(UnmanagedType.LPStr)] string optional_block_extension);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern NativeBlockStoreAPI* Longtail_CreateCacheBlockStoreAPI(NativeBlockStoreAPI* local_block_store, NativeBlockStoreAPI* remote_block_store);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern NativeBlockStoreAPI* Longtail_CreateCompressBlockStoreAPI(NativeBlockStoreAPI* backing_block_store, NativeCompressionRegistryAPI* compression_registry);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern NativeBlockStoreAPI* Longtail_CreateShareBlockStoreAPI(NativeBlockStoreAPI* backing_block_store);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern int Longtail_BlockStore_PutStoredBlock(NativeBlockStoreAPI* block_store_api, NativeStoredBlock* stored_block, NativeAsyncPutStoredBlockAPI* async_complete_api);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe extern static int Longtail_BlockStore_GetStoredBlock(NativeBlockStoreAPI* block_store_api, UInt64 block_hash, NativeAsyncGetStoredBlockAPI* async_complete_api);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern int Longtail_BlockStore_GetIndex(NativeBlockStoreAPI* block_store_api, NativeAsyncGetIndexAPI* async_complete_api);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern int Longtail_BlockStore_RetargetContent(NativeBlockStoreAPI* block_store_api, NativeContentIndex* content_index, NativeAsyncRetargetContentAPI* async_complete_api);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern int Longtail_BlockStore_GetStats(NativeBlockStoreAPI* block_store_api, ref NativeBlockStoreStats out_stats);
        
        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern int Longtail_ReadStoredBlockFromBuffer(void* buffer, UInt64 size, ref NativeStoredBlock* out_stored_block);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal unsafe static extern int Longtail_ReadStoredBlock(NativeStorageAPI* storage_api, [MarshalAs(UnmanagedType.LPStr)] string path, ref NativeStoredBlock* out_stored_block);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal unsafe static extern int Longtail_WriteStoredBlock(NativeStorageAPI* storage_api, NativeStoredBlock* stored_block, [MarshalAs(UnmanagedType.LPStr)] string path);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal unsafe static extern int Longtail_WriteStoredBlockToBuffer(NativeStoredBlock* stored_block, ref void* out_buffer, ref UInt64 out_size);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern void Longtail_StoredBlock_Dispose(NativeStoredBlock* stored_block);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal unsafe static extern UInt64 Longtail_GetBlockStoreAPISize();

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal unsafe static extern NativeBlockStoreAPI* Longtail_MakeBlockStoreAPI(
            void* mem,
            [MarshalAs(UnmanagedType.FunctionPtr)] Longtail_DisposeFunc dispose_func,
            [MarshalAs(UnmanagedType.FunctionPtr)] BlockStore_PutStoredBlockCallback put_stored_block_func,
            [MarshalAs(UnmanagedType.FunctionPtr)] BlockStore_PreflightGetCallback preflight_get_func,
            [MarshalAs(UnmanagedType.FunctionPtr)] BlockStore_GetStoredBlockCallback get_stored_block_func,
            [MarshalAs(UnmanagedType.FunctionPtr)] BlockStore_GetIndexCallback get_index_func,
            [MarshalAs(UnmanagedType.FunctionPtr)] BlockStore_RetargetContentCallback retarget_content_func,
            [MarshalAs(UnmanagedType.FunctionPtr)] BlockStore_GetStatsCallback get_stats_func);

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern UInt64 Longtail_GetCancelAPISize();

        [DllImport(LongtailDLLName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal unsafe static extern NativeCancelAPI* Longtail_MakeCancelAPI(
            void* mem,
            [MarshalAs(UnmanagedType.FunctionPtr)] Longtail_DisposeFunc dispose_func,
            [MarshalAs(UnmanagedType.FunctionPtr)] Longtail_CancelAPI_CreateTokenFunc create_token_func,
            [MarshalAs(UnmanagedType.FunctionPtr)] Longtail_CancelAPI_CancelFunc cancel_func,
            [MarshalAs(UnmanagedType.FunctionPtr)] Longtail_CancelAPI_IsCancelledFunc is_cancelled_func,
            [MarshalAs(UnmanagedType.FunctionPtr)] Longtail_CancelAPI_DisposeTokenFunc dispose_token_func);
    }
}
