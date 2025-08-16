using Legal_IA.Shared.Models;

namespace Legal_IA.Shared.Repositories.Interfaces;

/// <summary>
///     Interface for invoice repository to manage invoice-related database operations
/// </summary>
public interface IInvoiceRepository : IRepository<Invoice>
{
    /// <summary>
    ///     Gets all invoices for a specific user by their user ID.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>A list of invoices belonging to the user.</returns>
    Task<List<Invoice>> GetInvoicesByUserIdAsync(Guid userId);

    Task<bool> DeleteAsync(Guid id);
}

