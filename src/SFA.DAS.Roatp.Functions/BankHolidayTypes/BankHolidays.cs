using System.Collections.Generic;
using SFA.DAS.Roatp.Functions.ApplyTypes;

namespace SFA.DAS.Roatp.Functions.BankHolidayTypes
{
    public class BankHolidays
    {
        public string Division { get; set; }
        public List<Event> Events { get; set; }
    }
}