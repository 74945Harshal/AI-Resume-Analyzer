using AIResumeAnalyzer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIResumeAnalyzer.Persistence.Configurations;

public class ResumeAnalysisConfiguration : IEntityTypeConfiguration<ResumeAnalysis>
{
    public void Configure(EntityTypeBuilder<ResumeAnalysis> builder)
    {
        builder.HasKey(ra => ra.Id);
        builder.Property(ra => ra.Summary).IsRequired();
        builder.Property(ra => ra.MatchScore).IsRequired();

        builder.HasOne(ra => ra.Resume)
            .WithMany(r => r.Analyses)
            .HasForeignKey(ra => ra.ResumeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ra => ra.JobDescription)
            .WithMany(jd => jd.Analyses)
            .HasForeignKey(ra => ra.JobDescriptionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(ra => ra.Skills)
            .WithOne(s => s.ResumeAnalysis)
            .HasForeignKey(s => s.ResumeAnalysisId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(ra => ra.InterviewQuestions)
            .WithOne(iq => iq.ResumeAnalysis)
            .HasForeignKey(iq => iq.ResumeAnalysisId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
