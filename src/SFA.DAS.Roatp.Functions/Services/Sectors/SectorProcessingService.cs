using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.Roatp.Functions.ApplyTypes;

namespace SFA.DAS.Roatp.Functions.Services.Sectors
{
    public class SectorProcessingService : ISectorProcessingService
    {
        private const string DeliveredTrainingTypeOther = "Other";

        public List<OrganisationSector> BuildSectorDetails(IReadOnlyCollection<SubmittedApplicationAnswer> answers, Guid organisationId)
        {
            var sectorChoices = answers.Where(x => x.PageId == "7600").ToList();

            var sectorsToAdd = new List<OrganisationSector>();
            foreach (var sectorChoice in sectorChoices)
            {
                var sectorDescription = sectorChoice.Answer;
                var sector = GatherSectorDetails(answers, organisationId, sectorDescription);
                var sectorExperts = GatherSectorExpertsDetails(answers, sectorDescription);
                var deliveredTrainingTypes = GatherSectorDeliveredTrainingTypes(answers, sectorDescription);
                foreach (var trainingType in deliveredTrainingTypes)
                {
                    trainingType.OrganisationSectorExpert = sectorExperts;
                }
                sectorExperts.OrganisationSectorExpertDeliveredTrainingTypes = deliveredTrainingTypes;
                sector.OrganisationSectorExperts.Add(sectorExperts);

                sectorsToAdd.Add(sector);
            }

            return sectorsToAdd;
        }

        private OrganisationSector GatherSectorDetails(IReadOnlyCollection<SubmittedApplicationAnswer> answers, Guid organisationId,
            string sectorDescription)
        {
            var sectorDetails = GetSectorQuestionIdsForSectorDescription(sectorDescription);
            if (sectorDetails == null) return null;
            var sector = new OrganisationSector();
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
            sector.OrganisationSectorExperts = new List<OrganisationSectorExpert>();
            return sector;
        }

        private OrganisationSectorExpert GatherSectorExpertsDetails(IReadOnlyCollection<SubmittedApplicationAnswer> answers,
            string sectorDescription)
        {
            var sectorDetails = GetSectorQuestionIdsForSectorDescription(sectorDescription);
            if (sectorDetails == null) return null;
            var sectorExperts = new OrganisationSectorExpert();
            sectorExperts.FirstName =
                answers.FirstOrDefault(x => x.QuestionId == sectorDetails.FirstName)?.Answer;
            sectorExperts.LastName =
                answers.FirstOrDefault(x => x.QuestionId == sectorDetails.LastName)?.Answer;
            sectorExperts.JobRole = answers.FirstOrDefault(x => x.QuestionId == sectorDetails.JobRole)?.Answer;
            sectorExperts.TimeInRole =
                answers.FirstOrDefault(x => x.QuestionId == sectorDetails.TimeInRole)?.Answer;
            var isPartOfAnyOtherOrganisation = answers
                .FirstOrDefault(x => x.QuestionId == sectorDetails.IsPartOfAnyOtherOrganisations)?.Answer;
            var isPartOfAnyOtherOrganisationValue = false || isPartOfAnyOtherOrganisation == "Yes";

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
            if (experienceOfDelivering != "No experience")
                sectorExperts.SectorTrainingExperienceDuration = experienceOfDelivering;

            switch (experienceOfDelivering)
            {
                case "Less than a year":
                    sectorExperts.SectorTrainingExperienceDetails = answers.FirstOrDefault(x => x.QuestionId == $"{sectorDetails.ExperienceOfDelivering}-1")?.Answer;
                    break;
                case "One to 2 years":
                    sectorExperts.SectorTrainingExperienceDetails = answers.FirstOrDefault(x => x.QuestionId == $"{sectorDetails.ExperienceOfDelivering}-2")?.Answer;
                    break;
                case "3 to 5 years":
                    sectorExperts.SectorTrainingExperienceDetails = answers.FirstOrDefault(x => x.QuestionId == $"{sectorDetails.ExperienceOfDelivering}-3")?.Answer;
                    break;
                case "Over 5 years":
                    sectorExperts.SectorTrainingExperienceDetails = answers.FirstOrDefault(x => x.QuestionId == $"{sectorDetails.ExperienceOfDelivering}-4")?.Answer;
                    break;
            }

            var isQualifiedForSector = answers
                .FirstOrDefault(x => x.QuestionId == sectorDetails.DoTheyHaveQualifications)?.Answer;
            var isQualifiedForSectorValue = false || isQualifiedForSector == "Yes";
            sectorExperts.IsQualifiedForSector = isQualifiedForSectorValue;
            sectorExperts.QualificationDetails = answers
                .FirstOrDefault(x => x.QuestionId == sectorDetails.QualificationDetails)?.Answer;
            var isApprovedByAwardingBodies = answers
                .FirstOrDefault(x => x.QuestionId == sectorDetails.IsApprovedByAwardingBodies)?.Answer;
            var isApprovedByAwardingBodiesValue = false || isApprovedByAwardingBodies == "Yes";
            sectorExperts.IsApprovedByAwardingBodies = isApprovedByAwardingBodiesValue;
            sectorExperts.AwardingBodyNames = answers
                .FirstOrDefault(x => x.QuestionId == sectorDetails.NamesOfAwardingBodies)?.Answer;
            var hasSectorOrTradeBodyMembership = answers
                .FirstOrDefault(x => x.QuestionId == sectorDetails.HasSectorOrTradeBodyMembership)?.Answer;
            var hasSectorOrTradeBodyMembershipValue = false || hasSectorOrTradeBodyMembership == "Yes";
            sectorExperts.HasSectorOrTradeBodyMembership = hasSectorOrTradeBodyMembershipValue;
            sectorExperts.SectorOrTradeBodyNames = answers
                .FirstOrDefault(x => x.QuestionId == sectorDetails.SectorOrTradeBodyNames)?.Answer;
            var typeOfApprenticeshipDelivered = answers
                .FirstOrDefault(x => x.QuestionId == sectorDetails.WhatTypeOfTrainingDelivered)?.Answer;

            if (typeOfApprenticeshipDelivered != "No apprenticeship training delivered")
                sectorExperts.TypeOfApprenticeshipDelivered = typeOfApprenticeshipDelivered;

            var experienceInTrainingApprentices = answers
                .FirstOrDefault(x => x.QuestionId == sectorDetails.ExperienceOfDeliveringTraining)?.Answer;

            if (experienceInTrainingApprentices != "No experience")
                sectorExperts.ExperienceInTrainingApprentices = experienceInTrainingApprentices;

            var typicalDurationOfTrainingApprentices = answers
                .FirstOrDefault(x => x.QuestionId == sectorDetails.TypicalDurationOfTraining)?.Answer;

            if (typicalDurationOfTrainingApprentices != "No training delivered")
                sectorExperts.TypicalDurationOfTrainingApprentices = typicalDurationOfTrainingApprentices;

            return sectorExperts;
        }

        private List<OrganisationSectorExpertDeliveredTrainingType> GatherSectorDeliveredTrainingTypes(IReadOnlyCollection<SubmittedApplicationAnswer> answers, string sectorDescription)
        {
            var sectorDetails = GetSectorQuestionIdsForSectorDescription(sectorDescription);
            if (sectorDetails == null) return null;
            var sectorExpertDeliveredTrainingTypesList = new List<OrganisationSectorExpertDeliveredTrainingType>();

            var howTrainingDeliveredSelections =
                answers.Where(x => x.QuestionId == sectorDetails.HowHaveTheyDeliveredTraining);
            if (howTrainingDeliveredSelections != null && howTrainingDeliveredSelections.Any())
            {
                foreach (var howTrainingDelivered in howTrainingDeliveredSelections)
                {
                    var trainingType = new OrganisationSectorExpertDeliveredTrainingType();
                    if (howTrainingDelivered.Answer != DeliveredTrainingTypeOther)
                    {
                        trainingType.DeliveredTrainingType = howTrainingDelivered.Answer;
                    }
                    else
                    {
                        var trainingOtherValue = answers.FirstOrDefault(x =>
                                x.QuestionId == sectorDetails.HowHaveTheyDeliveredTrainingOther)?.Answer;

                        trainingType.DeliveredTrainingType = String.IsNullOrEmpty(trainingOtherValue) ? DeliveredTrainingTypeOther : trainingOtherValue;
                    }
                    sectorExpertDeliveredTrainingTypesList.Add(trainingType);
                }
            }

            return sectorExpertDeliveredTrainingTypesList;
        }

        private SectorDetails GetSectorQuestionIdsForSectorDescription(string sectorDescription)
        {
            switch (sectorDescription)
            {
                case SectorQuestionDetails.AgricultureEnvironmentalAndAnimalCare.Description:
                    return SectorQuestionDetails.AgricultureEnvironmentalAndAnimalCare.SectorDetails;
                case SectorQuestionDetails.BusinessAndAdministration.Description:
                    return SectorQuestionDetails.BusinessAndAdministration.SectorDetails;
                case SectorQuestionDetails.CareServices.Description:
                    return SectorQuestionDetails.CareServices.SectorDetails;
                case SectorQuestionDetails.CateringAndHospitality.Description:
                    return SectorQuestionDetails.CateringAndHospitality.SectorDetails;
                case SectorQuestionDetails.Construction.Description:
                    return SectorQuestionDetails.Construction.SectorDetails;
                case SectorQuestionDetails.CreativeAndDesign.Description:
                    return SectorQuestionDetails.CreativeAndDesign.SectorDetails;
                case SectorQuestionDetails.Digital.Description:
                    return SectorQuestionDetails.Digital.SectorDetails;
                case SectorQuestionDetails.EducationAndChildcare.Description:
                    return SectorQuestionDetails.EducationAndChildcare.SectorDetails;
                case SectorQuestionDetails.EngineeringAndManufacturing.Description:
                    return SectorQuestionDetails.EngineeringAndManufacturing.SectorDetails;
                case SectorQuestionDetails.HairAndBeauty.Description:
                    return SectorQuestionDetails.HairAndBeauty.SectorDetails;
                case SectorQuestionDetails.HealthAndScience.Description:
                    return SectorQuestionDetails.HealthAndScience.SectorDetails;
                case SectorQuestionDetails.LegalFinanceAndAccounting.Description:
                    return SectorQuestionDetails.LegalFinanceAndAccounting.SectorDetails;
                case SectorQuestionDetails.ProtectiveServices.Description:
                    return SectorQuestionDetails.ProtectiveServices.SectorDetails;
                case SectorQuestionDetails.SalesMarketingAndProcurement.Description:
                    return SectorQuestionDetails.SalesMarketingAndProcurement.SectorDetails;
                case SectorQuestionDetails.TransportAndLogistics.Description:
                    return SectorQuestionDetails.TransportAndLogistics.SectorDetails;
                default:
                    return null;
            }
        }
    }

    public interface ISectorProcessingService
    {
        List<OrganisationSector> BuildSectorDetails(IReadOnlyCollection<SubmittedApplicationAnswer> answers,
            Guid organisationId);
    }
}
