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
    public class FinanceExtract
    {
        private readonly ILogger<FinanceExtract> _logger;
        private readonly ApplyDataContext _applyDataContext;

        public FinanceExtract(ILogger<FinanceExtract> log, ApplyDataContext applyDataContext)
        {
            _logger = log;
            _applyDataContext = applyDataContext;
        }

        [FunctionName("FinanceExtract")]
        public async Task Run([TimerTrigger("%FinanceExtractSchedule%")] TimerInfo myTimer,
            [ServiceBus("%AdminFileExtractQueue%", Connection = "DASServiceBusConnectionString", EntityType = EntityType.Queue)] IAsyncCollector<AdminFileExtractRequest> clarificationFileExtractQueue)
        {
            if (myTimer.IsPastDue)
            {
                _logger.LogInformation("FinanceExtract function is running later than scheduled");
            }

            _logger.LogInformation($"FinanceExtract function executed at: {DateTime.Now}");

            var applications = await GetApplicationsToExtract();

            foreach (var application in applications)
            {
                await EnqueueFinanceFilesForExtract(clarificationFileExtractQueue, application);
                await MarkFinanceFilesExtractedForApplication(application.ApplicationId);
            }
        }

        public async Task<List<Apply>> GetApplicationsToExtract()
        {
            _logger.LogDebug($"Getting list of applications to extract");

            var applications = await _applyDataContext.Apply
                                .AsNoTracking()
                                .Include(x => x.ExtractedApplication)
                                .Where(app => app.ExtractedApplication != null && !app.ExtractedApplication.FinanceFilesExtracted)
                                .Where(app => app.FinancialReviewStatus == "Pass" || app.FinancialReviewStatus == "Fail" || app.FinancialReviewStatus == "Exempt")
                                .ToListAsync();

            return applications;
        }

        public async Task EnqueueFinanceFilesForExtract(IAsyncCollector<AdminFileExtractRequest> clarificationFileExtractQueue, Apply application)
        {
            _logger.LogDebug($"Enqueuing finance files for extract for application {application.ApplicationId}");

            if (application.FinancialGrade?.ClarificationFiles != null)
            {
                var clarificationFiles = application.FinancialGrade.ClarificationFiles.Where(x => x.Filename != null);

                foreach (var file in clarificationFiles)
                {
                    await clarificationFileExtractQueue.AddAsync(new AdminFileExtractRequest(application.ApplicationId, file));
                }
            }
        }

        public async Task MarkFinanceFilesExtractedForApplication(Guid applicationId)
        {
            _logger.LogDebug($"Marking FinanceFilesExtracted for application {applicationId}");

            try
            {
                var application = _applyDataContext.ExtractedApplications.Single(ans => ans.ApplicationId == applicationId);
                application.FinanceFilesExtracted = true;

                await _applyDataContext.SaveChangesAsync();
                _logger.LogInformation($"Successfully marked FinanceFilesExtracted for application {applicationId}");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"Unable to mark FinanceFilesExtracted for Application: {applicationId}");
            }
        }
    }
}