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
    public class FinanceExtract
    {
        private readonly ILogger<FinanceExtract> _logger;
        private readonly ApplyDataContext _applyDataContext;
        private readonly ServiceBusClient _serviceBusClient;

        public FinanceExtract(ILogger<FinanceExtract> log, ApplyDataContext applyDataContext, ServiceBusClient serviceBusClient)
        {
            _logger = log;
            _applyDataContext = applyDataContext;
            _serviceBusClient = serviceBusClient;
        }

        [Function("FinanceExtract")]
        public async Task Run([TimerTrigger("0 0 1 * * *")] TimerInfo myTimer)
        {
            if (myTimer.IsPastDue)
            {
                _logger.LogInformation("FinanceExtract function is running later than scheduled");
            }

            _logger.LogInformation($"FinanceExtract function executed at: {DateTime.Now}");

            var applications = await GetApplicationsToExtract();

            foreach (var application in applications)
            {
                await EnqueueFinanceFilesForExtract(application);
                await MarkFinanceFilesExtractedForApplication(application.ApplicationId);
            }
        }

        public async Task<List<Apply>> GetApplicationsToExtract()
        {
            _logger.LogDebug($"Getting list of applications to extract");

            var applications = await _applyDataContext.Apply
                                .AsNoTracking()
                                .Include(x => x.ExtractedApplication)
                                .Include(x => x.FinancialReview)
                                .Include(x => x.FinancialReview.ClarificationFiles)
                                .Where(app => app.ExtractedApplication != null && !app.ExtractedApplication.FinanceFilesExtracted)
                                .Where(app => app.FinancialReview.Status == "Pass" || app.FinancialReview.Status == "Fail" || app.FinancialReview.Status == "Exempt")
                                .ToListAsync();

            return applications;
        }

        public async Task EnqueueFinanceFilesForExtract(Apply application)
        {
            _logger.LogDebug($"Enqueuing finance files for extract for application {application.ApplicationId}");

            if (application.FinancialReview?.ClarificationFiles != null)
            {
                var clarificationFiles = application.FinancialReview.ClarificationFiles.Where(x => x.Filename != null);
                var sender = _serviceBusClient.CreateSender(Environment.GetEnvironmentVariable("AdminFileExtractQueue"));
                foreach (var file in clarificationFiles)
                {
                    var message = new ServiceBusMessage(JsonConvert.SerializeObject(new AdminFileExtractRequest(application.ApplicationId, file)));
                    await sender.SendMessageAsync(message);
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