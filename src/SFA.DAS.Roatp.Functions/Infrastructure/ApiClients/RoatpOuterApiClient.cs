using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.Roatp.Functions.Infrastructure.ApiClients
{
    public class RoatpOuterApiClient : ApiClientBase<RoatpOuterApiClient>, IRoatpOuterApiClient
    {
        public RoatpOuterApiClient(HttpClient client, ILogger<RoatpOuterApiClient> logger)
            : base(client, logger)
        {
        }

        public async Task UpdateProviderNames()
        {
            var response = await GetResponse("Providers/update-names");
        }
    }
}
