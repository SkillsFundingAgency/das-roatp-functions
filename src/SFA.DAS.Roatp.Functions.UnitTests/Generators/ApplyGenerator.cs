using SFA.DAS.Roatp.Functions.ApplyTypes;
using System;

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
    }
}
