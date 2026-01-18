using BrainShelf.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace BrainShelf.Infrastructure.Data;

/// <summary>
/// Application database context
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Entry> Entries => Set<Entry>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Metadata> Metadata => Set<Metadata>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Project entity
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Color).IsRequired().HasMaxLength(7);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            
            entity.HasMany(e => e.Entries)
                  .WithOne(e => e.Project)
                  .HasForeignKey(e => e.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Entry entity
        modelBuilder.Entity<Entry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.Url).HasMaxLength(2000);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            // Create indexes for searchable fields
            entity.HasIndex(e => e.Title);
            entity.HasIndex(e => e.Content);
            
            entity.HasMany(e => e.Tags)
                  .WithMany(t => t.Entries)
                  .UsingEntity(j => j.ToTable("EntryTags"));
        });

        // Configure Tag entity
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            // Create index for tag name for efficient searching
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Configure Metadata entity
        modelBuilder.Entity<Metadata>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Keywords).HasMaxLength(1000);
            entity.Property(e => e.ImageUrl).HasMaxLength(2000);
            entity.Property(e => e.FaviconUrl).HasMaxLength(2000);
            entity.Property(e => e.Author).HasMaxLength(200);
            entity.Property(e => e.SiteName).HasMaxLength(200);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasOne(e => e.Entry)
                  .WithOne(e => e.Metadata)
                  .HasForeignKey<Metadata>(e => e.EntryId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Create index on EntryId for efficient lookups
            entity.HasIndex(e => e.EntryId).IsUnique();
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Automatically update timestamps
        var entries = ChangeTracker.Entries<BaseEntity>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
