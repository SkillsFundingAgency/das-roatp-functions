using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Roatp.Functions.ApplyTypes;
using SFA.DAS.Roatp.Functions.BankHolidayTypes;

namespace SFA.DAS.Roatp.Functions.Infrastructure.ApiClients
{
    public class GovUkApiClient: ApiClientBase<GovUkApiClient>, IGovUkApiClient
    {
        public GovUkApiClient(HttpClient client, ILogger<GovUkApiClient> logger)
            : base(client, logger)
        {
        }
        public async Task<BankHolidayRoot> GetBankHolidays()
        {
            return await Get<BankHolidayRoot>("/bank-holidays.json");
        }
    }
}
