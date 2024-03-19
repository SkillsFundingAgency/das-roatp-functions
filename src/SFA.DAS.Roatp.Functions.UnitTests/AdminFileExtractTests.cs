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
    public class AdminFileExtractTests
    {
        private readonly Guid _applicationId = Guid.NewGuid();

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

            _applyApiClient.Setup(x => x.DownloadGatewaySubcontractorDeclarationClarificationFile(_applicationId, It.IsAny<string>())).ReturnsAsync(new MemoryStream());
            _applyApiClient.Setup(x => x.DownloadAssessorClarificationFile(_applicationId, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new MemoryStream());
            _applyApiClient.Setup(x => x.DownloadFinanceClarificationFile(_applicationId, It.IsAny<string>())).ReturnsAsync(new MemoryStream());

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
            await _sut.Run(JsonConvert.SerializeObject(request));

            _logger.Verify(x => x.Log(LogLevel.Debug, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
        }

        [Test]
        public async Task Run_Downloads_and_Saves_Gateway_File_Into_BlobStorage()
        {
            var gatewayReviewDetails = new GatewayReviewDetails { GatewaySubcontractorDeclarationClarificationUpload = "file.pdf" };

            var request = new AdminFileExtractRequest(_applicationId, gatewayReviewDetails);
            await _sut.Run(JsonConvert.SerializeObject(request));

            _applyApiClient.Verify(x => x.DownloadGatewaySubcontractorDeclarationClarificationFile(_applicationId, gatewayReviewDetails.GatewaySubcontractorDeclarationClarificationUpload), Times.Once);
            _blobClient.Verify(x => x.UploadAsync(It.IsAny<Stream>(), true, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Run_Downloads_and_Saves_Assessor_File_Into_BlobStorage()
        {
            var assessorClarificationOutcome = new AssessorClarificationOutcome { ApplicationId = _applicationId, ClarificationFile = "file.pdf" };

            var request = new AdminFileExtractRequest(assessorClarificationOutcome);
            await _sut.Run(JsonConvert.SerializeObject(request));

            _applyApiClient.Verify(x => x.DownloadAssessorClarificationFile(assessorClarificationOutcome.ApplicationId, assessorClarificationOutcome.SequenceNumber, assessorClarificationOutcome.SectionNumber, assessorClarificationOutcome.PageId, assessorClarificationOutcome.ClarificationFile), Times.AtLeastOnce);
            _blobClient.Verify(x => x.UploadAsync(It.IsAny<Stream>(), true, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Run_Downloads_and_Saves_Finance_File_Into_BlobStorage()
        {
            var financeClarificationFile = new FinancialReviewClarificationFile { Filename = "file.pdf" };

            var request = new AdminFileExtractRequest(_applicationId, financeClarificationFile);
            await _sut.Run(JsonConvert.SerializeObject(request));

            _applyApiClient.Verify(x => x.DownloadFinanceClarificationFile(_applicationId, financeClarificationFile.Filename), Times.Once);
            _blobClient.Verify(x => x.UploadAsync(It.IsAny<Stream>(), true, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
