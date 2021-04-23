using Microsoft.Extensions.Logging;
using SFA.DAS.QnA.Api.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.Roatp.Functions.Infrastructure.ApiClients
{
    public class QnaApiClient : ApiClientBase<QnaApiClient>, IQnaApiClient
    {
        public QnaApiClient(HttpClient client, ILogger<QnaApiClient> logger)
                    : base(client, logger)
        {
        }

        public async Task<IEnumerable<Section>> GetAllSectionsForApplication(Guid applicationId)
        {
            return await Get<List<Section>>($"Applications/{applicationId}/sections");
        }

        public async Task<Stream> DownloadFile(Guid applicationId, int sequenceNumber, int sectionNumber, string pageId, string questionId)
        {
            var response = await GetResponse($"/applications/{applicationId}/sequences/{sequenceNumber}/sections/{sectionNumber}/pages/{pageId}/questions/{questionId}/download");

            if(response.IsSuccessStatusCode)
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
