using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Roatp.Functions.Infrastructure.ApiClients;
using SFA.DAS.Roatp.Functions.Infrastructure.BlobStorage;
using SFA.DAS.Roatp.Functions.Infrastructure.Databases;

namespace SFA.DAS.Roatp.Functions
{
    public class ApplyFileExtract
    {
        private readonly ILogger<FileExtract> _logger;
        private readonly ApplyDataContext _applyDataContext;
        private readonly IQnaApiClient _qnaApiClient;
        private readonly IDatamartBlobStorageFactory _datamartBlobStorageFactory;

        public ApplyFileExtract(ILogger<FileExtract> log, ApplyDataContext applyDataContext, IQnaApiClient qnaApiClient, IDatamartBlobStorageFactory datamartBlobStorageFactory)
        {
            if (log != null)
            {
                log.LogInformation($"FileExtract within constructor." +
                                   $"ApplyDataContext is { (applyDataContext != null ? "not null" : "null")}. " +
                                   $"QnaApiClient is { (qnaApiClient != null ? "not null" : "null")}. " +
                                   $"DatamartBlobStorageFactory is { (datamartBlobStorageFactory != null ? "not null" : "null")}.");
            }
            else
            {
                Console.WriteLine("FileExtract within constructor BUT logger is null.");
            }

            _logger = log;
            _applyDataContext = applyDataContext;
            _qnaApiClient = qnaApiClient;
            _datamartBlobStorageFactory = datamartBlobStorageFactory;
        }


        [FunctionName("ApplyFileExtract")]
        public async Task Run([ServiceBusTrigger("SFA.DAS.Roatp.Functions.ApplyFileExtract", Connection = "DASServiceBusConnectionString")] QnAFileDownload fileToExtract)
        {

            _logger.LogDebug($"Saving QnA file into Datamart for application {fileToExtract.ApplicationId},  question: {fileToExtract.QuestionId}, filename: {fileToExtract.Filename}");

            var blobContainerClient = await _datamartBlobStorageFactory.GetQnABlobContainerClient();

            try
            {
                await using var filestream = await _qnaApiClient.DownloadFile(fileToExtract.ApplicationId, 
                    fileToExtract.SequenceNumber, 
                    fileToExtract.SectionNumber, 
                    fileToExtract.PageId,
                    fileToExtract.QuestionId);
                var blobName = $"{fileToExtract.ApplicationId}/{fileToExtract.PageId}/{fileToExtract.QuestionId}/Apply/{fileToExtract.Filename}";

                await blobContainerClient.UploadBlobAsync(blobName, filestream);
                _logger.LogInformation($"Saved QnA file into Datamart for application {fileToExtract.ApplicationId},  question: {fileToExtract.QuestionId}, filename: {fileToExtract.Filename}. Data-mart path: {blobName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to save QnA file into Datamart for application {fileToExtract.ApplicationId} and question {fileToExtract.QuestionId}, filename: {fileToExtract.Filename}");
            }
        }
    }
}