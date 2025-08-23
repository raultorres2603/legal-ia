using AI_Agent.Models;
using Legal_IA.Shared.Repositories.Interfaces;

namespace AI_Agent.Services;

public interface IUserDataAggregatorService
{
    Task<UserFullContext> GetUserFullContextAsync(Guid userId, CancellationToken cancellationToken = default);
}

public class UserDataAggregatorService(
    IUserContextRepository userContextRepository,
    IInvoiceRepository invoiceRepository,
    IInvoiceItemRepository invoiceItemRepository)
    : IUserDataAggregatorService
{
    public async Task<UserFullContext> GetUserFullContextAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        var userContext = await userContextRepository.GetUserContextAsync(userId, cancellationToken);
        var invoices = await invoiceRepository.GetInvoicesByUserIdAsync(userId);
        var invoiceItems = await invoiceItemRepository.GetByUserIdAsync(userId);

        return new UserFullContext
        {
            UserContext = userContext,
            Invoices = invoices,
            InvoiceItems = invoiceItems
        };
    }
}