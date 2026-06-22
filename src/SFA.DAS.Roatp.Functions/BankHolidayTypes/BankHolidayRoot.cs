using System.Text.Json.Serialization;

namespace SFA.DAS.Roatp.Functions.BankHolidayTypes;

public class BankHolidayRoot
{
    [JsonPropertyName("england-and-wales")]
    public BankHolidays EnglandAndWales { get; set; }
}
