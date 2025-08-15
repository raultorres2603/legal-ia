using System.Collections.Generic;
using Legal_IA.Models;

namespace Legal_IA.DTOs;

public class BatchCreateInvoiceItemOrchestratorInput
{
    public List<InvoiceItem> CreateRequests { get; set; } = new();
}

