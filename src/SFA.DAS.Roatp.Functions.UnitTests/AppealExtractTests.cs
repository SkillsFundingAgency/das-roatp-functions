using EntityFrameworkCore.Testing.Moq;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Roatp.Functions.ApplyTypes;
using SFA.DAS.Roatp.Functions.Infrastructure.Databases;
using SFA.DAS.Roatp.Functions.Requests;
using SFA.DAS.Roatp.Functions.UnitTests.Generators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Roatp.Functions.UnitTests
{
    public class AppealExtractTests
    {
        private Mock<ILogger<AppealExtract>> _logger;
        private ApplyDataContext _applyDataContext;
        private Mock<IAsyncCollector<AppealFileExtractRequest>> _appealFileExtractQueue;
        private readonly TimerInfo _timerInfo = new TimerInfo(null, null, false);

        private Apply _applicationWithAppealNotYetSubmitted;
        private Apply _application;

        private AppealExtract _sut;

        [SetUp]
        public void Setup()
        {
            _logger = new Mock<ILogger<AppealExtract>>();
            _applyDataContext = Create.MockedDbContextFor<ApplyDataContext>();

            _applicationWithAppealNotYetSubmitted = ApplyGenerator.GenerateApplication(Guid.NewGuid(), "Unsuccessful", DateTime.Today.AddDays(-1))
                    .AddExtractedApplicationDetails(true, true, true, false)
                    .AddAppealDetails(null, true);

            _application = ApplyGenerator.GenerateApplication(Guid.NewGuid(), "Unsuccessful", DateTime.Today.AddDays(-1))
                    .AddExtractedApplicationDetails(true, true, true, false)
                    .AddAppealDetails(DateTime.Today, true);

            var applications = new List<Apply> { _applicationWithAppealNotYetSubmitted, _application };
            _applyDataContext.Set<Apply>().AddRange(applications);
            _applyDataContext.SaveChanges();

            _appealFileExtractQueue = new Mock<IAsyncCollector<AppealFileExtractRequest>>();

            _sut = new AppealExtract(_logger.Object, _applyDataContext);
        }

        [Test]
        public async Task Run_Logs_Information_Message()
        {
            await _sut.Run(_timerInfo, _appealFileExtractQueue.Object);

            _logger.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
        }

        [Test]
        public async Task GetAppealsToExtract_Contains_Expected_Appeals()
        {
            var actualResults = await _sut.GetAppealsToExtract();

            CollectionAssert.IsNotEmpty(actualResults);
            CollectionAssert.Contains(actualResults, _application.Appeal);
            CollectionAssert.DoesNotContain(actualResults, _applicationWithAppealNotYetSubmitted.Appeal);
        }

        [Test]
        public async Task EnqueueAppealFilesForExtract_Enqueues_Requests()
        {
            await _sut.EnqueueAppealFilesForExtract(_appealFileExtractQueue.Object, _application.Appeal);

            _appealFileExtractQueue.Verify(x => x.AddAsync(It.IsAny<AppealFileExtractRequest>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        [Test]
        public async Task MarkAppealFilesExtractedForApplication_Saves_AppealFilesExtracted_Entry()
        {
            var applicationId = _application.ApplicationId;

            await _sut.MarkAppealFilesExtractedForApplication(applicationId);

            var extractedApplication = _applyDataContext.ExtractedApplications.AsQueryable().SingleOrDefault(app => app.ApplicationId == applicationId);

            Assert.IsTrue(extractedApplication.AppealFilesExtracted);
        }
    }
}
