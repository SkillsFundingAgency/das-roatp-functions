using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Refit;
using SFA.DAS.QnA.Api.Types;

namespace SFA.DAS.Roatp.Functions.Infrastructure.ApiClients;

public interface IQnaApiClient
{
    [Get("/Applications/{applicationId}/sections")]
    Task<IEnumerable<Section>> GetAllSectionsForApplication(Guid applicationId);
    [Get("/applications/{applicationId}/sequences/{sequenceNumber}/sections/{sectionNumber}/pages/{pageId}/questions/{questionId}/download")]
    Task<Stream> DownloadFile(Guid applicationId, int sequenceNumber, int sectionNumber, string pageId, string questionId);
    [Get("/Applications/{applicationId}/applicationData/{questionTag}")]
    Task<string> GetTabularDataByTag(Guid applicationId, string questionTag);
}
