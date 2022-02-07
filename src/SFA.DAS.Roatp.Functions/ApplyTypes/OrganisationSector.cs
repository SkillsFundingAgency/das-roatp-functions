using System;
using System.Collections;
using System.Collections.Generic;

namespace SFA.DAS.Roatp.Functions.ApplyTypes
{
    public class OrganisationSector
    {
        public int Id { get; set; }
        public Guid OrganisationId { get; set; }
        public string SectorName { get; set; }
        public string StandardsServed { get; set; }
        public int ExpectedNumberOfStarts { get; set; }
        public int NumberOfTrainers { get; set; }
        public virtual ICollection<OrganisationSectorExpert> OrganisationSectorExperts { get; set; }
    }
}
