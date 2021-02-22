using SFA.DAS.Roatp.Functions.ApplyTypes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Roatp.Functions.Infrastructure.ApiClients
{
    public interface IApplyApiClient
    {
        Task<IEnumerable<Apply>> GetApplicationsToExtract();
    }
}
