using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Legal_IA.Shared.Models;

namespace AI_Agent.Interfaces.Repositories
{
    public interface IInvoiceItemRepository
    {
        Task<List<InvoiceItem>> GetInvoiceItemsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
