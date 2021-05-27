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
            var jsonSerializerSettings = new JsonSerializerSettings 
            { 
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            modelBuilder.Entity<Apply>()
                .Property(prop => prop.ApplyData)
                .HasConversion(
                    con => JsonConvert.SerializeObject(con, jsonSerializerSettings),
                    con => JsonConvert.DeserializeObject<ApplyData>(con, jsonSerializerSettings));

            modelBuilder.Entity<ExtractedApplication>(entity =>
            {
                entity.HasOne(ea => ea.Apply)
                    .WithOne(app => app.ExtractedApplication)
                    .HasPrincipalKey<Apply>("ApplicationId")
                    .HasForeignKey<ExtractedApplication>("ApplicationId")
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<SubmittedApplicationAnswer>(entity =>
            {
                entity.Ignore(saa => saa.SequenceNumber)
                    .Ignore(saa => saa.SectionNumber)
                    .HasOne(saa => saa.ExtractedApplication)
                    .WithMany(ea => ea.SubmittedApplicationAnswers)
                    .HasPrincipalKey(ea => ea.ApplicationId)
                    .HasForeignKey(saa => saa.ApplicationId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });
        }

        public virtual DbSet<Apply> Apply { get; set; }
        public virtual DbSet<ExtractedApplication> ExtractedApplications { get; set; }
        public virtual DbSet<SubmittedApplicationAnswer> SubmittedApplicationAnswers { get; set; }
    }
}
