using Legal_IA.Models;
using Microsoft.EntityFrameworkCore;

namespace Legal_IA.Data;

public class LegalIADbContext : DbContext
{
    public LegalIADbContext(DbContextOptions<LegalIADbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DNI).IsRequired();
            entity.Property(e => e.CIF).IsRequired();
            entity.Property(e => e.Email).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.DNI).IsUnique();
            entity.HasIndex(e => e.CIF).IsUnique();
        });
    }
}