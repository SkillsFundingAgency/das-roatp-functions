using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Roatp.Functions.Infrastructure.ApiClients;

[ExcludeFromCodeCoverage]
public class DefaultHeadersHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Add("Accept", "application/json");
        return await base.SendAsync(request, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
    }
}
