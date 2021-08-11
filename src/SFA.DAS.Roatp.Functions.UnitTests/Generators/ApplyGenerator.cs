using SFA.DAS.Roatp.Functions.ApplyTypes;
using System;
using System.Collections.Generic;

namespace SFA.DAS.Roatp.Functions.UnitTests.Generators
{
    public static class ApplyGenerator
    {
        public static Apply GenerateApplication(Guid applicationId, string applicationStatus, DateTime? applicationSubmittedDate)
        {
            return new Apply
            {
                ApplicationId = applicationId,
                ApplicationStatus = applicationStatus,
                ApplyData = new ApplyData
                {
                    ApplyDetails = new ApplyDetails
                    {
                        ApplicationSubmittedOn = applicationSubmittedDate
                    }
                }
            };
        }

        public static Apply AddExtractedApplicationDetails(this Apply application, bool hasGatewayFilesExtracted, bool hasAssessorFilesExtracted, bool hasFinanceFilesExtracted)
        {
            application.ExtractedApplication = new ExtractedApplication
            {
                ApplicationId = application.ApplicationId,
                ExtractedDate = application.ApplyData.ApplyDetails.ApplicationSubmittedOn.Value.AddDays(1),
                GatewayFilesExtracted = hasGatewayFilesExtracted,
                AssessorFilesExtracted = hasAssessorFilesExtracted,
                FinanceFilesExtracted = hasFinanceFilesExtracted,
                Apply = application
            };

            return application;
        }

        public static Apply AddGatewayReviewDetails(this Apply application, string gatewayReviewStatus, bool hasSubcontractorDeclarationClarificationFile)
        {
            application.GatewayReviewStatus = gatewayReviewStatus;

            if (hasSubcontractorDeclarationClarificationFile)
            {
                GatewayReviewDetails gatewayReviewDetails = new GatewayReviewDetails
                {
                    GatewaySubcontractorDeclarationClarificationUpload = "file.pdf"
                };

                application.ApplyData.GatewayReviewDetails = gatewayReviewDetails;
            }

            return application;
        }

        public static Apply AddAssessorReviewDetails(this Apply application, string assessorReviewStatus, bool hasClarificationFiles)
        {
            application.AssessorReviewStatus = assessorReviewStatus;

            if (hasClarificationFiles)
            {
                List<AssessorClarificationOutcome> outcomes = new List<AssessorClarificationOutcome>
                {
                    new AssessorClarificationOutcome
                    {
                        ApplicationId = application.ApplicationId,
                        PageId = "pageId",
                        SectionNumber = 3,
                        SequenceNumber = 3,
                        ClarificationFile = "file.pdf",
                        Apply = application
                    }
                };

                application.AssessorClarificationOutcomes = outcomes;
            }

            return application;
        }

        public static Apply AddFinancialReviewDetails(this Apply application, string financialReviewStatus, bool hasClarificationFiles)
        {
            application.FinancialReview = new FinancialReviewDetails
            {
                ApplicationId = application.ApplicationId,
                Status = financialReviewStatus,
                Apply = application
            };

            if (hasClarificationFiles)
            {
                var clarificationFiles = new List<FinancialReviewClarificationFile>
                {
                    new FinancialReviewClarificationFile
                    {
                        ApplicationId = application.ApplicationId,
                        Filename = "file.pdf",
                        FinancialReview = application.FinancialReview
                    }
                };

                application.FinancialReview.ClarificationRequestedOn = application.ApplyData.ApplyDetails.ApplicationSubmittedOn.Value;
                application.FinancialReview.ClarificationFiles = clarificationFiles;
            }

            return application;
        }
    }
}
