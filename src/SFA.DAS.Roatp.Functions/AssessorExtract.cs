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

public class AssessorExtract
{
    private readonly ILogger<AssessorExtract> _logger;
    private readonly ApplyDataContext _applyDataContext;

    public AssessorExtract(ILogger<AssessorExtract> log, ApplyDataContext applyDataContext)
    {
        _logger = log;
        _applyDataContext = applyDataContext;
    }

    [Function("AssessorExtract")]
    [ServiceBusOutput("%AdminFileExtractQueue%", Connection = "ServiceBusConnectionString")]
    public async Task<List<AdminFileExtractRequest>> Run([TimerTrigger("%AssessorExtractSchedule%", RunOnStartup = false)] TimerInfo myTimer)
    {
        List<AdminFileExtractRequest> clarificationFileExtractQueue = [];
        if (myTimer.IsPastDue)
        {
            _logger.LogInformation("AssessorExtract function is running later than scheduled");
        }

        var applications = await GetApplicationsToExtract();

        foreach (var application in applications)
        {
            await EnqueueAssessorFilesForExtract(clarificationFileExtractQueue, application);
            await MarkAssessorFilesExtractedForApplication(application.ApplicationId);
        }
        return clarificationFileExtractQueue;
    }

    public async Task<List<Apply>> GetApplicationsToExtract()
    {
        _logger.LogDebug("Getting list of applications to extract");

        var applications = await _applyDataContext.Apply
                            .AsNoTracking()
                            .Include(x => x.ExtractedApplication)
                            .Include(x => x.AssessorClarificationOutcomes)
                            .Where(app => app.ExtractedApplication != null && !app.ExtractedApplication.AssessorFilesExtracted)
                            .Where(app => app.AssessorReviewStatus == "Approved" || app.AssessorReviewStatus == "Declined")
                            .ToListAsync();

        return applications;
    }

    private static async Task EnqueueAssessorFilesForExtract(List<AdminFileExtractRequest> clarificationFileExtractQueue, Apply application)
    {
        if (application.AssessorClarificationOutcomes == null) return;
        var clarificationFiles = application.AssessorClarificationOutcomes.Where(x => x.ClarificationFile != null);

        foreach (var file in clarificationFiles)
        {
            clarificationFileExtractQueue.Add(new AdminFileExtractRequest(file));
        }
    }

    public async Task MarkAssessorFilesExtractedForApplication(Guid applicationId)
    {
        _logger.LogDebug("Marking AssessorFilesExtracted for application {ApplicationId}", applicationId);

        try
        {
            var application = _applyDataContext.ExtractedApplications.Single(ans => ans.ApplicationId == applicationId);
            application.AssessorFilesExtracted = true;

            await _applyDataContext.SaveChangesAsync();
            _logger.LogInformation("Successfully marked AssessorFilesExtracted for application {ApplicationId}", applicationId);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Unable to mark AssessorFilesExtracted for Application: {ApplicationId}", applicationId);
        }
    }
}
