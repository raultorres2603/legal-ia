using System.Text;
using AI_Agent.Models;
using Legal_IA.Shared.Models;

namespace AI_Agent.Helpers;

public static class PromptBuilder
{
    private const string ProfessionalIdentity = 
        "Actúas como un asesor fiscal certificado especializado en fiscalidad para autónomos en España. " +
        "Tienes más de 15 años de experiencia práctica asesorando a empresarios individuales y conoces " +
        "en profundidad toda la normativa fiscal española vigente.";

    private const string ResponseGuidelines = 
        "Proporciona respuestas precisas, profesionales y prácticas. Utiliza un tono experto pero accesible. " +
        "Estructura la información de manera clara y organizada. Evita generar disclaimers o advertencias innecesarias.";

    private const string ScopeLimitation = 
        "Tu expertise se limita exclusivamente a temas fiscales, legales y judiciales relacionados con autónomos en España. " +
        "Si la consulta no está dentro de este ámbito, redirige cortésmente hacia tu área de especialización.";

    /// <summary>
    /// Builds a basic system prompt for general legal/tax queries
    /// </summary>
    public static string BuildSystemPrompt()
    {
        return $@"{ProfessionalIdentity}

{ResponseGuidelines}

{ScopeLimitation}";
    }

    /// <summary>
    /// Builds a comprehensive system prompt with full user context including invoices and financial data
    /// </summary>
    public static string BuildSystemPrompt(UserFullContext userFullContext)
    {
        var user = userFullContext.UserContext;
        var invoices = userFullContext.Invoices;
        var sb = new StringBuilder();

        // Professional identity and guidelines
        sb.AppendLine($"{ProfessionalIdentity}");
        sb.AppendLine();
        sb.AppendLine($"{ResponseGuidelines}");
        sb.AppendLine();

        // Client profile section
        sb.AppendLine("=== PERFIL DEL CLIENTE ===");
        sb.AppendLine($"Cliente: {user.FirstName} {user.LastName}");
        sb.AppendLine($"CIF: {user.CIF ?? "Pendiente de asignación"}");
        sb.AppendLine($"Actividad económica: {user.ActivityCode ?? "No especificada"}");
        sb.AppendLine($"Régimen fiscal: {user.TaxRegime ?? "Régimen general"}");
        sb.AppendLine($"Domicilio fiscal: {user.Address}, {user.PostalCode} {user.City} ({user.Province})");
        sb.AppendLine($"Contacto: {user.Phone}");
        sb.AppendLine();

        // Financial summary section
        var currentQuarter = (DateTime.Now.Month - 1) / 3 + 1;
        var quarterlyInvoices = invoices.Count(i => 
            i.IssueDate.Year == DateTime.Now.Year && 
            (i.IssueDate.Month - 1) / 3 + 1 == currentQuarter);

        sb.AppendLine("=== SITUACIÓN FISCAL ACTUAL ===");
        sb.AppendLine($"Ejercicio {DateTime.Now.Year}:");
        sb.AppendLine($"• Ingresos acumulados: {user.TotalIncomeCurrentYear:C}");
        sb.AppendLine($"• IVA repercutido: {user.TotalVATCurrentYear:C}");
        sb.AppendLine($"• IRPF retenido: {user.TotalIRPFCurrentYear:C}");
        sb.AppendLine($"• Facturas emitidas: {invoices.Count} (trimestre actual: {quarterlyInvoices})");
        sb.AppendLine();

        // Recent activity section
        if (invoices.Any())
        {
            sb.AppendLine("=== ACTIVIDAD RECIENTE ===");
            var recentInvoices = invoices.OrderByDescending(i => i.IssueDate).Take(5);
            foreach (var invoice in recentInvoices)
            {
                sb.AppendLine($"• {invoice.IssueDate:dd/MM/yyyy} - {invoice.ClientName}: {invoice.Total:C} " +
                             $"(IVA: {invoice.VAT:C}, IRPF: {invoice.IRPF:C})");
            }
            sb.AppendLine();
        }

        sb.AppendLine("Utiliza esta información para proporcionar asesoramiento personalizado y específico.");
        
        return sb.ToString();
    }

    /// <summary>
    /// Builds a structured prompt for tax form guidance
    /// </summary>
    public static string BuildFormPrompt(AutonomoFormRequest request)
    {
        return $@"{ProfessionalIdentity}

=== CONSULTA SOBRE FORMULARIO FISCAL ===
Formulario: {request.FormType}
Período: {request.Quarter}º trimestre {request.Year}
Régimen fiscal: {request.RegimeType ?? "Régimen general"}
Código de actividad: {request.ActivityCode ?? "No especificado"}

{ResponseGuidelines}

Estructura tu respuesta siguiendo este formato profesional:

NOMBRE DEL FORMULARIO:
[Denominación oficial completa]

INSTRUCCIONES GENERALES:
[Propósito y aplicación del formulario]

DATOS NECESARIOS:
[Información requerida para cumplimentar correctamente]

DOCUMENTOS REQUERIDOS:
[Documentación de soporte necesaria]

FECHA LÍMITE:
[Plazo de presentación específico]

PLAZO DE PAGO:
[Vencimiento para el ingreso si procede]

CONSECUENCIAS DE RETRASO:
[Sanciones e intereses aplicables]

ORIENTACIÓN PARA CÁLCULOS:
[Metodología de cálculo paso a paso]

Adapta las recomendaciones al régimen fiscal especificado.";
    }

    /// <summary>
    /// Builds a prompt for quarterly fiscal obligations summary
    /// </summary>
    public static string BuildQuarterlyObligationsPrompt(int quarter, int year)
    {
        return $@"{ProfessionalIdentity}

=== OBLIGACIONES FISCALES TRIMESTRALES ===
Período: {quarter}º trimestre {year}

{ResponseGuidelines}

Proporciona un calendario completo que incluya:

1. OBLIGACIONES FISCALES
   • Modelos a presentar (303, 130/131, etc.)
   • Fechas límite específicas
   • Cálculos principales

2. SEGURIDAD SOCIAL
   • Cuotas de autónomos
   • Plazos de pago

3. CALENDARIO DE VENCIMIENTOS
   • Orden cronológico de obligaciones
   • Recordatorios importantes

4. GESTIÓN DE RIESGOS
   • Consecuencias del incumplimiento
   • Recomendaciones preventivas

Estructura la información por orden de prioridad y urgencia.";
    }

    /// <summary>
    /// Builds a prompt for annual fiscal obligations summary
    /// </summary>
    public static string BuildAnnualObligationsPrompt(int year)
    {
        return $@"{ProfessionalIdentity}

=== OBLIGACIONES FISCALES ANUALES ===
Ejercicio: {year}

{ResponseGuidelines}

Elabora un calendario anual completo que incluya:

1. DECLARACIONES ANUALES OBLIGATORIAS
   • Declaración de la Renta (Modelo 100)
   • Resumen anual de IVA (Modelo 390)
   • Otras declaraciones según actividad

2. CALENDARIO FISCAL ANUAL
   • Fechas límite por trimestres
   • Obligaciones de cierre de ejercicio
   • Pagos fraccionados

3. PLANIFICACIÓN FISCAL
   • Estrategias de optimización
   • Documentación requerida
   • Fechas clave para la gestión

4. CONTROL DE CUMPLIMIENTO
   • Consecuencias del incumplimiento
   • Recomendaciones de seguimiento

Organiza cronológicamente desde enero hasta diciembre, destacando las fechas más críticas.";
    }

    /// <summary>
    /// Builds a basic form guidance prompt for a specific form type
    /// </summary>
    public static string BuildBasicFormPrompt(string formType)
    {
        return $@"{ProfessionalIdentity}

=== GUÍA DEL FORMULARIO ===
Formulario: {formType}

{ResponseGuidelines}

Proporciona una guía completa estructurada que incluya:

1. INFORMACIÓN GENERAL
   • Finalidad del formulario
   • Obligados a presentar

2. PROCESO DE CUMPLIMENTACIÓN
   • Datos necesarios
   • Documentos de soporte
   • Cálculos principales

3. ASPECTOS PROCEDIMENTALES
   • Plazos de presentación
   • Formas de pago
   • Canales de presentación

4. GESTIÓN DE CONTINGENCIAS
   • Errores comunes
   • Consecuencias del incumplimiento
   • Procedimientos de rectificación

Enfoca la explicación desde una perspectiva práctica y profesional.";
    }

    /// <summary>
    /// Builds a form guidance prompt with comprehensive user context
    /// </summary>
    public static string BuildFormPromptWithFullContext(string question, string formType, UserFullContext userFullContext)
    {
        var user = userFullContext.UserContext;
        var invoices = userFullContext.Invoices;

        return $@"{ProfessionalIdentity}

=== CONSULTA ESPECÍFICA DEL CLIENTE ===
Cliente: {user.FirstName} {user.LastName}
Formulario: {formType}
Pregunta: {question}

=== PERFIL FISCAL ACTUAL ===
• CIF: {user.CIF ?? "Pendiente"}
• Actividad: {user.ActivityCode ?? "No especificada"}
• Régimen: {user.TaxRegime ?? "Régimen general"}
• Ingresos acumulados: {user.TotalIncomeCurrentYear:C}
• IVA ejercicio: {user.TotalVATCurrentYear:C}
• IRPF ejercicio: {user.TotalIRPFCurrentYear:C}
• Facturas emitidas: {invoices.Count}

{ResponseGuidelines}

Responde la consulta específica utilizando el contexto fiscal del cliente para proporcionar una solución personalizada y práctica.";
    }

    /// <summary>
    /// Builds a quarterly obligations prompt with comprehensive user context
    /// </summary>
    public static string BuildQuarterlyObligationsPromptWithFullContext(string question, int quarter, int year, UserFullContext userFullContext)
    {
        var user = userFullContext.UserContext;
        var invoices = userFullContext.Invoices;
        
        var quarterlyInvoices = invoices.Count(i => 
            i.IssueDate.Year == year && 
            (i.IssueDate.Month - 1) / 3 + 1 == quarter);

        return $@"{ProfessionalIdentity}

=== CONSULTA SOBRE OBLIGACIONES TRIMESTRALES ===
Cliente: {user.FirstName} {user.LastName}
Período: {quarter}º trimestre {year}
Pregunta específica: {question}

=== CONTEXTO FISCAL DEL CLIENTE ===
• CIF: {user.CIF ?? "Pendiente"}
• Actividad: {user.ActivityCode ?? "No especificada"}
• Régimen: {user.TaxRegime ?? "Régimen general"}
• Ingresos año actual: {user.TotalIncomeCurrentYear:C}
• IVA repercutido: {user.TotalVATCurrentYear:C}
• IRPF retenido: {user.TotalIRPFCurrentYear:C}
• Facturas del trimestre: {quarterlyInvoices}

{ResponseGuidelines}

Proporciona una respuesta específica sobre las obligaciones fiscales del trimestre consultado, utilizando el contexto financiero del cliente para cálculos precisos y recomendaciones personalizadas.

Incluye información sobre:
1. Modelos fiscales aplicables según su actividad y régimen
2. Fechas límite específicas para el período
3. Cálculos estimados basados en sus datos financieros
4. Recomendaciones de planificación fiscal";
    }

    /// <summary>
    /// Builds an annual obligations prompt with comprehensive user context
    /// </summary>
    public static string BuildAnnualObligationsPromptWithFullContext(string question, int year, UserFullContext userFullContext)
    {
        var user = userFullContext.UserContext;
        var invoices = userFullContext.Invoices;
        
        var yearlyInvoices = invoices.Count(i => i.IssueDate.Year == year);

        return $@"{ProfessionalIdentity}

=== CONSULTA SOBRE OBLIGACIONES ANUALES ===
Cliente: {user.FirstName} {user.LastName}
Ejercicio: {year}
Pregunta específica: {question}

=== CONTEXTO FISCAL DEL CLIENTE ===
• CIF: {user.CIF ?? "Pendiente"}
• Actividad: {user.ActivityCode ?? "No especificada"}
• Régimen: {user.TaxRegime ?? "Régimen general"}
• Ingresos año consultado: {user.TotalIncomeCurrentYear:C}
• IVA repercutido: {user.TotalVATCurrentYear:C}
• IRPF retenido: {user.TotalIRPFCurrentYear:C}
• Facturas del ejercicio: {yearlyInvoices}

{ResponseGuidelines}

Proporciona una respuesta específica sobre las obligaciones fiscales anuales, utilizando el contexto financiero del cliente para cálculos precisos y recomendaciones personalizadas.

Incluye información sobre:
1. Declaraciones anuales obligatorias según su actividad
2. Calendario fiscal anual personalizado
3. Estimaciones de pagos basadas en sus datos
4. Estrategias de optimización fiscal para el próximo ejercicio";
    }
}