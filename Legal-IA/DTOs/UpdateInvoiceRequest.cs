using System;
using System.Collections.Generic;
using Legal_IA.Enums;

namespace Legal_IA.DTOs
{
    public class UpdateInvoiceRequest
    {
        public string InvoiceNumber { get; set; }
        public DateTime IssueDate { get; set; }
        public string ClientName { get; set; }
        public string ClientNIF { get; set; }
        public string ClientAddress { get; set; }
        public decimal Subtotal { get; set; }
        public decimal VAT { get; set; }
        public decimal IRPF { get; set; }
        public decimal Total { get; set; }
        public string Notes { get; set; }
        public List<UpdateInvoiceItemRequest> Items { get; set; }
        public Guid UserId { get; set; } // Added for consistency with model
        public InvoiceStatus Status { get; set; }
    }

    public class UpdateInvoiceItemRequest
    {
        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal VAT { get; set; }
        public decimal IRPF { get; set; }
        public decimal Total { get; set; }
    }
}
