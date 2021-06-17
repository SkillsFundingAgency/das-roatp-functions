using System;
using System.IO;
using System.Threading.Tasks;

namespace SFA.DAS.Roatp.Functions.Infrastructure.ApiClients
{
    public interface IApplyApiClient
    {
        Task<Stream> DownloadGatewaySubcontractorDeclarationClarificationFile(Guid applicationId, string fileName);

        Task<Stream> DownloadAssessorClarificationFile(Guid applicationId, int sequenceNumber, int sectionNumber, string pageId, string fileName);

        Task<Stream> DownloadFinanceClarificationFile(Guid applicationId, string fileName);
    }
}