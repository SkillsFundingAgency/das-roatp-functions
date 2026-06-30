using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Configuration;

namespace SFA.DAS.Roatp.Functions;

public interface IServiceBusQueueInitializer
{
    Task InitializeAsync();
}

[ExcludeFromCodeCoverage]
public class ServiceBusQueueInitializer : IServiceBusQueueInitializer
{
    private readonly IConfiguration _configuration;
    private readonly ServiceBusAdministrationClient _administrationClient;

    public ServiceBusQueueInitializer(IConfiguration configuration, ServiceBusAdministrationClient administrationClient)
    {
        _configuration = configuration;
        _administrationClient = administrationClient;
    }

    public async Task InitializeAsync()
    {
        var lockDuration = TimeSpan.FromMinutes(5);
        const int maxDeliveryCount = 10;
        const int maxSizeInMegabytes = 5120;

        await EnsureQueueExistsAsync(_configuration["ApplyFileExtractQueue"], lockDuration, maxDeliveryCount, maxSizeInMegabytes);
        await EnsureQueueExistsAsync(_configuration["AdminFileExtractQueue"], lockDuration, maxDeliveryCount, maxSizeInMegabytes);
        await EnsureQueueExistsAsync(_configuration["AppealFileExtractQueue"], lockDuration, maxDeliveryCount, maxSizeInMegabytes);
    }

    private async Task EnsureQueueExistsAsync(string queueName, TimeSpan lockDuration, int maxDeliveryCount, int maxSizeInMegabytes)
    {
        if (string.IsNullOrWhiteSpace(queueName))
        {
            throw new InvalidOperationException($"Queue name is not configured.");
        }

        if (!await _administrationClient.QueueExistsAsync(queueName))
        {
            var queueOptions = new CreateQueueOptions(queueName)
            {
                LockDuration = lockDuration,
                MaxDeliveryCount = maxDeliveryCount,
                MaxSizeInMegabytes = maxSizeInMegabytes
            };

            await _administrationClient.CreateQueueAsync(queueOptions);
        }
    }
}
