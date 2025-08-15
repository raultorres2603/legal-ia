using Legal_IA.Models;

namespace Legal_IA.Services;

public class FinancialCalculationService
{
    public class QuarterlyFinancialData
    {
        public decimal TotalIncomeBase { get; set; }
        public decimal TotalVATCharged { get; set; }
        public decimal TotalIRPFRetained { get; set; }
        public decimal TotalIncomeWithVAT { get; set; }
        public int InvoiceCount { get; set; }
        public int QuarterNumber { get; set; }
        public int Year { get; set; }
        public List<Invoice> Invoices { get; set; } = new();
    }

    public class YearlyFinancialData
    {
        public decimal TotalIncomeBase { get; set; }
        public decimal TotalVATCharged { get; set; }
        public decimal TotalIRPFRetained { get; set; }
        public decimal TotalIncomeWithVAT { get; set; }
        public int InvoiceCount { get; set; }
        public int Year { get; set; }
        public List<QuarterlyFinancialData> QuarterlyBreakdown { get; set; } = new();
    }

    public static QuarterlyFinancialData CalculateQuarterlyFinancials(
        List<Invoice> userInvoices, 
        int quarter, 
        int year)
    {
        var quarterlyData = new QuarterlyFinancialData
        {
            QuarterNumber = quarter,
            Year = year
        };

        // Determine quarter date ranges
        var (startDate, endDate) = GetQuarterDateRange(quarter, year);

        // Filter invoices for the specific quarter
        var quarterInvoices = userInvoices
            .Where(i => i.IssueDate >= startDate && i.IssueDate <= endDate)
            .ToList();

        quarterlyData.Invoices = quarterInvoices;
        quarterlyData.InvoiceCount = quarterInvoices.Count;

        // Calculate totals from invoice items
        foreach (var invoice in quarterInvoices)
        {
            if (invoice.Items != null)
            {
                foreach (var item in invoice.Items)
                {
                    var itemTotal = item.Quantity * item.UnitPrice;
                    quarterlyData.TotalIncomeBase += itemTotal;
                    quarterlyData.TotalVATCharged += itemTotal * (item.VAT / 100);
                    quarterlyData.TotalIRPFRetained += itemTotal * (item.IRPF / 100);
                }
            }
        }

        quarterlyData.TotalIncomeWithVAT = quarterlyData.TotalIncomeBase + quarterlyData.TotalVATCharged;

        return quarterlyData;
    }

    public static YearlyFinancialData CalculateYearlyFinancials(
        List<Invoice> userInvoices, 
        int year)
    {
        var yearlyData = new YearlyFinancialData
        {
            Year = year
        };

        // Calculate each quarter
        for (int quarter = 1; quarter <= 4; quarter++)
        {
            var quarterlyData = CalculateQuarterlyFinancials(userInvoices, quarter, year);
            yearlyData.QuarterlyBreakdown.Add(quarterlyData);

            // Add to yearly totals
            yearlyData.TotalIncomeBase += quarterlyData.TotalIncomeBase;
            yearlyData.TotalVATCharged += quarterlyData.TotalVATCharged;
            yearlyData.TotalIRPFRetained += quarterlyData.TotalIRPFRetained;
            yearlyData.InvoiceCount += quarterlyData.InvoiceCount;
        }

        yearlyData.TotalIncomeWithVAT = yearlyData.TotalIncomeBase + yearlyData.TotalVATCharged;

        return yearlyData;
    }

    private static (DateTime startDate, DateTime endDate) GetQuarterDateRange(int quarter, int year)
    {
        return quarter switch
        {
            1 => (new DateTime(year, 1, 1), new DateTime(year, 3, 31)),
            2 => (new DateTime(year, 4, 1), new DateTime(year, 6, 30)),
            3 => (new DateTime(year, 7, 1), new DateTime(year, 9, 30)),
            4 => (new DateTime(year, 10, 1), new DateTime(year, 12, 31)),
            _ => throw new ArgumentException("Quarter must be between 1 and 4", nameof(quarter))
        };
    }

    public static string FormatCurrency(decimal amount)
    {
        return amount.ToString("C", new System.Globalization.CultureInfo("es-ES"));
    }

    public static bool ShouldPresentModel347(decimal yearlyIncome)
    {
        return yearlyIncome > 3005.06m; // Spanish modelo 347 threshold
    }

    public static string GetQuarterDeadline(int quarter, int year)
    {
        return quarter switch
        {
            1 => $"20 de abril de {year}",
            2 => $"20 de julio de {year}",
            3 => $"20 de octubre de {year}",
            4 => $"30 de enero de {year + 1}",
            _ => "Fecha no válida"
        };
    }
}
