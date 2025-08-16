using AI_Agent.Interfaces;
using AI_Agent.Models;
using Legal_IA.Shared.Repositories.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Legal_IA.Functions.Activities
{
    public class LegalAiActivities(
        ILogger<LegalAiActivities> logger,
        ILegalAiAgent legalAiAgent,
        IUserRepository userRepository,
        IInvoiceRepository invoiceRepository)
    {
        [Function("BuildUserContextActivity")]
        public async Task<UserContext?> BuildUserContextActivity([ActivityTrigger] Guid userId)
        {
            try
            {
                logger.LogInformation("Building user context for userId: {UserId}", userId);

                var user = await userRepository.GetByIdAsync(userId);
                if (user == null) return null;

                var invoices = await invoiceRepository.GetInvoicesByUserIdAsync(userId);

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
                logger.LogError(ex, "Error building user context for userId: {UserId}", userId);
                return null;
            }
        }

        [Function("ProcessLegalQuestionActivity")]
        public async Task<LegalQueryResponse> ProcessLegalQuestionActivity([ActivityTrigger] ProcessLegalQuestionInput input)
        {
            try
            {
                logger.LogInformation("Processing legal question: {Question}", input.Request.Question);

                if (input.UserContext != null)
                {
                    return await legalAiAgent.ProcessQuestionAsync(input.Request, input.UserContext);
                }
                else
                {
                    return await legalAiAgent.ProcessQuestionAsync(input.Request);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing legal question");
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
        public async Task<AutonomoFormResponse> GetFormGuidanceActivity([ActivityTrigger] FormGuidanceInput input)
        {
            try
            {
                logger.LogInformation("Getting form guidance for: {FormType}", input.Request.FormType);

                if (input.UserContext != null)
                {
                    return await legalAiAgent.GetFormGuidanceAsync(input.Request, input.UserContext);
                }
                else
                {
                    return await legalAiAgent.GetFormGuidanceAsync(input.Request);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting form guidance");
                return new AutonomoFormResponse
                {
                    Success = false,
                    ErrorMessage = "Error processing form guidance request",
                    FormType = input.Request.FormType
                };
            }
        }

        [Function("GetQuarterlyObligationsActivity")]
        public async Task<string> GetQuarterlyObligationsActivity([ActivityTrigger] QuarterlyObligationsInput input)
        {
            try
            {
                logger.LogInformation("Getting quarterly obligations for Q{Quarter} {Year}", input.Quarter, input.Year);

                if (input.UserContext != null)
                {
                    return await legalAiAgent.GetQuarterlyObligationsAsync(input.Quarter, input.Year, input.UserContext);
                }
                else
                {
                    return await legalAiAgent.GetQuarterlyObligationsAsync(input.Quarter, input.Year);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting quarterly obligations");
                return "Error al obtener obligaciones trimestrales. Por favor, inténtelo de nuevo.";
            }
        }

        [Function("GetAnnualObligationsActivity")]
        public async Task<string> GetAnnualObligationsActivity([ActivityTrigger] AnnualObligationsInput input)
        {
            try
            {
                logger.LogInformation("Getting annual obligations for year {Year}", input.Year);

                if (input.UserContext != null)
                {
                    return await legalAiAgent.GetAnnualObligationsAsync(input.Year, input.UserContext);
                }
                else
                {
                    return await legalAiAgent.GetAnnualObligationsAsync(input.Year);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting annual obligations");
                return "Error al obtener obligaciones anuales. Por favor, inténtelo de nuevo.";
            }
        }

        [Function("ClassifyLegalQuestionActivity")]
        public async Task<bool> ClassifyLegalQuestionActivity([ActivityTrigger] string question)
        {
            try
            {
                logger.LogInformation("Classifying legal question: {Question}", question);
                return await legalAiAgent.IsLegalQuestionAsync(question);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error classifying legal question");
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
}