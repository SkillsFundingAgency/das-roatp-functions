using NUnit.Framework;
using SFA.DAS.QnA.Api.Types.Page;
using SFA.DAS.Roatp.Functions.Mappers;
using System;
using System.Collections.Generic;

namespace SFA.DAS.Roatp.Functions.UnitTests.Mappers
{
    public class SubmittedAnswerMapperTests
    {
        private readonly Guid _applicationId = Guid.NewGuid();
        private const string _pageId = "1";

        private Question _question;
        private string _submittedAnswer;

        [SetUp]
        public void Setup()
        {
            _question = new Question
                        {
                            QuestionId = "1",
                            Input = new Input
                            {
                                Type = "Text",
                                Options = new List<Option>()
                            }
                        };

            _submittedAnswer = "answer";
        }

        [Test]
        public void GetAnswer_when_Question_null_Returns_null()
        {
            _question = null;

            var result = SubmittedAnswerMapper.GetAnswer(_applicationId, _pageId, _question, _submittedAnswer);

            Assert.IsNull(result);
        }

        [Test]
        public void GetAnswer_when_SubmittedAnswer_null_Returns_null()
        {
            _submittedAnswer = null;

            var result = SubmittedAnswerMapper.GetAnswer(_applicationId, _pageId, _question, _submittedAnswer);

            Assert.IsNull(result);
        }

        [Test]
        public void GetAnswer_when_SubmittedAnswer_empty_Returns_null()
        {
            _submittedAnswer = string.Empty;

            var result = SubmittedAnswerMapper.GetAnswer(_applicationId, _pageId, _question, _submittedAnswer);

            Assert.IsNull(result);
        }

        [Test]
        public void GetAnswer_Returns_expected_result()
        {
            var result = SubmittedAnswerMapper.GetAnswer(_applicationId, _pageId, _question, _submittedAnswer);

            Assert.IsNotNull(result);
            Assert.AreEqual(_applicationId, result.ApplicationId);
            Assert.AreEqual(_pageId, result.PageId);
            Assert.AreEqual(_question.QuestionId, result.QuestionId);
            Assert.AreEqual(_question.Input.Type, result.QuestionType);
            Assert.AreEqual(_submittedAnswer, result.Answer);
        }
    }
}
