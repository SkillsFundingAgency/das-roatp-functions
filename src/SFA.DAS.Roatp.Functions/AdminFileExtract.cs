using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Roatp.Functions.Infrastructure.ApiClients;
using SFA.DAS.Roatp.Functions.Infrastructure.BlobStorage;
using SFA.DAS.Roatp.Functions.Requests;

namespace SFA.DAS.Roatp.Functions
{
    public class AdminFileExtract
    {
        private readonly ILogger<AdminFileExtract> _logger;
        private readonly IApplyApiClient _applyApiClient;
        private readonly IDatamartBlobStorageFactory _datamartBlobStorageFactory;

        public AdminFileExtract(ILogger<AdminFileExtract> log, IApplyApiClient applyApiClient, IDatamartBlobStorageFactory datamartBlobStorageFactory)
        {
            _logger = log;
            _applyApiClient = applyApiClient;
            _datamartBlobStorageFactory = datamartBlobStorageFactory;
        }

        [Function("AdminFileExtract")]
        public async Task Run([ServiceBusTrigger("%AdminFileExtractQueue%", Connection = "DASServiceBusConnectionString")] string messageContent)
        {
            AdminFileExtractRequest fileToExtract = JsonConvert.DeserializeObject<AdminFileExtractRequest>(messageContent);

            _logger.LogDebug($"Saving {fileToExtract.AdminFileType} clarification file into Datamart for application {fileToExtract.ApplicationId},  page: {fileToExtract.PageId}, filename: {fileToExtract.Filename}");

            try
            {
                switch (fileToExtract.AdminFileType)
                {
                    case AdminFileType.Gateway:
                        await ExtractGatewayFile(fileToExtract);
                        break;
                    case AdminFileType.Assessor:
                        await ExtractAssessorFile(fileToExtract);
                        break;
                    case AdminFileType.Finance:
                        await ExtractFinanceFile(fileToExtract);
                        break;
                    default:
                        throw new NotImplementedException($"{fileToExtract.AdminFileType} is not yet supported");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to save {fileToExtract.AdminFileType} clarification file into Datamart for application {fileToExtract.ApplicationId} and page {fileToExtract.PageId}, filename: {fileToExtract.Filename}");
                throw;
            }
        }

        private async Task ExtractGatewayFile(AdminFileExtractRequest fileToExtract)
        {
            var blobContainerClient = await _datamartBlobStorageFactory.GetAdminBlobContainerClient();

            await using var filestream = await _applyApiClient.DownloadGatewaySubcontractorDeclarationClarificationFile(fileToExtract.ApplicationId, fileToExtract.Filename);
            var blobName = $"{fileToExtract.ApplicationId}/{fileToExtract.PageId}/{fileToExtract.AdminFileType}/{fileToExtract.Filename}";

            var blobClient = blobContainerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(filestream, overwrite: true);

            _logger.LogInformation($"Saved {fileToExtract.AdminFileType} clarification file into Datamart for application {fileToExtract.ApplicationId},  page: {fileToExtract.PageId}, filename: {fileToExtract.Filename}. Data-mart path: {blobName}");
        }

        private async Task ExtractAssessorFile(AdminFileExtractRequest fileToExtract)
        {
            var blobContainerClient = await _datamartBlobStorageFactory.GetAdminBlobContainerClient();

            await using var filestream = await _applyApiClient.DownloadAssessorClarificationFile(fileToExtract.ApplicationId, fileToExtract.SequenceNumber, fileToExtract.SectionNumber, fileToExtract.PageId, fileToExtract.Filename);
            var blobName = $"{fileToExtract.ApplicationId}/{fileToExtract.PageId}/{fileToExtract.AdminFileType}/{fileToExtract.Filename}";

            var blobClient = blobContainerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(filestream, overwrite: true);

            _logger.LogInformation($"Saved {fileToExtract.AdminFileType} clarification file into Datamart for application {fileToExtract.ApplicationId},  page: {fileToExtract.PageId}, filename: {fileToExtract.Filename}. Data-mart path: {blobName}");
        }

        private async Task ExtractFinanceFile(AdminFileExtractRequest fileToExtract)
        {
            var blobContainerClient = await _datamartBlobStorageFactory.GetAdminBlobContainerClient();

            await using var filestream = await _applyApiClient.DownloadFinanceClarificationFile(fileToExtract.ApplicationId, fileToExtract.Filename);
            var blobName = $"{fileToExtract.ApplicationId}/{fileToExtract.PageId}/{fileToExtract.AdminFileType}/{fileToExtract.Filename}";

            var blobClient = blobContainerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(filestream, overwrite: true);

            _logger.LogInformation($"Saved {fileToExtract.AdminFileType} clarification file into Datamart for application {fileToExtract.ApplicationId},  page: {fileToExtract.PageId}, filename: {fileToExtract.Filename}. Data-mart path: {blobName}");
        }
    }
}