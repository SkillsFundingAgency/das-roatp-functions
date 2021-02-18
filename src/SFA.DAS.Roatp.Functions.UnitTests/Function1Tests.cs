using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Roatp.Functions.Configuration;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Roatp.Functions.UnitTests
{
    class Function1Tests
    {
        private IOptions<ApplyApiAuthentication> _applyApiAuthentication;
        private IOptions<QnaApiAuthentication> _qnaApiAuthentication;

        private Mock<IConfiguration> _configuration;
        private Mock<IServiceProvider> _serviceProvider;

        private Function1 _sut;

        [SetUp]
        public void Setup()
        {
            _applyApiAuthentication = Options.Create(new ApplyApiAuthentication());
            _qnaApiAuthentication = Options.Create(new QnaApiAuthentication());

            _configuration = new Mock<IConfiguration>();
            _configuration.SetupGet(x => x[It.Is<string>(s => s == "ApplyApiAuthentication")]).Returns(_applyApiAuthentication.Value.ToString());
            _configuration.SetupGet(x => x[It.Is<string>(s => s == "QnaApiAuthentication")]).Returns(_qnaApiAuthentication.Value.ToString());

            _serviceProvider = new Mock<IServiceProvider>();
            _serviceProvider.Setup(x => x.GetService(typeof(IOptions<ApplyApiAuthentication>))).Returns(_applyApiAuthentication);
            _serviceProvider.Setup(x => x.GetService(typeof(IOptions<QnaApiAuthentication>))).Returns(_qnaApiAuthentication);

            _sut = new Function1(_configuration.Object, _applyApiAuthentication, _serviceProvider.Object);
        }

        [Test]
        public async Task Run_Logs_Information_Message()
        {
            var _timerInfo = new TimerInfo(null, null, false);
            var _mockLogger = new Mock<ILogger>();

            await _sut.Run(_timerInfo, _mockLogger.Object);

            _mockLogger.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }
    }
}
