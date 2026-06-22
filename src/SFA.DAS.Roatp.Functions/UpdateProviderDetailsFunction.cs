using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Roatp.Functions.Infrastructure.ApiClients;

namespace SFA.DAS.Roatp.Functions;

public class UpdateProviderDetailsFunction
{
    private readonly ILogger<UpdateProviderDetailsFunction> _logger;
    private readonly IRoatpOuterApiClient _roatpOuterApiClient;

    public UpdateProviderDetailsFunction(ILogger<UpdateProviderDetailsFunction> logger, IRoatpOuterApiClient roatpOuterApiClient)
    {
        _logger = logger;
        _roatpOuterApiClient = roatpOuterApiClient;
    }

    [Function("UpdateProviderDetailsFunction")]
    public async Task Run([TimerTrigger("%UpdateProviderDetailsFunctionSchedule%")] TimerInfo myTimer)
    {
        _logger.LogInformation("UpdateProviderDetailsFunction executed at: {date}", DateTime.UtcNow);

        await _roatpOuterApiClient.UpdateProviderNames();
    }
}
