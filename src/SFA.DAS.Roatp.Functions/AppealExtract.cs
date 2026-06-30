using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.Roatp.Functions.ApplyTypes;
using SFA.DAS.Roatp.Functions.Infrastructure.Databases;
using SFA.DAS.Roatp.Functions.Requests;

namespace SFA.DAS.Roatp.Functions;

public class AppealExtract
{
    private readonly ILogger<AppealExtract> _logger;
    private readonly ApplyDataContext _applyDataContext;

    public AppealExtract(ILogger<AppealExtract> log, ApplyDataContext applyDataContext)
    {
        _logger = log;
        _applyDataContext = applyDataContext;
    }

    [Function("AppealExtract")]
    [ServiceBusOutput("%AppealFileExtractQueue%", Connection = "DASServiceBusConnectionString")]
    public async Task<IEnumerable<AppealFileExtractRequest>> Run([TimerTrigger("%AppealExtractSchedule%", RunOnStartup = false)] TimerInfo myTimer)
    {
        List<AppealFileExtractRequest> appealFileExtractQueue = [];
        if (myTimer.IsPastDue)
        {
            _logger.LogInformation("AppealExtract function is running later than scheduled");
        }

        var appeals = await GetAppealsToExtract();

        foreach (var appeal in appeals)
        {
            await EnqueueAppealFilesForExtract(appealFileExtractQueue, appeal);
            await MarkAppealFilesExtractedForApplication(appeal.ApplicationId);
        }
        return appealFileExtractQueue;
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

        return [.. applications.Select(ap => ap.Appeal)];
    }

    private static async Task EnqueueAppealFilesForExtract(List<AppealFileExtractRequest> appealFileExtractQueue, Appeal appeal)
    {
        if (appeal.AppealFiles == null) return;
        var appealFiles = appeal.AppealFiles.Where(x => x.FileName != null);

        foreach (var file in appealFiles)
        {
            appealFileExtractQueue.Add(new AppealFileExtractRequest() { ApplicationId = file.ApplicationId, FileName = file.FileName });
        }
    }

    public async Task MarkAppealFilesExtractedForApplication(Guid applicationId)
    {
        _logger.LogDebug("Marking AppealFilesExtracted for application {ApplicationId}", applicationId);

        try
        {
            var application = _applyDataContext.ExtractedApplications.Single(ans => ans.ApplicationId == applicationId);
            application.AppealFilesExtracted = true;

            await _applyDataContext.SaveChangesAsync();
            _logger.LogInformation("Successfully marked AppealFilesExtracted for application {ApplicationId}", applicationId);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Unable to mark AppealFilesExtracted for Application: {ApplicationId}", applicationId);
        }
    }
}
