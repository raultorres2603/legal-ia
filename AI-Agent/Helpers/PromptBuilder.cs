using AI_Agent.Models;
using Legal_IA.Shared.Models;

namespace AI_Agent.Helpers
{
    public static class PromptBuilder
    {
        /// <summary>
        /// Builds a generic system prompt for the AI agent, instructing it to answer only legal, tax, or judicial questions for freelancers in Spain.
        /// </summary>
        /// <param name="userFullContext"></param>
        public static string BuildSystemPrompt()
        {
            return @"Eres un asistente experto en derecho y fiscalidad para autónomos en España. Responde de manera clara, precisa y profesional. Si la pregunta no es legal o fiscal, indica que solo puedes responder sobre temas legales, fiscales o judiciales en España. Responde de manera clara, precisa y profesional, como lo haría un asesor fiscal humano. No incluyas advertencias, disclaimers ni referencias a ser un modelo de lenguaje. Limítate a proporcionar la información solicitada.";
        }

        /// <summary>
        /// Builds a detailed system prompt including user profile, invoice summary, recent invoices, and totals for highly personalized AI responses.
        /// </summary>
        /// <param name="userFullContext">The full user context including invoices and summary data.</param>
        /// <returns>A structured prompt string for the AI agent.</returns>
        public static string BuildSystemPrompt(UserFullContext userFullContext)
        {
            var user = userFullContext.UserContext;
            var invoices = userFullContext.Invoices;
            var sb = new System.Text.StringBuilder();
            
            // Section: Instructions
            sb.AppendLine("--- INSTRUCCIONES IMPORTANTES Y ESTRICTAMENTE NECESARIAS ---");
            sb.AppendLine("Eres un asistente experto en derecho y fiscalidad para autónomos en España. Responde de manera clara, precisa y profesional.\nSi la pregunta no es legal o fiscal, indica que solo puedes responder sobre temas legales, fiscales o judiciales en España.");
            sb.AppendLine("Responde de manera clara, precisa y profesional, como lo haría un asesor fiscal humano. No incluyas advertencias, disclaimers ni referencias a ser un modelo de lenguaje. Limítate a proporcionar la información solicitada.");

            // Section: User Info
            sb.AppendLine("--- INFORMACIÓN DEL USUARIO ---");
            sb.AppendLine($"Nombre: {user.FirstName} {user.LastName}");
            sb.AppendLine($"CIF: {user.CIF ?? "N/A"}");
            sb.AppendLine($"Actividad: {user.ActivityCode ?? "N/A"}");
            sb.AppendLine($"Régimen fiscal: {user.TaxRegime ?? "N/A"}");
            sb.AppendLine($"Dirección: {user.Address}, {user.PostalCode}, {user.City}, {user.Province}");
            sb.AppendLine($"Teléfono: {user.Phone}");
            sb.AppendLine();

            // Section: Invoice Summary
            sb.AppendLine("--- RESUMEN DE FACTURAS ---");
            sb.AppendLine($"Total facturas este año: {invoices.Count}");
            sb.AppendLine($"Total facturas este trimestre: {invoices.Count(i => i.IssueDate.Year == DateTime.Now.Year && ((i.IssueDate.Month - 1) / 3 + 1) == ((DateTime.Now.Month - 1) / 3 + 1))}");
            sb.AppendLine($"Ingresos año actual: {user.TotalIncomeCurrentYear}, IVA año actual: {user.TotalVATCurrentYear}, IRPF año actual: {user.TotalIRPFCurrentYear}");
            sb.AppendLine();

            // Section: Invoice Details (up to 10 most recent)
            sb.AppendLine("--- FACTURAS RECIENTES ---");
            if (invoices.Count > 0)
            {
                foreach (var i in invoices.OrderByDescending(i => i.IssueDate).Take(10))
                {
                    sb.AppendLine($"- Nº: {i.InvoiceNumber}, Fecha: {i.IssueDate:yyyy-MM-dd}, Cliente: {i.ClientName}, Total: {i.Total:C}, IVA: {i.VAT:C}, IRPF: {i.IRPF:C}");
                }
            }
            else
            {
                sb.AppendLine("No hay facturas registradas.");
            }
            sb.AppendLine();

            // Section: Totals
            sb.AppendLine("--- TOTALES ---");
            var totalAmount = invoices.Sum(i => i.Total);
            var totalVat = invoices.Sum(i => i.VAT);
            var totalIrpf = invoices.Sum(i => i.IRPF);
            sb.AppendLine($"Importe bruto: {totalAmount:C}, IVA: {totalVat:C}, IRPF: {totalIrpf:C}");
            sb.AppendLine();

            return sb.ToString();
        }

        /// <summary>
        /// Builds a prompt for the AI to provide guidance on a specific Spanish tax form for freelancers.
        /// </summary>
        /// <param name="request">The form request details.</param>
        /// <returns>A structured prompt string for the AI agent.</returns>
        public static string BuildFormPrompt(AutonomoFormRequest request)
        {
            return $@"
--- INSTRUCCIONES IMPORTANTES Y ESTRICTAMENTE NECESARIAS ---
Como experto en fiscalidad para autónomos en España, proporciona una guía completa para el modelo {request.FormType}.
Responde de manera clara, precisa y profesional, como lo haría un asesor fiscal humano. No incluyas advertencias, disclaimers ni referencias a ser un modelo de lenguaje. Limítate a proporcionar la información solicitada.

Información del contexto:
- Trimestre: {request.Quarter}
- Año: {request.Year}
- Régimen: {request.RegimeType}
- Código de actividad: {request.ActivityCode}

Proporciona la información en el siguiente formato estructurado:

NOMBRE DEL FORMULARIO:
[Nombre completo del modelo]

INSTRUCCIONES GENERALES:
[Explicación clara de qué es este modelo y para qué sirve]

DATOS NECESARIOS:
[Lista numerada de todos los datos que necesita para completar el formulario]

DOCUMENTOS REQUERIDOS:
[Lista de documentos que debe tener preparados]

FECHA LÍMITE:
[Fecha específica de presentación para el período indicado]

PLAZO DE PAGO:
[Cuándo debe realizar el pago si corresponde]

CONSECUENCIAS DE RETRASO:
[Sanciones e intereses por presentación o pago tardío]

ORIENTACIÓN PARA CÁLCULOS:
[Guía paso a paso para realizar los cálculos necesarios]

Responde de manera profesional y práctica, considerando el régimen fiscal específico mencionado.";
        }

        /// <summary>
        /// Builds a personalized form guidance prompt using basic user context.
        /// </summary>
        /// <param name="request">The form request details.</param>
        /// <param name="userContext">The user's context information.</param>
        /// <returns>A structured prompt string for the AI agent.</returns>
        public static string BuildPersonalizedFormPrompt(AutonomoFormRequest request, UserContext userContext)
        {
            return $@"
--- INSTRUCCIONES IMPORTANTES Y ESTRICTAMENTE NECESARIAS ---
Como experto en fiscalidad para autónomos en España, proporciona una guía PERSONALIZADA para {userContext.FirstName} {userContext.LastName} sobre el modelo {request.FormType}.
Responde de manera clara, precisa y profesional, como lo haría un asesor fiscal humano. No incluyas advertencias, disclaimers ni referencias a ser un modelo de lenguaje. Limítate a proporcionar la información solicitada.

Información específica del usuario:
- Trimestre: {request.Quarter}
- Año: {request.Year}
";
        }

        /// <summary>
        /// Builds a personalized form guidance prompt using full user context, including invoices and summary data.
        /// </summary>
        /// <param name="request">The form request details.</param>
        /// <param name="userFullContext">The full user context including invoices and summary data.</param>
        /// <returns>A structured prompt string for the AI agent.</returns>
        public static string BuildPersonalizedFormPrompt(AutonomoFormRequest request, UserFullContext userFullContext)
        {
            var user = userFullContext.UserContext;
            var invoices = userFullContext.Invoices;
            string invoiceSummary = $"Total facturas este año: {invoices.Count}, Total facturas este trimestre: {invoices.Count(i => i.IssueDate.Year == DateTime.Now.Year && ((i.IssueDate.Month - 1) / 3 + 1) == ((DateTime.Now.Month - 1) / 3 + 1))}";
            string incomeSummary = $"Ingresos año actual: {user.TotalIncomeCurrentYear}, IVA año actual: {user.TotalVATCurrentYear}, IRPF año actual: {user.TotalIRPFCurrentYear}";

            return $@"
--- INSTRUCCIONES IMPORTANTES Y ESTRICTAMENTE NECESARIAS ---
Como experto en fiscalidad para autónomos en España, proporciona una guía PERSONALIZADA para {user.FirstName} {user.LastName} sobre el modelo {request.FormType}.
Responde de manera clara, precisa y profesional, como lo haría un asesor fiscal humano. No incluyas advertencias, disclaimers ni referencias a ser un modelo de lenguaje. Limítate a proporcionar la información solicitada.

Información específica del usuario:
- Trimestre: {request.Quarter}
- Año: {request.Year}
- Régimen: {user.TaxRegime ?? "N/A"}
- Código de actividad: {user.ActivityCode ?? "N/A"}
- CIF: {user.CIF ?? "N/A"}
- Dirección: {user.Address}, {user.PostalCode}, {user.City}, {user.Province}
- Teléfono: {user.Phone}
- {invoiceSummary}
- {incomeSummary}

Incluye detalles relevantes de sus facturas e ingresos si es útil para la explicación o guía.";
        }

        /// <summary>
        /// Builds a prompt for the AI to provide a summary of quarterly obligations for freelancers in Spain.
        /// </summary>
        /// <param name="quarter">The quarter (1-4).</param>
        /// <param name="year">The year.</param>
        /// <returns>A structured prompt string for the AI agent.</returns>
        public static string BuildQuarterlyObligationsPrompt(int quarter, int year)
        {
            return $@"
--- INSTRUCCIONES IMPORTANTES Y ESTRICTAMENTE NECESARIAS ---
Como experto fiscal para autónomos en España, proporciona un resumen completo de todas las obligaciones fiscales y de Seguridad Social para el {quarter}º trimestre de {year}.
Responde de manera clara, precisa y profesional, como lo haría un asesor fiscal humano. No incluyas advertencias, disclaimers ni referencias a ser un modelo de lenguaje. Limítate a proporcionar la información solicitada.

Incluye:
1. Modelos a presentar (303, 130/131, etc.)
2. Fechas límite específicas
3. Obligaciones con la Seguridad Social
4. Recordatorios importantes
5. Consecuencias de incumplimiento

Estructura la información de manera clara y cronológica, empezando por las fechas más próximas.";
        }

        /// <summary>
        /// Builds a prompt for the AI to provide a summary of annual obligations for freelancers in Spain.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <returns>A structured prompt string for the AI agent.</returns>
        public static string BuildAnnualObligationsPrompt(int year)
        {
            return $@"
--- INSTRUCCIONES IMPORTANTES Y ESTRICTAMENTE NECESARIAS ---
Como experto fiscal para autónomos en España, proporciona un resumen completo de todas las obligaciones fiscales anuales para el año {year}.
Responde de manera clara, precisa y profesional, como lo haría un asesor fiscal humano. No incluyas advertencias, disclaimers ni referencias a ser un modelo de lenguaje. Limítate a proporcionar la información solicitada.

Incluye:
1. Declaración de la Renta (Modelo 100)
2. Resumen anual de IVA (Modelo 390)
3. Otras declaraciones anuales obligatorias
4. Fechas límite específicas
5. Recordatorios importantes para el cierre del ejercicio
6. Consecuencias de incumplimiento

Estructura la información cronológicamente desde enero hasta diciembre.";
        }

        // New methods needed by LegalAiAgent.cs

        /// <summary>
        /// Builds a basic form guidance prompt for a specific form type.
        /// </summary>
        /// <param name="formType">The form type (e.g., modelo-303, modelo-130).</param>
        /// <returns>A structured prompt string for the AI agent.</returns>
        public static string BuildBasicFormPrompt(string formType)
        {
            return $@"
--- INSTRUCCIONES IMPORTANTES Y ESTRICTAMENTE NECESARIAS ---
Como experto en fiscalidad para autónomos en España, proporciona una guía completa para el {formType}.
Responde de manera clara, precisa y profesional, como lo haría un asesor fiscal humano. No incluyas advertencias, disclaimers ni referencias a ser un modelo de lenguaje. Limítate a proporcionar la información solicitada.

Proporciona información sobre:
1. Qué es este formulario y para qué sirve
2. Quién debe presentarlo
3. Cuándo se presenta
4. Datos necesarios para completarlo
5. Documentos requeridos
6. Cálculos principales
7. Fechas límite y plazos de pago
8. Consecuencias del incumplimiento

Responde de manera profesional y práctica.";
        }

        /// <summary>
        /// Builds a form guidance prompt with full user context including invoices.
        /// </summary>
        /// <param name="question">The user's specific question about the form.</param>
        /// <param name="formType">The form type (e.g., modelo-303, modelo-130).</param>
        /// <param name="userFullContext">The full user context including invoices and summary data.</param>
        /// <returns>A structured prompt string for the AI agent.</returns>
        public static string BuildFormPromptWithFullContext(string question, string formType, UserFullContext userFullContext)
        {
            var user = userFullContext.UserContext;
            var invoices = userFullContext.Invoices;
            
            var sb = new System.Text.StringBuilder();
            
            sb.AppendLine("--- INSTRUCCIONES IMPORTANTES Y ESTRICTAMENTE NECESARIAS ---");
            sb.AppendLine($"Como experto en fiscalidad para autónomos en España, responde la pregunta específica de {user.FirstName} {user.LastName} sobre el {formType}.");
            sb.AppendLine("Responde de manera clara, precisa y profesional, como lo haría un asesor fiscal humano. No incluyas advertencias, disclaimers ni referencias a ser un modelo de lenguaje. Limítate a proporcionar la información solicitada.");
            sb.AppendLine();
            
            sb.AppendLine("--- PREGUNTA DEL USUARIO ---");
            sb.AppendLine(question);
            sb.AppendLine();
            
            sb.AppendLine("--- INFORMACIÓN DEL USUARIO ---");
            sb.AppendLine($"Nombre: {user.FirstName} {user.LastName}");
            sb.AppendLine($"CIF: {user.CIF ?? "N/A"}");
            sb.AppendLine($"Actividad: {user.ActivityCode ?? "N/A"}");
            sb.AppendLine($"Régimen fiscal: {user.TaxRegime ?? "N/A"}");
            sb.AppendLine($"Ingresos año actual: {user.TotalIncomeCurrentYear:C}");
            sb.AppendLine($"IVA año actual: {user.TotalVATCurrentYear:C}");
            sb.AppendLine($"IRPF año actual: {user.TotalIRPFCurrentYear:C}");
            sb.AppendLine($"Total facturas este año: {invoices.Count}");
            sb.AppendLine();
            
            sb.AppendLine("Usa esta información para proporcionar una respuesta personalizada y específica para su situación.");
            
            return sb.ToString();
        }

        /// <summary>
        /// Builds a quarterly obligations prompt with full user context including invoices.
        /// </summary>
        /// <param name="question">The user's specific question about quarterly obligations.</param>
        /// <param name="quarter">The quarter (1-4).</param>
        /// <param name="year">The year.</param>
        /// <param name="userFullContext">The full user context including invoices and summary data.</param>
        /// <returns>A structured prompt string for the AI agent.</returns>
        public static string BuildQuarterlyObligationsPromptWithFullContext(string question, int quarter, int year, UserFullContext userFullContext)
        {
            var user = userFullContext.UserContext;
            var invoices = userFullContext.Invoices;
            
            var sb = new System.Text.StringBuilder();
            
            sb.AppendLine("--- INSTRUCCIONES IMPORTANTES Y ESTRICTAMENTE NECESARIAS ---");
            sb.AppendLine($"Como experto fiscal para autónomos en España, responde la pregunta específica de {user.FirstName} {user.LastName} sobre las obligaciones del {quarter}º trimestre de {year}.");
            sb.AppendLine("Responde de manera clara, precisa y profesional, como lo haría un asesor fiscal humano. No incluyas advertencias, disclaimers ni referencias a ser un modelo de lenguaje. Limítate a proporcionar la información solicitada.");
            sb.AppendLine();
            
            sb.AppendLine("--- PREGUNTA DEL USUARIO ---");
            sb.AppendLine(question);
            sb.AppendLine();
            
            sb.AppendLine("--- INFORMACIÓN DEL USUARIO ---");
            sb.AppendLine($"Nombre: {user.FirstName} {user.LastName}");
            sb.AppendLine($"CIF: {user.CIF ?? "N/A"}");
            sb.AppendLine($"Actividad: {user.ActivityCode ?? "N/A"}");
            sb.AppendLine($"Régimen fiscal: {user.TaxRegime ?? "N/A"}");
            
            // Calculate quarterly data
            var quarterlyInvoices = invoices.Where(i => i.IssueDate.Year == year && 
                ((i.IssueDate.Month - 1) / 3 + 1) == quarter).ToList();
            
            sb.AppendLine($"Ingresos trimestre actual: {quarterlyInvoices.Sum(i => i.Subtotal):C}");
            sb.AppendLine($"IVA trimestre actual: {quarterlyInvoices.Sum(i => i.VAT):C}");
            sb.AppendLine($"IRPF trimestre actual: {quarterlyInvoices.Sum(i => i.IRPF):C}");
            sb.AppendLine($"Facturas en el trimestre: {quarterlyInvoices.Count}");
            sb.AppendLine();
            
            sb.AppendLine("Usa esta información para proporcionar una respuesta personalizada considerando su situación fiscal específica.");
            
            return sb.ToString();
        }

        /// <summary>
        /// Builds an annual obligations prompt with full user context including invoices.
        /// </summary>
        /// <param name="question">The user's specific question about annual obligations.</param>
        /// <param name="year">The year.</param>
        /// <param name="userFullContext">The full user context including invoices and summary data.</param>
        /// <returns>A structured prompt string for the AI agent.</returns>
        public static string BuildAnnualObligationsPromptWithFullContext(string question, int year, UserFullContext userFullContext)
        {
            var user = userFullContext.UserContext;
            var invoices = userFullContext.Invoices;
            
            var sb = new System.Text.StringBuilder();
            
            sb.AppendLine("--- INSTRUCCIONES IMPORTANTES Y ESTRICTAMENTE NECESARIAS ---");
            sb.AppendLine($"Como experto fiscal para autónomos en España, responde la pregunta específica de {user.FirstName} {user.LastName} sobre las obligaciones anuales del año {year}.");
            sb.AppendLine("Responde de manera clara, precisa y profesional, como lo haría un asesor fiscal humano. No incluyas advertencias, disclaimers ni referencias a ser un modelo de lenguaje. Limítate a proporcionar la información solicitada.");
            sb.AppendLine();
            
            sb.AppendLine("--- PREGUNTA DEL USUARIO ---");
            sb.AppendLine(question);
            sb.AppendLine();
            
            sb.AppendLine("--- INFORMACIÓN DEL USUARIO ---");
            sb.AppendLine($"Nombre: {user.FirstName} {user.LastName}");
            sb.AppendLine($"CIF: {user.CIF ?? "N/A"}");
            sb.AppendLine($"Actividad: {user.ActivityCode ?? "N/A"}");
            sb.AppendLine($"Régimen fiscal: {user.TaxRegime ?? "N/A"}");
            
            // Calculate annual data
            var annualInvoices = invoices.Where(i => i.IssueDate.Year == year).ToList();
            
            sb.AppendLine($"Ingresos año {year}: {annualInvoices.Sum(i => i.Subtotal):C}");
            sb.AppendLine($"IVA año {year}: {annualInvoices.Sum(i => i.VAT):C}");
            sb.AppendLine($"IRPF año {year}: {annualInvoices.Sum(i => i.IRPF):C}");
            sb.AppendLine($"Total facturas año {year}: {annualInvoices.Count}");
            sb.AppendLine();
            
            sb.AppendLine("Usa esta información para proporcionar una respuesta personalizada considerando su situación fiscal específica del año completo.");
            
            return sb.ToString();
        }
    }
}
