using SFA.DAS.Roatp.Functions.ApplyTypes;
using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace SFA.DAS.Roatp.Functions.Requests
{
    [Serializable]
    public class ApplyFileExtractRequest : ISerializable
    {
        public Guid ApplicationId { get; }
        public int SequenceNumber { get; }
        public int SectionNumber { get; }
        public string PageId { get; }
        public string QuestionId { get; }
        public string Filename { get; }

        public ApplyFileExtractRequest(SubmittedApplicationAnswer submittedApplicationAnswer)
        {
            ApplicationId = submittedApplicationAnswer.ApplicationId;
            PageId = submittedApplicationAnswer.PageId;
            QuestionId = submittedApplicationAnswer.QuestionId;
            SectionNumber = submittedApplicationAnswer.SectionNumber;
            SequenceNumber = submittedApplicationAnswer.SequenceNumber;
            Filename = submittedApplicationAnswer.Answer;
        }

        #region Serialization
        // This is the serialization constructor.
        // Satisfies rule CA2229: Implement serialization constructors
        protected ApplyFileExtractRequest(SerializationInfo info, StreamingContext context)
        {
            ApplicationId = (Guid)info.GetValue(nameof(ApplicationId), typeof(Guid));
            SequenceNumber = info.GetInt32(nameof(SequenceNumber));
            SectionNumber = info.GetInt32(nameof(SectionNumber));
            PageId = info.GetString(nameof(PageId));
            QuestionId = info.GetString(nameof(QuestionId));
            Filename = info.GetString(nameof(Filename));
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(ApplicationId), ApplicationId);
            info.AddValue(nameof(SequenceNumber), SequenceNumber);
            info.AddValue(nameof(SectionNumber), SectionNumber);
            info.AddValue(nameof(PageId), PageId);
            info.AddValue(nameof(QuestionId), QuestionId);
            info.AddValue(nameof(Filename), Filename);
        }
        #endregion
    }
}