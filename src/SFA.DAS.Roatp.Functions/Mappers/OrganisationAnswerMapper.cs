using SFA.DAS.Roatp.Functions.ApplyTypes;
using SFA.DAS.Roatp.Functions.ApplyTypes.Constants;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Roatp.Functions.Mappers
{
    public static class OrganisationAnswerMapper
    {
        private static string GetAnswerForPage(this IReadOnlyList<SubmittedApplicationAnswer> submittedAnswers, string pageId, string questionId)
        {
            return submittedAnswers.FirstOrDefault(ans => ans.PageId == pageId && ans.QuestionId == questionId)?.Answer;
        }

        public static OrganisationAnswer TransposeToOrganisationAnswer(Guid applicationId, IReadOnlyList<SubmittedApplicationAnswer> submittedAnswers)
        {
            var answer = new OrganisationAnswer { ApplicationId = applicationId };

            if (submittedAnswers != null)
            {
                answer.Ukprn = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.Preamble, RoatpPreambleQuestionIdConstants.UKPRN);
                answer.LegalName = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.Preamble, RoatpPreambleQuestionIdConstants.UkrlpLegalName);
                answer.TradingName = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.Preamble, RoatpPreambleQuestionIdConstants.UkrlpTradingName);
                answer.Website = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.Preamble, RoatpPreambleQuestionIdConstants.UkrlpWebsite);
                answer.AddressLine1 = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.Preamble, RoatpPreambleQuestionIdConstants.UkrlpLegalAddressLine1);
                answer.AddressLine2 = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.Preamble, RoatpPreambleQuestionIdConstants.UkrlpLegalAddressLine2);
                answer.AddressLine3 = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.Preamble, RoatpPreambleQuestionIdConstants.UkrlpLegalAddressLine3);
                answer.AddressLine4 = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.Preamble, RoatpPreambleQuestionIdConstants.UkrlpLegalAddressLine4);
                answer.AddressLine5 = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.Preamble, RoatpPreambleQuestionIdConstants.UkrlpLegalAddressTown);
                answer.Postcode = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.Preamble, RoatpPreambleQuestionIdConstants.UkrlpLegalAddressPostcode);
                answer.CompanyNumber = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.Preamble, RoatpPreambleQuestionIdConstants.UkrlpVerificationCompanyNumber);
                answer.CharityNumber = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.Preamble, RoatpPreambleQuestionIdConstants.UkrlpVerificationCharityRegNumber);
                answer.SoletraderOrPartnership = submittedAnswers.GetAnswerForPage(RoatpWorkflowPageIds.Preamble, RoatpPreambleQuestionIdConstants.UkrlpVerificationSoleTraderPartnership);
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
            }

            return answer;
        }
    }
}
