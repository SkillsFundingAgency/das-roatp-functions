using NUnit.Framework;
using SFA.DAS.QnA.Api.Types.Page;
using SFA.DAS.Roatp.Functions.ApplyTypes;
using SFA.DAS.Roatp.Functions.Mappers;
using System;
using System.Collections.Generic;

namespace SFA.DAS.Roatp.Functions.UnitTests.Mappers
{
    public class TabularDataMapperTests
    {
        private readonly Guid _applicationId = Guid.NewGuid();
        private const int _sequenceNumber = 1;
        private const int _sectionNumber = 1;
        private const string _pageId = "1";

        private Question _question;
        private TabularData _tabularData;

        [SetUp]
        public void Setup()
        {
            _question = new Question
                        {
                            QuestionId = "1",
                            Input = new Input
                            {
                                Type = "TabularData",
                                Options = new List<Option>()
                            }
                        };

            _tabularData = new TabularData
            {
                HeadingTitles = new List<string> { "title", "surname" },
                DataRows = new List<TabularDataRow>
                {
                    new TabularDataRow { Columns = new List<string> { "Dr", "Jekyll" } },
                    new TabularDataRow { Columns = new List<string> { "Mr", "Hyde" } }
                }
            };
        }

        [Test]
        public void GetAnswers_when_Question_null_Returns_empty()
        {
            _question = null;

            var result = TabularDataMapper.GetAnswers(_applicationId, _sequenceNumber, _sectionNumber, _pageId, _question, _tabularData);

            CollectionAssert.IsEmpty(result);
        }

        [Test]
        public void GetAnswers_when_TabularData_null_Returns_empty()
        {
            _tabularData = null;

            var result = TabularDataMapper.GetAnswers(_applicationId, _sequenceNumber, _sectionNumber, _pageId, _question, _tabularData);

            CollectionAssert.IsEmpty(result);
        }

        [Test]
        public void GetAnswers_when_TabularData_empty_Returns_empty()
        {
            _tabularData = new TabularData();

            var result = TabularDataMapper.GetAnswers(_applicationId, _sequenceNumber, _sectionNumber, _pageId, _question, _tabularData);

            CollectionAssert.IsEmpty(result);
        }

        [Test]
        public void GetAnswers_Returns_expected_result()
        {
            var expectedItemCount = _tabularData.DataRows.Count * _tabularData.HeadingTitles.Count;

            var result = TabularDataMapper.GetAnswers(_applicationId, _sequenceNumber, _sectionNumber, _pageId, _question, _tabularData);

            CollectionAssert.IsNotEmpty(result);
            Assert.AreEqual(expectedItemCount, result.Count);

            for (int row = 0; row < _tabularData.DataRows.Count; row++)
            {
                for (int column = 0; column < _tabularData.HeadingTitles.Count; column++)
                {
                    var answer = result[column + (row * _tabularData.DataRows.Count)];

                    Assert.AreEqual(_applicationId, answer.ApplicationId);
                    Assert.AreEqual(_sequenceNumber, answer.SequenceNumber);
                    Assert.AreEqual(_sectionNumber, answer.SectionNumber);
                    Assert.AreEqual(_pageId, answer.PageId);
                    Assert.AreEqual(_question.QuestionId, answer.QuestionId);
                    Assert.AreEqual(_question.Input.Type, answer.QuestionType);
                    Assert.AreEqual(_tabularData.DataRows[row].Columns[column], answer.Answer);
                    Assert.AreEqual(_tabularData.HeadingTitles[column], answer.ColumnHeading);
                }
            }
        }
    }
}
