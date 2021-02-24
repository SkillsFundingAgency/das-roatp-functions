﻿using EntityFrameworkCore.Testing.Moq;
using EntityFrameworkCore.Testing.Moq.Extensions;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.QnA.Api.Types;
using SFA.DAS.Roatp.Functions.ApplyTypes;
using SFA.DAS.Roatp.Functions.Infrastructure.ApiClients;
using SFA.DAS.Roatp.Functions.Infrastructure.Databases;
using SFA.DAS.Roatp.Functions.UnitTests.Generators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Roatp.Functions.UnitTests
{
    public class ApplicationExtractTests
    {
        private Mock<ILogger<ApplicationExtract>> _logger;
        private Mock<IQnaApiClient> _qnaApiClient;
        private ApplyDataContext _applyDataContext;

        private readonly TimerInfo _timerInfo = new TimerInfo(null, null, false);

        private Apply _application;
        private List<Section> _sections;

        private ApplicationExtract _sut;

        [SetUp]
        public void Setup()
        {
            _logger = new Mock<ILogger<ApplicationExtract>>();
            _qnaApiClient = new Mock<IQnaApiClient>();
            _applyDataContext = Create.MockedDbContextFor<ApplyDataContext>();

            _application = ApplyGenerator.GenerateApplication(Guid.NewGuid(), DateTime.Today.AddDays(-1));

            var applications = new List<Apply> { _application };
            _applyDataContext.Set<Apply>().AddRange(applications);
            _applyDataContext.Set<Apply>().AddFromSqlRawResult(applications);
            _applyDataContext.SaveChanges();

            _sections = QnaGenerator.GenerateSectionsForApplication(_application.ApplicationId);
            _qnaApiClient.Setup(x => x.GetAllSectionsForApplication(_application.ApplicationId)).ReturnsAsync(_sections);

            _sut = new ApplicationExtract(_logger.Object, _applyDataContext, _qnaApiClient.Object);
        }

        [Test]
        public async Task Run_Logs_Information_Message()
        {
            await _sut.Run(_timerInfo);

            _logger.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Test]
        public async Task GetApplicationsToExtract_Contains_Expected_Result()
        {
            var expectedApplicationId = _application.ApplicationId;
            var executionDateTime = _application.ApplyData.ApplyDetails.ApplicationSubmittedOn.Value.Date.AddDays(1);

            var actualResults = await _sut.GetApplicationsToExtract(executionDateTime);

            CollectionAssert.IsNotEmpty(actualResults);
            CollectionAssert.Contains(actualResults, expectedApplicationId);
        }

        [Test]
        public async Task ExtractAnswersForApplication_Contains_Expected_Result()
        {
            var firstPage = _sections[0].QnAData.Pages[0];
            var firstPageAnswer = firstPage.PageOfAnswers[0].Answers[0];

            var expectedResult = new SubmittedApplicationAnswer
            {
                ApplicationId = _application.ApplicationId,
                PageId = firstPage.PageId,
                QuestionId = firstPageAnswer.QuestionId,
                Answer = firstPageAnswer.Value
            };

            var applicationAnswers = await _sut.ExtractAnswersForApplication(_application.ApplicationId);
            var actualResult = applicationAnswers.FirstOrDefault(x => x.PageId == expectedResult.PageId);

            _qnaApiClient.Verify(x => x.GetAllSectionsForApplication(_application.ApplicationId), Times.Once);

            Assert.IsNotNull(actualResult);
            Assert.AreEqual(expectedResult.ApplicationId, actualResult.ApplicationId);
            Assert.AreEqual(expectedResult.PageId, actualResult.PageId);
            Assert.AreEqual(expectedResult.QuestionId, actualResult.QuestionId);
            Assert.AreEqual(expectedResult.Answer, actualResult.Answer);
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
    }
}