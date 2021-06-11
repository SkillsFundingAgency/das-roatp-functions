using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Roatp.Functions.ApplyTypes;
using SFA.DAS.Roatp.Functions.Infrastructure.ApiClients;
using SFA.DAS.Roatp.Functions.Infrastructure.BlobStorage;
using SFA.DAS.Roatp.Functions.Requests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Roatp.Functions.UnitTests
{
    public class AdminFileExtractTests
    {
        private Mock<ILogger<AdminFileExtract>> _logger;
        private Mock<IApplyApiClient> _applyApiClient;
        private Mock<IDatamartBlobStorageFactory> _datamartBlobStorageFactory;

        private Mock<BlobContainerClient> _blobContainerClient;
        private Mock<BlobClient> _blobClient;

        private AdminFileExtract _sut;

        [SetUp]
        public void Setup()
        {
            _logger = new Mock<ILogger<AdminFileExtract>>();
            _applyApiClient = new Mock<IApplyApiClient>();

            _applyApiClient.Setup(x => x.DownloadGatewaySubcontractorDeclarationClarificationFile(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(new MemoryStream());
            _applyApiClient.Setup(x => x.DownloadAssessorClarificationFile(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new MemoryStream());
            _applyApiClient.Setup(x => x.DownloadFinanceClarificationFile(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(new MemoryStream());

            SetupDatamartBlobStorageFactory();

            _sut = new AdminFileExtract(_logger.Object, _applyApiClient.Object, _datamartBlobStorageFactory.Object);
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
            _datamartBlobStorageFactory.Setup(fac => fac.GetAdminBlobContainerClient()).ReturnsAsync(_blobContainerClient.Object);
        }

        [Test]
        public async Task Run_Logs_Debug_Message()
        {
            var request = new AdminFileExtractRequest(new AssessorClarificationOutcome());
            await _sut.Run(request);

            _logger.Verify(x => x.Log(LogLevel.Debug, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
        }

        [Test]
        public async Task Run_Downloads_and_Saves_Gateway_File_Into_BlobStorage()
        {
            var request = new AdminFileExtractRequest(Guid.NewGuid(), new GatewayReviewDetails());
            await _sut.Run(request);

            _applyApiClient.Verify(x => x.DownloadGatewaySubcontractorDeclarationClarificationFile(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
            _blobClient.Verify(x => x.UploadAsync(It.IsAny<Stream>(), true, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Run_Downloads_and_Saves_Assessor_File_Into_BlobStorage()
        {
            var request = new AdminFileExtractRequest(new AssessorClarificationOutcome());
            await _sut.Run(request);

            _applyApiClient.Verify(x => x.DownloadAssessorClarificationFile(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);
            _blobClient.Verify(x => x.UploadAsync(It.IsAny<Stream>(), true, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Run_Downloads_and_Saves_Finance_File_Into_BlobStorage()
        {
            var request = new AdminFileExtractRequest(Guid.NewGuid(), new ClarificationFile());
            await _sut.Run(request);

            _applyApiClient.Verify(x => x.DownloadFinanceClarificationFile(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
            _blobClient.Verify(x => x.UploadAsync(It.IsAny<Stream>(), true, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
