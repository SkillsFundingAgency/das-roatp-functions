using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SFA.DAS.Roatp.Functions.ApplyTypes;

namespace SFA.DAS.Roatp.Functions.Infrastructure.Databases
{
    public class ApplyDataContext : DbContext
    {
        public ApplyDataContext(DbContextOptions<ApplyDataContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var jsonSerializerSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

            modelBuilder.Entity<Apply>()
                .Property(prop => prop.ApplyData)
                .HasConversion(
                    con => JsonConvert.SerializeObject(con, jsonSerializerSettings),
                    con => JsonConvert.DeserializeObject<ApplyData>(con, jsonSerializerSettings));

           
        }

        public virtual DbSet<Apply> Apply { get; set; }
        public virtual DbSet<ExtractedApplication> ExtractedApplications { get; set; }
        public virtual DbSet<SubmittedApplicationAnswer> SubmittedApplicationAnswers { get; set; }
    }
}
