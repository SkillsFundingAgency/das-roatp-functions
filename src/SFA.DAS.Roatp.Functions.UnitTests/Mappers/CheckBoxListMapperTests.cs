using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SFA.DAS.QnA.Api.Types.Page;
using SFA.DAS.Roatp.Functions.Mappers;

namespace SFA.DAS.Roatp.Functions.UnitTests.Mappers
{
    public class CheckBoxListMapperTests
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
                    Type = "CheckBoxList",
                    Options = new List<Option>
                                {
                                    new Option
                                    {
                                        Value = "one"
                                    },
                                    new Option
                                    {
                                        Value = "two,three"
                                    },
                                    new Option
                                    {
                                        Value = "four"
                                    }
                                }
                }
            };

            _submittedAnswer = string.Join(",", _question.Input.Options.Select(option => option.Value));
        }

        [Test]
        public void GetAnswers_when_Question_null_Returns_empty()
        {
            _question = null;

            var result = CheckBoxListMapper.GetAnswers(_applicationId, _sequenceNumber, _sectionNumber, _pageId, _question, _submittedAnswer);

            CollectionAssert.IsEmpty(result);
        }

        [Test]
        public void GetAnswers_when_SubmittedAnswer_null_Returns_empty()
        {
            _submittedAnswer = null;

            var result = CheckBoxListMapper.GetAnswers(_applicationId, _sequenceNumber, _sectionNumber, _pageId, _question, _submittedAnswer);

            CollectionAssert.IsEmpty(result);
        }

        [Test]
        public void GetAnswers_when_SubmittedAnswer_empty_Returns_empty()
        {
            _submittedAnswer = string.Empty;

            var result = CheckBoxListMapper.GetAnswers(_applicationId, _sequenceNumber, _sectionNumber, _pageId, _question, _submittedAnswer);

            CollectionAssert.IsEmpty(result);
        }

        [Test]
        public void GetAnswers_Returns_expected_result()
        {
            var expectedItemCount = _question.Input.Options.Count;

            var result = CheckBoxListMapper.GetAnswers(_applicationId, _sequenceNumber, _sectionNumber, _pageId, _question, _submittedAnswer);

            CollectionAssert.IsNotEmpty(result);

            Assert.Multiple(() =>
            {
                Assert.That(expectedItemCount, Is.EqualTo(result.Count));

                for (int index = 0; index < result.Count; index++)
                {
                    Assert.That(_applicationId, Is.EqualTo(result[index].ApplicationId));
                    Assert.That(_sequenceNumber, Is.EqualTo(result[index].SequenceNumber));
                    Assert.That(_sectionNumber, Is.EqualTo(result[index].SectionNumber));
                    Assert.That(_pageId, Is.EqualTo(result[index].PageId));
                    Assert.That(_question.QuestionId, Is.EqualTo(result[index].QuestionId));
                    Assert.That(_question.Input.Type, Is.EqualTo(result[index].QuestionType));
                    Assert.That(_question.Input.Options[index].Value, Is.EqualTo(result[index].Answer));
                }
            });
        }
    }
}
