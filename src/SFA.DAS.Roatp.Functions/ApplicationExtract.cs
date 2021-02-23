using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.QnA.Api.Types;
using SFA.DAS.Roatp.Functions.ApplyTypes;
using SFA.DAS.Roatp.Functions.Infrastructure.ApiClients;
using SFA.DAS.Roatp.Functions.Infrastructure.Databases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Roatp.Functions
{
    public class ApplicationExtract
    {
        private readonly ILogger<ApplicationExtract> _logger;
        private readonly ApplyDataContext _applyDataContext;
        private readonly IQnaApiClient _qnaApiClient;

        public ApplicationExtract(ILogger<ApplicationExtract> log, ApplyDataContext applyDataContext, IQnaApiClient qnaApiClient)
        {
            _logger = log;
            _applyDataContext = applyDataContext;
            _qnaApiClient = qnaApiClient;
        }


        [FunctionName("ApplicationExtract")]
        public async Task Run([TimerTrigger("0 0 */2 * * *", RunOnStartup = true)] TimerInfo myTimer)
        {
            if (myTimer.IsPastDue)
            {
                _logger.LogInformation("ApplicationExtract function is running later than scheduled");
            }

            _logger.LogInformation($"ApplicationExtract function executed at: {DateTime.Now}");

            var applications = await GetApplicationsToExtract(DateTime.Now);

            foreach (var applicationId in applications)
            {
                var answers = await ExtractAnswersForApplication(applicationId);
                await SaveExtractedAnswersForApplication(applicationId, answers);
            }
        }

        public async Task<List<Guid>> GetApplicationsToExtract(DateTime executionDateTime)
        {
            var applications = _applyDataContext.Apply
                                .FromSqlRaw(@"   SELECT *
                                             FROM Apply apply
                                             WHERE apply.ApplicationId NOT IN (SELECT ApplicationId FROM ExtractedApplications)
                                             AND CAST(JSON_VALUE(apply.ApplyData, '$.ApplyDetails.ApplicationSubmittedOn') AS DATE) < {0}",
                                 executionDateTime.Date)
                                .AsNoTracking();

            // TODO: EF keeps making this an INNER JOIN so have resorted to the above. I can't figure out what configuration is missing!
            //var applications = _applyDataContext.Apply
            //                    .FromSqlRaw(@"   SELECT *
            //                                     FROM Apply apply
            //                                     LEFT OUTER JOIN ExtractedApplications extract ON apply.ApplicationId = extract.ApplicationId
            //                                     WHERE extract.ApplicationId IS NULL
            //                                      AND CAST(JSON_VALUE(apply.ApplyData, '$.ApplyDetails.ApplicationSubmittedOn') AS DATE) < {0}",
            //                                     executionDateTime.Date)
            //                    .AsNoTracking();

            return await applications.Select(app => app.ApplicationId).ToListAsync();
        }

        public async Task<List<SubmittedApplicationAnswer>> ExtractAnswersForApplication(Guid applicationId)
        {
            var answers = new List<SubmittedApplicationAnswer>();

            var sections = await _qnaApiClient.GetAllSectionsForApplication(applicationId);

            if (sections != null)
            {
                foreach (var section in sections)
                {
                    var completedPages = section.QnAData.Pages.Where(pg => pg.Active && pg.Complete);

                    foreach (var page in completedPages)
                    {
                        foreach (var pageAnswer in page.PageOfAnswers.SelectMany(poa => poa.Answers))
                        {
                            if (string.IsNullOrWhiteSpace(pageAnswer.Value)) continue;

                            answers.Add(
                                new SubmittedApplicationAnswer
                                {
                                    ApplicationId = applicationId,
                                    PageId = page.PageId,
                                    QuestionId = pageAnswer.QuestionId,
                                    Answer = pageAnswer.Value
                                });
                        }
                    }
                }
            }

            return answers;
        }

        public async Task SaveExtractedAnswersForApplication(Guid applicationId, List<SubmittedApplicationAnswer> answers)
        {
            using (var dataContextTransaction = _applyDataContext.Database.BeginTransaction())
            {
                var existingAnswers = _applyDataContext.SubmittedApplicationAnswers.Where(ans => ans.ApplicationId == applicationId);
                _applyDataContext.SubmittedApplicationAnswers.RemoveRange(existingAnswers);

                var existingApplications = _applyDataContext.ExtractedApplications.Where(app => app.ApplicationId == applicationId);
                _applyDataContext.ExtractedApplications.RemoveRange(existingApplications);

                if (answers != null && answers.Any())
                {
                    answers.ForEach(a => a.ApplicationId = applicationId);
                    _applyDataContext.SubmittedApplicationAnswers.AddRange(answers);
                }

                var application = new ExtractedApplication { ApplicationId = applicationId, ExtractedDate = DateTime.UtcNow };
                _applyDataContext.ExtractedApplications.Add(application);

                try
                {
                    await _applyDataContext.SaveChangesAsync();
                    await dataContextTransaction.CommitAsync();
                }
                catch (NullReferenceException) when (dataContextTransaction is null && _applyDataContext.GetType () != typeof(ApplyDataContext))
                {
                    // Safe to ignore as it is the Unit Tests executing and it doesn't currently mock Transactions
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Unable to save extracted answers for Application: {applicationId}");
                    await dataContextTransaction.RollbackAsync();
                }
            }
        }
    }
}
