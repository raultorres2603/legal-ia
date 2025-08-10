using Legal_IA.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Legal_IA.Interfaces.Repositories;

/// <summary>
///   Interface for invoice item repository to manage invoice item-related database operations
/// </summary>
public interface IInvoiceItemRepository : IRepository<InvoiceItem>
{
    /// <summary>
    /// Gets all invoice items for a specific invoice by its ID.
    /// </summary>
    /// <param name="invoiceId">The ID of the invoice.</param>
    /// <returns>A collection of invoice items for the given invoice.</returns>
    Task<IEnumerable<InvoiceItem>> GetItemsByInvoiceIdAsync(int invoiceId);

    /// <summary>
    /// Gets all invoice items for a specific user by their user ID.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>A list of invoice items belonging to the user.</returns>
    Task<List<InvoiceItem>> GetByUserIdAsync(Guid userId);
}
