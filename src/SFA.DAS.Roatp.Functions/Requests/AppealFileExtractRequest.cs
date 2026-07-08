using System;

namespace SFA.DAS.Roatp.Functions.Requests;

public class AppealFileExtractRequest
{
    public Guid ApplicationId { get; set; }
    public string FileName { get; set; }
}
