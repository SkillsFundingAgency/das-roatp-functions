using Microsoft.Extensions.Logging;
using SFA.DAS.Roatp.Functions.ApplyTypes;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.Roatp.Functions.Infrastructure.ApiClients
{
    public class ApplyApiClient : ApiClientBase<ApplyApiClient>, IApplyApiClient
    {
        public ApplyApiClient(HttpClient client, ILogger<ApplyApiClient> logger)
            : base(client, logger)
        {
        }

        public async Task<IEnumerable<Apply>> GetApplicationsToExtract()
        {
            return await Get<List<Apply>>($"ApplicationExtract/Applications");
        }
    }
}
