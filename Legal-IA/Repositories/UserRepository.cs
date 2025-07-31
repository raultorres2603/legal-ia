using Legal_IA.Data;
using Legal_IA.Interfaces.Repositories;
using Legal_IA.Models;
using Microsoft.EntityFrameworkCore;

namespace Legal_IA.Repositories;

/// <summary>
///     User repository implementation with specific user operations
/// </summary>
public class UserRepository(LegalIADbContext context) : Repository<User>(context), IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByDNIAsync(string dni)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.DNI == dni);
    }

    public async Task<User?> GetByCIFAsync(string cif)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.CIF == cif);
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        return await _dbSet
            .Where(u => u.IsActive)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();
    }
}