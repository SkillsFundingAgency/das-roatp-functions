using Microsoft.Extensions.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;

namespace SFA.DAS.Roatp.Functions.Extensions;

public static class AddConfigurationExtension
{
    public static void AddConfiguration(this IConfigurationBuilder builder)
    {
        var configuration = builder.Build();

        StorageOptions opt = new()
        {
            ConfigurationKeys = configuration["ConfigNames"].Split(','),
            StorageConnectionString = configuration["ConfigurationStorageConnectionString"],
            EnvironmentName = configuration["EnvironmentName"],
            PreFixConfigurationKeys = false
        };

        builder.AddAzureTableStorage(options =>
        {
            options.ConfigurationKeys = configuration["ConfigNames"].Split(',');
            options.StorageConnectionString = configuration["ConfigurationStorageConnectionString"];
            options.EnvironmentName = configuration["EnvironmentName"];
            options.PreFixConfigurationKeys = false;
        });
    }
}
