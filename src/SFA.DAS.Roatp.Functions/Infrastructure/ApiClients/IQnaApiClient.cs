using SFA.DAS.QnA.Api.Types;
using SFA.DAS.QnA.Api.Types.Page;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SFA.DAS.Roatp.Functions.Infrastructure.ApiClients
{
    public interface IQnaApiClient
    {
        Task<IEnumerable<Section>> GetAllSectionsForApplication(Guid applicationId);
        Task<Stream> DownloadFile(Guid applicationId, int sequenceNumber, int sectionNumber, string pageId, string questionId);
        Task<string> GetTabularDataByTag(Guid applicationId, string questionTag);
    }
}
