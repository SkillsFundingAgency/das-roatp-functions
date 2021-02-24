using SFA.DAS.Roatp.Functions.ApplyTypes;
using System;

namespace SFA.DAS.Roatp.Functions.UnitTests.Generators
{
    public static class ApplyGenerator
    {
        public static Apply GenerateApplication(Guid applicationId, DateTime applictionSubmittedDate)
        {
            return new Apply
            {
                ApplicationId = applicationId,
                ApplyData = new ApplyData
                {
                    ApplyDetails = new ApplyDetails
                    {
                        ApplicationSubmittedOn = applictionSubmittedDate
                    }
                }
            };
        }
    }
}
