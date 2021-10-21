using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.Roatp.Functions.BankHolidayTypes;
using SFA.DAS.Roatp.Functions.Infrastructure.ApiClients;
using SFA.DAS.Roatp.Functions.Infrastructure.Databases;

namespace SFA.DAS.Roatp.Functions
{
    public class BankHolidayFulfillment
    {
        private readonly ILogger<BankHolidayFulfillment> _logger;
        private readonly ApplyDataContext _applyDataContext;
        private readonly IGovUkApiClient _govUkApiClient;

        public BankHolidayFulfillment(ApplyDataContext applyDataContext, ILogger<BankHolidayFulfillment> logger, IGovUkApiClient govUkApiClient)
        {
            _applyDataContext = applyDataContext;
            _logger = logger;
            _govUkApiClient = govUkApiClient;
        }

        [FunctionName("BankHolidayFulfillment")]
        public async Task Run([TimerTrigger("%BankHolidayFulfillmentSchedule%", RunOnStartup = true)] TimerInfo myTimer)
         {
            if (myTimer.IsPastDue)
            {
                _logger.LogInformation("BankHolidayFulfillment function is running later than scheduled");
            }

            _logger.LogInformation($"BankHolidayFulfillment function executed at: {DateTime.Now}");

            var bankHolidays = await GetCurrentBankHolidays();
            
            var bankHolidaysFromExternalSource = await _govUkApiClient.GetBankHolidays();

            var bankHolidaysToProcess = new List<BankHoliday>();
            if (bankHolidaysFromExternalSource.EnglandAndWales?.Events?.Any()==true)
            {
                foreach (var bankHoliday in bankHolidaysFromExternalSource.EnglandAndWales.Events)
                {
                    if (!bankHolidays.Select(x => x.BankHolidayDate).Contains(bankHoliday.Date))
                    {
                        bankHolidaysToProcess.Add(new BankHoliday {BankHolidayDate = bankHoliday.Date});
                    }
                }
            }

            if (bankHolidaysToProcess.Any())
            {
                _logger.LogInformation($"BankHolidayFulfillment function populating {bankHolidaysToProcess.Count} records");
                await _applyDataContext.BankHoliday.AddRangeAsync(bankHolidaysToProcess); 
                await _applyDataContext.SaveChangesAsync();
            }
            else
            {
                _logger.LogInformation("BankHolidayFulfillment function added no records as no new ones found");
            }
        }

        public async Task<List<BankHoliday>> GetCurrentBankHolidays()
        {
            _logger.LogDebug($"Getting list of bank holidays to process");

            var bankHolidays = await _applyDataContext.BankHoliday
                .AsNoTracking().ToListAsync();

            return bankHolidays;
        }
    }
}
