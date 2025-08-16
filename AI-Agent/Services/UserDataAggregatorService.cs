using AI_Agent.Interfaces.Repositories;
using AI_Agent.Models;
using Legal_IA.Shared.Models;
using Legal_IA.Shared.Repositories.Interfaces;

namespace AI_Agent.Services
{
    public interface IUserDataAggregatorService
    {
        Task<UserFullContext> GetUserFullContextAsync(Guid userId, CancellationToken cancellationToken = default);
    }

    public class UserDataAggregatorService(
        IUserContextRepository userContextRepository,
        Interfaces.Repositories.IInvoiceRepository invoiceRepository,
        Interfaces.Repositories.IInvoiceItemRepository invoiceItemRepository)
        : IUserDataAggregatorService
    {
        public async Task<UserFullContext> GetUserFullContextAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var userContext = await userContextRepository.GetUserContextAsync(userId, cancellationToken);
            var invoices = await invoiceRepository.GetInvoicesByUserIdAsync(userId, cancellationToken);
            var invoiceItems = await invoiceItemRepository.GetInvoiceItemsByUserIdAsync(userId, cancellationToken);

            return new UserFullContext
            {
                UserContext = userContext,
                Invoices = invoices,
                InvoiceItems = invoiceItems
            };
        }
    }
}
