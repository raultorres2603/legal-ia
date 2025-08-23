using Legal_IA.Shared.Data;
using Legal_IA.Shared.Models;
using Legal_IA.Shared.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Legal_IA.Shared.Repositories;

/// <summary>
///     User repository implementation with specific user operations
/// </summary>
public class UserRepository(LegalIaDbContext context, ILogger<UserRepository> logger)
    : Repository<User>(context), IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await DbSet.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByDNIAsync(string dni)
    {
        return await DbSet.FirstOrDefaultAsync(u => u.DNI == dni);
    }

    public async Task<User?> GetByCIFAsync(string cif)
    {
        return await DbSet.FirstOrDefaultAsync(u => u.CIF == cif);
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        try
        {
            return await DbSet
                .Where(u => u.IsActive)
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();
        }
        catch (Exception e)
        {
            throw new Exception("Error retrieving active users", e);
        }
    }

    public async Task<User?> GetByVerificationTokenAsync(string token)
    {
        return await DbSet.FirstOrDefaultAsync(u => u.EmailVerificationToken == token);
    }
}