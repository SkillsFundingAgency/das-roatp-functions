using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EntityFrameworkCore.Testing.Moq;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.QnA.Api.Types;
using SFA.DAS.Roatp.Functions.ApplyTypes;
using SFA.DAS.Roatp.Functions.Infrastructure.ApiClients;
using SFA.DAS.Roatp.Functions.Infrastructure.BlobStorage;
using SFA.DAS.Roatp.Functions.Infrastructure.Databases;
using SFA.DAS.Roatp.Functions.UnitTests.Generators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Roatp.Functions.UnitTests
{
    public class FileExtractTests
    {
        private Mock<ILogger<FileExtract>> _logger;
        private Mock<IQnaApiClient> _qnaApiClient;
        private Mock<IDatamartBlobStorageFactory> _datamartBlobStorageFactory;

        private Mock<BlobContainerClient> _blobContainerClient;

        private ApplyDataContext _applyDataContext;

        private readonly TimerInfo _timerInfo = new TimerInfo(null, null, false);

        private ExtractedApplication _extractedApplication;
        private List<Section> _sections;

        private FileExtract _sut;

        [SetUp]
        public void Setup()
        {
            _logger = new Mock<ILogger<FileExtract>>();
            _qnaApiClient = new Mock<IQnaApiClient>();

            _applyDataContext = Create.MockedDbContextFor<ApplyDataContext>();

            var application = ApplyGenerator.GenerateApplication(Guid.NewGuid(), "Submitted", DateTime.Today.AddDays(-1));
            _applyDataContext.Set<Apply>().Add(application);
            _applyDataContext.SaveChanges();

            _extractedApplication = ExtractedApplicationGenerator.GenerateExtractedApplication(application, DateTime.Today, false);
            _applyDataContext.Set<ExtractedApplication>().Add(_extractedApplication);
            _applyDataContext.SaveChanges();

            _sections = QnaGenerator.GenerateSectionsForApplication(_extractedApplication.ApplicationId);
            _qnaApiClient.Setup(x => x.GetAllSectionsForApplication(_extractedApplication.ApplicationId)).ReturnsAsync(_sections);
            _qnaApiClient.Setup(x => x.DownloadFile(_extractedApplication.ApplicationId, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new MemoryStream());

            var submittedAnswers = SubmittedAnswerGenerator.GenerateSubmittedAnswers(_extractedApplication.ApplicationId, _sections);
            _applyDataContext.Set<SubmittedApplicationAnswer>().AddRange(submittedAnswers);
            _applyDataContext.SaveChanges();

            SetupDatamartBlobStorageFactory();

            _sut = new FileExtract(_logger.Object, _applyDataContext, _qnaApiClient.Object, _datamartBlobStorageFactory.Object);
        }

        private void SetupDatamartBlobStorageFactory()
        {
            _blobContainerClient = new Mock<BlobContainerClient>();
            _blobContainerClient.Setup(x => x.CreateIfNotExistsAsync(It.IsAny<PublicAccessType>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<BlobContainerEncryptionScopeOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<Azure.Response<BlobContainerInfo>>());

            _datamartBlobStorageFactory = new Mock<IDatamartBlobStorageFactory>();
            _datamartBlobStorageFactory.Setup(fac => fac.GetQnABlobContainerClient()).ReturnsAsync(_blobContainerClient.Object);
        }

        [Test]
        public async Task Run_Logs_Information_Message()
        {
            await _sut.Run(_timerInfo);

            _logger.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
        }

        [Test]
        public async Task GetApplicationsToExtractQnaFiles_Contains_Expected_Applications()
        {
            var expectedApplicationId = _extractedApplication.ApplicationId;

            var actualResults = await _sut.GetApplicationsToExtractQnaFiles();

            CollectionAssert.IsNotEmpty(actualResults);
            CollectionAssert.Contains(actualResults, expectedApplicationId);
        }

        [Test]
        public async Task GetQnaFilesToExtractForApplication_Contains_Expected_Answers()
        {
            var fileUploadSection = _sections[2];
            var firstPage = fileUploadSection.QnAData.Pages[0];
            var fileUploadQuestion = firstPage.Questions[0];

            var expectedAnswer = new SubmittedApplicationAnswer
            {
                ApplicationId = _extractedApplication.ApplicationId,
                SequenceNumber = fileUploadSection.SequenceNo,
                SectionNumber = fileUploadSection.SectionNo,
                PageId = firstPage.PageId,
                QuestionId = fileUploadQuestion.QuestionId,
                QuestionType = fileUploadQuestion.Input.Type,
            };

            var extractedAnswers = await _sut.GetQnaFilesToExtractForApplication(_extractedApplication.ApplicationId);
            var actualAnswer = extractedAnswers.FirstOrDefault(x => x.PageId == expectedAnswer.PageId && x.QuestionId == expectedAnswer.QuestionId);

            Assert.IsNotNull(actualAnswer);
            Assert.AreEqual(expectedAnswer.ApplicationId, actualAnswer.ApplicationId);
            Assert.AreEqual(expectedAnswer.SequenceNumber, actualAnswer.SequenceNumber);
            Assert.AreEqual(expectedAnswer.SectionNumber, actualAnswer.SectionNumber);
            Assert.AreEqual(expectedAnswer.PageId, actualAnswer.PageId);
            Assert.AreEqual(expectedAnswer.QuestionId, actualAnswer.QuestionId);
            Assert.AreEqual(expectedAnswer.QuestionType, actualAnswer.QuestionType);
        }

        [Test]
        public async Task SaveQnaFilesIntoDatamartForApplication_Saves_File_Into_BlobStorage()
        {
            var applicationId = _extractedApplication.ApplicationId;
            var applicationAnswers = await _sut.GetQnaFilesToExtractForApplication(applicationId);

            await _sut.SaveQnaFilesIntoDatamartForApplication(applicationId, applicationAnswers);

            _blobContainerClient.Verify(x => x.UploadBlobAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        [Test]
        public async Task SaveQnaFilesIntoDatamartForApplication_Saves_QnaFilesExtracted_Entry()
        {
            var applicationId = _extractedApplication.ApplicationId;
            var applicationAnswers = await _sut.GetQnaFilesToExtractForApplication(applicationId);

            await _sut.SaveQnaFilesIntoDatamartForApplication(applicationId, applicationAnswers);

            var extractedApplication = _applyDataContext.ExtractedApplications.AsQueryable().SingleOrDefault(app => app.ApplicationId == applicationId);

            Assert.IsNotNull(extractedApplication);
            Assert.IsTrue(extractedApplication.QnaFilesExtracted);
        }
    }
}
