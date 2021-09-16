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
    public class GatewayExtractTests
    {
        private Mock<ILogger<GatewayExtract>> _logger;
        private ApplyDataContext _applyDataContext;
        private Mock<IAsyncCollector<AdminFileExtractRequest>> _adminFileExtractQueue;
        private readonly TimerInfo _timerInfo = new TimerInfo(null, null, false);

        private Apply _reviewInProgressApplication;
        private Apply _application;

        private GatewayExtract _sut;

        [SetUp]
        public void Setup()
        {
            _logger = new Mock<ILogger<GatewayExtract>>();
            _applyDataContext = Create.MockedDbContextFor<ApplyDataContext>();

            _reviewInProgressApplication = ApplyGenerator.GenerateApplication(Guid.NewGuid(), "GatewayAssessed", DateTime.Today.AddDays(-1))
                    .AddExtractedApplicationDetails(false, false, false, false)
                    .AddGatewayReviewDetails("In Progress", false);

            _application = ApplyGenerator.GenerateApplication(Guid.NewGuid(), "GatewayAssessed", DateTime.Today.AddDays(-1))
                    .AddExtractedApplicationDetails(false, false, false, false)
                    .AddGatewayReviewDetails("Pass", true);

            var applications = new List<Apply> { _reviewInProgressApplication, _application };
            _applyDataContext.Set<Apply>().AddRange(applications);
            _applyDataContext.SaveChanges();

            _adminFileExtractQueue = new Mock<IAsyncCollector<AdminFileExtractRequest>>();

            _sut = new GatewayExtract(_logger.Object, _applyDataContext);
        }

        [Test]
        public async Task Run_Logs_Information_Message()
        {
            await _sut.Run(_timerInfo, _adminFileExtractQueue.Object);

            _logger.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
        }

        [Test]
        public async Task GetApplicationsToExtract_Contains_Expected_Applications()
        {
            var actualResults = await _sut.GetApplicationsToExtract();

            CollectionAssert.IsNotEmpty(actualResults);
            CollectionAssert.Contains(actualResults, _application);
            CollectionAssert.DoesNotContain(actualResults, _reviewInProgressApplication);
        }

        [Test]
        public async Task EnqueueGatewayFilesForExtract_Enqueues_Requests()
        {
            await _sut.EnqueueGatewayFilesForExtract(_adminFileExtractQueue.Object, _application);

            _adminFileExtractQueue.Verify(x => x.AddAsync(It.IsAny<AdminFileExtractRequest>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        [Test]
        public async Task MarkGatewayFilesExtractedForApplication_Saves_FinanceFilesExtracted_Entry()
        {
            var applicationId = _application.ApplicationId;

            await _sut.MarkGatewayFilesExtractedForApplication(applicationId);

            var extractedApplication = _applyDataContext.ExtractedApplications.AsQueryable().SingleOrDefault(app => app.ApplicationId == applicationId);

            Assert.IsTrue(extractedApplication.GatewayFilesExtracted);
        }
    }
}
