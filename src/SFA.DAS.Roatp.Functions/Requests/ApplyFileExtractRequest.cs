using System;
using SFA.DAS.Roatp.Functions.ApplyTypes;

namespace SFA.DAS.Roatp.Functions.Requests;

public class ApplyFileExtractRequest
{
    public Guid ApplicationId { get; }
    public int SequenceNumber { get; }
    public int SectionNumber { get; }
    public string PageId { get; }
    public string QuestionId { get; }
    public string Filename { get; }

    public ApplyFileExtractRequest() { }

    public ApplyFileExtractRequest(SubmittedApplicationAnswer submittedApplicationAnswer)
    {
        ApplicationId = submittedApplicationAnswer.ApplicationId;
        PageId = submittedApplicationAnswer.PageId;
        QuestionId = submittedApplicationAnswer.QuestionId;
        SectionNumber = submittedApplicationAnswer.SectionNumber;
        SequenceNumber = submittedApplicationAnswer.SequenceNumber;
        Filename = submittedApplicationAnswer.Answer;
    }
}
