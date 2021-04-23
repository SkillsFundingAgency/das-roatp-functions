using SFA.DAS.Roatp.Functions.ApplyTypes;
using System;

namespace SFA.DAS.Roatp.Functions.UnitTests.Generators
{
    public static class ExtractedApplicationGenerator
    {
        public static ExtractedApplication GenerateExtractedApplication(Apply application, DateTime extractedDate, bool qnaFilesExtracted)
        {
            return new ExtractedApplication
            {
                ApplicationId = application.ApplicationId,
                ExtractedDate = extractedDate,
                QnaFilesExtracted = qnaFilesExtracted,
                Apply = application
            };
        }
    }
}
