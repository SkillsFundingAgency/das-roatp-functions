using SFA.DAS.QnA.Api.Types;
using SFA.DAS.QnA.Api.Types.Page;
using SFA.DAS.Roatp.Functions.ApplyTypes;
using SFA.DAS.Roatp.Functions.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Roatp.Functions.UnitTests.Generators
{
    public static class SubmittedAnswerGenerator
    {
        public static List<SubmittedApplicationAnswer> GenerateSubmittedAnswers(Guid applicationId, ICollection<Section> sections)
        {
            var answers = new List<SubmittedApplicationAnswer>();

            foreach (var section in sections)
            {
                var completedPages = section.QnAData.Pages.Where(pg => pg.Active && pg.Complete && !pg.NotRequired);

                foreach (var page in completedPages)
                {
                    var submittedPageAnswers = GeneratePageAnswers(applicationId, section.SequenceNo, section.SectionNo, page);
                    answers.AddRange(submittedPageAnswers);
                }
            }

            return answers;
        }

        private static List<SubmittedApplicationAnswer> GeneratePageAnswers(Guid applicationId, int sequenceNumber, int sectionNumber, Page page)
        {
            var submittedPageAnswers = new List<SubmittedApplicationAnswer>();

            if (page.PageOfAnswers != null && page.Questions != null)
            {
                var pageAnswers = page.PageOfAnswers[0].Answers;

                foreach (var question in page.Questions)
                {
                    var questionId = question.QuestionId;
                    var questionAnswer = pageAnswers.FirstOrDefault(ans => ans.QuestionId == questionId && !string.IsNullOrWhiteSpace(ans.Value));

                    var submittedAnswer = SubmittedAnswerMapper.GetAnswer(applicationId, sequenceNumber, sectionNumber, page.PageId, question, questionAnswer.Value);
                    submittedPageAnswers.Add(submittedAnswer);
                }
            }

            return submittedPageAnswers;
        }
    }
}
