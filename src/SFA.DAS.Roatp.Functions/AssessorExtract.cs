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
    public class AssessorExtract
    {
        private readonly ILogger<AssessorExtract> _logger;
        private readonly ApplyDataContext _applyDataContext;
        private readonly ServiceBusClient _serviceBusClient;

        public AssessorExtract(ILogger<AssessorExtract> log, ApplyDataContext applyDataContext, ServiceBusClient serviceBusClient)
        {
            _logger = log;
            _applyDataContext = applyDataContext;
            _serviceBusClient = serviceBusClient;
        }

        [Function("AssessorExtract")]
        public async Task Run([TimerTrigger("0 0 1 * * *")] TimerInfo myTimer)
        {
            if (myTimer.IsPastDue)
            {
                _logger.LogInformation("AssessorExtract function is running later than scheduled");
            }

            _logger.LogInformation($"AssessorExtract function executed at: {DateTime.Now}");

            var applications = await GetApplicationsToExtract();

            foreach (var application in applications)
            {
                await EnqueueAssessorFilesForExtract(application);
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

        public async Task EnqueueAssessorFilesForExtract(Apply application)
        {
            _logger.LogDebug($"Enqueuing assessor files for extract for application {application.ApplicationId}");
            var sender = _serviceBusClient.CreateSender(Environment.GetEnvironmentVariable("AdminFileExtractQueue"));
            if (application.AssessorClarificationOutcomes != null)
            {
                var clarificationFiles = application.AssessorClarificationOutcomes.Where(x => x.ClarificationFile != null);

                foreach (var file in clarificationFiles)
                {
                    var message = new ServiceBusMessage(JsonConvert.SerializeObject(new AdminFileExtractRequest(file)));
                    await sender.SendMessageAsync(message);
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