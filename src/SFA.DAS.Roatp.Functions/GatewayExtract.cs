using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Roatp.Functions.ApplyTypes;
using SFA.DAS.Roatp.Functions.Infrastructure.Databases;
using SFA.DAS.Roatp.Functions.Requests;

namespace SFA.DAS.Roatp.Functions
{
    public class GatewayExtract
    {
        private readonly ILogger<GatewayExtract> _logger;
        private readonly ApplyDataContext _applyDataContext;
        private readonly ServiceBusClient _serviceBusClient;

        public GatewayExtract(ILogger<GatewayExtract> log, ApplyDataContext applyDataContext, ServiceBusClient serviceBusClient)
        {
            _logger = log;
            _applyDataContext = applyDataContext;
            _serviceBusClient = serviceBusClient;
        }

        [Function("GatewayExtract")]
        public async Task Run([TimerTrigger("0 0 1 * * *")] TimerInfo myTimer)
        {
            if (myTimer.IsPastDue)
            {
                _logger.LogInformation("GatewayExtract function is running later than scheduled");
            }

            _logger.LogInformation($"GatewayExtract function executed at: {DateTime.Now}");

            var applications = await GetApplicationsToExtract();

            foreach (var application in applications)
            {
                await EnqueueGatewayFilesForExtract(application);
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

        public async Task EnqueueGatewayFilesForExtract(Apply application)
        {
            _logger.LogDebug($"Enqueuing gateway files for extract for application {application.ApplicationId}");
            var sender = _serviceBusClient.CreateSender(Environment.GetEnvironmentVariable("AdminFileExtractQueue"));
            if (application.ApplyData?.GatewayReviewDetails?.GatewaySubcontractorDeclarationClarificationUpload != null)
            {
                var message = new ServiceBusMessage(JsonConvert.SerializeObject(new AdminFileExtractRequest(application.ApplicationId, application.ApplyData.GatewayReviewDetails)));
                await sender.SendMessageAsync(message);
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