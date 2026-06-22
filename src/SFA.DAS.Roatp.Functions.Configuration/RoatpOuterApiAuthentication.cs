namespace SFA.DAS.Roatp.Functions.Configuration;

public class RoatpOuterApiAuthentication : IOuterApiClientConfiguration
{
    public string ApiBaseUrl { get; set; }
    public string SubscriptionKey { get; set; }
    public string ApiVersion { get; set; }
}
