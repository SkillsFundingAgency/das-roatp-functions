using System;

namespace SFA.DAS.Roatp.Functions.ApplyTypes
{
    public class SubmittedApplicationAnswer
    {
        public int Id { get; set; }
        public Guid ApplicationId { get; set; }
        public int SequenceNumber { get; set; }
        public int SectionNumber { get; set; }
        public string PageId { get; set; }
        public string QuestionId { get; set; }
        public string QuestionType { get; set; }
        public string Answer { get; set; }
        public string ColumnHeading { get; set; }

        public virtual ExtractedApplication ExtractedApplication { get; set; }
    }
}
