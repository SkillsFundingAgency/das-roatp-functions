using System;
using SFA.DAS.Roatp.Functions.ApplyTypes;

namespace SFA.DAS.Roatp.Functions.UnitTests.Generators;

public static class ExtractedApplicationGenerator
{
    public static ExtractedApplication GenerateExtractedApplication(Apply application, DateTime extractedDate)
    {
        return new ExtractedApplication
        {
            ApplicationId = application.ApplicationId,
            ExtractedDate = extractedDate,
            Apply = application
        };
    }
}
