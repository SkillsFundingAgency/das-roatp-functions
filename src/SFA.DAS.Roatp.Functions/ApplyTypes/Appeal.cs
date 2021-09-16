using System;
using System.Collections.Generic;

namespace SFA.DAS.Roatp.Functions.ApplyTypes
{
    public class Appeal
    {
        public Guid Id { get; set; }
        public Guid ApplicationId { get; set; }
        public DateTime? AppealSubmittedDate { get; set; }

        public virtual Apply Apply { get; set; }

        public virtual ICollection<AppealFile> AppealFiles { get; set; }
    }

    public class AppealFile
    {
        public Guid Id { get; set; }
        public Guid ApplicationId { get; set; }
        public string FileName { get; set; }

        public virtual Appeal Appeal { get; set; }
    }
}
