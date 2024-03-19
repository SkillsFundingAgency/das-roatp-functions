using System;
using System.Collections.Generic;
using NUnit.Framework;
using SFA.DAS.QnA.Api.Types.Page;
using SFA.DAS.Roatp.Functions.Mappers;

namespace SFA.DAS.Roatp.Functions.UnitTests.Mappers
{
    public class SubmittedAnswerMapperTests
    {
        private readonly Guid _applicationId = Guid.NewGuid();
        private const int _sequenceNumber = 1;
        private const int _sectionNumber = 1;
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

            var result = SubmittedAnswerMapper.GetAnswer(_applicationId, _sequenceNumber, _sectionNumber, _pageId, _question, _submittedAnswer);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetAnswer_when_SubmittedAnswer_null_Returns_null()
        {
            _submittedAnswer = null;

            var result = SubmittedAnswerMapper.GetAnswer(_applicationId, _sequenceNumber, _sectionNumber, _pageId, _question, _submittedAnswer);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetAnswer_when_SubmittedAnswer_empty_Returns_null()
        {
            _submittedAnswer = string.Empty;

            var result = SubmittedAnswerMapper.GetAnswer(_applicationId, _sequenceNumber, _sectionNumber, _pageId, _question, _submittedAnswer);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetAnswer_Returns_expected_result()
        {
            var result = SubmittedAnswerMapper.GetAnswer(_applicationId, _sequenceNumber, _sectionNumber, _pageId, _question, _submittedAnswer);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(_applicationId, Is.EqualTo(result.ApplicationId));
                Assert.That(_sequenceNumber, Is.EqualTo(result.SequenceNumber));
                Assert.That(_sectionNumber, Is.EqualTo(result.SectionNumber));
                Assert.That(_pageId, Is.EqualTo(result.PageId));
                Assert.That(_question.QuestionId, Is.EqualTo(result.QuestionId));
                Assert.That(_question.Input.Type, Is.EqualTo(result.QuestionType));
                Assert.That(_submittedAnswer, Is.EqualTo(result.Answer));
            });
        }
    }
}
