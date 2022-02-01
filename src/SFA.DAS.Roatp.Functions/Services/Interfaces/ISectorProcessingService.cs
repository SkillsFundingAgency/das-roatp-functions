using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Roatp.Functions.ApplyTypes;

namespace SFA.DAS.Roatp.Functions.Services.Interfaces
{
    public interface ISectorProcessingService
    {
        Task<OrganisationSectors> GatherSectorDetails(IReadOnlyCollection<SubmittedApplicationAnswer> answers,
            Guid organisationId,
            string sectorDescription);

        Task<OrganisationSectorExperts> GatherSectorExpertsDetails(IReadOnlyCollection<SubmittedApplicationAnswer> answers,
            string sectorDescription);

        Task<List<OrganisationSectorExpertDeliveredTrainingTypes>> GatherSectorDeliveredTrainingTypes(
            IReadOnlyCollection<SubmittedApplicationAnswer> answers, string sectorDescription);
    }
}