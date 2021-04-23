using NUnit.Framework;
using SFA.DAS.QnA.Api.Types.Page;
using SFA.DAS.Roatp.Functions.ApplyTypes;
using SFA.DAS.Roatp.Functions.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;

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
            Assert.AreEqual(expectedItemCount, result.Count);

            for(int index = 0; index < result.Count; index++)
            {
                Assert.AreEqual(_applicationId, result[index].ApplicationId);
                Assert.AreEqual(_sequenceNumber, result[index].SequenceNumber);
                Assert.AreEqual(_sectionNumber, result[index].SectionNumber);
                Assert.AreEqual(_pageId, result[index].PageId);
                Assert.AreEqual(_question.QuestionId, result[index].QuestionId);
                Assert.AreEqual(_question.Input.Type, result[index].QuestionType);
                Assert.AreEqual(_question.Input.Options[index].Value, result[index].Answer);
            }
        }
    }
}
