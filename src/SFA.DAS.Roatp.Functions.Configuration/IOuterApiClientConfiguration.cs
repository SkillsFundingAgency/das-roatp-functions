namespace SFA.DAS.Roatp.Functions.Configuration;

public interface IOuterApiClientConfiguration
{
    string ApiBaseUrl { get; }
    string SubscriptionKey { get; }
    string ApiVersion { get; }
}
