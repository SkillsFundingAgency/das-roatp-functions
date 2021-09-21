using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.Roatp.Functions.ApplyTypes;
using SFA.DAS.Roatp.Functions.Infrastructure.Databases;
using SFA.DAS.Roatp.Functions.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Roatp.Functions
{
    public class AppealExtract
    {
        private readonly ILogger<AppealExtract> _logger;
        private readonly ApplyDataContext _applyDataContext;

        public AppealExtract(ILogger<AppealExtract> log, ApplyDataContext applyDataContext)
        {
            _logger = log;
            _applyDataContext = applyDataContext;
        }

        [FunctionName("AppealExtract")]
        public async Task Run([TimerTrigger("%AppealExtractSchedule%")] TimerInfo myTimer,
            [ServiceBus("%AppealFileExtractQueue%", Connection = "DASServiceBusConnectionString", EntityType = EntityType.Queue)] IAsyncCollector<AppealFileExtractRequest> appealFileExtractQueue)
        {
            if (myTimer.IsPastDue)
            {
                _logger.LogInformation("AppealExtract function is running later than scheduled");
            }

            _logger.LogInformation($"AppealExtract function executed at: {DateTime.Now}");

            var appeals = await GetAppealsToExtract();

            foreach (var appeal in appeals)
            {
                await EnqueueAppealFilesForExtract(appealFileExtractQueue, appeal);
                await MarkAppealFilesExtractedForApplication(appeal.ApplicationId);
            }
        }

        public async Task<List<Appeal>> GetAppealsToExtract()
        {
            _logger.LogDebug($"Getting list of applications to extract");

            var applications = await _applyDataContext.Apply
                                .AsNoTracking()
                                .Include(x => x.ExtractedApplication)
                                .Include(x => x.Appeal)
                                .Include(x => x.Appeal.AppealFiles)
                                .Where(app => app.ExtractedApplication != null && !app.ExtractedApplication.AppealFilesExtracted)
                                .Where(app => app.Appeal.AppealSubmittedDate != null)
                                .ToListAsync();

            return applications.Select(ap => ap.Appeal).ToList();
        }

        public async Task EnqueueAppealFilesForExtract(IAsyncCollector<AppealFileExtractRequest> appealFileExtractQueue, Appeal appeal)
        {
            _logger.LogDebug($"Enqueuing appeal files for extract for application {appeal.ApplicationId}");

            if (appeal.AppealFiles != null)
            {
                var appealFiles = appeal.AppealFiles.Where(x => x.FileName != null);

                foreach (var file in appealFiles)
                {
                    await appealFileExtractQueue.AddAsync(new AppealFileExtractRequest(file));
                }
            }
        }

        public async Task MarkAppealFilesExtractedForApplication(Guid applicationId)
        {
            _logger.LogDebug($"Marking AppealFilesExtracted for application {applicationId}");

            try
            {
                var application = _applyDataContext.ExtractedApplications.Single(ans => ans.ApplicationId == applicationId);
                application.AppealFilesExtracted = true;

                await _applyDataContext.SaveChangesAsync();
                _logger.LogInformation($"Successfully marked AppealFilesExtracted for application {applicationId}");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"Unable to mark AppealFilesExtracted for Application: {applicationId}");
            }
        }
    }
}