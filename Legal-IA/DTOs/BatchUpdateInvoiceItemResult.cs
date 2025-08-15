using System.Collections.Generic;
using Legal_IA.Models;

namespace Legal_IA.DTOs
{
    public class BatchUpdateInvoiceItemResult
    {
        public bool Success { get; set; }
        public List<InvoiceItem> Items { get; set; } = new();
        public string? Error { get; set; }
    }
}

