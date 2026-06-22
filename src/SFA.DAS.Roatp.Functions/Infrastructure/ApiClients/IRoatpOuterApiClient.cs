using System.Threading.Tasks;
using Refit;

namespace SFA.DAS.Roatp.Functions.Infrastructure.ApiClients;

public interface IRoatpOuterApiClient
{
    [Post("/providers/update-names")]
    Task UpdateProviderNames();
}
