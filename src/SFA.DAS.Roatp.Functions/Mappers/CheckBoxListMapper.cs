using SFA.DAS.QnA.Api.Types.Page;
using SFA.DAS.Roatp.Functions.ApplyTypes;
using System;
using System.Collections.Generic;

namespace SFA.DAS.Roatp.Functions.Mappers
{
    public static class CheckBoxListMapper
    {
        public static List<SubmittedApplicationAnswer> GetAnswers(Guid applicationId, string pageId, Question question, string checkBoxListAnswer)
        {
            var answers = new List<SubmittedApplicationAnswer>();

            if (question?.Input?.Options != null && !string.IsNullOrEmpty(checkBoxListAnswer))
            {
                var questionId = question.QuestionId;
                var questionType = question.Input?.Type;

                foreach(var option in question.Input.Options)
                {
                    if(checkBoxListAnswer.Contains(option.Value))
                    {
                        var answer = new SubmittedApplicationAnswer
                        {
                            ApplicationId = applicationId,
                            PageId = pageId,
                            QuestionId = questionId,
                            QuestionType = questionType,
                            Answer = option.Value
                        };

                        answers.Add(answer);
                    }
                }
            }

            return answers;
        }
    }
}
