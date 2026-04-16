using System.Threading.Tasks;

namespace SFA.DAS.Roatp.Functions.Infrastructure.ApiClients
{
    public interface IRoatpOuterApiClient
    {
        Task UpdateProviderNames();
    }
}
