using System.Threading.Tasks;
using SFA.DAS.Roatp.Functions.ApplyTypes;

namespace SFA.DAS.Roatp.Functions.Infrastructure.ApiClients
{
    public interface IGovUkApiClient
    {
        Task<BankHolidayRoot> GetBankHolidays();
    }
}