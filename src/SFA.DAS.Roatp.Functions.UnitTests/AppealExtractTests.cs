using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EntityFrameworkCore.Testing.Moq;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Roatp.Functions.ApplyTypes;
using SFA.DAS.Roatp.Functions.Infrastructure.Databases;
using SFA.DAS.Roatp.Functions.Requests;
using SFA.DAS.Roatp.Functions.UnitTests.Generators;

namespace SFA.DAS.Roatp.Functions.UnitTests;

public class AppealExtractTests
{
    private Mock<ILogger<AppealExtract>> _logger;
    private ApplyDataContext _applyDataContext;
    private Mock<IEnumerable<AppealFileExtractRequest>> _appealFileExtractQueue;
    private readonly TimerInfo _timerInfo = new();

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

        _appealFileExtractQueue = new Mock<IEnumerable<AppealFileExtractRequest>>();

        _sut = new AppealExtract(_logger.Object, _applyDataContext);
    }

    [Test]
    public async Task Run_Logs_Information_Message()
    {
        await _sut.Run(_timerInfo);

        _logger.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
    }

    [Test]
    public async Task GetAppealsToExtract_Contains_Expected_Appeals()
    {
        var actualResults = await _sut.GetAppealsToExtract();

        Assert.That(actualResults, Is.Not.Empty);
        Assert.That(actualResults, Contains.Item(_application.Appeal));
        Assert.That(actualResults, Does.Not.Contain(_applicationWithAppealNotYetSubmitted.Appeal));
    }

    //[Test]
    //public async Task EnqueueAppealFilesForExtract_Enqueues_Requests()
    //{
    //    await _sut.EnqueueAppealFilesForExtract(_appealFileExtractQueue.Object, _application.Appeal);

    //    _appealFileExtractQueue.Verify(x => x.AddAsync(It.IsAny<AppealFileExtractRequest>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    //}

    [Test]
    public async Task MarkAppealFilesExtractedForApplication_Saves_AppealFilesExtracted_Entry()
    {
        var applicationId = _application.ApplicationId;

        await _sut.MarkAppealFilesExtractedForApplication(applicationId);

        var extractedApplication = _applyDataContext.ExtractedApplications.AsQueryable().SingleOrDefault(app => app.ApplicationId == applicationId);

        Assert.That(extractedApplication.AppealFilesExtracted, Is.True);
    }
}
