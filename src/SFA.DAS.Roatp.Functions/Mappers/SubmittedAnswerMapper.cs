using SFA.DAS.QnA.Api.Types.Page;
using SFA.DAS.Roatp.Functions.ApplyTypes;
using System;

namespace SFA.DAS.Roatp.Functions.Mappers
{
    public static class SubmittedAnswerMapper
    {
        public static SubmittedApplicationAnswer GetAnswer(Guid applicationId, int sequenceNumber, int sectionNumber, string pageId, Question question, string submittedAnswer)
        {
            var answer = default(SubmittedApplicationAnswer);

            if (question?.Input != null && !string.IsNullOrEmpty(submittedAnswer))
            {
                var questionId = question.QuestionId;
                var questionType = question.Input.Type;

                answer = new SubmittedApplicationAnswer
                {
                    ApplicationId = applicationId,
                    SequenceNumber = sequenceNumber,
                    SectionNumber = sectionNumber,
                    PageId = pageId,
                    QuestionId = questionId,
                    QuestionType = questionType,
                    Answer = submittedAnswer
                };
            }

            return answer;
        }
    }
}
