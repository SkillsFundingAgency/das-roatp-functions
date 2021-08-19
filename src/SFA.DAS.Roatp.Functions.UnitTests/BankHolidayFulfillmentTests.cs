using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Roatp.Functions.Infrastructure.ApiClients;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private DateTime _bankHolidayAlreadyPresent;
        private DateTime _bankHolidayAdded;

        [SetUp]
        public void Setup()
        {
            _logger = new Mock<ILogger<BankHolidayFulfillment>>();
            _apiClient = new Mock<IGovUkApiClient>();
            _bankHolidayAlreadyPresent = DateTime.Today;
            _bankHolidayAdded = DateTime.Today.AddDays(1);

            _bankHolidayRoot = new BankHolidayRoot { EnglandAndWales = new BankHolidays { Events = new List<Event> { new Event { Date = _bankHolidayAdded }, new Event {Date = _bankHolidayAlreadyPresent}} }};
            _applyDataContext = Create.MockedDbContextFor<ApplyDataContext>();
            _apiClient.Setup(x => x.GetBankHolidays()).ReturnsAsync(_bankHolidayRoot);
            var bankHolidays = new List<BankHoliday> { new BankHoliday {BankHolidayDate = _bankHolidayAlreadyPresent} };
            _applyDataContext.Set<BankHoliday>().AddRange(bankHolidays);
            _applyDataContext.SaveChanges();

        }

        [Test]
        public async Task Run_BankHolidayFulfillment_Log_Message()
        {
            _bankHolidayFulfillment = new BankHolidayFulfillment(_applyDataContext, _logger.Object, _apiClient.Object);
            _bankHolidayFulfillment.Run(_timerInfo);
            _logger.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
        }

        [Test]
        public async Task Run_BankHolidayFulfillment_GettingBankHolidays()
        {
            _bankHolidayFulfillment = new BankHolidayFulfillment(_applyDataContext, _logger.Object, _apiClient.Object);
            _bankHolidayFulfillment.Run(_timerInfo);
            _apiClient.Verify(x => x.GetBankHolidays(), Times.Once);
            _logger.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
        }

        [Test]
        public async Task Run_BankHolidayFulfillment_GettingCurrentBankHolidays()
        {
            _bankHolidayFulfillment = new BankHolidayFulfillment(_applyDataContext, _logger.Object, _apiClient.Object);
            _bankHolidayFulfillment.Run(_timerInfo);
            var submittedBankHolidays = _applyDataContext.BankHoliday.ToList();

            Assert.IsTrue(submittedBankHolidays.Any(x=>x.BankHolidayDate==_bankHolidayAdded));
            Assert.IsTrue(submittedBankHolidays.Any(x => x.BankHolidayDate == _bankHolidayAlreadyPresent));

        }
    }
}
