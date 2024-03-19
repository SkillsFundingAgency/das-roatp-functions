using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using EntityFrameworkCore.Testing.Moq;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SFA.DAS.Roatp.Functions.ApplyTypes;
using SFA.DAS.Roatp.Functions.Infrastructure.Databases;
using SFA.DAS.Roatp.Functions.UnitTests.Generators;

namespace SFA.DAS.Roatp.Functions.UnitTests
{
    public class AssessorExtractTests
    {
        private Mock<ILogger<AssessorExtract>> _logger;
        private ApplyDataContext _applyDataContext;
        private readonly TimerInfo _timerInfo = new TimerInfo();
        private Mock<ServiceBusClient> _serviceBusClient;
        private Mock<ServiceBusSender> _serviceBusSenderMock;

        private Apply _reviewInProgressApplication;
        private Apply _application;

        private AssessorExtract _sut;

        [SetUp]
        public void Setup()
        {
            _serviceBusSenderMock = new();
            _logger = new Mock<ILogger<AssessorExtract>>();
            _applyDataContext = Create.MockedDbContextFor<ApplyDataContext>();
            _serviceBusClient = new Mock<ServiceBusClient>();

            _serviceBusClient.Setup(x => x.CreateSender(It.IsAny<string>()))
                .Returns(_serviceBusSenderMock.Object);

            _reviewInProgressApplication = ApplyGenerator.GenerateApplication(Guid.NewGuid(), "GatewayAssessed", DateTime.Today.AddDays(-1))
                    .AddExtractedApplicationDetails(true, false, false, false)
                    .AddAssessorReviewDetails("In Progress", false);

            _application = ApplyGenerator.GenerateApplication(Guid.NewGuid(), "GatewayAssessed", DateTime.Today.AddDays(-1))
                    .AddExtractedApplicationDetails(true, false, false, false)
                    .AddAssessorReviewDetails("Approved", true);

            var applications = new List<Apply> { _reviewInProgressApplication, _application };
            _applyDataContext.Set<Apply>().AddRange(applications);
            _applyDataContext.SaveChanges();

            _sut = new AssessorExtract(_logger.Object, _applyDataContext, _serviceBusClient.Object);
        }

        [Test]
        public async Task Run_Logs_Information_Message()
        {
            await _sut.Run(_timerInfo);

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
        public async Task EnqueueAppealFilesForExtract_Enqueues_Requests()
        {
            await _sut.EnqueueAssessorFilesForExtract(_application);

            _serviceBusSenderMock.Verify(x => x.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        [Test]
        public async Task MarkAssessorFilesExtractedForApplication_Saves_AssessorFilesExtracted_Entry()
        {
            var applicationId = _application.ApplicationId;

            await _sut.MarkAssessorFilesExtractedForApplication(applicationId);

            var extractedApplication = _applyDataContext.ExtractedApplications.AsQueryable().SingleOrDefault(app => app.ApplicationId == applicationId);

            Assert.That(extractedApplication.AssessorFilesExtracted, Is.True);
        }
    }
}
