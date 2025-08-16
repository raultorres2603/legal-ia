using System.Threading;
using System.Threading.Tasks;
using AI_Agent.Models;

namespace AI_Agent.Interfaces.Repositories
{
    public interface IUserContextRepository
    {
        Task<UserContext> GetUserContextAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
