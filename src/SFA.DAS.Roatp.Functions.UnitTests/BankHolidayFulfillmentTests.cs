using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Roatp.Functions.ApplyTypes;
using SFA.DAS.Roatp.Functions.Infrastructure.ApiClients;
using SFA.DAS.Roatp.Functions.Infrastructure.BlobStorage;
using SFA.DAS.Roatp.Functions.Requests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EntityFrameworkCore.Testing.Moq;
using Microsoft.Azure.WebJobs;
using SFA.DAS.Roatp.Functions.BankHolidayTypes;
using SFA.DAS.Roatp.Functions.Infrastructure.Databases;

namespace SFA.DAS.Roatp.Functions.UnitTests
{
    public class BankHolidayFulfillmentTests
    {
        private Mock<ILogger<BankHolidayFulfillment>> _logger;
        private Mock<IGovUkApiClient> _apiClient;
        private BankHolidayRoot _bankHolidayRoot;
        private ApplyDataContext _applyDataContext;
        private BankHolidayFulfillment _bankHolidayFulfillment;
        private readonly TimerInfo _timerInfo = new TimerInfo(null, null, false);

        [SetUp]
        public void Setup()
        {
            _logger = new Mock<ILogger<BankHolidayFulfillment>>();
            _apiClient = new Mock<IGovUkApiClient>();
            _bankHolidayRoot = new BankHolidayRoot();
            _applyDataContext = Create.MockedDbContextFor<ApplyDataContext>();
            _apiClient.Setup(x => x.GetBankHolidays()).ReturnsAsync(_bankHolidayRoot);
        }

        
        [Test]
        public async Task Run_BankHolidayFulfillment_Log_Message()
        {
            _bankHolidayFulfillment = new BankHolidayFulfillment(_applyDataContext, _logger.Object, _apiClient.Object);
            _bankHolidayFulfillment.Run(_timerInfo);
            _apiClient.Verify(x=>x.GetBankHolidays(),Times.Once);
            _logger.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
        }
    }
}
