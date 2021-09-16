using System;
using System.Collections.Generic;

namespace SFA.DAS.Roatp.Functions.ApplyTypes
{
    public class ExtractedApplication
    {
        public int Id { get; set; }
        public Guid ApplicationId { get; set; }
        public DateTime ExtractedDate { get; set; }

        public bool GatewayFilesExtracted { get; set; }
        public bool AssessorFilesExtracted { get; set; }
        public bool FinanceFilesExtracted { get; set; }
        public bool AppealFilesExtracted { get; set; }

        public virtual Apply Apply { get; set; }
        public virtual ICollection<SubmittedApplicationAnswer> SubmittedApplicationAnswers { get; set; }
    }
}
