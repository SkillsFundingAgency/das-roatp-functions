using System;

namespace SFA.DAS.Roatp.Functions.ApplyTypes
{
    public class Organisation
    {
        public Guid Id { get; set; } 
        public string Name { get; set; }
        public string TradingName { get; set; }
        public string OrganisationType { get; set; }
        public int OrganisationUKPRN { get; set; }
        public string CompanyRegistrationNumber { get; set; }
        public string CharityRegistrationNumber { get; set; }
        public string OrganisationDetails { get; set; }
        public string Status { get; set; }
        public bool RoATPApproved { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? DeletedBy { get; set; }
    }
}
