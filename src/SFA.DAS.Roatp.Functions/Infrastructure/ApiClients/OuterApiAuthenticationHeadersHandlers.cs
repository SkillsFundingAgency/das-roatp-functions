using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.Roatp.Functions.Configuration;

namespace SFA.DAS.Roatp.Functions.Infrastructure.ApiClients;

public class OuterApiAuthenticationHeadersHandlers(IOuterApiClientConfiguration _config) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Add("Ocp-Apim-Subscription-Key", _config.SubscriptionKey);
        request.Headers.Add("X-Version", _config.ApiVersion);

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
