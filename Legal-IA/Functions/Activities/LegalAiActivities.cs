using AI_Agent.Interfaces;
using AI_Agent.Models;
using Legal_IA.Shared.Models;
using Legal_IA.Shared.Repositories.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Legal_IA.Functions.Orchestrators;

namespace Legal_IA.Functions.Activities
{
    public class LegalAiActivities(
        ILogger<LegalAiActivities> logger,
        ILegalAiAgent legalAiAgent,
        IUserRepository userRepository,
        IInvoiceRepository invoiceRepository,
        IInvoiceItemRepository invoiceItemRepository)
    {

        [Function("BuildUserFullContextActivity")]
        public async Task<UserFullContext?> BuildUserFullContextActivity([ActivityTrigger] BuildUserFullContextInput input)
        {
            try
            {
                logger.LogInformation("Building full user context for userId: {UserId}", input.UserId);

                var user = await userRepository.GetByIdAsync(input.UserId);
                if (user == null) return null;

                var userFullContext = new UserFullContext();

                // Build user context if requested
                if (input.IncludeUserData)
                {
                    var invoices = await invoiceRepository.GetInvoicesByUserIdAsync(input.UserId);
                    var currentYear = DateTime.Now.Year;
                    var currentQuarter = GetCurrentQuarter();

                    // Filter invoices for current year and quarter
                    var currentYearInvoices = invoices.Where(i => i.IssueDate.Year == currentYear).ToList();
                    var currentQuarterInvoices = currentYearInvoices.Where(i => GetQuarterFromDate(i.IssueDate) == currentQuarter).ToList();

                    userFullContext.UserContext = new UserContext
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

                        ActivityCode = null, // Could be added to User model
                        TaxRegime = "general" // Could be added to User model
                    };
                }

                // Include invoice data if requested
                if (input.IncludeInvoiceData)
                {
                    userFullContext.Invoices = await invoiceRepository.GetInvoicesByUserIdAsync(input.UserId);
                    userFullContext.InvoiceItems = await invoiceItemRepository.GetInvoiceItemsByUserIdAsync(input.UserId);
                }

                return userFullContext;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error building full user context for userId: {UserId}", input.UserId);
                return null;
            }
        }

        [Function("ProcessLegalQuestionWithFullContextActivity")]
        public async Task<LegalQueryResponse> ProcessLegalQuestionWithFullContextActivity([ActivityTrigger] ProcessLegalQuestionInput input)
        {
            try
            {
                logger.LogInformation("Processing legal question with full context: {Question}", input.Request?.Question);

                if (input.UserFullContext != null)
                {
                    return await legalAiAgent.ProcessQuestionWithFullContextAsync(input.Request, input.UserFullContext, CancellationToken.None);
                }

                return await legalAiAgent.ProcessQuestionAsync(input.Request);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing legal question with full context");
                return new LegalQueryResponse
                {
                    Success = false,
                    ErrorMessage = "Error processing legal question",
                    Answer = "Lo siento, ha ocurrido un error al procesar su consulta. Por favor, inténtelo de nuevo.",
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        [Function("GetFormGuidanceWithFullContextActivity")]
        public async Task<string> GetFormGuidanceWithFullContextActivity([ActivityTrigger] FormGuidanceInput input)
        {
            try
            {
                logger.LogInformation("Getting form guidance with full context for form: {FormType}", input.FormType);

                if (input.UserFullContext != null)
                {
                    return await legalAiAgent.GetFormGuidanceWithFullContextAsync(input.Question, input.FormType, input.UserFullContext, CancellationToken.None);
                }

                return await legalAiAgent.GetFormGuidanceAsync(input.FormType);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting form guidance with full context for form: {FormType}", input.FormType);
                return "Error al obtener la guía del formulario. Por favor, inténtelo de nuevo.";
            }
        }

        [Function("GetQuarterlyObligationsWithFullContextActivity")]
        public async Task<string> GetQuarterlyObligationsWithFullContextActivity([ActivityTrigger] QuarterlyObligationsInput input)
        {
            try
            {
                logger.LogInformation("Getting quarterly obligations with full context for Q{Quarter} {Year}", input.Quarter, input.Year);

                if (input.UserFullContext != null)
                {
                    return await legalAiAgent.GetQuarterlyObligationsWithFullContextAsync(input.Question, input.Quarter, input.Year, input.UserFullContext, CancellationToken.None);
                }

                return await legalAiAgent.GetQuarterlyObligationsAsync(input.Quarter, input.Year);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting quarterly obligations with full context for Q{Quarter} {Year}", input.Quarter, input.Year);
                return "Error al obtener obligaciones trimestrales. Por favor, inténtelo de nuevo.";
            }
        }

        [Function("GetAnnualObligationsWithFullContextActivity")]
        public async Task<string> GetAnnualObligationsWithFullContextActivity([ActivityTrigger] AnnualObligationsInput input)
        {
            try
            {
                logger.LogInformation("Getting annual obligations with full context for year {Year}", input.Year);

                if (input.UserFullContext != null)
                {
                    return await legalAiAgent.GetAnnualObligationsWithFullContextAsync(input.Question, input.Year, input.UserFullContext, CancellationToken.None);
                }

                return await legalAiAgent.GetAnnualObligationsAsync(input.Year);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting annual obligations with full context for year {Year}", input.Year);
                return "Error al obtener obligaciones anuales. Por favor, inténtelo de nuevo.";
            }
        }

        [Function("ClassifyLegalQuestionActivity")]
        public async Task<bool> ClassifyLegalQuestionActivity([ActivityTrigger] string question)
        {
            try
            {
                logger.LogInformation("Classifying legal question: {Question}", question);
                return await legalAiAgent.ClassifyLegalQuestionAsync(question, CancellationToken.None);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error classifying legal question: {Question}", question);
                // Return true as fallback to avoid rejecting legal questions incorrectly
                return true;
            }
        }

        // Helper methods
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
}