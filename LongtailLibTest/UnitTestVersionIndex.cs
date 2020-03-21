using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LongtailLibTest
{
    [TestClass]
    public class UnitTestVersionIndex
    {
        [TestMethod]
        public unsafe void TestCreateVersionIndex()
        {
            Longtail.LogCallback myLogCallBack =
                (void* context, int level, string str) =>
                {
                    Console.WriteLine(str);
                };
            Longtail.Lib.Longtail_SetLogLevel(0);
            Longtail.Lib.Longtail_SetLog(myLogCallBack, null);
            Longtail.Longtail_StorageAPI* storage_api = Longtail.Lib.Longtail_CreateFSStorageAPI();
            Longtail.Longtail_HashAPI* hash_api = Longtail.Lib.Longtail_CreateBlake3HashAPI();
            Longtail.Longtail_JobAPI* job_api = Longtail.Lib.Longtail_CreateBikeshedJobAPI(4);
            Longtail.Longtail_FileInfos* file_infos = null;
            int err = Longtail.Lib.Longtail_GetFilesRecursively(storage_api, "sample_folder", ref file_infos);
            Assert.AreEqual(0, err);
            Longtail.Longtail_VersionIndex* version_index = null;
            uint file_count = Longtail.Lib.Longtail_FileInfos_GetPathCount(file_infos);
            Assert.AreEqual((uint)18, file_count);
            uint[] asset_tags = new uint[file_count];
            for (uint i = 0; i < file_count; ++i)
            {
                asset_tags[i] = 0;
            }
            err = Longtail.Lib.Longtail_CreateVersionIndexUtil(
                storage_api,
                hash_api,
                job_api,
                null,
                "sample_folder",
                file_infos,
                asset_tags,
                512,
                ref version_index);
            Assert.AreEqual(0, err);
            Longtail.Lib.Longtail_Free(version_index);
            Longtail.Lib.Longtail_Free(file_infos);
            Longtail.Lib.Longtail_DisposeAPI((Longtail.Longtail_API*)job_api);
            Longtail.Lib.Longtail_DisposeAPI((Longtail.Longtail_API*)hash_api);
            Longtail.Lib.Longtail_DisposeAPI((Longtail.Longtail_API*)storage_api);
            Longtail.Lib.Longtail_SetLog(null, null);
        }
    }
}
