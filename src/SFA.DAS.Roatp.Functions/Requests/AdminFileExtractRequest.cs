using System;
using SFA.DAS.Roatp.Functions.ApplyTypes;

namespace SFA.DAS.Roatp.Functions.Requests;

public enum AdminFileType
{
    Gateway,
    Assessor,
    Finance
}

public class AdminFileExtractRequest
{
    public Guid ApplicationId { get; }
    public int SequenceNumber { get; }
    public int SectionNumber { get; }
    public string PageId { get; }
    public string Filename { get; }
    public AdminFileType AdminFileType { get; }

    public AdminFileExtractRequest() { }

    public AdminFileExtractRequest(Guid applicationId, GatewayReviewDetails gatewayReviewDetails)
    {
        ApplicationId = applicationId;
        PageId = "GatewayClarificationFiles";
        Filename = gatewayReviewDetails.GatewaySubcontractorDeclarationClarificationUpload;
        AdminFileType = AdminFileType.Gateway;
    }

    public AdminFileExtractRequest(AssessorClarificationOutcome assessorClarification)
    {
        ApplicationId = assessorClarification.ApplicationId;
        PageId = assessorClarification.PageId;
        SectionNumber = assessorClarification.SectionNumber;
        SequenceNumber = assessorClarification.SequenceNumber;
        Filename = assessorClarification.ClarificationFile;
        AdminFileType = AdminFileType.Assessor;
    }

    public AdminFileExtractRequest(Guid applicationId, FinancialReviewClarificationFile financialClarificationFile)
    {
        ApplicationId = applicationId;
        PageId = "FinanceClarificationFiles";
        Filename = financialClarificationFile.Filename;
        AdminFileType = AdminFileType.Finance;
    }
}
