using System.ClientModel;
using AI_Agent.Interfaces;
using AI_Agent.Models;
using OpenAI;
using OpenAI.Chat;
using Legal_IA.Services;
using Legal_IA.Interfaces.Repositories;

namespace AI_Agent;

public class LegalAiAgent : ILegalAiAgent
{
    private readonly ChatClient _chatClient;
    private readonly OpenAIClient _openAiClient;
    private readonly string _systemPrompt;
    private readonly IInvoiceRepository? _invoiceRepository;

    public LegalAiAgent(string apiKey, IInvoiceRepository? invoiceRepository = null)
    {
        // Configure OpenAI client to use OpenRouter.ai
        var openRouterClient = new OpenAIClient(new ApiKeyCredential(apiKey), new OpenAIClientOptions
        {
            Endpoint = new Uri("https://openrouter.ai/api/v1")
        });
        
        _openAiClient = openRouterClient;
        _invoiceRepository = invoiceRepository;
        // Use a reliable free model - Google Gemma 2 9B has excellent Spanish capabilities and is confirmed available
        _chatClient = _openAiClient.GetChatClient("google/gemma-2-9b-it:free");
        _systemPrompt = BuildSystemPrompt();
    }

    /// <summary>
    /// Populates UserContext with real financial data from the database for the specified quarter and year
    /// </summary>
    public async Task<UserContext> PopulateUserContextWithFinancialDataAsync(UserContext userContext, int quarter, int year, CancellationToken cancellationToken = default)
    {
        if (_invoiceRepository == null)
        {
            // Return the original context if no repository is available
            return userContext;
        }

        try
        {
            // Get all invoices for the user
            var userInvoices = await _invoiceRepository.GetInvoicesByUserIdAsync(userContext.UserId);

            // Calculate quarterly financial data
            var quarterlyData = FinancialCalculationService.CalculateQuarterlyFinancials(userInvoices, quarter, year);
            
            // Calculate yearly financial data
            var yearlyData = FinancialCalculationService.CalculateYearlyFinancials(userInvoices, year);

            // Update UserContext with calculated financial data
            userContext.TotalIncomeCurrentQuarter = quarterlyData.TotalIncomeBase;
            userContext.TotalVATCurrentQuarter = quarterlyData.TotalVATCharged;
            userContext.TotalIRPFCurrentQuarter = quarterlyData.TotalIRPFRetained;
            userContext.InvoiceCountCurrentQuarter = quarterlyData.InvoiceCount;

            userContext.TotalIncomeCurrentYear = yearlyData.TotalIncomeBase;
            userContext.TotalVATCurrentYear = yearlyData.TotalVATCharged;
            userContext.TotalIRPFCurrentYear = yearlyData.TotalIRPFRetained;
            userContext.InvoiceCountCurrentYear = yearlyData.InvoiceCount;

            return userContext;
        }
        catch (Exception)
        {
            // Log the error but return the original context to prevent breaking the AI agent
            // In a production environment, you'd want proper logging here
            return userContext;
        }
    }

    /// <summary>
    /// Enhanced method to get comprehensive financial insights for form completion
    /// </summary>
    public async Task<string> GetFinancialInsightsForFormAsync(UserContext userContext, string formType, int quarter, int year, CancellationToken cancellationToken = default)
    {
        if (_invoiceRepository == null)
        {
            return "No se pudieron obtener datos financieros para completar el formulario.";
        }

        try
        {

            // Get all invoices for detailed analysis
            var userInvoices = await _invoiceRepository.GetInvoicesByUserIdAsync(userContext.UserId);
            var quarterlyData = FinancialCalculationService.CalculateQuarterlyFinancials(userInvoices, quarter, year);
            var yearlyData = FinancialCalculationService.CalculateYearlyFinancials(userInvoices, year);

            var financialSummary = $@"
RESUMEN FINANCIERO DETALLADO PARA {formType.ToUpper()}

DATOS DEL TRIMESTRE {quarter}/{year}:
- Ingresos base (sin IVA): {FinancialCalculationService.FormatCurrency(quarterlyData.TotalIncomeBase)}
- IVA facturado: {FinancialCalculationService.FormatCurrency(quarterlyData.TotalVATCharged)}
- IRPF retenido: {FinancialCalculationService.FormatCurrency(quarterlyData.TotalIRPFRetained)}
- Total con IVA: {FinancialCalculationService.FormatCurrency(quarterlyData.TotalIncomeWithVAT)}
- Número de facturas: {quarterlyData.InvoiceCount}

DATOS ANUALES {year}:
- Ingresos base acumulados: {FinancialCalculationService.FormatCurrency(yearlyData.TotalIncomeBase)}
- IVA facturado acumulado: {FinancialCalculationService.FormatCurrency(yearlyData.TotalVATCharged)}
- IRPF retenido acumulado: {FinancialCalculationService.FormatCurrency(yearlyData.TotalIRPFRetained)}
- Total facturas emitidas: {yearlyData.InvoiceCount}

OBLIGACIONES ESPECÍFICAS:
- Fecha límite {formType}: {FinancialCalculationService.GetQuarterDeadline(quarter, year)}
- ¿Debe presentar Modelo 347?: {(FinancialCalculationService.ShouldPresentModel347(yearlyData.TotalIncomeBase) ? "SÍ" : "NO")}

DESGLOSE POR TRIMESTRES:";

            foreach (var q in yearlyData.QuarterlyBreakdown)
            {
                financialSummary += $@"
T{q.QuarterNumber}: {FinancialCalculationService.FormatCurrency(q.TotalIncomeBase)} (base) + {FinancialCalculationService.FormatCurrency(q.TotalVATCharged)} (IVA) = {FinancialCalculationService.FormatCurrency(q.TotalIncomeWithVAT)}";
            }

            return financialSummary;
        }
        catch (Exception ex)
        {
            return $"Error al obtener insights financieros: {ex.Message}";
        }
    }

    public async Task<string> GetLegalAdviceAsync(string userQuestion, CancellationToken cancellationToken = default)
    {
        try
        {
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(_systemPrompt),
                new UserChatMessage(userQuestion)
            };

            var options = new ChatCompletionOptions
            {
                MaxOutputTokenCount = 1000,
                Temperature = 0.3f
            };

            var response = await _chatClient.CompleteChatAsync(messages, options, cancellationToken);

            return response.Value.Content[0].Text;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error al procesar la consulta legal: {ex.Message}", ex);
        }
    }

    public async Task<bool> IsLegalQuestionAsync(string question, CancellationToken cancellationToken = default)
    {
        try
        {
            var classificationPrompt = @"
Clasifica si esta pregunta está relacionada con temas legales, fiscales o judiciales en España.

Responde ÚNICAMENTE con una palabra: 'SÍ' o 'NO'

Temas legales incluyen:
- IVA, IRPF, impuestos, declaraciones fiscales
- Modelos de Hacienda (303, 130, 131, etc.)
- Autónomos, obligaciones fiscales
- Derecho civil, penal, laboral, mercantil
- Contratos, obligaciones legales
- Seguridad Social

Pregunta: " + question;

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(classificationPrompt)
            };

            var chatClient = _openAiClient.GetChatClient("google/gemma-2-9b-it:free");
            var options = new ChatCompletionOptions
            {
                MaxOutputTokenCount = 5, // Reduced to force a simple response
                Temperature = 0.0f // Zero temperature for consistent classification
            };

            var response = await chatClient.CompleteChatAsync(messages, options, cancellationToken);
            var answer = response.Value.Content[0].Text.Trim().ToUpper();

            // More robust classification logic
            bool isLegal = answer.Contains("SÍ") || answer.Contains("SI") || answer.StartsWith("S");
            
            // Additional fallback - check for common Spanish tax/legal keywords
            if (!isLegal)
            {
                var legalKeywords = new[] { "iva", "irpf", "impuesto", "fiscal", "hacienda", "modelo", "autonomo", "autónomo", 
                                          "declaracion", "declaración", "derecho", "legal", "contrato", "obligacion", "obligación" };
                var questionLower = question.ToLower();
                isLegal = legalKeywords.Any(keyword => questionLower.Contains(keyword));
            }
            
            return isLegal;
        }
        catch
        {
            // En caso de error, asumimos que es una pregunta legal para no rechazar incorrectamente
            return true;
        }
    }

    public async Task<LegalQueryResponse> ProcessQuestionAsync(LegalQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = new LegalQueryResponse
        {
            SessionId = request.SessionId,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            if (string.IsNullOrWhiteSpace(request.Question))
            {
                response.Answer = "Por favor, formule su consulta legal.";
                response.Success = true;
                response.IsLegalQuestion = false;
                return response;
            }

            response.IsLegalQuestion = await IsLegalQuestionAsync(request.Question, cancellationToken);

            if (!response.IsLegalQuestion)
            {
                response.Answer =
                    "Lo siento, solo puedo responder preguntas relacionadas con temas legales, fiscales y judiciales en España. " +
                    "Por favor, formule una consulta relacionada con mi área de especialización.";
                response.Success = true;
                return response;
            }

            response.Answer = await GetLegalAdviceAsync(request.Question, cancellationToken);
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.ErrorMessage = $"Error al procesar la consulta: {ex.Message}";
            response.Answer = "Lo siento, ha ocurrido un error al procesar su consulta. Por favor, inténtelo de nuevo.";
        }

        return response;
    }

    public async Task<AutonomoFormResponse> GetFormGuidanceAsync(AutonomoFormRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = new AutonomoFormResponse
        {
            FormType = request.FormType,
            Success = false
        };

        try
        {
            var formPrompt = $@"
Eres un experto en fiscalidad para autónomos en España. Debes proporcionar una guía para el modelo {request.FormType}.

Contexto:
- Trimestre: {request.Quarter}
- Año: {request.Year}
- Régimen: {request.RegimeType}
- Código de actividad: {request.ActivityCode}

INSTRUCCIÓN CRÍTICA: Responde ÚNICAMENTE usando este formato EXACTO. NO añadas markdown, asteriscos, ni formato adicional:

NOMBRE DEL FORMULARIO:
Modelo {request.FormType}

INSTRUCCIONES GENERALES:
El Modelo {request.FormType} [Explica brevemente qué es y para qué sirve]

DATOS NECESARIOS:
1. [Primer dato necesario]
2. [Segundo dato necesario]
3. [Tercer dato necesario]
4. [Cuarto dato necesario]
5. [Quinto dato necesario]

DOCUMENTOS REQUERIDOS:
1. [Primer documento requerido]
2. [Segundo documento requerido]
3. [Tercer documento requerido]
4. [Cuarto documento requerido]

FECHA LÍMITE:
[Fecha específica de presentación para {request.Quarter} de {request.Year}]

PLAZO DE PAGO:
[Cuándo debe realizar el pago - misma fecha u otra fecha específica]

CONSECUENCIAS DE RETRASO:
1. [Primera consecuencia - recargo específico]
2. [Segunda consecuencia - interés de demora]
3. [Tercera consecuencia - sanciones adicionales]

ORIENTACIÓN PARA CÁLCULOS:
Paso 1: [Primer paso para calcular]
Paso 2: [Segundo paso para calcular]
Paso 3: [Tercer paso para calcular]
Paso 4: [Cuarto paso para calcular]
[Continúa con orientación específica sobre cómo completar cada casilla del formulario]

Proporciona información específica y práctica para autónomos en régimen {request.RegimeType} con actividad {request.ActivityCode}.";

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(_systemPrompt),
                new UserChatMessage(formPrompt)
            };

            var options = new ChatCompletionOptions
            {
                MaxOutputTokenCount = 1500,
                Temperature = 0.2f
            };

            var aiResponse = await _chatClient.CompleteChatAsync(messages, options, cancellationToken);
            var content = aiResponse.Value.Content[0].Text;

            // Parse the structured response
            ParseFormGuidanceResponse(content, response);
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.ErrorMessage = $"Error al obtener guía del formulario: {ex.Message}";
        }

        return response;
    }

    public async Task<string> GetQuarterlyObligationsAsync(int quarter, int year, UserContext userContext,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // First, populate UserContext with real financial data from database
            var enrichedUserContext = await PopulateUserContextWithFinancialDataAsync(userContext, quarter, year, cancellationToken);
            
            var personalizedSystemPrompt = BuildSystemPrompt(enrichedUserContext);

            var quarterPrompt = $@"
Como experto fiscal para autónomos en España, proporciona un resumen PERSONALIZADO para {enrichedUserContext.FirstName} {enrichedUserContext.LastName} de sus obligaciones fiscales y de Seguridad Social para el {quarter}º trimestre de {year}.

Datos específicos del usuario para personalizar la respuesta:
- Ingresos del trimestre: {enrichedUserContext.TotalIncomeCurrentQuarter:C}
- IVA facturado: {enrichedUserContext.TotalVATCurrentQuarter:C}
- IRPF retenido: {enrichedUserContext.TotalIRPFCurrentQuarter:C}
- Facturas emitidas: {enrichedUserContext.InvoiceCountCurrentQuarter}
- Régimen fiscal: {enrichedUserContext.TaxRegime ?? "No especificado"}
- Ubicación: {enrichedUserContext.City}, {enrichedUserContext.Province}

PERSONALIZA la respuesta indicando:
1. Modelos específicos que debe presentar según sus ingresos
2. Importes aproximados a declarar/pagar basados en sus datos reales
3. Si aplican umbrales específicos según su facturación
4. Obligaciones específicas de su provincia/comunidad autónoma
5. Fechas límite específicas con recordatorios personalizados

Estructura la información cronológicamente y destaca las tareas más urgentes.";

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(personalizedSystemPrompt),
                new UserChatMessage(quarterPrompt)
            };

            var options = new ChatCompletionOptions
            {
                MaxOutputTokenCount = 1500,
                Temperature = 0.2f
            };

            var response = await _chatClient.CompleteChatAsync(messages, options, cancellationToken);
            return response.Value.Content[0].Text;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Error al obtener obligaciones trimestrales personalizadas: {ex.Message}", ex);
        }
    }

    public async Task<string> GetAnnualObligationsAsync(int year, UserContext userContext,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // First, populate UserContext with real financial data from database
            var enrichedUserContext = await PopulateUserContextWithFinancialDataAsync(userContext, 4, year, cancellationToken); // Use Q4 for annual calculations
            
            var personalizedSystemPrompt = BuildSystemPrompt(enrichedUserContext);

            var annualPrompt = $@"
Como experto fiscal para autónomos en España, proporciona un calendario PERSONALIZADO para {enrichedUserContext.FirstName} {enrichedUserContext.LastName} de obligaciones anuales para {year}.

Datos específicos del usuario:
- Ingresos anuales actuales: {enrichedUserContext.TotalIncomeCurrentYear:C}
- IVA facturado anual: {enrichedUserContext.TotalVATCurrentYear:C}
- IRPF retenido anual: {enrichedUserContext.TotalIRPFCurrentYear:C}
- Total facturas emitidas: {enrichedUserContext.InvoiceCountCurrentYear}
- Régimen fiscal: {enrichedUserContext.TaxRegime ?? "No especificado"}
- Actividad: {enrichedUserContext.ActivityCode ?? "No especificada"}

PERSONALIZA la respuesta con:
1. Obligaciones específicas según su volumen de facturación
2. Estimaciones de importes a pagar/devolver basadas en sus datos
3. Si debe presentar modelo 347 (operaciones > 3.005,06€)
4. Deducciones aplicables según su actividad específica
5. Proyecciones para planificación fiscal del próximo año

Organiza cronológicamente y destaca las fechas críticas para su situación específica.";

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(personalizedSystemPrompt),
                new UserChatMessage(annualPrompt)
            };

            var options = new ChatCompletionOptions
            {
                MaxOutputTokenCount = 2000,
                Temperature = 0.2f
            };

            var response = await _chatClient.CompleteChatAsync(messages, options, cancellationToken);
            return response.Value.Content[0].Text;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error al obtener obligaciones anuales personalizadas: {ex.Message}",
                ex);
        }
    }

    // New personalized methods with UserContext
    public async Task<LegalQueryResponse> ProcessQuestionAsync(LegalQueryRequest request, UserContext userContext,
        CancellationToken cancellationToken = default)
    {
        var response = new LegalQueryResponse
        {
            SessionId = request.SessionId,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            if (string.IsNullOrWhiteSpace(request.Question))
            {
                response.Answer = $"Hola {userContext.FirstName}, por favor formule su consulta legal.";
                response.Success = true;
                response.IsLegalQuestion = false;
                return response;
            }

            response.IsLegalQuestion = await IsLegalQuestionAsync(request.Question, cancellationToken);

            if (!response.IsLegalQuestion)
            {
                response.Answer =
                    $"Hola {userContext.FirstName}, solo puedo responder preguntas relacionadas con temas legales, fiscales y judiciales en España. " +
                    "Por favor, formule una consulta relacionada con mi área de especialización.";
                response.Success = true;
                return response;
            }

            response.Answer = await GetLegalAdviceAsync(request.Question, userContext, cancellationToken);
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.ErrorMessage = $"Error al procesar la consulta: {ex.Message}";
            response.Answer =
                $"Lo siento {userContext.FirstName}, ha ocurrido un error al procesar su consulta. Por favor, inténtelo de nuevo.";
        }

        return response;
    }

    public async Task<string> GetLegalAdviceAsync(string userQuestion, UserContext userContext,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var personalizedSystemPrompt = BuildSystemPrompt(userContext);

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(personalizedSystemPrompt),
                new UserChatMessage(userQuestion)
            };

            var options = new ChatCompletionOptions
            {
                MaxOutputTokenCount = 1200, // Increased for personalized responses
                Temperature = 0.3f
            };

            var response = await _chatClient.CompleteChatAsync(messages, options, cancellationToken);

            return response.Value.Content[0].Text;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error al procesar la consulta legal personalizada: {ex.Message}", ex);
        }
    }

    public async Task<AutonomoFormResponse> GetFormGuidanceAsync(AutonomoFormRequest request, UserContext userContext,
        CancellationToken cancellationToken = default)
    {
        var response = new AutonomoFormResponse
        {
            FormType = request.FormType,
            Success = false
        };

        try
        {
            var formPrompt = $@"
Eres un experto en fiscalidad para autónomos en España. Debes proporcionar una guía para el modelo {request.FormType}.

Contexto:
- Trimestre: {request.Quarter}
- Año: {request.Year}
- Régimen: {request.RegimeType}
- Código de actividad: {request.ActivityCode}

INSTRUCCIÓN CRÍTICA: Responde ÚNICAMENTE usando este formato EXACTO. NO añadas markdown, asteriscos, ni formato adicional:

NOMBRE DEL FORMULARIO:
Modelo {request.FormType}

INSTRUCCIONES GENERALES:
El Modelo {request.FormType} [Explica brevemente qué es y para qué sirve]

DATOS NECESARIOS:
1. [Primer dato necesario]
2. [Segundo dato necesario]
3. [Tercer dato necesario]
4. [Cuarto dato necesario]
5. [Quinto dato necesario]

DOCUMENTOS REQUERIDOS:
1. [Primer documento requerido]
2. [Segundo documento requerido]
3. [Tercer documento requerido]
4. [Cuarto documento requerido]

FECHA LÍMITE:
[Fecha específica de presentación para {request.Quarter} de {request.Year}]

PLAZO DE PAGO:
[Cuándo debe realizar el pago - misma fecha u otra fecha específica]

CONSECUENCIAS DE RETRASO:
1. [Primera consecuencia - recargo específico]
2. [Segunda consecuencia - interés de demora]
3. [Tercera consecuencia - sanciones adicionales]

ORIENTACIÓN PARA CÁLCULOS:
Paso 1: [Primer paso para calcular]
Paso 2: [Segundo paso para calcular]
Paso 3: [Tercer paso para calcular]
Paso 4: [Cuarto paso para calcular]
[Continúa con orientación específica sobre cómo completar cada casilla del formulario]

Proporciona información específica y práctica para autónomos en régimen {request.RegimeType} con actividad {request.ActivityCode}.";

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(_systemPrompt),
                new UserChatMessage(formPrompt)
            };

            var options = new ChatCompletionOptions
            {
                MaxOutputTokenCount = 1500,
                Temperature = 0.2f
            };

            var aiResponse = await _chatClient.CompleteChatAsync(messages, options, cancellationToken);
            var content = aiResponse.Value.Content[0].Text;

            // Parse the structured response
            ParseFormGuidanceResponse(content, response);
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.ErrorMessage = $"Error al obtener guía del formulario: {ex.Message}";
        }

        return response;
    }

    // Interface required methods (without UserContext)
    public async Task<string> GetQuarterlyObligationsAsync(int quarter, int year, CancellationToken cancellationToken = default)
    {
        try
        {
            var quarterPrompt = $@"
Como experto fiscal para autónomos en España, proporciona un resumen completo de todas las obligaciones fiscales y de Seguridad Social para el {quarter}º trimestre de {year}.

Incluye:
1. Modelos a presentar (303, 130/131, etc.)
2. Fechas límite específicas
3. Obligaciones con la Seguridad Social
4. Recordatorios importantes
5. Consecuencias de incumplimiento

Estructura la información de manera clara y cronológica, empezando por las fechas más próximas.";

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(_systemPrompt),
                new UserChatMessage(quarterPrompt)
            };

            var options = new ChatCompletionOptions
            {
                MaxOutputTokenCount = 1200,
                Temperature = 0.2f
            };

            var response = await _chatClient.CompleteChatAsync(messages, options, cancellationToken);
            return response.Value.Content[0].Text;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error al obtener obligaciones trimestrales: {ex.Message}", ex);
        }
    }

    public async Task<string> GetAnnualObligationsAsync(int year, CancellationToken cancellationToken = default)
    {
        try
        {
            var annualPrompt = $@"
Como experto fiscal para autónomos en España, proporciona un calendario completo de obligaciones anuales para {year}.

Incluye:
1. Declaración anual de IRPF (Modelo 100)
2. Resumen anual de IVA (Modelo 390)
3. Declaración de operaciones con terceros (Modelo 347)
4. Resumen anual de retenciones (Modelo 190)
5. Otras obligaciones anuales específicas
6. Fechas límite y plazos de presentación
7. Documentación necesaria para cada declaración

Organiza la información cronológicamente y destaca las fechas más importantes.";

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(_systemPrompt),
                new UserChatMessage(annualPrompt)
            };

            var options = new ChatCompletionOptions
            {
                MaxOutputTokenCount = 1500,
                Temperature = 0.2f
            };

            var response = await _chatClient.CompleteChatAsync(messages, options, cancellationToken);
            return response.Value.Content[0].Text;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error al obtener obligaciones anuales: {ex.Message}", ex);
        }
    }

    public async Task<string> ProcessQuestionAsync(string userQuestion, CancellationToken cancellationToken = default)
    {
        var request = new LegalQueryRequest { Question = userQuestion };
        var response = await ProcessQuestionAsync(request, cancellationToken);
        return response.Answer;
    }

    private void ParseFormGuidanceResponse(string content, AutonomoFormResponse response)
    {
        try
        {
            // First, let's set the raw content as instructions as a fallback
            if (string.IsNullOrWhiteSpace(content))
            {
                response.Instructions = "No se pudo generar la guía para este formulario.";
                return;
            }

            var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var currentSection = "";
            var currentList = new List<string>();

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine)) continue;

                // Check for Spanish headers (case insensitive and more flexible)
                if (trimmedLine.ToUpper().Contains("NOMBRE DEL FORMULARIO") || 
                    trimmedLine.ToUpper().Contains("FORMULARIO") && trimmedLine.Contains(":"))
                {
                    response.FormName = ExtractValueAfterColon(trimmedLine);
                    currentSection = "name";
                }
                else if (trimmedLine.ToUpper().Contains("INSTRUCCIONES GENERALES") || 
                         trimmedLine.ToUpper().Contains("INSTRUCCIONES") && trimmedLine.Contains(":"))
                {
                    SaveCurrentSection(currentSection, currentList, response);
                    currentSection = "instructions";
                    currentList.Clear();
                }
                else if (trimmedLine.ToUpper().Contains("DATOS NECESARIOS") || 
                         trimmedLine.ToUpper().Contains("DATOS") && trimmedLine.Contains(":"))
                {
                    SaveCurrentSection(currentSection, currentList, response);
                    currentSection = "data";
                    currentList.Clear();
                }
                else if (trimmedLine.ToUpper().Contains("DOCUMENTOS REQUERIDOS") || 
                         trimmedLine.ToUpper().Contains("DOCUMENTOS") && trimmedLine.Contains(":"))
                {
                    SaveCurrentSection(currentSection, currentList, response);
                    currentSection = "documents";
                    currentList.Clear();
                }
                else if (trimmedLine.ToUpper().Contains("FECHA LÍMITE") || 
                         trimmedLine.ToUpper().Contains("FECHA") && trimmedLine.Contains(":"))
                {
                    SaveCurrentSection(currentSection, currentList, response);
                    response.DueDate = ExtractValueAfterColon(trimmedLine);
                    currentSection = "deadline";
                }
                else if (trimmedLine.ToUpper().Contains("PLAZO DE PAGO") || 
                         trimmedLine.ToUpper().Contains("PAGO") && trimmedLine.Contains(":"))
                {
                    response.PaymentDeadline = ExtractValueAfterColon(trimmedLine);
                    currentSection = "payment";
                }
                else if (trimmedLine.ToUpper().Contains("CONSECUENCIAS") && trimmedLine.Contains(":"))
                {
                    SaveCurrentSection(currentSection, currentList, response);
                    currentSection = "consequences";
                    currentList.Clear();
                }
                else if (trimmedLine.ToUpper().Contains("ORIENTACIÓN") && trimmedLine.Contains(":") ||
                         trimmedLine.ToUpper().Contains("CÁLCULOS") && trimmedLine.Contains(":"))
                {
                    SaveCurrentSection(currentSection, currentList, response);
                    currentSection = "calculations";
                    currentList.Clear();
                }
                else if (!trimmedLine.Contains(":") || trimmedLine.StartsWith("-") || trimmedLine.StartsWith("•") || 
                         char.IsDigit(trimmedLine[0]))
                {
                    // This is content, not a header
                    currentList.Add(trimmedLine);
                }
                else
                {
                    // Fallback: add to current list
                    currentList.Add(trimmedLine);
                }
            }

            // Save the last section
            SaveCurrentSection(currentSection, currentList, response);

            // If we couldn't parse anything properly, put all content in instructions
            if (string.IsNullOrEmpty(response.Instructions) && 
                string.IsNullOrEmpty(response.FormName) && 
                (response.RequiredData?.Count ?? 0) == 0)
            {
                response.Instructions = content;
                response.FormName = $"Modelo {response.FormType}";
            }
        }
        catch (Exception ex)
        {
            // If parsing fails completely, put all content in instructions
            response.Instructions = content;
            response.FormName = $"Modelo {response.FormType}";
            response.ErrorMessage = $"Error parsing response: {ex.Message}";
        }
    }

    private void SaveCurrentSection(string currentSection, List<string> currentList, AutonomoFormResponse response)
    {
        if (currentList.Count == 0) return;

        var content = string.Join(" ", currentList).Trim();
        
        switch (currentSection)
        {
            case "instructions":
                response.Instructions = content;
                break;
            case "data":
                response.RequiredData = new List<string>(currentList);
                break;
            case "documents":
                response.RequiredDocuments = new List<string>(currentList);
                break;
            case "consequences":
                response.Consequences = new List<string>(currentList);
                break;
            case "calculations":
                response.CalculationGuidance = content;
                break;
        }
    }

    private string ExtractValueAfterColon(string line)
    {
        var colonIndex = line.IndexOf(':');
        if (colonIndex >= 0 && colonIndex < line.Length - 1)
        {
            return line.Substring(colonIndex + 1).Trim();
        }
        return line.Trim();
    }

    private string BuildSystemPrompt(UserContext? userContext = null)
    {
        var basePrompt = @"
Eres un asistente legal especializado en el sistema jurídico español, con expertise especial en asesoramiento para trabajadores autónomos. Tu conocimiento incluye:

ÁREAS DE ESPECIALIZACIÓN:
- Derecho Civil: contratos, obligaciones, derechos reales, familia, sucesiones
- Derecho Penal: delitos, faltas, procedimiento penal
- Derecho Laboral: contratos de trabajo, despidos, seguridad social
- Derecho Mercantil: sociedades, contratos mercantiles, derecho concursal
- Derecho Administrativo: procedimiento administrativo, recursos
- Derecho Fiscal: IRPF, IS, IVA, tributos locales, procedimientos tributarios
- Derecho Constitucional: derechos fundamentales, organización del Estado
- Derecho Procesal: procedimientos civiles, penales y contencioso-administrativos
- Derecho Europeo aplicable en España

ESPECIALIZACIÓN EN AUTÓNOMOS - MODELOS Y FORMULARIOS:
- Modelo 303 (IVA trimestral): declaración y liquidación del IVA
- Modelo 130 (IRPF trimestral): pago fraccionado de IRPF para autónomos
- Modelo 131 (IRPF trimestral): estimación objetiva/módulos
- Modelo 036/037: alta en Hacienda, modificación de datos
- Modelo 390: resumen anual del IVA
- Modelo 100: declaración anual del IRPF
- Modelo 200: declaración del Impuesto sobre Sociedades (si aplica)
- Modelo 347: declaración de operaciones con terceros
- Modelo 115: retenciones de alquileres (si aplica)
- Modelo 190: resumen anual de retenciones e ingresos a cuenta
- Modelo 349: declaración recapitulativa de operaciones intracomunitarias
- TA.0521: alta en el RETA (Régimen Especial de Trabajadores Autónomos)
- Formularios de la Seguridad Social: variación de datos, cese, etc.";

        if (userContext != null)
            basePrompt += $@"

INFORMACIÓN ESPECÍFICA DEL USUARIO (CONFIDENCIAL - USAR SOLO PARA PERSONALIZACIÓN):
- Nombre: {userContext.FirstName} {userContext.LastName}
- DNI: {userContext.DNI}
- CIF: {userContext.CIF ?? "No registrado"}
- Nombre comercial: {userContext.BusinessName}
- Dirección: {userContext.Address}, {userContext.PostalCode} {userContext.City}, {userContext.Province}
- Código de actividad: {userContext.ActivityCode ?? "No especificado"}
- Régimen fiscal: {userContext.TaxRegime ?? "No especificado"}

DATOS FINANCIEROS ACTUALES DEL USUARIO:
- Ingresos año actual: {userContext.TotalIncomeCurrentYear:C}
- IVA facturado año actual: {userContext.TotalVATCurrentYear:C}
- IRPF retenido año actual: {userContext.TotalIRPFCurrentYear:C}
- Ingresos trimestre actual: {userContext.TotalIncomeCurrentQuarter:C}
- IVA facturado trimestre actual: {userContext.TotalVATCurrentQuarter:C}
- IRPF retenido trimestre actual: {userContext.TotalIRPFCurrentQuarter:C}
- Facturas emitidas año: {userContext.InvoiceCountCurrentYear}
- Facturas emitidas trimestre: {userContext.InvoiceCountCurrentQuarter}

INSTRUCCIONES PARA PERSONALIZACIÓN:
1. Utiliza los datos financieros reales para proporcionar orientación específica
2. Calcula aproximaciones de impuestos basadas en los ingresos actuales
3. Proporciona fechas límite específicas según el trimestre y año actuales
4. Adapta las recomendaciones al régimen fiscal del usuario
5. Incluye referencias a su ubicación geográfica cuando sea relevante
6. Menciona el volumen de facturación para contextualizar obligaciones";

        basePrompt += @"

GUÍA PARA COMPLETAR MODELOS:
1. Identifica el modelo específico que necesita el autónomo
2. Explica qué datos son necesarios para completarlo
3. Indica plazos de presentación y pago
4. Menciona consecuencias de presentación tardía
5. Proporciona orientación sobre cálculos específicos
6. Explica diferencias entre regímenes (general, módulos, recargo de equivalencia)
7. Indica qué documentación de soporte necesita conservar

INFORMACIÓN SOBRE OBLIGACIONES DE AUTÓNOMOS:
- Obligaciones fiscales periódicas (trimestrales y anuales)
- Obligaciones con la Seguridad Social
- Llevanza de libros registro (ingresos, gastos, IVA, bienes de inversión)
- Facturación y requisitos de las facturas
- Deducciones y gastos deducibles específicos para autónomos
- Regímenes especiales (RECC, módulos, etc.)

DIRECTRICES DE RESPUESTA:
1. Proporciona información jurídica y fiscal precisa basada en la legislación española vigente
2. Cita las normas legales relevantes (leyes, reglamentos, instrucciones de la AEAT)
3. Explica los conceptos de manera clara, adaptándote al nivel de conocimiento del usuario
4. Proporciona ejemplos prácticos cuando sea posible
5. Indica fechas límite específicas para cada obligación
6. Menciona las consecuencias de incumplimiento (sanciones, intereses)
7. Diferencia entre información general y casos que requieren asesoramiento personalizado
8. Mantén un tono profesional pero accesible

LIMITACIONES IMPORTANTES:
- Proporciona asesoramiento fiscal personalizado específico
- Completa formularios con datos reales del usuario
- No sustituyes la consulta con un asesor fiscal o abogado
- Siempre recomienda verificar información con profesionales para casos complejos
- Procesas datos personales y fiscales sensibles
- Realiza cálculos de liquidaciones definitivos con datos reales si los hay

IMPORTANTE: Solo responde preguntas relacionadas con temas legales, fiscales y judiciales, especialmente aquellas relacionadas con las obligaciones de trabajadores autónomos en España. Si la consulta no está relacionada con estas materias, indica que no puedes ayudar con ese tipo de preguntas.

Responde siempre en español y estructura la información de manera clara y ordenada.";

        return basePrompt;
    }
}
