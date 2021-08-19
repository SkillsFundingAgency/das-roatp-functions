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
    public class GatewayExtract
    {
        private readonly ILogger<GatewayExtract> _logger;
        private readonly ApplyDataContext _applyDataContext;

        public GatewayExtract(ILogger<GatewayExtract> log, ApplyDataContext applyDataContext)
        {
            _logger = log;
            _applyDataContext = applyDataContext;
        }

        //[FunctionName("GatewayExtract")]
        public async Task Run([TimerTrigger("%GatewayExtractSchedule%")] TimerInfo myTimer,
            [ServiceBus("%AdminFileExtractQueue%", Connection = "DASServiceBusConnectionString", EntityType = EntityType.Queue)] IAsyncCollector<AdminFileExtractRequest> clarificationFileExtractQueue)
        {
            if (myTimer.IsPastDue)
            {
                _logger.LogInformation("GatewayExtract function is running later than scheduled");
            }

            _logger.LogInformation($"GatewayExtract function executed at: {DateTime.Now}");

            var applications = await GetApplicationsToExtract();

            foreach (var application in applications)
            {
                await EnqueueGatewayFilesForExtract(clarificationFileExtractQueue, application);
                await MarkGatewayFilesExtractedForApplication(application.ApplicationId);
            }
        }

        public async Task<List<Apply>> GetApplicationsToExtract()
        {
            _logger.LogDebug($"Getting list of applications to extract");

            var applications = await _applyDataContext.Apply
                                .AsNoTracking()
                                .Include(x => x.ExtractedApplication)
                                .Where(app => app.ExtractedApplication != null && !app.ExtractedApplication.GatewayFilesExtracted)
                                .Where(app => app.GatewayReviewStatus == "Pass" || app.GatewayReviewStatus == "Fail" || app.GatewayReviewStatus == "Rejected")
                                .ToListAsync();

            return applications;
        }

        public async Task EnqueueGatewayFilesForExtract(IAsyncCollector<AdminFileExtractRequest> clarificationFileExtractQueue, Apply application)
        {
            _logger.LogDebug($"Enqueuing gateway files for extract for application {application.ApplicationId}");

            if (application.ApplyData?.GatewayReviewDetails?.GatewaySubcontractorDeclarationClarificationUpload != null)
            {
                await clarificationFileExtractQueue.AddAsync(new AdminFileExtractRequest(application.ApplicationId, application.ApplyData.GatewayReviewDetails));
            }
        }

        public async Task MarkGatewayFilesExtractedForApplication(Guid applicationId)
        {
            _logger.LogDebug($"Marking GatewayFilesExtracted for application {applicationId}");

            try
            {
                var application = _applyDataContext.ExtractedApplications.Single(ans => ans.ApplicationId == applicationId);
                application.GatewayFilesExtracted = true;

                await _applyDataContext.SaveChangesAsync();
                _logger.LogInformation($"Successfully marked GatewayFilesExtracted for application {applicationId}");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"Unable to mark GatewayFilesExtracted for Application: {applicationId}");
            }
        }
    }
}