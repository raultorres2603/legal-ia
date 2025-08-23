using Legal_IA.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Legal_IA.Shared.Data;

public class LegalIaDbContext(DbContextOptions<LegalIaDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceItem> InvoiceItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // ...existing code...
    }
}