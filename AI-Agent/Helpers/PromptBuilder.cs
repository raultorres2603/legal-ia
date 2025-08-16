using AI_Agent.Models;

namespace AI_Agent.Helpers
{
    public static class PromptBuilder
    {
        public static string BuildSystemPrompt()
        {
            return @"Eres un asistente experto en derecho y fiscalidad para autónomos en España. Responde de manera clara, precisa y profesional. Si la pregunta no es legal o fiscal, indica que solo puedes responder sobre temas legales, fiscales o judiciales en España.";
        }

        public static string BuildSystemPrompt(UserContext userContext)
        {
            return $@"Eres un asistente experto en derecho y fiscalidad para autónomos en España. Responde de manera clara, precisa y profesional. El usuario se llama {userContext.FirstName} {userContext.LastName}. Si la pregunta no es legal o fiscal, indica que solo puedes responder sobre temas legales, fiscales o judiciales en España.";
        }

        public static string BuildFormPrompt(AutonomoFormRequest request)
        {
            return $@"
Como experto en fiscalidad para autónomos en España, proporciona una guía completa para el modelo {request.FormType}.

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

        public static string BuildPersonalizedFormPrompt(AutonomoFormRequest request, UserContext userContext)
        {
            return $@"
Como experto en fiscalidad para autónomos en España, proporciona una guía PERSONALIZADA para {userContext.FirstName} {userContext.LastName} sobre el modelo {request.FormType}.

Información específica del usuario:
- Trimestre: {request.Quarter}
- Año: {request.Year}
";
        }

        public static string BuildQuarterlyObligationsPrompt(int quarter, int year)
        {
            return $@"
Como experto fiscal para autónomos en España, proporciona un resumen completo de todas las obligaciones fiscales y de Seguridad Social para el {quarter}º trimestre de {year}.

Incluye:
1. Modelos a presentar (303, 130/131, etc.)
2. Fechas límite específicas
3. Obligaciones con la Seguridad Social
4. Recordatorios importantes
5. Consecuencias de incumplimiento

Estructura la información de manera clara y cronológica, empezando por las fechas más próximas.";
        }

        public static string BuildAnnualObligationsPrompt(int year)
        {
            return $@"
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
        }
    }
}

