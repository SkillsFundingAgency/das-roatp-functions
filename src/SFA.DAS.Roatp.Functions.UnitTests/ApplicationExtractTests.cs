using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EntityFrameworkCore.Testing.Moq;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.QnA.Api.Types;
using SFA.DAS.Roatp.Functions.ApplyTypes;
using SFA.DAS.Roatp.Functions.Infrastructure.ApiClients;
using SFA.DAS.Roatp.Functions.Infrastructure.Databases;
using SFA.DAS.Roatp.Functions.Requests;
using SFA.DAS.Roatp.Functions.Services.Sectors;
using SFA.DAS.Roatp.Functions.UnitTests.Generators;

namespace SFA.DAS.Roatp.Functions.UnitTests;

public class ApplicationExtractTests
{
    private Mock<ILogger<ApplicationExtract>> _logger;
    private Mock<IQnaApiClient> _qnaApiClient;
    private Mock<ISectorProcessingService> _sectorProcessingService;
    private ApplyDataContext _applyDataContext;
    private Mock<IEnumerable<ApplyFileExtractRequest>> _applyFileExtractQueue;
    private readonly TimerInfo _timerInfo = new();

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

        _inProgressApplication = ApplyGenerator.GenerateApplication(Guid.NewGuid(), "In Progress", null);
        _application = ApplyGenerator.GenerateApplication(Guid.NewGuid(), "Submitted", DateTime.Today.AddDays(-1));

        var applications = new List<Apply> { _inProgressApplication, _application };
        _applyDataContext.Set<Apply>().AddRange(applications);
        _applyDataContext.SaveChanges();

        _sections = QnaGenerator.GenerateSectionsForApplication(_application.ApplicationId);
        _qnaApiClient.Setup(x => x.GetAllSectionsForApplication(_application.ApplicationId)).ReturnsAsync(_sections);

        _applyFileExtractQueue = new Mock<IEnumerable<ApplyFileExtractRequest>>();

        _sut = new ApplicationExtract(_logger.Object, _applyDataContext, _qnaApiClient.Object, _sectorProcessingService.Object);
    }

    [Test]
    public async Task GetApplicationsToExtract_Contains_Expected_Applications()
    {
        var expectedApplicationId = _application.ApplicationId;
        var executionDateTime = _application.ApplyData.ApplyDetails.ApplicationSubmittedOn.Value.Date.AddDays(1);

        var actualResults = await _sut.GetApplicationsToExtract(executionDateTime);

        Assert.That(actualResults, Is.Not.Empty);
        Assert.That(actualResults, Contains.Item(expectedApplicationId));
        Assert.That(actualResults, Does.Not.Contain(_inProgressApplication.ApplicationId));
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

        Assert.That(actualQuestion, Is.Not.Null);
        Assert.That(expectedQuestion.ApplicationId, Is.EqualTo(actualQuestion.ApplicationId));
        Assert.That(expectedQuestion.SequenceNumber, Is.EqualTo(actualQuestion.SequenceNumber));
        Assert.That(expectedQuestion.SectionNumber, Is.EqualTo(actualQuestion.SectionNumber));
        Assert.That(expectedQuestion.PageId, Is.EqualTo(actualQuestion.PageId));
        Assert.That(expectedQuestion.QuestionId, Is.EqualTo(actualQuestion.QuestionId));
        Assert.That(expectedQuestion.QuestionType, Is.EqualTo(actualQuestion.QuestionType));
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

        Assert.That(actualAnswer, Is.Not.Null);
        Assert.That(expectedAnswer.Answer, Is.EqualTo(actualAnswer.Answer));
        Assert.That(expectedAnswer.ColumnHeading, Is.EqualTo(actualAnswer.ColumnHeading));
        Assert.That(expectedAnswer.RowNumber, Is.EqualTo(actualAnswer.RowNumber));
        Assert.That(expectedAnswer.ColumnNumber, Is.EqualTo(actualAnswer.ColumnNumber));
    }

    [Test]
    public async Task SaveExtractedAnswersForApplication_Saves_Answers()
    {
        var applicationId = _application.ApplicationId;
        var applicationAnswers = await _sut.ExtractAnswersForApplication(applicationId);

        await _sut.SaveExtractedAnswersForApplication(applicationId, applicationAnswers);

        var submittedAnswers = _applyDataContext.SubmittedApplicationAnswers.AsQueryable().Where(app => app.ApplicationId == applicationId).ToList();

        Assert.That(submittedAnswers, Is.Not.Empty);
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
    public async Task LoadOrganisationManagementForApplication_Loads_OrganisationManagement()
    {
        var applicationId = _application.ApplicationId;
        var organisationId = _application.OrganisationId;

        var applicationAnswers = await _sut.ExtractAnswersForApplication(applicationId);

        await _sut.LoadOrganisationManagementForApplication(applicationId, applicationAnswers);

        var organisationManagementAnswers = _applyDataContext.OrganisationManagement.AsQueryable().Where(app => app.OrganisationId == organisationId).ToList();

        Assert.That(organisationManagementAnswers, Is.Not.Empty);
        Assert.That(organisationManagementAnswers.Count, Is.EqualTo(3));
        Assert.That(organisationManagementAnswers[0].TimeInRoleMonths, Is.EqualTo(26));
        Assert.That(organisationManagementAnswers[1].TimeInRoleMonths, Is.EqualTo(13));
        Assert.That(organisationManagementAnswers[2].TimeInRoleMonths, Is.EqualTo(39));
    }

    [Test]
    public async Task LoadOrganisationPersonnelForApplication_Loads_OrganisationPersonnel()
    {
        var applicationId = _application.ApplicationId;
        var organisationId = _application.OrganisationId;
        var applicationAnswers = await _sut.ExtractAnswersForApplication(applicationId);

        await _sut.LoadOrganisationPersonnelForApplication(applicationId, applicationAnswers);

        var loadedOrganisationPersonnel = _applyDataContext.OrganisationPersonnel.AsQueryable().Where(app => app.OrganisationId == organisationId);

        Assert.That(loadedOrganisationPersonnel, Is.Not.Null);
        Assert.That(loadedOrganisationPersonnel.Where(a => a.PersonnelType == PersonnelType.CompanyDirector).Count(), Is.GreaterThan(0));
        Assert.That(loadedOrganisationPersonnel.Where(a => a.PersonnelType == PersonnelType.PersonWithSignificantControl).Count(), Is.GreaterThan(0));
        Assert.That(loadedOrganisationPersonnel.Where(a => a.PersonnelType == PersonnelType.CharityTrustee).Count(), Is.GreaterThan(0));
        Assert.That(loadedOrganisationPersonnel.Where(a => a.PersonnelType == PersonnelType.PersonInControl).Count(), Is.GreaterThan(0));
    }
}
