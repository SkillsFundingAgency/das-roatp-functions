using System;
using System.Collections.Generic;

namespace SFA.DAS.Roatp.Functions.ApplyTypes
{
    public class FinancialReviewDetails
    {
        public List<ClarificationFile> ClarificationFiles { get; set; }
        public DateTime? ClarificationRequestedOn { get; set; }
    }

    public class ClarificationFile
    {
        public string Filename { get; set; }
    }
}
