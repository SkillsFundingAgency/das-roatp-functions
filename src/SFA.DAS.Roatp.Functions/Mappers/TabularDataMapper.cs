using SFA.DAS.QnA.Api.Types.Page;
using SFA.DAS.Roatp.Functions.ApplyTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Roatp.Functions.Mappers
{
    public static class TabularDataMapper
    {
        public static List<SubmittedApplicationAnswer> GetAnswers(Guid applicationId, string pageId, Question question, TabularData tabularData)
        {
            var answers = new List<SubmittedApplicationAnswer>();

            if (question?.Input != null && tabularData?.DataRows != null && tabularData?.HeadingTitles != null)
            {
                var questionId = question.QuestionId;
                var questionType = question.Input.Type;

                foreach (var row in tabularData.DataRows)
                {
                    for (int column = 0; column < row.Columns.Count; column++)
                    {
                        string columnAnswer = row.Columns[column];
                        string columnHeading = tabularData.HeadingTitles.ElementAtOrDefault(column);

                        if (string.IsNullOrEmpty(columnAnswer)) continue;

                        var answer = new SubmittedApplicationAnswer
                        {
                            ApplicationId = applicationId,
                            PageId = pageId,
                            QuestionId = questionId,
                            QuestionType = questionType,
                            Answer = columnAnswer,
                            ColumnHeading = columnHeading
                        };

                        answers.Add(answer);
                    }
                }
            }

            return answers;
        }
    }
}
