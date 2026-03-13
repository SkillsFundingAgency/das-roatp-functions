using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Roatp.Functions.Infrastructure.ApiClients;

namespace SFA.DAS.Roatp.Functions.UnitTests
{
    public class UpdateProviderDetailsFunctionTests
    {
        private Mock<ILogger<UpdateProviderDetailsFunction>> _logger;
        private Mock<IRoatpOuterApiClient> _roatpOuterApiClient;
        private readonly TimerInfo _timerInfo = new TimerInfo(null, null, false);
        private UpdateProviderDetailsFunction _sut;

        [SetUp]
        public void Setup()
        {
            _logger = new Mock<ILogger<UpdateProviderDetailsFunction>>();

            _roatpOuterApiClient = new Mock<IRoatpOuterApiClient>();

            _sut = new UpdateProviderDetailsFunction(_logger.Object, _roatpOuterApiClient.Object);
        }

        [Test]
        public async Task Run_Logs_Information_Message()
        {
            await _sut.Run(_timerInfo);

            _logger.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
        }

        [Test]
        public async Task Run_Calls_RoatpOuterApi()
        {
            await _sut.Run(_timerInfo);

            _roatpOuterApiClient.Verify(x => x.UpdateProviderNames(), Times.Once);
        }
    }
}
