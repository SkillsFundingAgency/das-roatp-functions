using SFA.DAS.Roatp.Functions.ApplyTypes;
using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace SFA.DAS.Roatp.Functions.Requests
{
    public enum AdminFileType
    {
        Gateway,
        Assessor,
        Finance
    }

    [Serializable]
    public class AdminFileExtractRequest : ISerializable
    {
        public Guid ApplicationId { get; }
        public int SequenceNumber { get; }
        public int SectionNumber { get; }
        public string PageId { get; }
        public string Filename { get; }
        public AdminFileType AdminFileType { get; }

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

        #region Serialization
        // This is the serialization constructor.
        // Satisfies rule CA2229: Implement serialization constructors
        protected AdminFileExtractRequest(SerializationInfo info, StreamingContext context)
        {
            ApplicationId = (Guid)info.GetValue(nameof(ApplicationId), typeof(Guid));
            SequenceNumber = info.GetInt32(nameof(SequenceNumber));
            SectionNumber = info.GetInt32(nameof(SectionNumber));
            PageId = info.GetString(nameof(PageId));
            AdminFileType = (AdminFileType)info.GetValue(nameof(AdminFileType), typeof(AdminFileType));
            Filename = info.GetString(nameof(Filename));
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(ApplicationId), ApplicationId);
            info.AddValue(nameof(SequenceNumber), SequenceNumber);
            info.AddValue(nameof(SectionNumber), SectionNumber);
            info.AddValue(nameof(PageId), PageId);
            info.AddValue(nameof(AdminFileType), AdminFileType);
            info.AddValue(nameof(Filename), Filename);
        }
        #endregion
    }
}