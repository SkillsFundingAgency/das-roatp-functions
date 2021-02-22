using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Net.Http.Headers;

namespace SFA.DAS.Roatp.Functions.Infrastructure.Tokens
{
    public static class BearerTokenGenerator
    {
        public static AuthenticationHeaderValue GenerateToken(string tenantId, string clientId, string clientSecret, string resourceId)
        {
            var authority = $"https://login.microsoftonline.com/{tenantId}";
            var clientCredential = new ClientCredential(clientId, clientSecret);
            var context = new AuthenticationContext(authority, true);
            var result = context.AcquireTokenAsync(resourceId, clientCredential).GetAwaiter().GetResult();

            return new AuthenticationHeaderValue("Bearer", result.AccessToken);
        }
    }
}
