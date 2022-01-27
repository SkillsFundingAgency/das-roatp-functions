using System;

namespace SFA.DAS.Roatp.Functions.ApplyTypes
{
    public class OrganisationPersonnel
    {
        public int Id { get; set; }
        public Guid OrganisationId { get; set; }
        public int PersonnelType { get; set; }
        public string Name { get; set; }
        public int? DateOfBirthMonth { get; set; }
        public int? DateOfBirthYear { get; set; }
    }
}
