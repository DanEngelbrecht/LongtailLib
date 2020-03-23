using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LongtailLibTest
{
    [TestClass]
    public unsafe class UnitTestBlockStoreAPI
    {
        [TestMethod]
        public unsafe void TestCreateFSBlockStoreAPI()
        {
            LongtailLib.Longtail_StorageAPI* storage_api = LongtailLib.API.Longtail_CreateInMemStorageAPI();
            LongtailLib.Longtail_BlockStoreAPI* block_store_api = LongtailLib.API.Longtail_CreateFSBlockStoreAPI(storage_api, "store");
            LongtailLib.API.Longtail_DisposeAPI((LongtailLib.Longtail_API*)block_store_api);
            LongtailLib.API.Longtail_DisposeAPI((LongtailLib.Longtail_API*)storage_api);
        }

        [TestMethod]
        public unsafe void TestCreateCreateCacheBlockStoreAPI()
        {
            LongtailLib.Longtail_StorageAPI* remote_storage_api = LongtailLib.API.Longtail_CreateInMemStorageAPI();
            LongtailLib.Longtail_StorageAPI* local_storage_api = LongtailLib.API.Longtail_CreateInMemStorageAPI();
            LongtailLib.Longtail_BlockStoreAPI* remote_block_store_api = LongtailLib.API.Longtail_CreateFSBlockStoreAPI(remote_storage_api, "remote");
            LongtailLib.Longtail_BlockStoreAPI* local_block_store_api = LongtailLib.API.Longtail_CreateFSBlockStoreAPI(local_storage_api, "local");
            LongtailLib.Longtail_BlockStoreAPI* block_store_api = LongtailLib.API.Longtail_CreateCacheBlockStoreAPI(local_block_store_api, remote_block_store_api);
            LongtailLib.API.Longtail_DisposeAPI((LongtailLib.Longtail_API*)block_store_api);
            LongtailLib.API.Longtail_DisposeAPI((LongtailLib.Longtail_API*)local_block_store_api);
            LongtailLib.API.Longtail_DisposeAPI((LongtailLib.Longtail_API*)remote_block_store_api);
            LongtailLib.API.Longtail_DisposeAPI((LongtailLib.Longtail_API*)local_storage_api);
            LongtailLib.API.Longtail_DisposeAPI((LongtailLib.Longtail_API*)remote_storage_api);
        }

        [TestMethod]
        public unsafe void TestCreateCreateCompressBlockStoreAPI()
        {
            LongtailLib.Longtail_CompressionRegistryAPI* compression_registry = LongtailLib.API.Longtail_CreateCompressionRegistry();
            LongtailLib.Longtail_StorageAPI* backing_storage_api = LongtailLib.API.Longtail_CreateInMemStorageAPI();
            LongtailLib.Longtail_BlockStoreAPI* backing_block_store_api = LongtailLib.API.Longtail_CreateFSBlockStoreAPI(backing_storage_api, "compressed");
            LongtailLib.Longtail_BlockStoreAPI* block_store_api = LongtailLib.API.Longtail_CreateCompressBlockStoreAPI(backing_block_store_api, compression_registry);
            LongtailLib.API.Longtail_DisposeAPI((LongtailLib.Longtail_API*)block_store_api);
            LongtailLib.API.Longtail_DisposeAPI((LongtailLib.Longtail_API*)backing_block_store_api);
            LongtailLib.API.Longtail_DisposeAPI((LongtailLib.Longtail_API*)backing_storage_api);
            LongtailLib.API.Longtail_DisposeAPI((LongtailLib.Longtail_API*)compression_registry);
        }

        [TestMethod]
        public unsafe void TestBlockStoreGetIndex()
        {
            LongtailLib.Longtail_JobAPI* job_api = LongtailLib.API.Longtail_CreateBikeshedJobAPI(4);
            LongtailLib.Longtail_StorageAPI* storage_api = LongtailLib.API.Longtail_CreateInMemStorageAPI();
            LongtailLib.Longtail_BlockStoreAPI* block_store_api = LongtailLib.API.Longtail_CreateFSBlockStoreAPI(storage_api, "store");
            LongtailLib.Longtail_ContentIndex* content_index = null;
            Assert.AreEqual(0, LongtailLib.API.Longtail_BlockStore_GetIndex(block_store_api, job_api, 0, null, ref content_index));
            LongtailLib.API.Longtail_Free(content_index);
            LongtailLib.API.Longtail_DisposeAPI((LongtailLib.Longtail_API*)block_store_api);
            LongtailLib.API.Longtail_DisposeAPI((LongtailLib.Longtail_API*)storage_api);
            LongtailLib.API.Longtail_DisposeAPI((LongtailLib.Longtail_API*)job_api);
        }
    }
}
