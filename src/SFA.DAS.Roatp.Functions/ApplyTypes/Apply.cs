using System;

namespace SFA.DAS.Roatp.Functions.ApplyTypes
{
    public class Apply
    {
        public Guid Id { get; set; }
        public Guid ApplicationId { get; set; }
        public Guid OrganisationId { get; set; }

        public string ApplicationStatus { get; set; }
        public string GatewayReviewStatus { get; set; }
        public string FinancialReviewStatus { get; set; }
        public string AssessorReviewStatus { get; set; }
        public string ModerationStatus { get; set; }

        public ApplyData ApplyData { get; set; }
    }

    public class ApplyData
    {
        public ApplyDetails ApplyDetails { get; set; }
    }

    public class ApplyDetails
    {
        public string ReferenceNumber { get; set; }
        public string UKPRN { get; set; }
        public string OrganisationName { get; set; }
        public string TradingName { get; set; }
        public int ProviderRoute { get; set; }
        public string ProviderRouteName { get; set; }
        public DateTime? ApplicationSubmittedOn { get; set; }
        public Guid? ApplicationSubmittedBy { get; set; }
        public DateTime? ApplicationWithdrawnOn { get; set; }
        public string ApplicationWithdrawnBy { get; set; }
        public DateTime? ApplicationRemovedOn { get; set; }
        public string ApplicationRemovedBy { get; set; }
    }
}
