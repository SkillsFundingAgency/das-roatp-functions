using System;
using System.IO;
using System.Threading.Tasks;
using Refit;

namespace SFA.DAS.Roatp.Functions.Infrastructure.ApiClients;

public interface IApplyApiClient
{
    [Get("/Accreditation/{applicationId}/SubcontractDeclaration/ContractFileClarification/{fileName}")]
    Task<Stream> DownloadGatewaySubcontractorDeclarationClarificationFile(Guid applicationId, string fileName);
    [Get("/Clarification/Applications/{applicationId}/Sequences/{sequenceNumber}/Sections/{sectionNumber}/Page/{pageId}/Download/{fileName}")]
    Task<Stream> DownloadAssessorClarificationFile(Guid applicationId, int sequenceNumber, int sectionNumber, string pageId, string fileName);
    [Get("/Clarification/Applications/{applicationId}/Download/{fileName}")]
    Task<Stream> DownloadFinanceClarificationFile(Guid applicationId, string fileName);
    [Get("/Appeals/{applicationId}/files/{fileName}")]
    Task<Stream> DownloadAppealFile(Guid applicationId, string fileName);
}
