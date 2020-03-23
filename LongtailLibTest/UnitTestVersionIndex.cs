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
            LongtailLib.LogCallback myLogCallBack =
                (void* context, int level, string str) =>
                {
                    Console.WriteLine(str);
                };
            LongtailLib.API.Longtail_SetLogLevel(0);
            LongtailLib.API.Longtail_SetLog(myLogCallBack, null);
            LongtailLib.Longtail_StorageAPI* storage_api = LongtailLib.API.Longtail_CreateFSStorageAPI();
            LongtailLib.Longtail_HashAPI* hash_api = LongtailLib.API.Longtail_CreateBlake3HashAPI();
            LongtailLib.Longtail_JobAPI* job_api = LongtailLib.API.Longtail_CreateBikeshedJobAPI(4);
            LongtailLib.Longtail_FileInfos* file_infos = null;
            int err = LongtailLib.API.Longtail_GetFilesRecursively(storage_api, "D:\\github\\DanEngelbrecht\\LongtailLib\\LongtailLibTest\\TestData\\sample_folder", ref file_infos);
            Assert.AreEqual(0, err);
            LongtailLib.Longtail_VersionIndex* version_index = null;
            LongtailLib.Longtail_Paths* paths = LongtailLib.API.Longtail_FileInfos_GetPaths(file_infos);
            uint path_count = LongtailLib.API.Longtail_Paths_GetCount(paths);
            Assert.AreEqual((uint)18, path_count);
            uint[] asset_tags = new uint[path_count];
            for (uint i = 0; i < path_count; ++i)
            {
                asset_tags[i] = 0;
            }
            err = LongtailLib.API.Longtail_CreateVersionIndex(
                storage_api,
                hash_api,
                job_api,
                null,
                "D:\\github\\DanEngelbrecht\\LongtailLib\\LongtailLibTest\\TestData\\sample_folder",
                file_infos,
                asset_tags,
                512,
                ref version_index);
            Assert.AreEqual(0, err);
            LongtailLib.API.Longtail_Free(version_index);
            LongtailLib.API.Longtail_Free(file_infos);
            LongtailLib.API.Longtail_DisposeAPI((LongtailLib.Longtail_API*)job_api);
            LongtailLib.API.Longtail_DisposeAPI((LongtailLib.Longtail_API*)hash_api);
            LongtailLib.API.Longtail_DisposeAPI((LongtailLib.Longtail_API*)storage_api);
            LongtailLib.API.Longtail_SetLog(null, null);
        }
    }
}
