using SFA.DAS.Roatp.Functions.ApplyTypes;
using SFA.DAS.Roatp.Functions.ApplyTypes.Constants;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Roatp.Functions.Mappers
{
    public static class SubmittedOrganisationAnswerMapper
    {
        private static string GetAnswerForPage(this IReadOnlyList<SubmittedApplicationAnswer> submittedAnswers, string pageId, string questionId)
        {
            return submittedAnswers.FirstOrDefault(ans => ans.PageId == pageId && ans.QuestionId == questionId)?.Answer;
        }

        public static SubmittedOrganisationAnswer TransposeToSubmittedOrganisationAnswer(Guid applicationId, IReadOnlyList<SubmittedApplicationAnswer> submittedAnswers)
        {
            var answer = new SubmittedOrganisationAnswer { ApplicationId = applicationId };

            if (submittedAnswers != null)
            {
                answer.Ukprn = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.Preamble, RoatpPreambleQuestionIdConstants.UKPRN);
                answer.LegalName = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.Preamble, RoatpPreambleQuestionIdConstants.UkrlpLegalName);
                answer.TradingName = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.Preamble, RoatpPreambleQuestionIdConstants.UkrlpTradingName);
                answer.Website = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.Preamble, RoatpPreambleQuestionIdConstants.UkrlpWebsite)
                    ?? submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.WebsiteManuallyEntered, RoatpYourOrganisationQuestionIdConstants.WebsiteManuallyEntered);
                answer.AddressLine1 = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.Preamble, RoatpPreambleQuestionIdConstants.UkrlpLegalAddressLine1);
                answer.AddressLine2 = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.Preamble, RoatpPreambleQuestionIdConstants.UkrlpLegalAddressLine2);
                answer.AddressLine3 = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.Preamble, RoatpPreambleQuestionIdConstants.UkrlpLegalAddressLine3);
                answer.AddressLine4 = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.Preamble, RoatpPreambleQuestionIdConstants.UkrlpLegalAddressLine4);
                answer.AddressLine5 = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.Preamble, RoatpPreambleQuestionIdConstants.UkrlpLegalAddressTown);
                answer.Postcode = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.Preamble, RoatpPreambleQuestionIdConstants.UkrlpLegalAddressPostcode);
                answer.CompanyNumber = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.Preamble, RoatpPreambleQuestionIdConstants.UkrlpVerificationCompanyNumber);
                answer.CharityNumber = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.Preamble, RoatpPreambleQuestionIdConstants.UkrlpVerificationCharityRegNumber);
                answer.SoletraderOrPartnership = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.WhosInControl.PartnershipType, RoatpYourOrganisationQuestionIdConstants.SoleTradeOrPartnership)
                    ?? submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.Preamble, RoatpPreambleQuestionIdConstants.UkrlpVerificationSoleTraderPartnership);
                answer.OnRoatp = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.Preamble, RoatpPreambleQuestionIdConstants.OnRoatp);
                answer.ProviderRoute = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.ProviderRoute, RoatpPreambleQuestionIdConstants.ApplyProviderRoute);
                answer.LevyPayingEmployer = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.Preamble, RoatpPreambleQuestionIdConstants.LevyPayingEmployer);

                answer.HasParentCompany = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.YourOrganisationParentCompanyCheck, RoatpYourOrganisationQuestionIdConstants.PartnershipType);
                answer.ParentCompanyOrCharityNumber = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.YourOrganisationParentCompanyCheck, RoatpYourOrganisationQuestionIdConstants.ParentCompanyNumber);
                answer.ParentCompanyOrCharityName = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.YourOrganisationParentCompanyCheck, RoatpYourOrganisationQuestionIdConstants.ParentCompanyName);
                answer.CompaniesHouseDirectors = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.YourOrganisationParentCompanyCheck, RoatpYourOrganisationQuestionIdConstants.CompaniesHouseDirectors);
                answer.CompaniesHousePSCs = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.YourOrganisationParentCompanyCheck, RoatpYourOrganisationQuestionIdConstants.CompaniesHousePSCs);
                answer.CharityComissionTrustees = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.YourOrganisationParentCompanyCheck, RoatpYourOrganisationQuestionIdConstants.CharityCommissionTrustees);
                answer.PeopleInControl = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.YourOrganisationParentCompanyCheck, RoatpYourOrganisationQuestionIdConstants.AddPeopleInControl);

                answer.OrganisationType = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.DescribeYourOrganisation.MainSupportingStartPage, RoatpYourOrganisationQuestionIdConstants.OrganisationTypeMainSupporting)
                    ?? submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.DescribeYourOrganisation.EmployerStartPage, RoatpYourOrganisationQuestionIdConstants.OrganisationTypeEmployer);
                answer.EducationalInstituteType = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.DescribeYourOrganisation.EducationalInstituteType, RoatpYourOrganisationQuestionIdConstants.EducationalInstituteType);
                answer.PublicBodyType = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.DescribeYourOrganisation.PublicBodyType, RoatpYourOrganisationQuestionIdConstants.PublicBodyType);
                answer.SchoolType = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.DescribeYourOrganisation.SchoolType, RoatpYourOrganisationQuestionIdConstants.SchoolType);

                answer.RegisteredWithESFA = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.DescribeYourOrganisation.RegisteredESFA, RoatpYourOrganisationQuestionIdConstants.RegisteredESFA);
                answer.ReceivingFundingFromESFA = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.DescribeYourOrganisation.FundedESFA, RoatpYourOrganisationQuestionIdConstants.FundedESFA);
                answer.MonitoredByOfficeOfStudents = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.DescribeYourOrganisation.SupportedByOFS, RoatpYourOrganisationQuestionIdConstants.SupportedByOFS);
                answer.FundedByOfficeOfStudents = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.ExperienceAndAccreditations.OfficeForStudents, RoatpYourOrganisationQuestionIdConstants.OfficeForStudents);

                answer.DescribeYourOrganisation = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.DescribeYourOrganisation.OrganisationDescription, RoatpYourOrganisationQuestionIdConstants.OrganisationDescription);
                answer.ActivelyTradingFor = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.DescribeYourOrganisation.ActivelyTradingForMainSupporting, RoatpYourOrganisationQuestionIdConstants.ActivelyTradingForMainSupporting) 
                    ?? submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.DescribeYourOrganisation.ActivelyTradingForEmployer, RoatpYourOrganisationQuestionIdConstants.ActivelyTradingForEmployer);

                answer.OfferInitialTeacherTraining = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.ExperienceAndAccreditations.InitialTeacherTraining, RoatpYourOrganisationQuestionIdConstants.InitialTeacherTraining);
                answer.PostgraduateTeachingApprenticeshipOnly = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.ExperienceAndAccreditations.IsPostGradTrainingOnlyApprenticeship, RoatpYourOrganisationQuestionIdConstants.IsPostGradTrainingOnlyApprenticeship);
                answer.HadFullOfstedInspection = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.ExperienceAndAccreditations.HasHadFullInspection, RoatpYourOrganisationQuestionIdConstants.HasHadFullInspection);
                answer.GotFullOfstedGrade = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.ExperienceAndAccreditations.ReceivedFullInspectionGradeForApprenticeships, RoatpYourOrganisationQuestionIdConstants.ReceivedFullInspectionGradeForApprenticeships);
                answer.FullOfstedGradeWithinLast3Years = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.ExperienceAndAccreditations.GradeWithinLast3YearsOfsFunded, RoatpYourOrganisationQuestionIdConstants.GradeWithinLast3YearsOfsFunded)
                    ?? submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.ExperienceAndAccreditations.GradeWithinLast3YearsNonOfsFunded, RoatpYourOrganisationQuestionIdConstants.GradeWithinLast3YearsNonOfsFunded);
                answer.OverallEffectivenessOfstedGrade = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.ExperienceAndAccreditations.FullInspectionOverallEffectivenessGrade, RoatpYourOrganisationQuestionIdConstants.FullInspectionOverallEffectivenessGrade);
                answer.ApprenticeshipsOfstedGrade = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.ExperienceAndAccreditations.FullInspectionApprenticeshipGradeOfsFunded, RoatpYourOrganisationQuestionIdConstants.FullInspectionApprenticeshipGradeOfsFunded)
                    ?? submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.ExperienceAndAccreditations.FullInspectionApprenticeshipGradeNonOfsFunded, RoatpYourOrganisationQuestionIdConstants.FullInspectionApprenticeshipGradeNonOfsFunded);
                answer.HadMonitoringVisit = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.ExperienceAndAccreditations.HasHadMonitoringVisit, RoatpYourOrganisationQuestionIdConstants.HasHadMonitoringVisit);
                answer.HadTwoInsufficientMonitoringVisits = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.ExperienceAndAccreditations.Has2MonitoringVisitsGradedInadequate, RoatpYourOrganisationQuestionIdConstants.Has2MonitoringVisitsGradedInadequate);
                answer.HadMonitoringVisitGradedInadequateInLast18Months = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.ExperienceAndAccreditations.HasMonitoringVisitGradedInadequateInLast18Months, RoatpYourOrganisationQuestionIdConstants.HasMonitoringVisitGradedInadequateInLast18Months);
                answer.MaintainedFundingSinceFullOfstedInspection = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.ExperienceAndAccreditations.HasMaintainedFundingSinceInspection, RoatpYourOrganisationQuestionIdConstants.HasMaintainedFundingSinceInspection);
                answer.HadShortOfstedInspectionWithinLast3Years = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.ExperienceAndAccreditations.HasHadShortInspectionWithinLast3Years, RoatpYourOrganisationQuestionIdConstants.HasHadShortInspectionWithinLast3Years);
                answer.MaintainedGradeInShortOfstedInspection = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.ExperienceAndAccreditations.HasMaintainedFullGradeInShortInspection, RoatpYourOrganisationQuestionIdConstants.HasMaintainedFullGradeInShortInspection);
                answer.DeliveredApprenticeshipTrainingAsSubcontractor = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.ExperienceAndAccreditations.SubcontractorDeclaration, RoatpYourOrganisationQuestionIdConstants.HasDeliveredTrainingAsSubcontractor);
            }

            return answer;
        }
    }
}
