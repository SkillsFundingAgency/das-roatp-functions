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

public class FinanceExtract
{
    private readonly ILogger<FinanceExtract> _logger;
    private readonly ApplyDataContext _applyDataContext;

    public FinanceExtract(ILogger<FinanceExtract> log, ApplyDataContext applyDataContext)
    {
        _logger = log;
        _applyDataContext = applyDataContext;
    }

    [Function("FinanceExtract")]
    [ServiceBusOutput("%AdminFileExtractQueue%", Connection = "DASServiceBusConnectionString")]
    public async Task<List<AdminFileExtractRequest>> Run([TimerTrigger("%FinanceExtractSchedule%")] TimerInfo myTimer)
    {
        List<AdminFileExtractRequest> clarificationFileExtractQueue = [];
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
        return clarificationFileExtractQueue;
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

    private static async Task EnqueueFinanceFilesForExtract(List<AdminFileExtractRequest> clarificationFileExtractQueue, Apply application)
    {
        if (application.FinancialReview?.ClarificationFiles == null) return;
        var clarificationFiles = application.FinancialReview.ClarificationFiles.Where(x => x.Filename != null);

        foreach (var file in clarificationFiles)
        {
            clarificationFileExtractQueue.Add(new AdminFileExtractRequest(application.ApplicationId, file));
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
