using Legal_IA.Models;
using Microsoft.EntityFrameworkCore;

namespace Legal_IA.Data;

public class LegalIaDbContext(DbContextOptions<LegalIaDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceItem> InvoiceItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DNI).IsRequired();
            entity.Property(e => e.CIF).IsRequired(false); // CIF is only required if BusinessName is set
            entity.Property(e => e.Email).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.DNI).IsUnique();
            entity.HasIndex(e => e.CIF).IsUnique();
        });
        // Invoice configuration
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.InvoiceNumber).IsRequired();
            entity.Property(e => e.IssueDate).IsRequired();
            entity.Property(e => e.ClientName).IsRequired();
            entity.Property(e => e.ClientNIF).IsRequired();
            entity.Property(e => e.ClientAddress).IsRequired();
            entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.VAT).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.IRPF).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.Total).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.Notes).IsRequired(false);
            entity.HasMany(e => e.Items)
                .WithOne(i => i.Invoice)
                .HasForeignKey(i => i.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.UserId).IsRequired();
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // InvoiceItem configuration
        modelBuilder.Entity<InvoiceItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.Quantity).IsRequired();
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.VAT).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.IRPF).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.Total).HasColumnType("decimal(18,2)").IsRequired();
            entity.HasOne(i => i.Invoice)
                .WithMany(e => e.Items)
                .HasForeignKey(i => i.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}