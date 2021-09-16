using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Roatp.Functions.Infrastructure.ApiClients;
using SFA.DAS.Roatp.Functions.Infrastructure.BlobStorage;
using SFA.DAS.Roatp.Functions.Requests;

namespace SFA.DAS.Roatp.Functions
{
    public class AppealFileExtract
    {
        private readonly ILogger<AppealFileExtract> _logger;
        private readonly IApplyApiClient _applyApiClient;
        private readonly IDatamartBlobStorageFactory _datamartBlobStorageFactory;

        public AppealFileExtract(ILogger<AppealFileExtract> log, IApplyApiClient applyApiClient, IDatamartBlobStorageFactory datamartBlobStorageFactory)
        {
            _logger = log;
            _applyApiClient = applyApiClient;
            _datamartBlobStorageFactory = datamartBlobStorageFactory;
        }

        [FunctionName("AppealFileExtract")]
        public async Task Run([ServiceBusTrigger("%AppealFileExtractQueue%", Connection = "DASServiceBusConnectionString")] AppealFileExtractRequest fileToExtract)
        {
            _logger.LogDebug($"Saving appeal file into Datamart for application {fileToExtract.ApplicationId} and filename: {fileToExtract.FileName}");

            var blobContainerClient = await _datamartBlobStorageFactory.GetAppealBlobContainerClient();

            try
            {
                await using var filestream = await _applyApiClient.DownloadAppealFile(fileToExtract.ApplicationId, fileToExtract.FileName);
                var blobName = $"{fileToExtract.ApplicationId}/Appeal/{fileToExtract.FileName}";

                var blobClient = blobContainerClient.GetBlobClient(blobName);
                await blobClient.UploadAsync(filestream, overwrite: true);

                _logger.LogInformation($"Saved appeal file into Datamart for application {fileToExtract.ApplicationId} and filename: {fileToExtract.FileName}. Data-mart path: {blobName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to save appeal file into Datamart for application {fileToExtract.ApplicationId} and filename: {fileToExtract.FileName}");
                throw;
            }
        }
    }
}