using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Roatp.Functions.ApplyTypes;
using SFA.DAS.Roatp.Functions.Infrastructure.ApiClients;
using SFA.DAS.Roatp.Functions.Infrastructure.BlobStorage;
using SFA.DAS.Roatp.Functions.Requests;

namespace SFA.DAS.Roatp.Functions.UnitTests
{
    public class ApplyFileExtractTests
    {
        private Mock<ILogger<ApplyFileExtract>> _logger;
        private Mock<IQnaApiClient> _qnaApiClient;
        private Mock<IDatamartBlobStorageFactory> _datamartBlobStorageFactory;

        private Mock<BlobContainerClient> _blobContainerClient;
        private Mock<BlobClient> _blobClient;

        private ApplyFileExtract _sut;

        [SetUp]
        public void Setup()
        {
            _logger = new Mock<ILogger<ApplyFileExtract>>();
            _qnaApiClient = new Mock<IQnaApiClient>();

            _qnaApiClient.Setup(x => x.DownloadFile(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new MemoryStream());

            SetupDatamartBlobStorageFactory();

            _sut = new ApplyFileExtract(_logger.Object, _qnaApiClient.Object, _datamartBlobStorageFactory.Object);
        }

        private void SetupDatamartBlobStorageFactory()
        {
            _blobContainerClient = new Mock<BlobContainerClient>();
            _blobContainerClient.Setup(x => x.CreateIfNotExistsAsync(It.IsAny<PublicAccessType>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<BlobContainerEncryptionScopeOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<Azure.Response<BlobContainerInfo>>());

            _blobClient = new Mock<BlobClient>();
            _blobContainerClient.Setup(x => x.GetBlobClient(It.IsAny<string>()))
                .Returns(_blobClient.Object);

            _datamartBlobStorageFactory = new Mock<IDatamartBlobStorageFactory>();
            _datamartBlobStorageFactory.Setup(fac => fac.GetQnABlobContainerClient()).ReturnsAsync(_blobContainerClient.Object);
        }

        [Test]
        public async Task Run_Logs_Debug_Message()
        {
            var request = new ApplyFileExtractRequest(new SubmittedApplicationAnswer());
            await _sut.Run(JsonConvert.SerializeObject(request));

            _logger.Verify(x => x.Log(LogLevel.Debug, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
        }

        [Test]
        public async Task Run_Saves_File_Into_BlobStorage()
        {
            var request = new ApplyFileExtractRequest(new SubmittedApplicationAnswer());
            await _sut.Run(JsonConvert.SerializeObject(request));

            _blobClient.Verify(x => x.UploadAsync(It.IsAny<Stream>(), true, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }
    }
}
