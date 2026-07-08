namespace SFA.DAS.Roatp.Functions.Configuration;

public interface IOuterApiClientConfiguration
{
    string BaseUrl { get; }
    string SubscriptionKey { get; }
    string ApiVersion { get; }
}
