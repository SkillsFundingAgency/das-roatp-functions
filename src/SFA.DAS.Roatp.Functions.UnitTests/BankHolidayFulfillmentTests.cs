using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Roatp.Functions.Infrastructure.ApiClients;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EntityFrameworkCore.Testing.Moq;
using Microsoft.Azure.WebJobs;
using SFA.DAS.Roatp.Functions.BankHolidayTypes;
using SFA.DAS.Roatp.Functions.Infrastructure.Databases;
using Microsoft.EntityFrameworkCore;

namespace SFA.DAS.Roatp.Functions.UnitTests
{
    [TestFixture]
    public class BankHolidayFulfillmentTests
    {
        private Mock<ILogger<BankHolidayFulfillment>> _logger;
        private ApplyDataContext _applyDataContext;
        private Mock<IGovUkApiClient> _govUkApiClient;
        private readonly TimerInfo _timerInfo = new TimerInfo(null, null, false);

        private BankHoliday _bankHolidayAlreadyPresentInDatabase;
        private BankHoliday _bankHolidayNotPresentInDatabase;
        private BankHolidayRoot _bankHolidayRoot;

        private BankHolidayFulfillment _sut;

        [SetUp]
        public void Setup()
        {
            _logger = new Mock<ILogger<BankHolidayFulfillment>>();
            _applyDataContext = Create.MockedDbContextFor<ApplyDataContext>();
            _govUkApiClient = new Mock<IGovUkApiClient>();

            _bankHolidayAlreadyPresentInDatabase = new BankHoliday { BankHolidayDate = DateTime.Today };
            _bankHolidayNotPresentInDatabase = new BankHoliday { BankHolidayDate = DateTime.Today.AddDays(1) };

            _applyDataContext.Set<BankHoliday>().Add(_bankHolidayAlreadyPresentInDatabase);
            _applyDataContext.SaveChanges();

            _bankHolidayRoot = new BankHolidayRoot 
            { 
                EnglandAndWales = new BankHolidays 
                { 
                    Events = new List<Event> 
                    { 
                        new Event { Date = _bankHolidayAlreadyPresentInDatabase.BankHolidayDate },
                        new Event { Date = _bankHolidayNotPresentInDatabase.BankHolidayDate }
                    } 
                }
            };
            
            _govUkApiClient.Setup(x => x.GetBankHolidays()).ReturnsAsync(_bankHolidayRoot);

            _sut = new BankHolidayFulfillment(_applyDataContext, _logger.Object, _govUkApiClient.Object);
        }

        [Test]
        public async Task Run_Logs_Information_Message()
        {
            await _sut.Run(_timerInfo);

            _logger.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
        }

        [Test]
        public async Task Run_Gets_BankHolidays_From_GovUk_Api()
        {
            await _sut.Run(_timerInfo);

            var bankHolidays = await _applyDataContext.BankHoliday.AsNoTracking().ToListAsync();

            _govUkApiClient.Verify(x => x.GetBankHolidays(), Times.Once);
        }

        [Test]
        public async Task Run_Adds_New_BankHolidays_From_GovUk_Api_Into_Database()
        {
            var initialBankHolidays = await _applyDataContext.BankHoliday.AsNoTracking().ToListAsync();

            await _sut.Run(_timerInfo);

            var resultantBankHolidays = await _applyDataContext.BankHoliday.AsNoTracking().ToListAsync();

            CollectionAssert.AreNotEquivalent(initialBankHolidays, resultantBankHolidays);
            Assert.Greater(resultantBankHolidays.Count, initialBankHolidays.Count);
        }

        [Test]
        public async Task GetCurrentBankHolidays_Returns_Expected_BankHolidays()
        {
            var expectedBankHolidays = new List<BankHoliday> { _bankHolidayAlreadyPresentInDatabase };

            var actualBankHolidays = await _sut.GetCurrentBankHolidays();

            CollectionAssert.IsNotEmpty(actualBankHolidays);
            CollectionAssert.AreEquivalent(expectedBankHolidays, actualBankHolidays);
        }
    }
}
