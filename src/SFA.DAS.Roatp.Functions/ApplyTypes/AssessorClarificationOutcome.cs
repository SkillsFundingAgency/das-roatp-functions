using System;

namespace SFA.DAS.Roatp.Functions.ApplyTypes
{
    public class AssessorClarificationOutcome
    {
        public Guid ApplicationId { get; set; }
        public int SequenceNumber { get; set; }
        public int SectionNumber { get; set; }
        public string PageId { get; set; }
        public string ClarificationFile { get; set; }

        public virtual Apply Apply { get; set; }
    }
}
