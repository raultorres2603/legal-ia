using AI_Agent.Interfaces;
using AI_Agent.Models;
using Legal_IA.Interfaces.Repositories;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Legal_IA.Functions.Activities;

public class LegalAiActivities
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ILegalAiAgent _legalAiAgent;
    private readonly ILogger<LegalAiActivities> _logger;
    private readonly IUserRepository _userRepository;

    public LegalAiActivities(ILogger<LegalAiActivities> logger,
        ILegalAiAgent legalAiAgent,
        IUserRepository userRepository,
        IInvoiceRepository invoiceRepository)
    {
        _logger = logger;
        _legalAiAgent = legalAiAgent;
        _userRepository = userRepository;
        _invoiceRepository = invoiceRepository;
    }

    [Function("BuildUserContextActivity")]
    public async Task<UserContext?> BuildUserContextActivity([ActivityTrigger] Guid userId)
    {
        try
        {
            _logger.LogInformation("Building user context for userId: {UserId}", userId);

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return null;

            var invoices = await _invoiceRepository.GetInvoicesByUserIdAsync(userId);

            var currentYear = DateTime.Now.Year;
            var currentQuarter = GetCurrentQuarter();

            // Filter invoices for current year and quarter
            var currentYearInvoices = invoices.Where(i => i.IssueDate.Year == currentYear).ToList();
            var currentQuarterInvoices =
                currentYearInvoices.Where(i => GetQuarterFromDate(i.IssueDate) == currentQuarter).ToList();

            return new UserContext
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                DNI = user.DNI,
                CIF = user.CIF,
                BusinessName = user.BusinessName,
                Address = user.Address,
                PostalCode = user.PostalCode,
                City = user.City,
                Province = user.Province,
                Phone = user.Phone,

                // Calculate financial data from invoices
                TotalIncomeCurrentYear = currentYearInvoices.Sum(i => i.Subtotal),
                TotalVATCurrentYear = currentYearInvoices.Sum(i => i.VAT),
                TotalIRPFCurrentYear = currentYearInvoices.Sum(i => i.IRPF),
                TotalIncomeCurrentQuarter = currentQuarterInvoices.Sum(i => i.Subtotal),
                TotalVATCurrentQuarter = currentQuarterInvoices.Sum(i => i.VAT),
                TotalIRPFCurrentQuarter = currentQuarterInvoices.Sum(i => i.IRPF),
                InvoiceCountCurrentYear = currentYearInvoices.Count,
                InvoiceCountCurrentQuarter = currentQuarterInvoices.Count,

                // These could be enhanced with additional user profile fields
                ActivityCode = null, // Could be added to User model
                TaxRegime = "general" // Could be added to User model
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building user context for userId: {UserId}", userId);
            return null;
        }
    }

    [Function("ProcessLegalQuestionActivity")]
    public async Task<LegalQueryResponse> ProcessLegalQuestionActivity([ActivityTrigger] dynamic input)
    {
        try
        {
            var request = (LegalQueryRequest)input.Request;
            var userContext = (UserContext?)input.UserContext;

            _logger.LogInformation("Processing legal question: {Question}", request.Question);

            if (userContext != null) return await _legalAiAgent.ProcessQuestionAsync(request, userContext);

            return await _legalAiAgent.ProcessQuestionAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing legal question");
            return new LegalQueryResponse
            {
                Success = false,
                ErrorMessage = "Error processing legal question",
                Answer = "Lo siento, ha ocurrido un error al procesar su consulta. Por favor, inténtelo de nuevo.",
                Timestamp = DateTime.UtcNow
            };
        }
    }

    [Function("GetFormGuidanceActivity")]
    public async Task<AutonomoFormResponse> GetFormGuidanceActivity([ActivityTrigger] dynamic input)
    {
        try
        {
            var request = (AutonomoFormRequest)input.Request;
            var userContext = (UserContext?)input.UserContext;

            _logger.LogInformation("Getting form guidance for: {FormType}", request.FormType);

            if (userContext != null) return await _legalAiAgent.GetFormGuidanceAsync(request, userContext);

            return await _legalAiAgent.GetFormGuidanceAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting form guidance");
            return new AutonomoFormResponse
            {
                Success = false,
                ErrorMessage = "Error processing form guidance request",
                FormType = ((AutonomoFormRequest)input.Request).FormType
            };
        }
    }

    [Function("GetQuarterlyObligationsActivity")]
    public async Task<string> GetQuarterlyObligationsActivity([ActivityTrigger] dynamic input)
    {
        try
        {
            var quarter = (int)input.Quarter;
            var year = (int)input.Year;
            var userContext = (UserContext?)input.UserContext;

            _logger.LogInformation("Getting quarterly obligations for Q{Quarter} {Year}", quarter, year);

            if (userContext != null)
                return await _legalAiAgent.GetQuarterlyObligationsAsync(quarter, year, userContext);

            return await _legalAiAgent.GetQuarterlyObligationsAsync(quarter, year);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting quarterly obligations");
            return "Error al obtener obligaciones trimestrales. Por favor, inténtelo de nuevo.";
        }
    }

    [Function("GetAnnualObligationsActivity")]
    public async Task<string> GetAnnualObligationsActivity([ActivityTrigger] dynamic input)
    {
        try
        {
            var year = (int)input.Year;
            var userContext = (UserContext?)input.UserContext;

            _logger.LogInformation("Getting annual obligations for year {Year}", year);

            if (userContext != null) return await _legalAiAgent.GetAnnualObligationsAsync(year, userContext);

            return await _legalAiAgent.GetAnnualObligationsAsync(year);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting annual obligations");
            return "Error al obtener obligaciones anuales. Por favor, inténtelo de nuevo.";
        }
    }

    [Function("ClassifyLegalQuestionActivity")]
    public async Task<bool> ClassifyLegalQuestionActivity([ActivityTrigger] string question)
    {
        try
        {
            _logger.LogInformation("Classifying legal question: {Question}", question);
            return await _legalAiAgent.IsLegalQuestionAsync(question);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error classifying legal question");
            // Return true as fallback to avoid rejecting legal questions incorrectly
            return true;
        }
    }

    private static int GetCurrentQuarter()
    {
        var month = DateTime.Now.Month;
        return month switch
        {
            >= 1 and <= 3 => 1,
            >= 4 and <= 6 => 2,
            >= 7 and <= 9 => 3,
            >= 10 and <= 12 => 4,
            _ => 1
        };
    }

    private static int GetQuarterFromDate(DateTime date)
    {
        var month = date.Month;
        return month switch
        {
            >= 1 and <= 3 => 1,
            >= 4 and <= 6 => 2,
            >= 7 and <= 9 => 3,
            >= 10 and <= 12 => 4,
            _ => 1
        };
    }
}