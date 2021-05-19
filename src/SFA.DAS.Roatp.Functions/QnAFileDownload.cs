using System;

namespace SFA.DAS.Roatp.Functions
{
    public class QnAFileDownload
    {
        public Guid ApplicationId { get; set; }
        public int SequenceNumber { get; set; }
        public int SectionNumber { get; set; }
        public string PageId { get; set; }
        public string QuestionId { get; set; }
        public string Filename { get; set; }
    }
}