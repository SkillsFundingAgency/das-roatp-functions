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
    public class AppealExtract
    {
        private readonly ILogger<AppealExtract> _logger;
        private readonly ApplyDataContext _applyDataContext;
        private readonly ServiceBusClient _serviceBusClient;

        public AppealExtract(ILogger<AppealExtract> log, ApplyDataContext applyDataContext, ServiceBusClient serviceBusClient)
        {
            _logger = log;
            _applyDataContext = applyDataContext;
            _serviceBusClient = serviceBusClient;
        }

        [Function("AppealExtract")]
        public async Task Run([TimerTrigger("0 0 1 * * *")] TimerInfo myTimer)
        {
            if (myTimer.IsPastDue)
            {
                _logger.LogInformation("AppealExtract function is running later than scheduled");
            }

            _logger.LogInformation($"AppealExtract function executed at: {DateTime.Now}");

            var appeals = await GetAppealsToExtract();
            foreach (var appeal in appeals)
            {
                await EnqueueAppealFilesForExtract(appeal);
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

        public async Task EnqueueAppealFilesForExtract(Appeal appeal)
        {
            _logger.LogDebug($"Enqueuing appeal files for extract for application {appeal.ApplicationId}");

            if (appeal.AppealFiles != null)
            {
                var appealFiles = appeal.AppealFiles.Where(x => x.FileName != null);

                var sender = _serviceBusClient.CreateSender(Environment.GetEnvironmentVariable("AppealFileExtractQueue"));

                foreach (var file in appealFiles)
                {
                    var message = new ServiceBusMessage(JsonConvert.SerializeObject(new AppealFileExtractRequest(file)));
                    await sender.SendMessageAsync(message);
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