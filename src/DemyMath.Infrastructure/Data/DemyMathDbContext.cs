using DemyMath.Domain.Entities;
using DemyMath.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DemyMath.Infrastructure.Data;

public class DemyMathDbContext
    : IdentityDbContext<ApplicationUser>
{
    public DemyMathDbContext(
        DbContextOptions<DemyMathDbContext> options)
        : base(options)
    {
    }

    public DbSet<Topic> Topics => Set<Topic>();
    public DbSet<TopicPrerequisite> TopicPrerequisites => Set<TopicPrerequisite>();
    public DbSet<ContentItem> ContentItems => Set<ContentItem>();
    public DbSet<LocalizedString> LocalizedStrings => Set<LocalizedString>();

    public DbSet<Test> Tests => Set<Test>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<MultipleChoiceQuestion> MultipleChoiceQuestions => Set<MultipleChoiceQuestion>();
    public DbSet<NumericQuestion> NumericQuestions => Set<NumericQuestion>();
    public DbSet<CoordinatePointQuestion> CoordinatePointQuestions => Set<CoordinatePointQuestion>();
    public DbSet<QuestionOption> QuestionOptions => Set<QuestionOption>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Topic>(entity =>
        {
            entity.Property(t => t.TitleStringId)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(t => t.DescriptionStringId)
                .HasMaxLength(200);

            entity.Property(t => t.Icon)
                .HasMaxLength(32);

            entity.HasIndex(t => t.TitleStringId);
        });

        builder.Entity<TopicPrerequisite>(entity =>
        {
            entity.HasKey(p => new
            {
                p.TopicId,
                p.PrerequisiteTopicId
            });

            entity.ToTable(table =>
                table.HasCheckConstraint(
                    "CK_TopicPrerequisite_NoSelfReference",
                    "\"TopicId\" <> \"PrerequisiteTopicId\""));

            entity.HasOne(p => p.Topic)
                .WithMany(t => t.Prerequisites)
                .HasForeignKey(p => p.TopicId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.PrerequisiteTopic)
                .WithMany()
                .HasForeignKey(p => p.PrerequisiteTopicId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ContentItem>(entity =>
        {
            entity.Property(c => c.Type)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(c => c.TextStringId)
                .HasMaxLength(200);

            entity.Property(c => c.VideoUrl)
                .HasMaxLength(2048);

            entity.HasIndex(c => new
            {
                c.TopicId,
                c.Order
            }).IsUnique();

            entity.HasOne(c => c.Topic)
                .WithMany(t => t.ContentItems)
                .HasForeignKey(c => c.TopicId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<LocalizedString>(entity =>
        {
            entity.HasKey(s => new
            {
                s.StringId,
                s.Culture
            });

            entity.Property(s => s.StringId)
                .HasMaxLength(200);

            entity.Property(s => s.Culture)
                .HasMaxLength(10);

            entity.Property(s => s.Value)
                .IsRequired();
        });

        builder.Entity<Test>(entity =>
        {
            entity.HasIndex(t => t.TopicId)
                .IsUnique();

            entity.HasOne(t => t.Topic)
                .WithOne()
                .HasForeignKey<Test>(t => t.TopicId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Question>(entity =>
        {
            entity.UseTptMappingStrategy();
            entity.ToTable("Questions");

            entity.Property(q => q.TextStringId)
                .HasMaxLength(200)
                .IsRequired();

            entity.HasIndex(q => new
            {
                q.TestId,
                q.Order
            }).IsUnique();

            entity.HasOne(q => q.Test)
                .WithMany(t => t.Questions)
                .HasForeignKey(q => q.TestId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<MultipleChoiceQuestion>()
            .ToTable("MultipleChoiceQuestions");

        builder.Entity<NumericQuestion>()
            .ToTable(
                "NumericQuestions",
                table => table.HasCheckConstraint(
                    "CK_NumericQuestion_NonNegativeTolerance",
                    "\"Tolerance\" >= 0"));

        builder.Entity<CoordinatePointQuestion>()
            .ToTable(
                "CoordinatePointQuestions",
                table =>
                {
                    table.HasCheckConstraint(
                        "CK_CoordinateQuestion_NonNegativeTolerance",
                        "\"Tolerance\" >= 0");

                    table.HasCheckConstraint(
                        "CK_CoordinateQuestion_ValidXRange",
                        "\"MinX\" < \"MaxX\"");

                    table.HasCheckConstraint(
                        "CK_CoordinateQuestion_ValidYRange",
                        "\"MinY\" < \"MaxY\"");

                    table.HasCheckConstraint(
                        "CK_CoordinateQuestion_PositiveGridStep",
                        "\"GridStep\" > 0");
                });

        builder.Entity<QuestionOption>(entity =>
        {
            entity.Property(o => o.TextStringId)
                .HasMaxLength(200)
                .IsRequired();

            entity.HasIndex(o => new
            {
                o.QuestionId,
                o.Order
            }).IsUnique();
        });

        builder.Entity<MultipleChoiceQuestion>()
            .HasMany(q => q.Options)
            .WithOne(o => o.Question)
            .HasForeignKey(o => o.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}