using Microsoft.EntityFrameworkCore;
using SmartCRM.Domain.Entities;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace SmartCRM.Infrastructure.Persistence;

public class SmartCrmDbContext : DbContext
{
    public SmartCrmDbContext(DbContextOptions<SmartCrmDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Donor> Donors { get; set; }
    public DbSet<Campaign> Campaigns { get; set; }
    public DbSet<Donation> Donations { get; set; }
    public DbSet<KnowledgeBase> KnowledgeBases { get; set; }
    public DbSet<SentEmail> SentEmails { get; set; }
    public DbSet<SystemSetting> SystemSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Add pgvector extension
        modelBuilder.HasPostgresExtension("vector");

        modelBuilder.Entity<KnowledgeBase>(entity =>
        {
            entity.Property(e => e.Embedding)
                .HasColumnType("vector(384)"); // Updated for Ollama all-minilm (384)
        });
    }

    public IQueryable<KnowledgeBase> GetSimilarKnowledge(Vector vector, int limit = 3)
    {
        return KnowledgeBases
            .OrderBy(x => x.Embedding!.L2Distance(vector))
            .Take(limit);
    }
}
