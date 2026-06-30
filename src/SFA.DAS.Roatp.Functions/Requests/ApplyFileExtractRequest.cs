using System;
using SFA.DAS.Roatp.Functions.ApplyTypes;

namespace SFA.DAS.Roatp.Functions.Requests;

public class ApplyFileExtractRequest
{
    public Guid ApplicationId { get; set; }
    public int SequenceNumber { get; set; }
    public int SectionNumber { get; set; }
    public string PageId { get; set; }
    public string QuestionId { get; set; }
    public string Filename { get; set; }

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
