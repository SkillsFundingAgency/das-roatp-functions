using Newtonsoft.Json;
using SFA.DAS.Roatp.Functions.ApplyTypes;

namespace SFA.DAS.Roatp.Functions.BankHolidayTypes
{
    public class BankHolidayRoot
    {
        [JsonProperty("england-and-wales")]
        public BankHolidays EnglandAndWales { get; set; }
    }
}