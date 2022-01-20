using System;

namespace SFA.DAS.Roatp.Functions.ApplyTypes
{
    public class OrganisationManagement
    {
        public int Id { get; set; }
        public Guid OrganisationId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string JobRole { get; set; }
        public int TimeInRoleMonths { get; set; }
        public bool IsPartOfAnyOtherOrganisation { get; set; }
        public string OtherOrganisationNames { get; set; }
        public int DateOfBirthMonth { get; set; }
        public int DateOfBirthYear { get; set; }
        public string ContactNumber { get; set; }
        public string Email { get; set; }
    }
}
