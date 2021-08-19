using Newtonsoft.Json;

namespace SFA.DAS.Roatp.Functions.ApplyTypes
{
    public class BankHolidayRoot
    {
        [JsonProperty("england-and-wales")]
        public BankHolidays EnglandAndWales { get; set; }
    }
}