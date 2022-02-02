using EntityFrameworkCore.Testing.Moq;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.QnA.Api.Types;
using SFA.DAS.Roatp.Functions.ApplyTypes;
using SFA.DAS.Roatp.Functions.Infrastructure.ApiClients;
using SFA.DAS.Roatp.Functions.Infrastructure.Databases;
using SFA.DAS.Roatp.Functions.Requests;
using SFA.DAS.Roatp.Functions.UnitTests.Generators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using SFA.DAS.QnA.Api.Types.Page;
using SFA.DAS.Roatp.Functions.Services;

namespace SFA.DAS.Roatp.Functions.UnitTests
{
    public class ApplicationExtractTests
    {
        private Mock<ILogger<ApplicationExtract>> _logger;
        private Mock<IQnaApiClient> _qnaApiClient;
        private Mock<ISectorProcessingService> _sectorProcessingService;
        private ApplyDataContext _applyDataContext;
        private Mock<IAsyncCollector<ApplyFileExtractRequest>> _applyFileExtractQueue;
        private readonly TimerInfo _timerInfo = new TimerInfo(null, null, false);

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

            _applyFileExtractQueue = new Mock<IAsyncCollector<ApplyFileExtractRequest>>();

            _sut = new ApplicationExtract(_logger.Object, _applyDataContext, _qnaApiClient.Object, _sectorProcessingService.Object);
        }

        [Test]
        public async Task Run_Logs_Information_Message()
        {
            await _sut.Run(_timerInfo, _applyFileExtractQueue.Object);

            _logger.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
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

            Assert.IsNotNull(actualQuestion);
            Assert.AreEqual(expectedQuestion.ApplicationId, actualQuestion.ApplicationId);
            Assert.AreEqual(expectedQuestion.SequenceNumber, actualQuestion.SequenceNumber);
            Assert.AreEqual(expectedQuestion.SectionNumber, actualQuestion.SectionNumber);
            Assert.AreEqual(expectedQuestion.PageId, actualQuestion.PageId);
            Assert.AreEqual(expectedQuestion.QuestionId, actualQuestion.QuestionId);
            Assert.AreEqual(expectedQuestion.QuestionType, actualQuestion.QuestionType);
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

            Assert.IsNotNull(actualAnswer);
            Assert.AreEqual(expectedAnswer.Answer, actualAnswer.Answer);
            Assert.AreEqual(expectedAnswer.ColumnHeading, actualAnswer.ColumnHeading);
            Assert.AreEqual(expectedAnswer.RowNumber, actualAnswer.RowNumber);
            Assert.AreEqual(expectedAnswer.ColumnNumber, actualAnswer.ColumnNumber);
        }

        [Test]
        public async Task SaveExtractedAnswersForApplication_Saves_Answers()
        {
            var applicationId = _application.ApplicationId;
            var applicationAnswers = await _sut.ExtractAnswersForApplication(applicationId);

            await _sut.SaveExtractedAnswersForApplication(applicationId, applicationAnswers);

            var submittedAnswers = _applyDataContext.SubmittedApplicationAnswers.AsQueryable().Where(app => app.ApplicationId == applicationId).ToList();

            CollectionAssert.IsNotEmpty(submittedAnswers);
            Assert.AreEqual(applicationAnswers.Count, submittedAnswers.Count);
        }

        [Test]
        public async Task SaveExtractedAnswersForApplication_Saves_ApplicationExtracted_Entry()
        {
            var applicationId = _application.ApplicationId;
            var applicationAnswers = await _sut.ExtractAnswersForApplication(applicationId);

            await _sut.SaveExtractedAnswersForApplication(applicationId, applicationAnswers);

            var extractedApplication = _applyDataContext.ExtractedApplications.AsQueryable().SingleOrDefault(app => app.ApplicationId == applicationId);

            Assert.IsNotNull(extractedApplication);
        }


        [Test]
        public async Task SaveSectorDetailsForApplication_Saves_Sectors_Experts_And_TrainingTypes_Entry()
        {
            const string businessDeliveredTrainingType = "other business training";
            var applicationId = _application.ApplicationId;
            
            var optionAgriculture = new Option
            {
                Value = "Agriculture&comma; environmental and animal care",
                Label = "Agriculture, environmental and animal care"
            };
            var optionBusiness = new Option
            {
                Value = "Business and administration",
                Label = "Business and administration"
            };
            var options = new List<Option>
            {
                optionAgriculture,
                optionBusiness
            };
            var section = QnaGenerator.GenerateSection(applicationId, 7, 6, "7600", "DAT-7600", "CheckboxList",
                "Agriculture&comma; environmental and animal care,Business and administration", options);
            var sections = new List<Section> { section };
            var sectorAgriculture = new OrganisationSectors
            {
                SectorName = "Agriculture, environmental and animal care",
                OrganisationSectorExperts = new List<OrganisationSectorExperts>()
            };
            var sectorExpertAgriculture = new OrganisationSectorExperts
            {
                FirstName = "Aggie"
            };

            var sectorAgricultureTrainingTypes = new List<OrganisationSectorExpertDeliveredTrainingTypes>
            {
                new OrganisationSectorExpertDeliveredTrainingTypes
                {
                    DeliveredTrainingType = "other agriculture training"
                }
            };

            var sectorBusiness = new OrganisationSectors
            {
                SectorName = "Business and administration",
                OrganisationSectorExperts = new List<OrganisationSectorExperts>()
            };
            var sectorExpertBusiness = new OrganisationSectorExperts
            {
                FirstName = "Brian"
            };

            var sectorBusinessTrainingTypes = new List<OrganisationSectorExpertDeliveredTrainingTypes>
            {
                new OrganisationSectorExpertDeliveredTrainingTypes
                {
                    DeliveredTrainingType = businessDeliveredTrainingType
                },
                new OrganisationSectorExpertDeliveredTrainingTypes
                {
                    DeliveredTrainingType = "e-learning business training"
                }
            };

            _qnaApiClient.Setup(x => x.GetAllSectionsForApplication(applicationId)).ReturnsAsync(sections);
            _sectorProcessingService.Setup(x =>
                x.GatherSectorDetails(It.IsAny<IReadOnlyCollection<SubmittedApplicationAnswer>>(), It.IsAny<Guid>(),
                    "Agriculture&comma; environmental and animal care")).ReturnsAsync(sectorAgriculture);
            _sectorProcessingService.Setup(x =>
                x.GatherSectorExpertsDetails(It.IsAny<IReadOnlyCollection<SubmittedApplicationAnswer>>(), 
                    "Agriculture&comma; environmental and animal care")).ReturnsAsync(sectorExpertAgriculture);
            _sectorProcessingService.Setup(x =>
                x.GatherSectorDeliveredTrainingTypes(It.IsAny<IReadOnlyCollection<SubmittedApplicationAnswer>>(),
                    "Agriculture&comma; environmental and animal care")).ReturnsAsync(sectorAgricultureTrainingTypes);
            _sectorProcessingService.Setup(x =>
                x.GatherSectorDetails(It.IsAny<IReadOnlyCollection<SubmittedApplicationAnswer>>(), It.IsAny<Guid>(),
                    "Business and administration")).ReturnsAsync(sectorBusiness);
            _sectorProcessingService.Setup(x =>
                x.GatherSectorExpertsDetails(It.IsAny<IReadOnlyCollection<SubmittedApplicationAnswer>>(),
                    "Business and administration")).ReturnsAsync(sectorExpertBusiness);
            _sectorProcessingService.Setup(x =>
                x.GatherSectorDeliveredTrainingTypes(It.IsAny<IReadOnlyCollection<SubmittedApplicationAnswer>>(),
                    "Business and administration")).ReturnsAsync(sectorBusinessTrainingTypes);

            var applicationAnswers = await _sut.ExtractAnswersForApplication(applicationId);

            await _sut.SaveSectorDetailsForApplication(applicationId, applicationAnswers);

            var actualSectors = _applyDataContext.OrganisationSectors.ToList();
            var actualSectorExperts = _applyDataContext.OrganisationSectorExperts.ToList();
            var actualDeliveredTrainingTypes = _applyDataContext.OrganisationSectorExpertDeliveredTrainingTypes.ToList();

            Assert.AreEqual(2,actualSectors.Count);
            Assert.AreEqual(2, actualSectorExperts.Count); 
            Assert.AreEqual(3, actualDeliveredTrainingTypes.Count);
            var actualSector = actualSectors.FirstOrDefault(x => x.SectorName ==sectorBusiness.SectorName);
            Assert.AreEqual(actualSector.SectorName,sectorBusiness.SectorName);
            var actualSectorExpert = actualSectorExperts.FirstOrDefault(x => x.OrganisationSectorId == actualSector.Id);
            Assert.AreEqual(actualSectorExpert.FirstName, sectorExpertBusiness.FirstName);
            var actualTrainingTypes = actualDeliveredTrainingTypes.Where(x=> x.OrganisationSectorExpertId == actualSectorExpert.Id);
            Assert.AreEqual(2,actualTrainingTypes.Count());
            Assert.AreEqual(1, actualTrainingTypes.Count(x=>x.DeliveredTrainingType==businessDeliveredTrainingType));
        }

        [Test]
        public async Task EnqueueApplyFilesForExtract_Enqueues_Requests()
        {
            var applicationId = _application.ApplicationId;
            var applicationAnswers = await _sut.ExtractAnswersForApplication(applicationId);

            await _sut.EnqueueApplyFilesForExtract(_applyFileExtractQueue.Object, applicationAnswers);

            _applyFileExtractQueue.Verify(x => x.AddAsync(It.IsAny<ApplyFileExtractRequest>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }
    }
}
