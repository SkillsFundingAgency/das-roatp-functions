using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using SFA.DAS.Roatp.Functions.Configuration;
using System.Threading.Tasks;

namespace SFA.DAS.Roatp.Functions.Infrastructure.BlobStorage
{
    public interface IDatamartBlobStorageFactory
    {
        Task<BlobContainerClient> GetQnABlobContainerClient();
        Task<BlobContainerClient> GetAdminBlobContainerClient();
        Task<BlobContainerClient> GetAppealBlobContainerClient();
    }

    public class DatamartBlobStorageFactory : IDatamartBlobStorageFactory
    {
        private const string BLOB_CONTAINER_NAME = "roatpapply";
        private readonly string _datamartBlobStorageConnectionString;

        public DatamartBlobStorageFactory(IOptions<ConnectionStrings> connectionStrings)
        {
            _datamartBlobStorageConnectionString = connectionStrings.Value.DatamartBlobStorageConnectionString;
        }

        public async Task<BlobContainerClient> GetQnABlobContainerClient()
        {
            var client = new BlobContainerClient(_datamartBlobStorageConnectionString, BLOB_CONTAINER_NAME);
            await client.CreateIfNotExistsAsync();
            return client;
        }

        public async Task<BlobContainerClient> GetAdminBlobContainerClient()
        {
            var client = new BlobContainerClient(_datamartBlobStorageConnectionString, BLOB_CONTAINER_NAME);
            await client.CreateIfNotExistsAsync();
            return client;
        }

        public async Task<BlobContainerClient> GetAppealBlobContainerClient()
        {
            var client = new BlobContainerClient(_datamartBlobStorageConnectionString, BLOB_CONTAINER_NAME);
            await client.CreateIfNotExistsAsync();
            return client;
        }
    }
}
