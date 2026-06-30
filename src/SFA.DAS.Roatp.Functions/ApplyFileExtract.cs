using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Roatp.Functions.Infrastructure.ApiClients;
using SFA.DAS.Roatp.Functions.Infrastructure.BlobStorage;
using SFA.DAS.Roatp.Functions.Requests;

namespace SFA.DAS.Roatp.Functions;

public class ApplyFileExtract
{
    private readonly ILogger<ApplyFileExtract> _logger;
    private readonly IQnaApiClient _qnaApiClient;
    private readonly IDatamartBlobStorageFactory _datamartBlobStorageFactory;

    public ApplyFileExtract(ILogger<ApplyFileExtract> log, IQnaApiClient qnaApiClient, IDatamartBlobStorageFactory datamartBlobStorageFactory)
    {
        _logger = log;
        _qnaApiClient = qnaApiClient;
        _datamartBlobStorageFactory = datamartBlobStorageFactory;
    }


    [Function("ApplyFileExtract")]
    public async Task Run([ServiceBusTrigger("%ApplyFileExtractQueue%", Connection = "DASServiceBusConnectionString")] ApplyFileExtractRequest fileToExtract)
    {
        _logger.LogDebug("Saving QnA file into Datamart for application {ApplicationId},  question: {QuestionId}, filename: {Filename}", fileToExtract.ApplicationId, fileToExtract.QuestionId, fileToExtract.Filename);

        var blobContainerClient = await _datamartBlobStorageFactory.GetQnABlobContainerClient();

        try
        {
            await using var filestream = await _qnaApiClient.DownloadFile(fileToExtract.ApplicationId, fileToExtract.SequenceNumber, fileToExtract.SectionNumber, fileToExtract.PageId, fileToExtract.QuestionId);
            var blobName = $"{fileToExtract.ApplicationId}/{fileToExtract.PageId}/{fileToExtract.QuestionId}/Apply/{fileToExtract.Filename}";

            var blobClient = blobContainerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(filestream, overwrite: true);

            _logger.LogInformation("Saved QnA file into Datamart for application {ApplicationId},  question: {QuestionId}, filename: {Filename}. Data-mart path: {BlobName}", fileToExtract.ApplicationId, fileToExtract.QuestionId, fileToExtract.Filename, blobName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to save QnA file into Datamart for application {ApplicationId} and question {QuestionId}, filename: {Filename}", fileToExtract.ApplicationId, fileToExtract.QuestionId, fileToExtract.Filename);
            throw;
        }
    }
}
