using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.QnA.Api.Types.Page;
using SFA.DAS.Roatp.Functions.ApplyTypes;
using SFA.DAS.Roatp.Functions.Infrastructure.ApiClients;
using SFA.DAS.Roatp.Functions.Infrastructure.Databases;
using SFA.DAS.Roatp.Functions.Mappers;
using SFA.DAS.Roatp.Functions.Requests;
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
        private const int DeliveringApprenticeshipTraining = 7;
        private const int ManagementHierarchy = 3;
        private const int MonthsInYear = 12;
        private const string QuestionIdCompaniesHouseDirectors = "YO-70";
        private const string QuestionIdCompaniesHousePsCs = "YO-71";
        private const string QuestionIdCharityTrustees = "YO-80";
        private const string QuestionIdOrganisationNameSoleTrade = "PRE-20";
        private const string QuestionIdPartnership = "YO-110";
        private const string QuestionIdSoleTrade = "YO-120";

        public ApplicationExtract(ILogger<ApplicationExtract> log, ApplyDataContext applyDataContext, IQnaApiClient qnaApiClient)
        {
            _logger = log;
            _applyDataContext = applyDataContext;
            _qnaApiClient = qnaApiClient;
        }


        [FunctionName("ApplicationExtract")]
        public async Task Run([TimerTrigger("%ApplicationExtractSchedule%")] TimerInfo myTimer,
            [ServiceBus("%ApplyFileExtractQueue%", Connection = "DASServiceBusConnectionString", EntityType = EntityType.Queue)] IAsyncCollector<ApplyFileExtractRequest> applyFileExtractQueue)
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
                await EnqueueApplyFilesForExtract(applyFileExtractQueue, answers);
                using (var transaction = _applyDataContext.Database.BeginTransaction())
                {
                    await SaveExtractedAnswersForApplication(applicationId, answers);
                    await LoadOrganisationManagementForApplication(applicationId, answers);
                    await LoadOrganisationPersonnelForApplication(applicationId, answers);
                    await transaction.CommitAsync();
                }
            }
        }

        public async Task LoadOrganisationPersonnelForApplication(Guid applicationId, List<SubmittedApplicationAnswer> answers)
        {
            _logger.LogDebug($"Load Organisation Personnel for application {applicationId}");
            
            try
            {
                var organisationPersonnel = new List<OrganisationPersonnel>();
                var application = _applyDataContext.Apply.Where(app => app.ApplicationId == applicationId).FirstOrDefault();

                var submittedAnswersCompaniesHouseDirectors = ExtractTabularAnswerOrganisationPersonnel(answers, application.OrganisationId, QuestionIdCompaniesHouseDirectors, PersonnelType.CompanyDirector);
                organisationPersonnel.AddRange(submittedAnswersCompaniesHouseDirectors);

                var submittedAnswersCompaniesHousePsCs = ExtractTabularAnswerOrganisationPersonnel(answers, application.OrganisationId, QuestionIdCompaniesHousePsCs, PersonnelType.PersonWithSignificantControl);
                organisationPersonnel.AddRange(submittedAnswersCompaniesHousePsCs);

                var submittedAnswersCharityTrustees = ExtractTabularAnswerOrganisationPersonnel(answers, application.OrganisationId, QuestionIdCharityTrustees, PersonnelType.CharityTrustee);
                organisationPersonnel.AddRange(submittedAnswersCharityTrustees);

                var submittedAnswersSoleTrade = ExtractSoleTradeOrganisationPersonnel(answers, application.OrganisationId, QuestionIdSoleTrade, PersonnelType.PersonInControl);
                organisationPersonnel.AddRange(submittedAnswersSoleTrade);

                var submittedAnswersPartnership = ExtractTabularAnswerOrganisationPersonnel(answers, application.OrganisationId, QuestionIdPartnership, PersonnelType.PersonInControl);
                organisationPersonnel.AddRange(submittedAnswersPartnership);

                _applyDataContext.OrganisationPersonnel.AddRange(organisationPersonnel);

                await _applyDataContext.SaveChangesAsync();

                _logger.LogInformation($"Organisation Personnel successfully load for application {applicationId}");
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to load Organisation Personnel for Application: {applicationId}");
            }
        }

        private static List<OrganisationPersonnel> ExtractTabularAnswerOrganisationPersonnel(List<SubmittedApplicationAnswer> answers, Guid organisationId, string questionId, PersonnelType personnelType)
        {
            var submittedAnswersOrganisationPersonnel = answers.Where(answer => answer.QuestionId == questionId).GroupBy(a => a.RowNumber).ToList();
            var organisationPersonnel = new List<OrganisationPersonnel>();
            foreach (var person in submittedAnswersOrganisationPersonnel)
            {
                var orgPersonnel = new OrganisationPersonnel
                {
                    OrganisationId = organisationId,
                    PersonnelType = (int)personnelType,
                };
                foreach (var record in person)
                {
                    switch (record.ColumnHeading)
                    {
                        case "Name":
                            orgPersonnel.Name = record.Answer;
                            break;
                        case "Date of birth":
                            var dob = Convert.ToDateTime(record.Answer);
                            orgPersonnel.DateOfBirthMonth = dob.Month;
                            orgPersonnel.DateOfBirthYear = dob.Year;
                            break;
                    }
                }
                organisationPersonnel.Add(orgPersonnel);
            }
            return organisationPersonnel;
        }

        private static List<OrganisationPersonnel> ExtractSoleTradeOrganisationPersonnel(List<SubmittedApplicationAnswer> answers, Guid organisationId, string questionId, PersonnelType personnelType)
        {
            var submittedAnswersOrganisationPersonnel = answers.Where(answer => answer.QuestionId == questionId).GroupBy(a => a.RowNumber).ToList();
            var organisationPersonnel = new List<OrganisationPersonnel>();

            var submittedAnswersOrganisationNameSoleTrade = answers.Where(answer => answer.QuestionId == QuestionIdOrganisationNameSoleTrade);
            if (submittedAnswersOrganisationPersonnel.Count > 0)
            {
                foreach (var person in submittedAnswersOrganisationPersonnel)
                {
                    var orgPersonnel = new OrganisationPersonnel
                    {
                        OrganisationId = organisationId,
                        PersonnelType = (int)personnelType,
                        Name = submittedAnswersOrganisationNameSoleTrade.FirstOrDefault()?.Answer
                    };
                    foreach (var record in person)
                    {
                        if (record.Answer == null) continue;
                        var dobArray = record.Answer.Split(",");
                        if (dobArray.Length != 2) continue;
                        orgPersonnel.DateOfBirthMonth = int.Parse(dobArray[0]);
                        orgPersonnel.DateOfBirthYear = int.Parse(dobArray[1]);
                    }
                    organisationPersonnel.Add(orgPersonnel);
                }
            }
            else
            {
                var orgPersonnel = new OrganisationPersonnel
                {
                    OrganisationId = organisationId,
                    PersonnelType = (int)personnelType,
                    Name = submittedAnswersOrganisationNameSoleTrade.FirstOrDefault()?.Answer
                };
                organisationPersonnel.Add(orgPersonnel);
            }
            return organisationPersonnel;
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
                        var submittedPageAnswers = ExtractPageAnswers(applicationId, section.SequenceNo, section.SectionNo, page);
                        answers.AddRange(submittedPageAnswers);
                    }
                }
            }

            return answers;
        }

        private static List<SubmittedApplicationAnswer> ExtractPageAnswers(Guid applicationId, int sequenceNumber, int sectionNumber, Page page)
        {
            var submittedPageAnswers = new List<SubmittedApplicationAnswer>();

            if (page.PageOfAnswers != null && page.Questions != null)
            {
                // Note: RoATP only has a single PageOfAnswers in a page unless it's a MultipleFileUpload page
                var pageAnswers = "MultipleFileUpload".Equals(page.DisplayType) ? page.PageOfAnswers.SelectMany(answers => answers.Answers).ToList()
                                    : page.PageOfAnswers[0].Answers;

                foreach (var question in page.Questions)
                {
                    var submittedQuestionAnswers = ExtractQuestionAnswers(applicationId, sequenceNumber, sectionNumber, page.PageId, question, pageAnswers);
                    submittedPageAnswers.AddRange(submittedQuestionAnswers);
                }
            }

            return submittedPageAnswers;
        }

        private static List<SubmittedApplicationAnswer> ExtractQuestionAnswers(Guid applicationId, int sequenceNumber, int sectionNumber, string pageId, Question question, ICollection<Answer> answers)
        {
            var submittedQuestionAnswers = new List<SubmittedApplicationAnswer>();

            var questionId = question.QuestionId;

            // Note: RoATP only has a single answer per question
            var questionAnswer = answers?.FirstOrDefault(ans => ans.QuestionId == questionId && !string.IsNullOrWhiteSpace(ans.Value));

            if (questionAnswer != null)
            {
                switch (question.Input.Type.ToUpper())
                {
                    case "TABULARDATA":
                        var tabularData = JsonConvert.DeserializeObject<TabularData>(questionAnswer.Value);
                        var tabularAnswers = TabularDataMapper.GetAnswers(applicationId, sequenceNumber, sectionNumber, pageId, question, tabularData);
                        submittedQuestionAnswers.AddRange(tabularAnswers);
                        break;
                    case "CHECKBOXLIST":
                    case "COMPLEXCHECKBOXLIST":
                        var checkboxAnswers = CheckBoxListMapper.GetAnswers(applicationId, sequenceNumber, sectionNumber, pageId, question, questionAnswer.Value);
                        submittedQuestionAnswers.AddRange(checkboxAnswers);
                        break;
                    default:
                        var submittedAnswer = SubmittedAnswerMapper.GetAnswer(applicationId, sequenceNumber, sectionNumber, pageId, question, questionAnswer.Value);
                        submittedQuestionAnswers.Add(submittedAnswer);
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
                                var furtherQuestionAnswers = ExtractQuestionAnswers(applicationId, sequenceNumber, sectionNumber, pageId, furtherQuestion, answers);
                                submittedFurtherQuestionAnswers.AddRange(furtherQuestionAnswers);
                            }
                        }
                    }

                    submittedQuestionAnswers.AddRange(submittedFurtherQuestionAnswers);
                }
            }

            return submittedQuestionAnswers;
        }

        public async Task SaveExtractedAnswersForApplication(Guid applicationId, List<SubmittedApplicationAnswer> answers)
        {
            _logger.LogDebug($"Saving extracted answers for application {applicationId}");
            try
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

                await _applyDataContext.SaveChangesAsync();

                _logger.LogInformation($"Extracted answers successfully saved for application {applicationId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to save extracted answers for Application: {applicationId}");
            }
        }

        public async Task EnqueueApplyFilesForExtract(IAsyncCollector<ApplyFileExtractRequest> applyFileExtractQueue, List<SubmittedApplicationAnswer> answers)
        {
            var applyFiles = answers.Where(answer => "FILEUPLOAD".Equals(answer.QuestionType, StringComparison.OrdinalIgnoreCase)).ToList();

            foreach (var submittedApplicationAnswer in applyFiles)
            {
                await applyFileExtractQueue.AddAsync(new ApplyFileExtractRequest(submittedApplicationAnswer));
            }
        }

        private List<OrganisationManagement> LoadOrganisationManagementAnswers(Guid applicationId, List<SubmittedApplicationAnswer> answers)
        {
            var organisationManagementAnswers = new List<OrganisationManagement>();

            var application = _applyDataContext.Apply.Where(app => app.ApplicationId == applicationId).FirstOrDefault();
            var submittedAnswersOrganisationManagement = answers.Where(answer => answer.SequenceNumber == DeliveringApprenticeshipTraining && answer.SectionNumber == ManagementHierarchy).GroupBy(a => a.RowNumber).ToList();

            foreach (var managementHierarchyPerson in submittedAnswersOrganisationManagement)
            {
                var organisationManagement = new OrganisationManagement
                {
                    OrganisationId = application.OrganisationId,
                };
                foreach (var personDetails in managementHierarchyPerson)
                {
                    switch (personDetails.ColumnHeading)
                    {
                        case "First Name":
                            organisationManagement.FirstName = personDetails.Answer;
                            break;
                        case "Last Name":
                            organisationManagement.LastName = personDetails.Answer;
                            break;
                        case "Job role":
                            organisationManagement.JobRole = personDetails.Answer;
                            break;
                        case "Years in role":
                            organisationManagement.TimeInRoleMonths += int.Parse(personDetails.Answer) * MonthsInYear; 
                            break;
                        case "Months in role":
                            organisationManagement.TimeInRoleMonths += int.Parse(personDetails.Answer);
                            break;
                        case "Part of another organisation":
                            organisationManagement.IsPartOfAnyOtherOrganisation = personDetails.Answer.Equals("Yes") ? true : false;
                            break;
                        case "Organisation details":
                            organisationManagement.OtherOrganisationNames = personDetails.Answer;
                            break;
                        case "Month":
                            organisationManagement.DateOfBirthMonth = int.Parse(personDetails.Answer);
                            break;
                        case "Year":
                            organisationManagement.DateOfBirthYear = int.Parse(personDetails.Answer);
                            break;
                        case "Email":
                            organisationManagement.Email = personDetails.Answer;
                            break;
                        case "Contact number":
                            organisationManagement.ContactNumber = personDetails.Answer;
                            break;
                        default:
                            break;
                    }
                }
                organisationManagementAnswers.Add(organisationManagement);
            }
            return organisationManagementAnswers;
        }
        
        public async Task LoadOrganisationManagementForApplication(Guid applicationId, List<SubmittedApplicationAnswer> answers)
        {
            _logger.LogDebug($"OrganisationManagement extract for application {applicationId}");
            try
            {
                var organisationManagementAnswers = LoadOrganisationManagementAnswers(applicationId, answers);
                _applyDataContext.OrganisationManagement.AddRange(organisationManagementAnswers);
                await _applyDataContext.SaveChangesAsync();

                _logger.LogInformation($"OrganisationManagement successfully extract for application {applicationId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to extract OrganisationManagement for Application: {applicationId}");
            }
        }
    }
}
