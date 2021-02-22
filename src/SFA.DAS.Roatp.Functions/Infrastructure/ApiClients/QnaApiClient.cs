using Microsoft.Extensions.Logging;
using SFA.DAS.QnA.Api.Types;
using System;
using System.Collections.Generic;
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
    }
}
