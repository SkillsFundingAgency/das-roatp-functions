using System.Threading.Tasks;
using SFA.DAS.Roatp.Functions.ApplyTypes;
using SFA.DAS.Roatp.Functions.BankHolidayTypes;

namespace SFA.DAS.Roatp.Functions.Infrastructure.ApiClients
{
    public interface IGovUkApiClient
    {
        Task<BankHolidayRoot> GetBankHolidays();
    }
}