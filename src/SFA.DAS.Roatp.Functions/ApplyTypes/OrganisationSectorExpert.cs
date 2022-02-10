using System.Collections.Generic;

namespace SFA.DAS.Roatp.Functions.ApplyTypes
{
    public class OrganisationSectorExpert
    {
        public int Id { get; set; }
        public int OrganisationSectorId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string JobRole { get; set; }
        public string TimeInRole { get; set; }
        public bool IsPartOfAnyOtherOrganisation { get; set; }
        public string OtherOrganisationNames { get; set; }
        public int DateOfBirthMonth { get; set; }
        public int DateOfBirthYear { get; set; }
        public string ContactNumber { get; set; }
        public string Email { get; set; }
        public string SectorTrainingExperienceDuration { get; set; }
        public string SectorTrainingExperienceDetails { get; set; }
        public bool IsQualifiedForSector { get; set; }
        public string QualificationDetails { get; set; }
        public bool IsApprovedByAwardingBodies { get; set; }
        public string AwardingBodyNames { get; set; }
        public bool HasSectorOrTradeBodyMembership { get; set; }
        public string SectorOrTradeBodyNames { get; set; }
        public string TypeOfApprenticeshipDelivered { get; set; }
        public string ExperienceInTrainingApprentices { get; set; }
        public string TypicalDurationOfTrainingApprentices { get; set; }
        public virtual OrganisationSector OrganisationSector { get; set; }
        public virtual ICollection<OrganisationSectorExpertDeliveredTrainingType> OrganisationSectorExpertDeliveredTrainingTypes { get; set; }
    }
}