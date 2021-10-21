using SFA.DAS.QnA.Api.Types.Page;
using SFA.DAS.Roatp.Functions.ApplyTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Roatp.Functions.Mappers
{
    public static class TabularDataMapper
    {
        public static List<SubmittedApplicationAnswer> GetAnswers(Guid applicationId, int sequenceNumber, int sectionNumber, string pageId, Question question, TabularData tabularData)
        {
            var answers = new List<SubmittedApplicationAnswer>();

            if (question?.Input != null && tabularData?.DataRows != null && tabularData?.HeadingTitles != null)
            {
                var questionId = question.QuestionId;
                var questionType = question.Input.Type;

                for (int row = 0; row < tabularData.DataRows.Count; row++)
                {
                    var dataRow = tabularData.DataRows[row];
                    if (dataRow?.Columns is null) continue;

                    for (int column = 0; column < dataRow.Columns.Count; column++)
                    {
                        string columnAnswer = dataRow.Columns[column];
                        string columnHeading = tabularData.HeadingTitles.ElementAtOrDefault(column);

                        if (string.IsNullOrEmpty(columnAnswer)) continue;

                        var answer = new SubmittedApplicationAnswer
                        {
                            ApplicationId = applicationId,
                            SequenceNumber = sequenceNumber,
                            SectionNumber = sectionNumber,
                            PageId = pageId,
                            QuestionId = questionId,
                            QuestionType = questionType,
                            Answer = columnAnswer,
                            ColumnHeading = columnHeading,
                            RowNumber = row,
                            ColumnNumber = column
                        };

                        answers.Add(answer);
                    }
                }
            }

            return answers;
        }
    }
}
