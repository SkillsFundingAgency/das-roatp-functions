using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using EntityFrameworkCore.Testing.Moq;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SFA.DAS.QnA.Api.Types;
using SFA.DAS.Roatp.Functions.ApplyTypes;
using SFA.DAS.Roatp.Functions.Infrastructure.ApiClients;
using SFA.DAS.Roatp.Functions.Infrastructure.Databases;
using SFA.DAS.Roatp.Functions.Requests;
using SFA.DAS.Roatp.Functions.Services.Sectors;
using SFA.DAS.Roatp.Functions.UnitTests.Generators;

namespace SFA.DAS.Roatp.Functions.UnitTests
{
    public class ApplicationExtractTests
    {
        private Mock<ILogger<ApplicationExtract>> _logger;
        private Mock<IQnaApiClient> _qnaApiClient;
        private Mock<ISectorProcessingService> _sectorProcessingService;
        private ApplyDataContext _applyDataContext;
        private Mock<IAsyncCollector<ApplyFileExtractRequest>> _applyFileExtractQueue;
        private Mock<ServiceBusClient> _serviceBusClient;
        private Mock<ServiceBusSender> _serviceBusSenderMock;

        private Apply _inProgressApplication;
        private Apply _application;
        private List<Section> _sections;

        private ApplicationExtract _sut;

        [SetUp]
        public void Setup()
        {
            _logger = new Mock<ILogger<ApplicationExtract>>();
            _qnaApiClient = new Mock<IQnaApiClient>();
            _sectorProcessingService = new Mock<ISectorProcessingService>();
            _applyDataContext = Create.MockedDbContextFor<ApplyDataContext>();
            _serviceBusClient = new Mock<ServiceBusClient>();
            _serviceBusSenderMock = new();
            _serviceBusClient.Setup(x => x.CreateSender(It.IsAny<string>()))
                .Returns(_serviceBusSenderMock.Object);

            _inProgressApplication = ApplyGenerator.GenerateApplication(Guid.NewGuid(), "In Progress", null);
            _application = ApplyGenerator.GenerateApplication(Guid.NewGuid(), "Submitted", DateTime.Today.AddDays(-1));

            var applications = new List<Apply> { _inProgressApplication, _application };
            _applyDataContext.Set<Apply>().AddRange(applications);
            _applyDataContext.SaveChanges();

            _sections = QnaGenerator.GenerateSectionsForApplication(_application.ApplicationId);
            _qnaApiClient.Setup(x => x.GetAllSectionsForApplication(_application.ApplicationId)).ReturnsAsync(_sections);

            _applyFileExtractQueue = new Mock<IAsyncCollector<ApplyFileExtractRequest>>();

            _sut = new ApplicationExtract(_logger.Object, _applyDataContext, _qnaApiClient.Object, _sectorProcessingService.Object, _serviceBusClient.Object);
        }

        [Test]
        public async Task GetApplicationsToExtract_Contains_Expected_Applications()
        {
            var expectedApplicationId = _application.ApplicationId;
            var executionDateTime = _application.ApplyData.ApplyDetails.ApplicationSubmittedOn.Value.Date.AddDays(1);

            var actualResults = await _sut.GetApplicationsToExtract(executionDateTime);

            CollectionAssert.IsNotEmpty(actualResults);
            CollectionAssert.Contains(actualResults, expectedApplicationId);
            CollectionAssert.DoesNotContain(actualResults, _inProgressApplication.ApplicationId);
        }

        [Test]
        public async Task ExtractAnswersForApplication_Contains_Expected_Questions()
        {
            var firstSection = _sections[0];
            var firstPage = firstSection.QnAData.Pages[0];
            var firstPageQuestion = firstPage.Questions[0];

            var expectedQuestion = new SubmittedApplicationAnswer
            {
                ApplicationId = _application.ApplicationId,
                SequenceNumber = firstSection.SequenceNo,
                SectionNumber = firstSection.SectionNo,
                PageId = firstPage.PageId,
                QuestionId = firstPageQuestion.QuestionId,
                QuestionType = firstPageQuestion.Input.Type
            };

            var extractedQuestions = await _sut.ExtractAnswersForApplication(_application.ApplicationId);
            var actualQuestion = extractedQuestions.FirstOrDefault(x => x.PageId == expectedQuestion.PageId && x.QuestionId == expectedQuestion.QuestionId);

            _qnaApiClient.Verify(x => x.GetAllSectionsForApplication(_application.ApplicationId), Times.Once);

            Assert.Multiple(() =>
            {
                Assert.That(actualQuestion, Is.Not.Null);
                Assert.That(expectedQuestion.ApplicationId, Is.EqualTo(actualQuestion.ApplicationId));
                Assert.That(expectedQuestion.SequenceNumber, Is.EqualTo(actualQuestion.SequenceNumber));
                Assert.That(expectedQuestion.SectionNumber, Is.EqualTo(actualQuestion.SectionNumber));
                Assert.That(expectedQuestion.PageId, Is.EqualTo(actualQuestion.PageId));
                Assert.That(expectedQuestion.QuestionId, Is.EqualTo(actualQuestion.QuestionId));
                Assert.That(expectedQuestion.QuestionType, Is.EqualTo(actualQuestion.QuestionType));
            });
        }

        [Test]
        public async Task ExtractAnswersForApplication_Contains_Expected_QuestionAnswers()
        {
            var firstSection = _sections[0];
            var firstPage = firstSection.QnAData.Pages[0];
            var firstPageQuestion = firstPage.Questions[0];
            var firstPageAnswer = firstPage.PageOfAnswers[0].Answers[0];

            var expectedAnswer = new SubmittedApplicationAnswer
            {
                Answer = firstPageAnswer.Value,
                ColumnHeading = null
            };

            var extractedQuestions = await _sut.ExtractAnswersForApplication(_application.ApplicationId);
            var actualAnswer = extractedQuestions.FirstOrDefault(x => x.PageId == firstPage.PageId && x.QuestionId == firstPageQuestion.QuestionId);

            _qnaApiClient.Verify(x => x.GetAllSectionsForApplication(_application.ApplicationId), Times.Once);

            Assert.Multiple(() =>
            {
                Assert.That(actualAnswer, Is.Not.Null);
                Assert.That(expectedAnswer.Answer, Is.EqualTo(actualAnswer.Answer));
                Assert.That(expectedAnswer.ColumnHeading, Is.EqualTo(actualAnswer.ColumnHeading));
                Assert.That(expectedAnswer.RowNumber, Is.EqualTo(actualAnswer.RowNumber));
                Assert.That(expectedAnswer.ColumnNumber, Is.EqualTo(actualAnswer.ColumnNumber));
            });
        }

        [Test]
        public async Task SaveExtractedAnswersForApplication_Saves_Answers()
        {
            var applicationId = _application.ApplicationId;
            var applicationAnswers = await _sut.ExtractAnswersForApplication(applicationId);

            await _sut.SaveExtractedAnswersForApplication(applicationId, applicationAnswers);

            var submittedAnswers = _applyDataContext.SubmittedApplicationAnswers.AsQueryable().Where(app => app.ApplicationId == applicationId).ToList();

            CollectionAssert.IsNotEmpty(submittedAnswers);
            Assert.That(applicationAnswers.Count, Is.EqualTo(submittedAnswers.Count));
        }

        [Test]
        public async Task SaveExtractedAnswersForApplication_Saves_ApplicationExtracted_Entry()
        {
            var applicationId = _application.ApplicationId;
            var applicationAnswers = await _sut.ExtractAnswersForApplication(applicationId);

            await _sut.SaveExtractedAnswersForApplication(applicationId, applicationAnswers);

            var extractedApplication = _applyDataContext.ExtractedApplications.AsQueryable().SingleOrDefault(app => app.ApplicationId == applicationId);

            Assert.That(extractedApplication, Is.Not.Null);
        }

        [Test]
        public async Task EnqueueApplyFilesForExtract_Enqueues_Requests()
        {
            var applicationId = _application.ApplicationId;
            var applicationAnswers = await _sut.ExtractAnswersForApplication(applicationId);

            await _sut.EnqueueApplyFilesForExtract(applicationAnswers);

            _serviceBusSenderMock.Verify(x => x.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        [Test]
        public async Task LoadOrganisationManagementForApplication_Loads_OrganisationManagement()
        {
            var applicationId = _application.ApplicationId;
            var organisationId = _application.OrganisationId;

            var applicationAnswers = await _sut.ExtractAnswersForApplication(applicationId);

            await _sut.LoadOrganisationManagementForApplication(applicationId, applicationAnswers);

            var organisationManagementAnswers = _applyDataContext.OrganisationManagement.AsQueryable().Where(app => app.OrganisationId == organisationId).ToList();

            CollectionAssert.IsNotEmpty(organisationManagementAnswers);
            Assert.Multiple(() =>
            {
                Assert.That(organisationManagementAnswers.Count, Is.EqualTo(3));
                Assert.That(organisationManagementAnswers[0].TimeInRoleMonths, Is.EqualTo(26));
                Assert.That(organisationManagementAnswers[1].TimeInRoleMonths, Is.EqualTo(13));
                Assert.That(organisationManagementAnswers[2].TimeInRoleMonths, Is.EqualTo(39));
            });
        }

        [Test]
        public async Task LoadOrganisationPersonnelForApplication_Loads_OrganisationPersonnel()
        {
            var applicationId = _application.ApplicationId;
            var organisationId = _application.OrganisationId;
            var applicationAnswers = await _sut.ExtractAnswersForApplication(applicationId);

            await _sut.LoadOrganisationPersonnelForApplication(applicationId, applicationAnswers);

            var loadedOrganisationPersonnel = _applyDataContext.OrganisationPersonnel.AsQueryable().Where(app => app.OrganisationId == organisationId);

            Assert.Multiple(() =>
            {
                Assert.That(loadedOrganisationPersonnel, Is.Not.Null);
                Assert.That(loadedOrganisationPersonnel.Where(a => a.PersonnelType == PersonnelType.CompanyDirector).Any());
                Assert.That(loadedOrganisationPersonnel.Where(a => a.PersonnelType == PersonnelType.PersonWithSignificantControl).Any());
                Assert.That(loadedOrganisationPersonnel.Where(a => a.PersonnelType == PersonnelType.CharityTrustee).Any());
                Assert.That(loadedOrganisationPersonnel.Where(a => a.PersonnelType == PersonnelType.PersonInControl).Any());
            });
        }
    }
}