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
    public class AssessorExtract
    {
        private readonly ILogger<AssessorExtract> _logger;
        private readonly ApplyDataContext _applyDataContext;

        public AssessorExtract(ILogger<AssessorExtract> log, ApplyDataContext applyDataContext)
        {
            _logger = log;
            _applyDataContext = applyDataContext;
        }

        [FunctionName("AssessorExtract")]
        public async Task Run([TimerTrigger("%AssessorExtractSchedule%")] TimerInfo myTimer,
            [ServiceBus("%AdminFileExtractQueue%", Connection = "DASServiceBusConnectionString", EntityType = EntityType.Queue)] IAsyncCollector<AdminFileExtractRequest> clarificationFileExtractQueue)
        {
            if (myTimer.IsPastDue)
            {
                _logger.LogInformation("AssessorExtract function is running later than scheduled");
            }

            _logger.LogInformation($"AssessorExtract function executed at: {DateTime.Now}");

            var applications = await GetApplicationsToExtract();

            foreach (var application in applications)
            {
                await EnqueueAssessorFilesForExtract(clarificationFileExtractQueue, application);
                await MarkAssessorFilesExtractedForApplication(application.ApplicationId);
            }
        }

        public async Task<List<Apply>> GetApplicationsToExtract()
        {
            _logger.LogDebug($"Getting list of applications to extract");

            var applications = await _applyDataContext.Apply
                                .AsNoTracking()
                                .Include(x => x.ExtractedApplication)
                                .Include(x => x.AssessorClarificationOutcomes)
                                .Where(app => app.ExtractedApplication != null && !app.ExtractedApplication.AssessorFilesExtracted)
                                .Where(app => app.AssessorReviewStatus == "Approved" || app.AssessorReviewStatus == "Declined")
                                .ToListAsync();

            return applications;
        }

        public async Task EnqueueAssessorFilesForExtract(IAsyncCollector<AdminFileExtractRequest> clarificationFileExtractQueue, Apply application)
        {
            _logger.LogDebug($"Enqueuing assessor files for extract for application {application.ApplicationId}");

            if (application.AssessorClarificationOutcomes != null)
            {
                var clarificationFiles = application.AssessorClarificationOutcomes.Where(x => x.ClarificationFile != null);

                foreach (var file in clarificationFiles)
                {
                    await clarificationFileExtractQueue.AddAsync(new AdminFileExtractRequest(file));
                }
            } 
        }

        public async Task MarkAssessorFilesExtractedForApplication(Guid applicationId)
        {
            _logger.LogDebug($"Marking AssessorFilesExtracted for application {applicationId}");

            try
            {
                var application = _applyDataContext.ExtractedApplications.Single(ans => ans.ApplicationId == applicationId);
                application.AssessorFilesExtracted = true;

                await _applyDataContext.SaveChangesAsync();
                _logger.LogInformation($"Successfully marked AssessorFilesExtracted for application {applicationId}");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"Unable to mark AssessorFilesExtracted for Application: {applicationId}");
            }
        }
    }
}