using SFA.DAS.QnA.Api.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Roatp.Functions.Infrastructure.ApiClients
{
    public interface IQnaApiClient
    {
        Task<IEnumerable<Section>> GetAllSectionsForApplication(Guid applicationId);
    }
}
