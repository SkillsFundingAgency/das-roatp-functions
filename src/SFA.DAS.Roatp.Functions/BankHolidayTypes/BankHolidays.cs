using System.Collections.Generic;

namespace SFA.DAS.Roatp.Functions.BankHolidayTypes;

public class BankHolidays
{
    public string Division { get; set; }
    public List<Event> Events { get; set; }
}
