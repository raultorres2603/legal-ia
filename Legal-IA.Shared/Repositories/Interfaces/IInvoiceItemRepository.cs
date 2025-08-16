using Legal_IA.Shared.Models;

namespace Legal_IA.Shared.Repositories.Interfaces;

public interface IInvoiceItemRepository: IRepository<InvoiceItem>
{
    Task<bool> DeleteAsync(Guid id);
    Task<List<InvoiceItem>> GetByUserIdAsync(Guid userId);
}