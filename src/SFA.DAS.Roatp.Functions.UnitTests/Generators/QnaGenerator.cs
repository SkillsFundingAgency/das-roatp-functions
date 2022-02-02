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
            return new List<Section>
            {
                GenerateSection(applicationId, 1, 1, "page1", "question1", "Text", "answer1"),
                GenerateSection(applicationId, 1, 2, "page2", "question2", "Text", "answer2"),
                GenerateSection(applicationId, 1, 3, "page3", "question3", "FileUpload", "file.pdf")
            };
        }

        public static Section GenerateSection(Guid applicationId, int sequenceNo, int sectionNo, string pageId, string questionId, string inputType, string answer, List<Option> options)
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
                                        Options = options
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

        public static Section GenerateSection(Guid applicationId, int sequenceNo, int sectionNo, string pageId, string questionId, string inputType, string answer)
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
