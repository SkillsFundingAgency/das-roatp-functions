using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SFA.DAS.Roatp.Functions.ApplyTypes;
using SFA.DAS.Roatp.Functions.Services;

namespace SFA.DAS.Roatp.Functions.UnitTests
{
    public class SectorProcessingServiceTests
    {
        private SectorProcessingService _sectorProcessingService;
        private readonly Guid _organisationId = Guid.NewGuid();
        private string _sectorDescription = SectorQuestionDetails.AgricultureEnvironmentalAndAnimalCare.Description;
        private SectorLookupDetails _sectorLookupDetails; 
        const string TypeOfApprenticeshipDeliveredNoneDelivered = "No apprenticeship training delivered";
        const string ExperienceOfTrainingApprenticesNoExperience = "No experience";
        const string TypicalDurationOfTrainingApprenticesNoTrainingDelivered = "No training delivered";
        const string HowTrainingDeliveredOther = "Other";

        [SetUp]
        public void Setup()
        {
            _sectorLookupDetails = SectorQuestionDetails.AgricultureEnvironmentalAndAnimalCare.SectorLookupDetails;
            _sectorProcessingService = new SectorProcessingService();
        }

        [Test]
        public async Task GatherSectorDetails_ValidDescriptionAndAnswers_ReturnsExpectedSectorDetails()
        {
            const string whatStandardsOfferedAnswer = "what standards offered answer";
            const int howManyStartsAnswer = 10;
            const int howManyEmployees = 1;
            var answers = new List<SubmittedApplicationAnswer>
            {
                new SubmittedApplicationAnswer { QuestionId = _sectorLookupDetails.WhatStandardsOffered, Answer = whatStandardsOfferedAnswer},
                new SubmittedApplicationAnswer { QuestionId = _sectorLookupDetails.HowManyStarts, Answer = howManyStartsAnswer.ToString() },
                new SubmittedApplicationAnswer { QuestionId = _sectorLookupDetails.HowManyEmployees, Answer = howManyEmployees.ToString() }
            };

            var sector = await _sectorProcessingService.GatherSectorDetails(answers, _organisationId, _sectorDescription);

            Assert.AreEqual(_sectorLookupDetails.Name, sector.SectorName);
            Assert.AreEqual(_organisationId, sector.OrganisationId);
            Assert.AreEqual( whatStandardsOfferedAnswer, sector.StandardsServed);
            Assert.AreEqual(howManyStartsAnswer, sector.ExpectedNumberOfStarts);
            Assert.AreEqual( howManyEmployees, sector.NumberOfTrainers);
            Assert.AreEqual(0,sector.OrganisationSectorExperts.Count);
        }

        [Test]
        public async Task GatherSectorDetails_InvalidDescription_NullReturned()
        {
            var answers = new List<SubmittedApplicationAnswer>();
            var sector = await _sectorProcessingService.GatherSectorDetails(answers, _organisationId, "invalid sector description");
            Assert.IsNull(sector);
        }

        [TestCase("Yes", "No experience","","No", "No", "No","","","")]
        [TestCase("No", "No experience","","No", "No", "No", "", "","")]
        [TestCase("Yes", "Less than a year","-1","No", "No", "No", "", "","")]
        [TestCase("Yes", "One to 2 years","-2","No", "No", "No", "", "","")]
        [TestCase("Yes", "3 to 5 years","-3","No", "No", "No", "", "","")]
        [TestCase("Yes", "Over 5 years","-4","No", "No", "No", "", "","")]
        [TestCase("Yes", "No experience", "","Yes", "No", "No", "", "","")]
        [TestCase("Yes", "Over 5 years", "-4", "No", "Yes", "No", "", "","")]
        [TestCase("Yes", "Over 5 years", "-4", "No", "No", "Yes", "", "","")]
        [TestCase("Yes", "Over 5 years", "-4", "No", "No", "No", "other thing", "","")]
        [TestCase("Yes", "Over 5 years", "-4", "No", "No", "No", TypeOfApprenticeshipDeliveredNoneDelivered, "","")]
        [TestCase("Yes", "Over 5 years", "-4", "No", "No", "No", "type of apprenticeship delivered", "","")]
        [TestCase("Yes", "Over 5 years", "-4", "No", "No", "No", "", ExperienceOfTrainingApprenticesNoExperience,"")]
        [TestCase("Yes", "Over 5 years", "-4", "No", "No", "No", "", "experience of training apprentices description","")]
        [TestCase("Yes", "Over 5 years", "-4", "No", "No", "No", "", "experience of training apprentices description", TypicalDurationOfTrainingApprenticesNoTrainingDelivered)]
        [TestCase("Yes", "Over 5 years", "-4", "No", "No", "No", "", "experience of training apprentices description", "Typical duration of training apprentices")]

        public async Task GatherSectorExpertDetails_ValidDescriptionAndAnswers_ReturnsExpectedDetails(string isPartOfOtherOrganisation, string experienceOfDelivering, string experienceExtraTag, string isQualifiedForSector, string isApprovedByAwardingBodies, string hasSectorOrTradeBodyMembership, 
                                                                                                    string typeOfApprenticeshipDelivered, string experienceInTrainingApprentices, string typicalDurationOfTrainingApprentices)
        {
            var isPartOfOtherOrganisationValue = isPartOfOtherOrganisation == "Yes";
            var isQualifiedForSectorValue = isQualifiedForSector == "Yes";
            var isApprovedByAwardingBodiesValue = isApprovedByAwardingBodies == "Yes";
            var hasSectorOrTradeBodyMembershipValues = hasSectorOrTradeBodyMembership == "Yes";
            const string firstName = "first name";
            const string lastName = "last name";
            const string jobRole = "job role";
            const string timeInRole = "time in role";
            const string otherOrganisationNames = "other organisation names";
            const int dateOfBirthMonth = 2;
            const int dateOfBirthYear = 1990;
            const string contactNumber = "1234554321";
            const string email = "test@test.com";
            const string qualificationDetails = "qualification details";
            const string awardingBodyNames = "awarding body names";
            const string sectorOrTradeBodyNames = "sector or trade body names";
            
        
         var answers = new List<SubmittedApplicationAnswer>
            {
                new SubmittedApplicationAnswer { QuestionId = _sectorLookupDetails.FirstName, Answer = firstName},
                new SubmittedApplicationAnswer { QuestionId = _sectorLookupDetails.LastName, Answer = lastName},
                new SubmittedApplicationAnswer { QuestionId = _sectorLookupDetails.JobRole, Answer = jobRole},
                new SubmittedApplicationAnswer { QuestionId = _sectorLookupDetails.TimeInRole, Answer = timeInRole},
                new SubmittedApplicationAnswer { QuestionId = _sectorLookupDetails.IsPartOfAnyOtherOrganisations, Answer = isPartOfOtherOrganisation},
                new SubmittedApplicationAnswer { QuestionId = _sectorLookupDetails.OtherOrganisations, Answer = otherOrganisationNames},
                new SubmittedApplicationAnswer { QuestionId = _sectorLookupDetails.DateOfBirth, Answer = $"{dateOfBirthMonth},{dateOfBirthYear}"},
                new SubmittedApplicationAnswer { QuestionId = _sectorLookupDetails.ContactNumber, Answer = contactNumber},
                new SubmittedApplicationAnswer { QuestionId = _sectorLookupDetails.Email, Answer = email},
                new SubmittedApplicationAnswer { QuestionId = _sectorLookupDetails.ExperienceOfDelivering, Answer = experienceOfDelivering},
                new SubmittedApplicationAnswer { QuestionId = $"{_sectorLookupDetails.ExperienceOfDelivering}-1", Answer = $"{experienceOfDelivering}-1"},
                new SubmittedApplicationAnswer { QuestionId = $"{_sectorLookupDetails.ExperienceOfDelivering}-2", Answer = $"{experienceOfDelivering}-2"},
                new SubmittedApplicationAnswer { QuestionId = $"{_sectorLookupDetails.ExperienceOfDelivering}-3", Answer = $"{experienceOfDelivering}-3"},
                new SubmittedApplicationAnswer { QuestionId = $"{_sectorLookupDetails.ExperienceOfDelivering}-4", Answer = $"{experienceOfDelivering}-4"},
                new SubmittedApplicationAnswer { QuestionId = _sectorLookupDetails.DoTheyHaveQualifications, Answer = isQualifiedForSector},
                new SubmittedApplicationAnswer { QuestionId = _sectorLookupDetails.QualificationDetails, Answer = qualificationDetails},
                new SubmittedApplicationAnswer { QuestionId = _sectorLookupDetails.IsApprovedByAwardingBodies, Answer = isApprovedByAwardingBodies},
                new SubmittedApplicationAnswer { QuestionId = _sectorLookupDetails.NamesOfAwardingBodies, Answer = awardingBodyNames},
                new SubmittedApplicationAnswer { QuestionId = _sectorLookupDetails.HasSectorOrTradeBodyMembership, Answer = hasSectorOrTradeBodyMembership},
                new SubmittedApplicationAnswer { QuestionId = _sectorLookupDetails.SectorOrTradeBodyNames, Answer = sectorOrTradeBodyNames},
                new SubmittedApplicationAnswer { QuestionId = _sectorLookupDetails.WhatTypeOfTrainingDelivered, Answer = typeOfApprenticeshipDelivered},
                new SubmittedApplicationAnswer { QuestionId = _sectorLookupDetails.ExperienceOfDeliveringTraining, Answer = experienceInTrainingApprentices},
                new SubmittedApplicationAnswer { QuestionId = _sectorLookupDetails.TypicalDurationOfTraining, Answer = typicalDurationOfTrainingApprentices},

            };

            var sectorExpert = await _sectorProcessingService.GatherSectorExpertsDetails(answers, _sectorDescription);

            Assert.AreEqual(firstName, sectorExpert.FirstName);
            Assert.AreEqual(lastName, sectorExpert.LastName);
            Assert.AreEqual(jobRole, sectorExpert.JobRole);
            Assert.AreEqual(timeInRole, sectorExpert.TimeInRole);
            Assert.AreEqual(isPartOfOtherOrganisationValue, sectorExpert.IsPartOfAnyOtherOrganisation);
            Assert.AreEqual(otherOrganisationNames, sectorExpert.OtherOrganisationNames);
            Assert.AreEqual(dateOfBirthMonth, sectorExpert.DateOfBirthMonth);
            Assert.AreEqual(dateOfBirthYear, sectorExpert.DateOfBirthYear);
            Assert.AreEqual(contactNumber, sectorExpert.ContactNumber);
            Assert.AreEqual(email, sectorExpert.Email);
            var experienceOfDeliveringValue = (string)null;
            var experienceDetails = (string)null;
            if (experienceOfDelivering != "No experience")
            {
                experienceOfDeliveringValue = experienceOfDelivering;
                experienceDetails = $"{experienceOfDeliveringValue}{experienceExtraTag}";
            }

            Assert.AreEqual(experienceOfDeliveringValue, sectorExpert.SectorTrainingExperienceDuration);
            Assert.AreEqual(experienceDetails, sectorExpert.SectorTrainingExperienceDetails);
            Assert.AreEqual(isQualifiedForSectorValue, sectorExpert.IsQualifiedForSector);
            Assert.AreEqual(qualificationDetails, sectorExpert.QualificationDetails);
            Assert.AreEqual(isApprovedByAwardingBodiesValue, sectorExpert.IsApprovedByAwardingBodies);
            Assert.AreEqual(awardingBodyNames, sectorExpert.AwardingBodyNames);
            Assert.AreEqual(hasSectorOrTradeBodyMembershipValues, sectorExpert.HasSectorOrTradeBodyMembership);
            Assert.AreEqual(sectorOrTradeBodyNames, sectorExpert.SectorOrTradeBodyNames);

            if (typeOfApprenticeshipDelivered == TypeOfApprenticeshipDeliveredNoneDelivered)
                typeOfApprenticeshipDelivered = null;

            Assert.AreEqual(typeOfApprenticeshipDelivered, sectorExpert.TypeOfApprenticeshipDelivered);

            if (experienceInTrainingApprentices == ExperienceOfTrainingApprenticesNoExperience)
                experienceInTrainingApprentices = null;

            Assert.AreEqual(experienceInTrainingApprentices, sectorExpert.ExperienceInTrainingApprentices);

            if (typicalDurationOfTrainingApprentices == TypicalDurationOfTrainingApprenticesNoTrainingDelivered)
                typicalDurationOfTrainingApprentices = null;
            
            Assert.AreEqual(typicalDurationOfTrainingApprentices, sectorExpert.TypicalDurationOfTrainingApprentices);
        }

        [Test]
        public async Task GatherSectorExpertDetails_InvalidDescription_NullReturned()
        {
            var answers = new List<SubmittedApplicationAnswer>();
            var sectorExpert = await _sectorProcessingService.GatherSectorExpertsDetails(answers, "invalid sector description");
            Assert.IsNull(sectorExpert);
        }

        [Test]
        public async Task GatherSectorDeliveredTrainingTypes_ValidDescriptionAndAnswers_ReturnsTrainingTypesDetails()
        {
            var otherText = "other text";
            var other = "Other";
            var classroomBasedTraining = "Classroom-based training";
            var coaching = "Coaching";
            var eLearning = "E-learning";
            var mentoring = "Mentoring";
            var onTheJob = "On the job";

            var answersHowTheyDeliveredTraining = new List<string>
            {
                classroomBasedTraining,
                coaching,
                eLearning,
                mentoring,
                onTheJob,
                other
            };

            var answers = new List<SubmittedApplicationAnswer>();
            foreach (var answer in answersHowTheyDeliveredTraining)
            {
                answers.Add(new SubmittedApplicationAnswer
                {
                    QuestionId = _sectorLookupDetails.HowHaveTheyDeliveredTraining, Answer = answer
                });
            }

            answers.Add(new SubmittedApplicationAnswer { QuestionId = _sectorLookupDetails.HowHaveTheyDeliveredTrainingOther, Answer = otherText });
            var trainingTypes = await _sectorProcessingService.GatherSectorDeliveredTrainingTypes(answers, _sectorDescription);

            Assert.AreEqual(6, trainingTypes.Count);
            Assert.AreEqual(1,trainingTypes.Count(x => x.DeliveredTrainingType==classroomBasedTraining));
            Assert.AreEqual(1, trainingTypes.Count(x => x.DeliveredTrainingType == coaching));
            Assert.AreEqual(1, trainingTypes.Count(x => x.DeliveredTrainingType == eLearning));
            Assert.AreEqual(1, trainingTypes.Count(x => x.DeliveredTrainingType == mentoring));
            Assert.AreEqual(1, trainingTypes.Count(x => x.DeliveredTrainingType == onTheJob));
            Assert.AreEqual(0, trainingTypes.Count(x => x.DeliveredTrainingType == other));
            Assert.AreEqual(1, trainingTypes.Count(x => x.DeliveredTrainingType == otherText));
        }

        [Test]
        public async Task GatherSectorDeliveredTrainingTypes_InvalidDescription_NullReturned()
        {
            var answers = new List<SubmittedApplicationAnswer>();
            var sector = await _sectorProcessingService.GatherSectorDeliveredTrainingTypes(answers, "invalid  description");
            Assert.IsNull(sector);
        }
    }
}
