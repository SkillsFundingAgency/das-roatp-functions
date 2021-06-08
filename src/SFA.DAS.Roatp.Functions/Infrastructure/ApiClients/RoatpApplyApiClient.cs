using Microsoft.Extensions.Logging;
using SFA.DAS.Roatp.Functions.ApplyTypes;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.Roatp.Functions.Infrastructure.ApiClients
{
    public class RoatpApplyApiClient : ApiClientBase<RoatpApplyApiClient>, IRoatpApplyApiClient
    {
        public RoatpApplyApiClient(HttpClient client, ILogger<RoatpApplyApiClient> logger)
                    : base(client, logger)
        {
        }

        public async Task<Stream> DownloadGatewaySubcontractorDeclarationClarificationFile(Guid applicationId, string fileName)
        {
            var response = await GetResponse($"/Accreditation/{applicationId}/SubcontractDeclaration/ContractFileClarification/{fileName}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStreamAsync();
            }
            else
            {
                return null;
            }
        }

        public async Task<Stream> DownloadAssessorClarificationFile(Guid applicationId, int sequenceNumber, int sectionNumber, string pageId, string fileName)
        {
            var response = await GetResponse($"/Clarification/Applications/{applicationId}/Sequences/{sequenceNumber}/Sections/{sectionNumber}/Page/{pageId}/Download/{fileName}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStreamAsync();
            }
            else
            {
                return null;
            }
        }

        public async Task<Stream> DownloadFinanceClarificationFile(Guid applicationId, string fileName)
        {
            var response = await GetResponse($"Clarification/Applications/{applicationId}/Download/{fileName}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStreamAsync();
            }
            else
            {
                return null;
            }
        }
    }
}
