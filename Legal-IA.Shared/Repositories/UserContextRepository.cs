using Legal_IA.Shared.Data;
using Legal_IA.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Legal_IA.Shared.Repositories
{
    /// <summary>
    /// Repository for fetching user context data for AI agent personalization.
    /// </summary>
    public class UserContextRepository : Interfaces.IUserContextRepository
    {
        private readonly LegalIaDbContext _context;

        public UserContextRepository(LegalIaDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Fetches user context data for the specified user ID and maps it to a UserContext model.
        /// </summary>
        /// <param name="userId">The user's unique identifier.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The user's context information for AI personalization.</returns>
        public async Task<UserContext> GetUserContextAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
            if (user == null)
                throw new InvalidOperationException($"User with ID {userId} not found.");

            // Map properties from User to UserContext (add more as needed)
            return new UserContext
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                DNI = user.DNI,
                CIF = user.CIF,
                BusinessName = user.BusinessName,
                Address = user.Address,
                PostalCode = user.PostalCode,
                City = user.City,
                Province = user.Province,
                Phone = user.Phone,
                // Financial summary fields can be filled here if available in User or related tables
            };
        }
    }
}
