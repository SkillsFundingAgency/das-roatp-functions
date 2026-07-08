using System.Threading.Tasks;
using Refit;
using SFA.DAS.Roatp.Functions.BankHolidayTypes;

namespace SFA.DAS.Roatp.Functions.Infrastructure.ApiClients;

public interface IGovUkApiClient
{
    [Get("/bank-holidays.json")]
    Task<BankHolidayRoot> GetBankHolidays();
}
