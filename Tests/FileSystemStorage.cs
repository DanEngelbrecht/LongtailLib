using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Runtime.InteropServices;
using System.Threading;

namespace LongtailLib
{
    public class FileSystemStorage : IStorage, IDisposable
    {
        public FileSystemStorage(IFileSystem fileSystem)
        {
            m_FileSystem = fileSystem;
        }

        public long OpenFileCount
        {
            get { return this.m_OpenFileCount; }
        }
        public long BytesRead
        {
            get { return this.m_BytesRead; }
        }
        public long BytesWritten
        {
            get { return this.m_BytesWritten; }
        }

        void IStorage.OpenReadFile(string path, ref IntPtr outOpenFile)
        {
            try
            {
                string nativePath = DenormalizePath(path);
                Stream s = m_FileSystem.File.OpenRead(nativePath);
                GCHandle pinned = GCHandle.Alloc(s, GCHandleType.Normal);
                IntPtr rawPtr = GCHandle.ToIntPtr(pinned);
                var openFile = new OpenFile { m_Stream = s, m_Pinned = pinned };
                if (!m_OpenFiles.TryAdd(rawPtr, openFile))
                {
                    openFile.m_Pinned.Free();
                    openFile.m_Stream.Close();
                    throw new IOException("File `" + path + "` is already open");
                }
                outOpenFile = rawPtr;

                Interlocked.Add(ref m_OpenFileCount, 1);
            }
            catch(IOException e)
            {
                throw;
            }
        }

        void IStorage.GetSize(IntPtr f, ref ulong outSize)
        {
            OpenFile openFile;
            if (!m_OpenFiles.TryGetValue(f, out openFile))
            {
                throw new ArgumentException();
            }
            var s = openFile.m_Stream;
            outSize = (ulong)s.Length;
        }

        void IStorage.Read(IntPtr f, ulong offset, ulong length, byte[] output)
        {
            OpenFile openFile;
            if (!m_OpenFiles.TryGetValue(f, out openFile))
            {
                throw new ArgumentException();
            }
            var s = openFile.m_Stream;
            try
            {
                s.Seek((long)offset, SeekOrigin.Begin);
                s.Read(output, 0, (int)length);

                Interlocked.Add(ref m_BytesRead, (long)length);
            }
            catch (IOException e)
            {
                throw;
            }
        }

        void IStorage.OpenWriteFile(string path, ulong initialSize, ref IntPtr outOpenFile)
        {
            try
            {
                string nativePath = DenormalizePath(path);
                Stream s = m_FileSystem.File.OpenWrite(nativePath);
                s.SetLength((long)initialSize);
                GCHandle pinned = GCHandle.Alloc(s, GCHandleType.Normal);
                IntPtr rawPtr = GCHandle.ToIntPtr(pinned);
                var openFile = new OpenFile { m_Stream = s, m_Pinned = pinned };
                if (!m_OpenFiles.TryAdd(rawPtr, openFile))
                {
                    openFile.m_Pinned.Free();
                    openFile.m_Stream.Close();
                    throw new IOException("File `" + path + "` is already open");
                }
                outOpenFile = rawPtr;
                Interlocked.Add(ref m_OpenFileCount, 1);
            }
            catch (IOException e)
            {
                throw;
            }
        }

        void IStorage.Write(IntPtr f, ulong offset, ulong length, byte[] input)
        {
            OpenFile openFile;
            if (!m_OpenFiles.TryGetValue(f, out openFile))
            {
                throw new ArgumentException();
            }
            var s = openFile.m_Stream;
            try
            {
                s.Seek((long)offset, SeekOrigin.Begin);
                s.Write(input, 0, (int)length);

                Interlocked.Add(ref m_BytesWritten, (long)length);
            }
            catch (IOException e)
            {
                throw;
            }
        }

        void IStorage.SetSize(IntPtr f, ulong length)
        {
            OpenFile openFile;
            if (!m_OpenFiles.TryGetValue(f, out openFile))
            {
                throw new ArgumentException();
            }
            try
            {
                var s = openFile.m_Stream;
                s.SetLength((long)length);
            }
            catch (IOException e)
            {
                throw;
            }
        }

        const ushort Longtail_StorageAPI_OtherExecuteAccess = 0001;
        const ushort Longtail_StorageAPI_OtherWriteAccess = 0002;
        const ushort Longtail_StorageAPI_OtherReadAccess = 0004;

        const ushort Longtail_StorageAPI_GroupExecuteAccess = 0010;
        const ushort Longtail_StorageAPI_GroupWriteAccess = 0020;
        const ushort Longtail_StorageAPI_GroupReadAccess = 0040;

        const ushort Longtail_StorageAPI_UserExecuteAccess = 0100;
        const ushort Longtail_StorageAPI_UserWriteAccess = 0200;
        const ushort Longtail_StorageAPI_UserReadAccess = 0400;

        void IStorage.SetPermissions(string path, ushort permissions)
        {
            string nativePath = DenormalizePath(path);
            var fileInfo = m_FileSystem.FileInfo.FromFileName(nativePath);
            if (!fileInfo.Exists)
            {
                return;
            }
            if ((permissions & (Longtail_StorageAPI_OtherWriteAccess | Longtail_StorageAPI_GroupWriteAccess | Longtail_StorageAPI_UserWriteAccess)) == 0)
            {
                if ((fileInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    fileInfo.Attributes |= FileAttributes.ReadOnly;
                    m_FileSystem.File.SetAttributes(nativePath, fileInfo.Attributes);
                }
            }
        }

        ushort IStorage.GetPermissions(string path)
        {
            string nativePath = DenormalizePath(path);
            var fileInfo = m_FileSystem.FileInfo.FromFileName(nativePath);
            if (!fileInfo.Exists)
            {
                return 0;
            }
            ushort permissions = Longtail_StorageAPI_UserReadAccess | Longtail_StorageAPI_GroupReadAccess | Longtail_StorageAPI_OtherReadAccess;
            if ((fileInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                permissions |= (Longtail_StorageAPI_UserExecuteAccess | Longtail_StorageAPI_GroupExecuteAccess | Longtail_StorageAPI_OtherExecuteAccess);
            }
            if ((fileInfo.Attributes & FileAttributes.ReadOnly) == 0)
            {
                permissions |= Longtail_StorageAPI_UserWriteAccess | Longtail_StorageAPI_GroupWriteAccess | Longtail_StorageAPI_OtherWriteAccess;
            }
            return permissions;
        }

        void IStorage.CloseFile(IntPtr f)
        {
            OpenFile openFile;
            if (!m_OpenFiles.TryRemove(f, out openFile))
            {
                throw new ArgumentException();
            }
            openFile.m_Pinned.Free();
            openFile.m_Stream.Close();
            Interlocked.Add(ref m_OpenFileCount, -1);
        }

        void IStorage.CreateDir(string path)
        {
            string nativePath = DenormalizePath(path);
            m_FileSystem.Directory.CreateDirectory(nativePath);
        }

        void IStorage.RenameFile(string sourcePath, string targetPath)
        {
            try
            {
                string nativeSourcePath = DenormalizePath(sourcePath);
                string nativeTargetPath = DenormalizePath(targetPath);
                m_FileSystem.File.Move(nativeSourcePath, nativeTargetPath);
            }
            catch (IOException e)
            {
                throw;
            }
        }

        string IStorage.ConcatPath(string rootPath, string subPath)
        {
            // Due to bug in longtail (subPath starts with \\) we do a simple concatenation
            string parentPath = DenormalizePath(rootPath);
            string childPath = DenormalizePath(subPath);
            string fullPath = m_FileSystem.Path.Combine(parentPath, childPath);
            string normalizedPath = NormalizePath(fullPath);
            return normalizedPath;
        }

        bool IStorage.IsDir(string path)
        {
            return IsPathDir(DenormalizePath(path));
        }

        bool IStorage.IsFile(string path)
        {
            string nativePath = DenormalizePath(path);
            var fileInfo = m_FileSystem.FileInfo.FromFileName(nativePath);
            if (fileInfo.Exists && ((fileInfo.Attributes & FileAttributes.Directory) == 0))
            {
                return true;
            }
            return false;
        }

        void IStorage.RemoveDir(string path)
        {
            string nativePath = DenormalizePath(path);
            m_FileSystem.Directory.Delete(nativePath);
        }

        void IStorage.RemoveFile(string path)
        {
            string nativePath = DenormalizePath(path);
            m_FileSystem.File.Delete(nativePath);
        }

        bool IStorage.StartFind(string path, ref IntPtr outIterator)
        {
            if (path.Length > 0 && (!IsPathDir(DenormalizePath(path))))
            {
                return false;
            }
            string nativePath = DenormalizePath(path);
            var iterator = m_FileSystem.Directory.EnumerateFileSystemEntries(nativePath);
            var enumerator = iterator.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                return false;
            }
            string item = enumerator.Current;
            GCHandle pinned = GCHandle.Alloc(iterator, GCHandleType.Normal);
            IntPtr rawPtr = GCHandle.ToIntPtr(pinned);
            var openIterator = new OpenIterator { m_Iterator = enumerator, m_Pinned = pinned };
            if (!m_OpenIterators.TryAdd(rawPtr, openIterator))
            {
                openIterator.m_Pinned.Free();
                openIterator.m_Iterator.Dispose();
                throw new IOException("Cannot scan the path `" + path + "`");
            }
            outIterator = rawPtr;
            return true;
        }

        bool IStorage.FindNext(IntPtr iterator)
        {
            OpenIterator openIterator;
            if (!m_OpenIterators.TryGetValue(iterator, out openIterator))
            {
                throw new ArgumentException();
            }
            IEnumerator<string> it = openIterator.m_Iterator;
            return it.MoveNext();
        }

        void IStorage.CloseFind(IntPtr iterator)
        {
            OpenIterator openIterator;
            if (!m_OpenIterators.TryRemove(iterator, out openIterator))
            {
                throw new ArgumentException();
            }
            openIterator.m_Pinned.Free();
            openIterator.m_Iterator.Dispose();
        }

        IteratorEntryProperties IStorage.GetEntryProperties(IntPtr iterator)
        {
            OpenIterator openIterator;
            if (!m_OpenIterators.TryGetValue(iterator, out openIterator))
            {
                throw new ArgumentException();
            }

            IEnumerator<string> it = openIterator.m_Iterator;
            string fullPath = it.Current;

            bool isDir = IsPathDir(fullPath);
            ulong size;
            ushort permissions = 0;
            string fileName;

            if (isDir)
            {
                var dirInfo = m_FileSystem.DirectoryInfo.FromDirectoryName(fullPath);
                size = (ulong)0;
                fileName = dirInfo.Name;
                permissions |= (Longtail_StorageAPI_UserExecuteAccess | Longtail_StorageAPI_GroupExecuteAccess | Longtail_StorageAPI_OtherExecuteAccess);
                if ((dirInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    permissions |= Longtail_StorageAPI_UserWriteAccess | Longtail_StorageAPI_GroupWriteAccess | Longtail_StorageAPI_OtherWriteAccess;
                }
            }
            else
            {
                var fileInfo = m_FileSystem.FileInfo.FromFileName(fullPath);
                size = (ulong)fileInfo.Length;
                fileName = fileInfo.Name;
                permissions |= Longtail_StorageAPI_UserWriteAccess | Longtail_StorageAPI_GroupWriteAccess | Longtail_StorageAPI_OtherWriteAccess;
                if ((fileInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    permissions |= Longtail_StorageAPI_UserWriteAccess | Longtail_StorageAPI_GroupWriteAccess | Longtail_StorageAPI_OtherWriteAccess;
                }
            }

            return new IteratorEntryProperties
            {
                m_FileName = fileName,
                m_Size = size,
                m_Permissions = permissions,
                m_IsDir = isDir
            };
        }

        public void Dispose()
        {
            if (!m_OpenIterators.IsEmpty)
            {
                throw new IOException("There are still file iterators open");
            }
            if (!m_OpenFiles.IsEmpty)
            {
                throw new IOException("There are still files open");
            }
        }

        static string NormalizePath(string path)
        {
            return path.Replace("\\", "/");
        }

        static string DenormalizePath(string path)
        {
            return path.Replace("/", "\\");
        }

        bool IsPathDir(string path)
        {
            if (!m_FileSystem.Directory.Exists(path))
            {
                return false;
            }
            var attributes = m_FileSystem.File.GetAttributes(path);
            bool isDir = (attributes & FileAttributes.Directory) == FileAttributes.Directory;
            return isDir;
        }

        IFileSystem m_FileSystem;

        struct OpenFile
        {
            public Stream m_Stream;
            public GCHandle m_Pinned;
        };
        class OpenIterator
        {
            public IEnumerator<string> m_Iterator;
            public GCHandle m_Pinned;
        };

        ConcurrentDictionary<IntPtr, OpenFile> m_OpenFiles = new ConcurrentDictionary<IntPtr, OpenFile>();
        ConcurrentDictionary<IntPtr, OpenIterator> m_OpenIterators = new ConcurrentDictionary<IntPtr, OpenIterator>();
        long m_BytesWritten;
        long m_BytesRead;
        long m_OpenFileCount;
    }
}
