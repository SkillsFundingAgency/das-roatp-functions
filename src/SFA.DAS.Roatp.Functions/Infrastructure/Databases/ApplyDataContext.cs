using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SFA.DAS.Roatp.Functions.ApplyTypes;
using SFA.DAS.Roatp.Functions.BankHolidayTypes;

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

            modelBuilder.Entity<Apply>(entity =>
            {
                entity.Property(prop => prop.ApplyData)
                .HasConversion(
                    con => JsonConvert.SerializeObject(con, jsonSerializerSettings),
                    con => JsonConvert.DeserializeObject<ApplyData>(con, jsonSerializerSettings));
            });

            modelBuilder.Entity<Appeal>(entity =>
            {
                entity.ToTable("Appeal");

                entity.HasOne(appeal => appeal.Apply)
                    .WithOne(app => app.Appeal)
                    .HasPrincipalKey<Apply>("ApplicationId")
                    .HasForeignKey<Appeal>("ApplicationId")
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<AppealFile>(entity =>
            {
                entity.ToTable("AppealFile");

                entity.HasOne(appealFile => appealFile.Appeal)
                    .WithMany(appeal => appeal.AppealFiles)
                    .HasPrincipalKey(ea => ea.ApplicationId)
                    .HasForeignKey(saa => saa.ApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

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
                entity.HasOne(saa => saa.ExtractedApplication)
                    .WithMany(ea => ea.SubmittedApplicationAnswers)
                    .HasPrincipalKey(ea => ea.ApplicationId)
                    .HasForeignKey(saa => saa.ApplicationId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<AssessorClarificationOutcome>(entity =>
            {
                entity.ToTable("ModeratorPageReviewOutcome");

                entity.HasOne(aco => aco.Apply)
                    .WithMany(app => app.AssessorClarificationOutcomes)
                    .HasPrincipalKey(app => app.ApplicationId)
                    .HasForeignKey(aco => aco.ApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<FinancialReviewDetails>(entity =>
            {
                entity.HasOne(fr => fr.Apply)
                    .WithOne(app => app.FinancialReview)
                    .HasPrincipalKey<Apply>("ApplicationId")
                    .HasForeignKey<FinancialReviewDetails>("ApplicationId")
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<FinancialReviewClarificationFile>(entity =>
            {
                entity.HasOne(cf => cf.FinancialReview)
                    .WithMany(fr => fr.ClarificationFiles)
                    .HasPrincipalKey(fr => fr.ApplicationId)
                    .HasForeignKey(cf => cf.ApplicationId)
                    .OnDelete(DeleteBehavior.NoAction);
            });
        }

        public virtual DbSet<Apply> Apply { get; set; }
        public virtual DbSet<Appeal> Appeals { get; set; }
        public virtual DbSet<AppealFile> AppealFiles { get; set; }
        public virtual DbSet<ExtractedApplication> ExtractedApplications { get; set; }
        public virtual DbSet<SubmittedApplicationAnswer> SubmittedApplicationAnswers { get; set; }
        public virtual DbSet<AssessorClarificationOutcome> AssessorClarificationOutcomes { get; set; }
        public virtual DbSet<FinancialReviewDetails> FinancialReview { get; set; }
        public virtual DbSet<FinancialReviewClarificationFile> FinancialReviewClarificationFile { get; set; }
        public virtual DbSet<BankHoliday> BankHoliday { get; set; }
        public virtual DbSet<OrganisationManagement> OrganisationManagement { get; set; }
        public virtual DbSet<OrganisationPersonnel> OrganisationPersonnel { get; set; }
    }
}
