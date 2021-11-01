using System;
using System.Collections.Generic;

namespace SFA.DAS.Roatp.Functions.ApplyTypes
{
    public class FinancialReviewDetails
    {
        public Guid Id { get; set; }
        public Guid ApplicationId { get; set; }
        public string Status { get; set; }
        public DateTime? ClarificationRequestedOn { get; set; }

        public virtual Apply Apply { get; set; }
        public virtual ICollection<FinancialReviewClarificationFile> ClarificationFiles { get; set; }
    }

    public class FinancialReviewClarificationFile
    {
        public Guid Id { get; set; }
        public Guid ApplicationId { get; set; }
        public string Filename { get; set; }

        public virtual FinancialReviewDetails FinancialReview { get; set; }
    }
}
