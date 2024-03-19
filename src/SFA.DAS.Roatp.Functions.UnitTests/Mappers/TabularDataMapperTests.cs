using System;
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SFA.DAS.QnA.Api.Types.Page;
using SFA.DAS.Roatp.Functions.Mappers;

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

            Assert.Multiple(() =>
            {
                Assert.That(expectedItemCount, Is.EqualTo(result.Count));

                for (int row = 0; row < _tabularData.DataRows.Count; row++)
                {
                    for (int column = 0; column < _tabularData.HeadingTitles.Count; column++)
                    {
                        var answer = result[column + (row * _tabularData.DataRows.Count)];

                        Assert.That(_applicationId, Is.EqualTo(answer.ApplicationId));
                        Assert.That(_sequenceNumber, Is.EqualTo(answer.SequenceNumber));
                        Assert.That(_sectionNumber, Is.EqualTo(answer.SectionNumber));
                        Assert.That(_pageId, Is.EqualTo(answer.PageId));
                        Assert.That(_question.QuestionId, Is.EqualTo(answer.QuestionId));
                        Assert.That(_question.Input.Type, Is.EqualTo(answer.QuestionType));
                        Assert.That(_tabularData.DataRows[row].Columns[column], Is.EqualTo(answer.Answer));
                        Assert.That(_tabularData.HeadingTitles[column], Is.EqualTo(answer.ColumnHeading));
                        Assert.That(row, Is.EqualTo(answer.RowNumber));
                        Assert.That(column, Is.EqualTo(answer.ColumnNumber));
                    }
                }
            });
        }
    }
}
