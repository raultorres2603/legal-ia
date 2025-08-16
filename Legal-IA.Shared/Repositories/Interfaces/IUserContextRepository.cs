using System;
using System.Threading;
using System.Threading.Tasks;
using Legal_IA.Shared.Models;

namespace Legal_IA.Shared.Repositories.Interfaces
{
    /// <summary>
    /// Interface for fetching user context data for AI agent personalization.
    /// </summary>
    public interface IUserContextRepository
    {
        /// <summary>
        /// Fetches user context data for the specified user ID and maps it to a UserContext model.
        /// </summary>
        /// <param name="userId">The user's unique identifier.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The user's context information for AI personalization.</returns>
        Task<UserContext> GetUserContextAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}

