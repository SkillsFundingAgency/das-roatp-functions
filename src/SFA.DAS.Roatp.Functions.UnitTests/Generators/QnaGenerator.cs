using SFA.DAS.QnA.Api.Types;
using SFA.DAS.QnA.Api.Types.Page;
using System;
using System.Collections.Generic;

namespace SFA.DAS.Roatp.Functions.UnitTests.Generators
{
    public static class QnaGenerator
    {
        public static List<Section> GenerateSectionsForApplication(Guid applicationId)
        {
            var tabularDataAnswer = "{\"Caption\":null,\"HeadingTitles\":[\"First Name\",\"Last Name\",\"Job role\",\"Years in role\",\"Months in role\",\"Part of another organisation\",\"Organisation details\",\"Month\",\"Year\",\"Email\",\"Contact number\"],\"DataRows\":[{\"Id\":\"91baaa9e-5167-46dc-8447-cf6fc36fac31\",\"Columns\":[\"t\",\"t\",\"t\",\"1\",\"1\",\"No\",\"\",\"1\",\"1990\",\"t@t.com\",\"6545645645\"]},{\"Id\":\"64e5f397-7510-410f-8709-141e80c1b9a4\",\"Columns\":[\"t2\",\"t2\",\"t2\",\"2\",\"2\",\"Yes\",\"test\",\"1\",\"1980\",\"t2@t2.com\",\"56456456451\"]},{\"Id\":\"57bf57c2-5323-45e1-9fe4-3ea20c169166\",\"Columns\":[\"r\",\"r\",\"t\",\"3\",\"3\",\"No\",\"\",\"3\",\"1980\",\"r@r.com\",\"56456456451\"]}]}";
            return new List<Section>
            {
                GenerateSection(applicationId, 1, 1, "page1", "question1", "Text", "answer1"),
                GenerateSection(applicationId, 1, 2, "page2", "question2", "Text", "answer2"),
                GenerateSection(applicationId, 1, 3, "page3", "question3", "FileUpload", "file.pdf"),
                GenerateSection(applicationId, 7, 3, "page7", "question7.3", "TabularData", tabularDataAnswer)
            };
        }

        private static Section GenerateSection(Guid applicationId, int sequenceNo, int sectionNo, string pageId, string questionId, string inputType, string answer)
        {
            return new Section
            {
                Id = Guid.NewGuid(),
                ApplicationId = applicationId,
                SequenceNo = sequenceNo,
                SectionNo = sectionNo,
                QnAData = new QnAData
                {
                    Pages = new List<Page>
                    {
                        new Page
                        {
                            PageId = pageId,
                            Active = true,
                            Complete = true,
                            Questions = new List<Question>
                            {
                                new Question
                                {
                                    QuestionId = questionId,
                                    Input = new Input
                                    {
                                        Type = inputType,
                                        Options = new List<Option>()
                                    },
                                }
                            },
                            PageOfAnswers = new List<PageOfAnswers>
                            {
                                new PageOfAnswers
                                {
                                    Answers = new List<Answer>
                                    {
                                        new Answer
                                        {
                                            QuestionId = questionId,
                                            Value = answer
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
