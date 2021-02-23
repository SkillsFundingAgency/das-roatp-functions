using System;

namespace SFA.DAS.Roatp.Functions.ApplyTypes
{
    public class SubmittedApplicationAnswer
    {
        public Guid Id { get; set; }
        public Guid ApplicationId { get; set; }
        public string PageId { get; set; }
        public string QuestionId { get; set; }
        public string Answer { get; set; }
    }
}
