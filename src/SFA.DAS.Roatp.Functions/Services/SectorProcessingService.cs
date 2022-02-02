using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Roatp.Functions.ApplyTypes;

namespace SFA.DAS.Roatp.Functions.Services
{
    public class SectorProcessingService: ISectorProcessingService
    {
        private const string ResponseYes = "Yes";
        private const string ExperienceOfDeliveringNoExperience = "No experience";
        private const string ExperienceOfDeliveringLessThanAYear = "Less than a year";
        private const string ExperienceOfDeliveringOneToTwoYears = "One to 2 years";
        private const string ExperienceOfDeliveringThreeToFiveYears = "3 to 5 years";
        private const string ExperienceOfDeliveringOverFiveYears = "Over 5 years";
        private const string TypeOfApprenticeshipDeliveredNoneDelivered = "No apprenticeship training delivered";
        private const string ExperienceOfTrainingApprenticesNoExperience = "No experience";
        private const string TypicalDurationOfTrainingApprenticesNoTrainingDelivered = "No training delivered";
        private const string HowTrainingDeliveredOther = "Other";


        public async Task<OrganisationSectors> GatherSectorDetails(IReadOnlyCollection<SubmittedApplicationAnswer> answers, Guid organisationId,
            string sectorDescription)
        {
            var sectorDetails = GetSectorQuestionIdsForSectorDescription(sectorDescription);
            if (sectorDetails == null) return null;
            var sector = new OrganisationSectors();
            sector.OrganisationId = organisationId;
            sector.StandardsServed = answers
                .FirstOrDefault(x => x.QuestionId == sectorDetails.WhatStandardsOffered)?.Answer;
            int.TryParse(answers.FirstOrDefault(x => x.QuestionId == sectorDetails.HowManyStarts)?.Answer,
                out var expectedNumberOfStarts);
            sector.ExpectedNumberOfStarts = expectedNumberOfStarts;
            int.TryParse(answers.FirstOrDefault(x => x.QuestionId == sectorDetails.HowManyEmployees)?.Answer,
                out var numberOfEmployees);
            sector.NumberOfTrainers = numberOfEmployees;
            sector.SectorName = sectorDetails.Name;
            sector.OrganisationSectorExperts = new List<OrganisationSectorExperts>();
            return sector;
        }

        public async Task<OrganisationSectorExperts> GatherSectorExpertsDetails(IReadOnlyCollection<SubmittedApplicationAnswer> answers,
            string sectorDescription)
        {
            var sectorDetails = GetSectorQuestionIdsForSectorDescription(sectorDescription);
            if (sectorDetails == null) return null;
            var sectorExperts = new OrganisationSectorExperts();
            sectorExperts.FirstName =
                answers.FirstOrDefault(x => x.QuestionId == sectorDetails.FirstName)?.Answer;
            sectorExperts.LastName =
                answers.FirstOrDefault(x => x.QuestionId == sectorDetails.LastName)?.Answer;
            sectorExperts.JobRole = answers.FirstOrDefault(x => x.QuestionId == sectorDetails.JobRole)?.Answer;
            sectorExperts.TimeInRole =
                answers.FirstOrDefault(x => x.QuestionId == sectorDetails.TimeInRole)?.Answer;
            var isPartOfAnyOtherOrganisation = answers
                .FirstOrDefault(x => x.QuestionId == sectorDetails.IsPartOfAnyOtherOrganisations)?.Answer;
            var isPartOfAnyOtherOrganisationValue = false || isPartOfAnyOtherOrganisation == ResponseYes;

            sectorExperts.IsPartOfAnyOtherOrganisation = isPartOfAnyOtherOrganisationValue;
            sectorExperts.OtherOrganisationNames = answers
                .FirstOrDefault(x => x.QuestionId == sectorDetails.OtherOrganisations)?.Answer;
            var dateOfBirthDetails = answers.FirstOrDefault(x => x.QuestionId == sectorDetails.DateOfBirth)
                ?.Answer.Split(",");
            if (dateOfBirthDetails is { Length: 2 })
            {
                int.TryParse(dateOfBirthDetails[0], out var dateOfBirthMonth);
                int.TryParse(dateOfBirthDetails[1], out var dateOfBirthYear);
                sectorExperts.DateOfBirthMonth = dateOfBirthMonth;
                sectorExperts.DateOfBirthYear = dateOfBirthYear;
            }

            sectorExperts.ContactNumber =
                answers.FirstOrDefault(x => x.QuestionId == sectorDetails.ContactNumber)?.Answer;
            sectorExperts.Email = answers.FirstOrDefault(x => x.QuestionId == sectorDetails.Email)?.Answer;
            var experienceOfDelivering = answers
                .FirstOrDefault(x => x.QuestionId == sectorDetails.ExperienceOfDelivering)?.Answer;
            if (experienceOfDelivering != ExperienceOfDeliveringNoExperience)
                sectorExperts.SectorTrainingExperienceDuration = experienceOfDelivering;

           switch (experienceOfDelivering)
           {
               case ExperienceOfDeliveringLessThanAYear:
                   sectorExperts.SectorTrainingExperienceDetails = answers.FirstOrDefault(x => x.QuestionId == $"{sectorDetails.ExperienceOfDelivering}-1")?.Answer;
                   break;
               case ExperienceOfDeliveringOneToTwoYears:
                   sectorExperts.SectorTrainingExperienceDetails = answers.FirstOrDefault(x => x.QuestionId == $"{sectorDetails.ExperienceOfDelivering}-2")?.Answer;
                   break;
               case ExperienceOfDeliveringThreeToFiveYears:
                   sectorExperts.SectorTrainingExperienceDetails = answers.FirstOrDefault(x => x.QuestionId == $"{sectorDetails.ExperienceOfDelivering}-3")?.Answer;
                   break;
                case ExperienceOfDeliveringOverFiveYears:
                   sectorExperts.SectorTrainingExperienceDetails = answers.FirstOrDefault(x => x.QuestionId == $"{sectorDetails.ExperienceOfDelivering}-4")?.Answer;
                   break;
           }

            var isQualifiedForSector = answers
                .FirstOrDefault(x => x.QuestionId == sectorDetails.DoTheyHaveQualifications)?.Answer;
            var isQualifiedForSectorValue = false || isQualifiedForSector == ResponseYes;
            sectorExperts.IsQualifiedForSector = isQualifiedForSectorValue;
            sectorExperts.QualificationDetails = answers
                .FirstOrDefault(x => x.QuestionId == sectorDetails.QualificationDetails)?.Answer;
            var isApprovedByAwardingBodies = answers
                .FirstOrDefault(x => x.QuestionId == sectorDetails.IsApprovedByAwardingBodies)?.Answer;
            var isApprovedByAwardingBodiesValue = false || isApprovedByAwardingBodies == ResponseYes;
            sectorExperts.IsApprovedByAwardingBodies = isApprovedByAwardingBodiesValue;
            sectorExperts.AwardingBodyNames = answers
                .FirstOrDefault(x => x.QuestionId == sectorDetails.NamesOfAwardingBodies)?.Answer;
            var hasSectorOrTradeBodyMembership = answers
                .FirstOrDefault(x => x.QuestionId == sectorDetails.HasSectorOrTradeBodyMembership)?.Answer;
            var hasSectorOrTradeBodyMembershipValue = false || hasSectorOrTradeBodyMembership == ResponseYes;
            sectorExperts.HasSectorOrTradeBodyMembership = hasSectorOrTradeBodyMembershipValue;
            sectorExperts.SectorOrTradeBodyNames = answers
                .FirstOrDefault(x => x.QuestionId == sectorDetails.SectorOrTradeBodyNames)?.Answer;
            var typeOfApprenticeshipDelivered = answers
                .FirstOrDefault(x => x.QuestionId == sectorDetails.WhatTypeOfTrainingDelivered)?.Answer;

            if (typeOfApprenticeshipDelivered!= TypeOfApprenticeshipDeliveredNoneDelivered)
                sectorExperts.TypeOfApprenticeshipDelivered = typeOfApprenticeshipDelivered;

            var experienceInTrainingApprentices = answers
                .FirstOrDefault(x => x.QuestionId == sectorDetails.ExperienceOfDeliveringTraining)?.Answer;

            if (experienceInTrainingApprentices!= ExperienceOfTrainingApprenticesNoExperience) 
                sectorExperts.ExperienceInTrainingApprentices = experienceInTrainingApprentices;
            
            var typicalDurationOfTrainingApprentices = answers
                .FirstOrDefault(x => x.QuestionId == sectorDetails.TypicalDurationOfTraining)?.Answer;

            if (typicalDurationOfTrainingApprentices!= TypicalDurationOfTrainingApprenticesNoTrainingDelivered)
                sectorExperts.TypicalDurationOfTrainingApprentices = typicalDurationOfTrainingApprentices;
           
            return sectorExperts;
        }

        public async Task<List<OrganisationSectorExpertDeliveredTrainingTypes>> GatherSectorDeliveredTrainingTypes(IReadOnlyCollection<SubmittedApplicationAnswer> answers, string sectorDescription)
        {
            var sectorDetails = GetSectorQuestionIdsForSectorDescription(sectorDescription);
            if (sectorDetails == null) return null;
            var sectorExpertDeliveredTrainingTypesList = new List<OrganisationSectorExpertDeliveredTrainingTypes>();

            var howTrainingDeliveredSelections =
                answers.Where(x => x.QuestionId == sectorDetails.HowHaveTheyDeliveredTraining);
            if (howTrainingDeliveredSelections != null && howTrainingDeliveredSelections.Any())
            {
                foreach (var howTrainingDelivered in howTrainingDeliveredSelections)
                {
                    var trainingType = new OrganisationSectorExpertDeliveredTrainingTypes();
                    if (howTrainingDelivered.Answer != HowTrainingDeliveredOther)
                    {
                        trainingType.DeliveredTrainingType = howTrainingDelivered.Answer;
                    }
                    else
                        trainingType.DeliveredTrainingType = answers.FirstOrDefault(x =>
                            x.QuestionId == sectorDetails.HowHaveTheyDeliveredTrainingOther)?.Answer;

                    sectorExpertDeliveredTrainingTypesList.Add(trainingType);
                }
            }

           return sectorExpertDeliveredTrainingTypesList;
        }

        private SectorLookupDetails GetSectorQuestionIdsForSectorDescription(string sectorDescription)
        {
            switch (sectorDescription)
            {
                case SectorQuestionDetails.AgricultureEnvironmentalAndAnimalCare.Description:
                    return SectorQuestionDetails.AgricultureEnvironmentalAndAnimalCare.SectorLookupDetails;
                case SectorQuestionDetails.BusinessAndAdministration.Description:
                    return SectorQuestionDetails.BusinessAndAdministration.SectorLookupDetails;
                case SectorQuestionDetails.CareServices.Description:
                    return SectorQuestionDetails.CareServices.SectorLookupDetails;
                case SectorQuestionDetails.CateringAndHospitality.Description:
                    return SectorQuestionDetails.CateringAndHospitality.SectorLookupDetails;
                case SectorQuestionDetails.Construction.Description:
                    return SectorQuestionDetails.Construction.SectorLookupDetails;
                case SectorQuestionDetails.CreativeAndDesign.Description:
                    return SectorQuestionDetails.CreativeAndDesign.SectorLookupDetails;
                case SectorQuestionDetails.Digital.Description:
                    return SectorQuestionDetails.Digital.SectorLookupDetails;
                case SectorQuestionDetails.EducationAndChildcare.Description:
                    return SectorQuestionDetails.EducationAndChildcare.SectorLookupDetails;
                case SectorQuestionDetails.EngineeringAndManufacturing.Description:
                    return SectorQuestionDetails.EngineeringAndManufacturing.SectorLookupDetails;
                case SectorQuestionDetails.HairAndBeauty.Description:
                    return SectorQuestionDetails.HairAndBeauty.SectorLookupDetails;
                case SectorQuestionDetails.HealthAndScience.Description:
                    return SectorQuestionDetails.HealthAndScience.SectorLookupDetails;
                case SectorQuestionDetails.LegalFinanceAndAccounting.Description:
                    return SectorQuestionDetails.LegalFinanceAndAccounting.SectorLookupDetails;
                case SectorQuestionDetails.ProtectiveServices.Description:
                    return SectorQuestionDetails.ProtectiveServices.SectorLookupDetails;
                case SectorQuestionDetails.SalesMarketingAndProcurement.Description:
                    return SectorQuestionDetails.SalesMarketingAndProcurement.SectorLookupDetails;
                case SectorQuestionDetails.TransportAndLogistics.Description:
                    return SectorQuestionDetails.TransportAndLogistics.SectorLookupDetails;
                default:
                    return null;
            }
        }
    }
}
