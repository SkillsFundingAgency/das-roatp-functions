using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.Roatp.Functions.ApplyTypes;
using SFA.DAS.Roatp.Functions.Infrastructure.ApiClients;
using SFA.DAS.Roatp.Functions.Infrastructure.BlobStorage;
using SFA.DAS.Roatp.Functions.Infrastructure.Databases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Roatp.Functions
{
    public class FileExtract
    {
        private readonly ILogger<FileExtract> _logger;
        private readonly ApplyDataContext _applyDataContext;
        private readonly IQnaApiClient _qnaApiClient;
        private readonly IDatamartBlobStorageFactory _datamartBlobStorageFactory;

        public FileExtract(ILogger<FileExtract> log, ApplyDataContext applyDataContext, IQnaApiClient qnaApiClient, IDatamartBlobStorageFactory datamartBlobStorageFactory)
        {
            _logger = log;
            _applyDataContext = applyDataContext;
            _qnaApiClient = qnaApiClient;
            _datamartBlobStorageFactory = datamartBlobStorageFactory;
        }


        [FunctionName("FileExtract")]
        public async Task Run([TimerTrigger("%FileExtractSchedule%")] TimerInfo myTimer)
        {
            if (myTimer.IsPastDue)
            {
                _logger.LogInformation("FileExtract function is running later than scheduled");
            }

            _logger.LogInformation($"FileExtract function executed at: {DateTime.Now}");

            var applications = await GetApplicationsToExtractQnaFiles();

            foreach (var applicationId in applications)
            {
                var filesToExtract = await GetQnaFilesToExtractForApplication(applicationId);
                await SaveQnaFilesIntoDatamartForApplication(applicationId, filesToExtract);
            }
        }

        public async Task<List<Guid>> GetApplicationsToExtractQnaFiles()
        {
            _logger.LogDebug($"Getting list of applications to extract QnA files for.");

            var applications = _applyDataContext.ExtractedApplications.AsNoTracking()
                                .Where(app => app.QnaFilesExtracted == false);

            return await applications.Select(app => app.ApplicationId).ToListAsync();
        }

        public async Task<List<SubmittedApplicationAnswer>> GetQnaFilesToExtractForApplication(Guid applicationId)
        {
            _logger.LogDebug($"Getting list QnA files to extract for application {applicationId}");

            return await _applyDataContext.SubmittedApplicationAnswers.AsNoTracking()
                            .Where(ans => ans.ApplicationId == applicationId && ans.QuestionType.ToUpper() == "FILEUPLOAD").ToListAsync();
        }

        public async Task SaveQnaFilesIntoDatamartForApplication(Guid applicationId, List<SubmittedApplicationAnswer> fileUploads)
        {
            _logger.LogDebug($"Saving QnA files into Datamart for application {applicationId}");

            var blobContainerClient = await _datamartBlobStorageFactory.GetQnABlobContainerClient();

            foreach (var file in fileUploads)
            {
                try
                {
                    using var filestream = await _qnaApiClient.DownloadFile(file.ApplicationId, file.SequenceNumber, file.SectionNumber, file.PageId, file.QuestionId);
                    var blobName = $"{file.ApplicationId}/{file.PageId}/{file.QuestionId}/Apply/{file.Answer}";

                    await blobContainerClient.UploadBlobAsync(blobName, filestream);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Unable to save QnA file into Datamart for application {applicationId} and question {file.QuestionId}");
                }
            }

            var application = await _applyDataContext.ExtractedApplications.FirstAsync(app => app.ApplicationId == applicationId);
            application.QnaFilesExtracted = true;
            await _applyDataContext.SaveChangesAsync();
        }
    }
}
