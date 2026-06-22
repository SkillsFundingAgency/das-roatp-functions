using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Roatp.Functions.Infrastructure.ApiClients;

namespace SFA.DAS.Roatp.Functions.UnitTests;

public class UpdateProviderDetailsFunctionTests
{
    private Mock<ILogger<UpdateProviderDetailsFunction>> _logger;
    private Mock<IRoatpOuterApiClient> _roatpOuterApiClient;
    private readonly TimerInfo _timerInfo = new();
    private UpdateProviderDetailsFunction _sut;

    [SetUp]
    public void Setup()
    {
        _logger = new Mock<ILogger<UpdateProviderDetailsFunction>>();

        _roatpOuterApiClient = new Mock<IRoatpOuterApiClient>();

        _sut = new UpdateProviderDetailsFunction(_logger.Object, _roatpOuterApiClient.Object);
    }

    [Test]
    public async Task Run_Calls_RoatpOuterApi()
    {
        await _sut.Run(_timerInfo);

        _roatpOuterApiClient.Verify(x => x.UpdateProviderNames(), Times.Once);
    }
}
