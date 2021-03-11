using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.QnA.Api.Types.Page;
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
        public async Task Run([TimerTrigger("%ApplicationExtractSchedule%")] TimerInfo myTimer)
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
            _logger.LogDebug($"Getting list of applications to extract.");

            var applications = await _applyDataContext.Apply
                                .AsNoTracking()
                                .Include(x => x.ExtractedApplication)
                                .Where(app => app.ExtractedApplication == null && app.ApplicationStatus != "In Progress")
                                .ToListAsync();

            // Note: Because ApplyData uses a JSON Conversion, you have to ensure it was populated above
            return applications.Where(x => x.ApplyData.ApplyDetails.ApplicationSubmittedOn < executionDateTime.Date)
                               .Select(app => app.ApplicationId).ToList();
        }

        public async Task<List<SubmittedApplicationAnswer>> ExtractAnswersForApplication(Guid applicationId)
        {
            _logger.LogDebug($"Extracting answers for application {applicationId}");

            var answers = new List<SubmittedApplicationAnswer>();

            var sections = await _qnaApiClient.GetAllSectionsForApplication(applicationId);

            if (sections != null)
            {
                foreach (var section in sections)
                {
                    var completedPages = section.QnAData.Pages.Where(pg => pg.Active && pg.Complete && !pg.NotRequired);

                    foreach (var page in completedPages)
                    {
                        var submittedPageAnswers = ExtractPageAnswers(applicationId, page);
                        answers.AddRange(submittedPageAnswers);
                    }
                }
            }

            return answers;
        }

        private static List<SubmittedApplicationAnswer> ExtractPageAnswers(Guid applicationId, Page page)
        {
            var submittedPageAnswers = new List<SubmittedApplicationAnswer>();

            if (page.PageOfAnswers != null && page.Questions != null)
            {
                // Note: RoATP only has a single PageOfAnswers in a page (i.e page.AllowMultipleAnswers is always false)
                var pageAnswers = page.PageOfAnswers[0].Answers;

                foreach (var question in page.Questions)
                {
                    var submittedQuestionAnswers = ExtractQuestionAnswers(applicationId, page.PageId, question, pageAnswers);
                    submittedPageAnswers.AddRange(submittedQuestionAnswers);
                }
            }

            return submittedPageAnswers;
        }

        private static List<SubmittedApplicationAnswer> ExtractQuestionAnswers(Guid applicationId, string pageId, Question question, ICollection<Answer> answers)
        {
            var submittedQuestionAnswers = new List<SubmittedApplicationAnswer>();

            var questionId = question.QuestionId;
            var questionType = question.Input.Type;

            var questionAnswers = answers?.Where(ans => ans.QuestionId == questionId && !string.IsNullOrWhiteSpace(ans.Value));

            if (questionAnswers != null)
            {
                switch (questionType.ToUpper())
                {
                    // TODO: Sort out a mapper for each question type
                    case "CHECKBOXLIST":
                    case "COMPLEXCHECKBOXLIST":
                        foreach (var questionAnswer in questionAnswers)
                        {
                            foreach (var questionAnswerEntry in questionAnswer.Value.Split(","))
                            {
                                var submittedAnswer = new SubmittedApplicationAnswer
                                {
                                    ApplicationId = applicationId,
                                    PageId = pageId,
                                    QuestionId = questionId,
                                    QuestionType = questionType,
                                    Answer = questionAnswerEntry,
                                    ColumnHeading = null
                                };

                                submittedQuestionAnswers.Add(submittedAnswer);
                            }
                        }
                        break;
                    default:
                        foreach (var questionAnswer in questionAnswers)
                        {
                            var submittedAnswer = new SubmittedApplicationAnswer
                            {
                                ApplicationId = applicationId,
                                PageId = pageId,
                                QuestionId = questionId,
                                QuestionType = questionType,
                                Answer = questionAnswer.Value,
                                ColumnHeading = null
                            };

                            submittedQuestionAnswers.Add(submittedAnswer);
                        }
                        break;
                }

                // We have to do similar for extracting any matching further question
                if (question.Input.Options != null)
                {
                    var submittedFurtherQuestionAnswers = new List<SubmittedApplicationAnswer>();

                    var submittedValues = submittedQuestionAnswers.Where(sqa => sqa.QuestionId == questionId).Select(ans => ans.Answer);

                    foreach (var option in question.Input.Options.Where(opt => opt.FurtherQuestions != null))
                    {
                        // Check that option was selected
                        if (submittedValues.Contains(option.Value))
                        {
                            foreach (var furtherQuestion in option.FurtherQuestions)
                            {
                                var furtherQuestionAnswers = ExtractQuestionAnswers(applicationId, pageId, furtherQuestion, answers);
                                submittedFurtherQuestionAnswers.AddRange(furtherQuestionAnswers);
                            }
                        }
                    }

                    submittedQuestionAnswers.AddRange(submittedFurtherQuestionAnswers);
                }
            }

            return submittedQuestionAnswers;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "We want to catch all exceptions")]
        public async Task SaveExtractedAnswersForApplication(Guid applicationId, List<SubmittedApplicationAnswer> answers)
        {
            _logger.LogDebug($"Saving extracted answers for application {applicationId}");

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

                    _logger.LogInformation($"Extracted answers successfully saved for application {applicationId}");
                }
                catch (NullReferenceException) when (dataContextTransaction is null && _applyDataContext.GetType() != typeof(ApplyDataContext))
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
