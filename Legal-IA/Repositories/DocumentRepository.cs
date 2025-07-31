using Legal_IA.Data;
using Legal_IA.Interfaces.Repositories;
using Legal_IA.Models;
using Microsoft.EntityFrameworkCore;

namespace Legal_IA.Repositories;

/// <summary>
///     Document repository implementation with specific document operations
/// </summary>
public class DocumentRepository : Repository<Document>, IDocumentRepository
{
    public DocumentRepository(LegalIADbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Document>> GetByUserIdAsync(Guid userId)
    {
        return await DbSet
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Document>> GetByTypeAsync(DocumentType type)
    {
        return await DbSet
            .Where(d => d.Type == type)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Document>> GetByStatusAsync(DocumentStatus status)
    {
        return await DbSet
            .Where(d => d.Status == status)
            .OrderByDescending(d => d.UpdatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Document>> GetTemplatesAsync()
    {
        return await DbSet
            .Where(d => d.IsTemplate)
            .OrderBy(d => d.Type)
            .ThenBy(d => d.Title)
            .ToListAsync();
    }

    public async Task<IEnumerable<Document>> SearchAsync(string searchTerm, Guid? userId = null)
    {
        var query = DbSet.AsQueryable();

        if (userId.HasValue)
            query = query.Where(d => d.UserId == userId.Value);

        query = query.Where(d =>
            d.Title.Contains(searchTerm) ||
            d.Description.Contains(searchTerm) ||
            d.Tags.Contains(searchTerm));

        return await query
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Document>> GetByQuarterAndYearAsync(int quarter, int year)
    {
        return await DbSet
            .Where(d => d.Quarter == quarter && d.Year == year)
            .OrderBy(d => d.Type)
            .ThenBy(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Document>> GetRecentDocumentsAsync(Guid userId, int count = 10)
    {
        return await DbSet
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.UpdatedAt)
            .Take(count)
            .ToListAsync();
    }
}