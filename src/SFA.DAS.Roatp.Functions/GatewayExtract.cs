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

public class GatewayExtract
{
    private readonly ILogger<GatewayExtract> _logger;
    private readonly ApplyDataContext _applyDataContext;

    public GatewayExtract(ILogger<GatewayExtract> log, ApplyDataContext applyDataContext)
    {
        _logger = log;
        _applyDataContext = applyDataContext;
    }

    [Function("GatewayExtract")]
    [ServiceBusOutput("%AdminFileExtractQueue%", Connection = "DASServiceBusConnectionString")]
    public async Task<List<AdminFileExtractRequest>> Run([TimerTrigger("%GatewayExtractSchedule%")] TimerInfo myTimer)
    {
        List<AdminFileExtractRequest> clarificationFileExtractQueue = [];
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
        return clarificationFileExtractQueue;
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

    private static async Task EnqueueGatewayFilesForExtract(List<AdminFileExtractRequest> clarificationFileExtractQueue, Apply application)
    {
        if (application.ApplyData?.GatewayReviewDetails?.GatewaySubcontractorDeclarationClarificationUpload == null) return;

        clarificationFileExtractQueue.Add(new AdminFileExtractRequest(application.ApplicationId, application.ApplyData.GatewayReviewDetails));
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
