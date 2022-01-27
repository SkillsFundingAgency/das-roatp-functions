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
            var answerQuestionPRE20OrganisationNameSoleTrade = "Test Sole Trade";
            var tabularDataAnswerQuestionYO110Partnership = "{\"Caption\":null,\"HeadingTitles\":[\"Name\",\"Date of birth\"],\"DataRows\":[{\"Id\":null,\"Columns\":[\"Test2\",\"\"]},{\"Id\":\"8af43486-ed5b-4be1-aa64-44ecc8d84fcf\",\"Columns\":[\"Test1\",\"Jan 2000\"]}]}";
            var tabularDataAnswerQuestionYO70CompaniesHouseDirectors = "{\"Caption\":\"Company directors\",\"HeadingTitles\":[\"Name\",\"Date of birth\"],\"DataRows\":[{\"Id\":\"tZNFgB-vB_VNioL0qDMcg88OO88\",\"Columns\":[\"BOUGHEY, Barbara\",\"Jan 1946\"]},{\"Id\":\"CITN-lDCmNzMsNrFC7w-8qQb27A\",\"Columns\":[\"COLL, Maria\",\"Jun 1950\"]},{\"Id\":\"-tMw6CdJKnjsyQZGKKV9xD4hvcE\",\"Columns\":[\"HILL, Norma\",\"Jul 1947\"]},{\"Id\":\"k5jEpiHjH6BmVe8SyhbMl_TqrmQ\",\"Columns\":[\"JONES, Tom\",\"Feb 1981\"]},{\"Id\":\"JyAwTurzJCPID9dDfCbUtSnBHMY\",\"Columns\":[\"MIDDLEHURST, Colin\",\"Nov 1953\"]},{\"Id\":\"ybXCovirMY7ruF4YXf48CxQMsQo\",\"Columns\":[\"RUDDY, Fiona Patricia\",\"Aug 1979\"]},{\"Id\":\"XD-abSqEne2hyGOEXuVxEUn6NTs\",\"Columns\":[\"TAYLOR, Sharon\",\"Mar 1972\"]}]}";
            var tabularDataAnswerQuestionYO71CompaniesHousePSCs = "{\"Caption\":\"People with significant control (PSCs)\",\"HeadingTitles\":[\"Name\",\"Date of birth\"],\"DataRows\":[{\"Id\":\"/company/02819229/persons-with-significant-control/individual/wj1KQRRIiF6tHy3dOc4G9WT5UnY\",\"Columns\":[\"Mrs Fiona Patricia Ruddy\",\"Aug 1979\"]}]}";
            var tabularDataAnswerQuestionYO80CharityTrustees = "{\"Caption\":null,\"HeadingTitles\":[\"Name\",\"Date of birth\"],\"DataRows\":[{\"Id\":\"3687405\",\"Columns\":[\"NORMA HILL\",\"Jan 1980\"]},{\"Id\":\"11692997\",\"Columns\":[\"Maria Coll\",\"Feb 1982\"]},{\"Id\":\"11692998\",\"Columns\":[\"Barbara Boughey\",\"Mar 1983\"]},{\"Id\":\"11765624\",\"Columns\":[\"FIONA PATRICIA RUDDY\",\"Apr 1984\"]},{\"Id\":\"12180484\",\"Columns\":[\"Tom Jones\",\"May 1985\"]},{\"Id\":\"12378238\",\"Columns\":[\"Sharon Taylor\",\"Jun 1986\"]},{\"Id\":\"12555212\",\"Columns\":[\"Colin Middlehurst\",\"Jul 1987\"]}]}";
            var answerQuestionYO120SoleTrade = "01,2000";
            var tabularDataAnswer = "{\"Caption\":null,\"HeadingTitles\":[\"First Name\",\"Last Name\",\"Job role\",\"Years in role\",\"Months in role\",\"Part of another organisation\",\"Organisation details\",\"Month\",\"Year\",\"Email\",\"Contact number\"],\"DataRows\":[{\"Id\":\"91baaa9e-5167-46dc-8447-cf6fc36fac31\",\"Columns\":[\"t\",\"t\",\"t\",\"1\",\"1\",\"No\",\"\",\"1\",\"1990\",\"t@t.com\",\"6545645645\"]},{\"Id\":\"64e5f397-7510-410f-8709-141e80c1b9a4\",\"Columns\":[\"t2\",\"t2\",\"t2\",\"2\",\"2\",\"Yes\",\"test\",\"1\",\"1980\",\"t2@t2.com\",\"56456456451\"]},{\"Id\":\"57bf57c2-5323-45e1-9fe4-3ea20c169166\",\"Columns\":[\"r\",\"r\",\"t\",\"3\",\"3\",\"No\",\"\",\"3\",\"1980\",\"r@r.com\",\"56456456451\"]}]}";
            return new List<Section>
            {
                GenerateSection(applicationId, 0, 1, "page0", "PRE-20", "Text", answerQuestionPRE20OrganisationNameSoleTrade),
                GenerateSection(applicationId, 1, 1, "page1", "question1", "Text", "answer1"),
                GenerateSection(applicationId, 1, 2, "page2", "question2", "Text", "answer2"),
                GenerateSection(applicationId, 1, 3, "page3", "question3", "FileUpload", "file.pdf"),
                GenerateSection(applicationId, 1, 3, "page1", "YO-110", "TabularData", tabularDataAnswerQuestionYO110Partnership),
                GenerateSection(applicationId, 1, 3, "page1", "YO-70", "TabularData", tabularDataAnswerQuestionYO70CompaniesHouseDirectors),
                GenerateSection(applicationId, 1, 3, "page1", "YO-71", "TabularData", tabularDataAnswerQuestionYO71CompaniesHousePSCs),
                GenerateSection(applicationId, 1, 3, "page1", "YO-80", "TabularData", tabularDataAnswerQuestionYO80CharityTrustees),
                GenerateSection(applicationId, 1, 3, "page1", "YO-120", "MonthAndYear", answerQuestionYO120SoleTrade),
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
