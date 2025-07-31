using Legal_IA.DTOs;
using Legal_IA.Interfaces.Services;
using Legal_IA.Models;
using Microsoft.Extensions.Logging;

namespace Legal_IA.Services;

/// <summary>
///     AI Document Generator service implementation
/// </summary>
public class AIDocumentGeneratorService : IAIDocumentGeneratorService
{
    private readonly ILogger<AIDocumentGeneratorService> _logger;

    public AIDocumentGeneratorService(ILogger<AIDocumentGeneratorService> logger)
    {
        _logger = logger;
    }

    public async Task<string> GenerateDocumentContentAsync(DocumentResponse document,
        Dictionary<string, object> parameters)
    {
        _logger.LogInformation("Generating content for document {DocumentId} of type {DocumentType}",
            document.Id, document.Type);

        // TODO: Replace with actual AI Agent integration
        // For now, we'll generate template-based content
        var content = await GenerateContentBasedOnType(document, parameters);

        _logger.LogInformation("Content generated successfully for document {DocumentId}", document.Id);
        return content;
    }

    private async Task<string> GenerateContentBasedOnType(DocumentResponse document,
        Dictionary<string, object> parameters)
    {
        // Simulate async AI processing
        await Task.Delay(100);

        return document.Type switch
        {
            DocumentType.Invoice => GenerateInvoiceContent(document, parameters),
            DocumentType.VATReturn => GenerateVATReturnContent(document, parameters),
            DocumentType.IRPFReturn => GenerateIRPFReturnContent(document, parameters),
            DocumentType.SocialSecurityForm => GenerateSocialSecurityFormContent(document, parameters),
            DocumentType.ExpenseReport => GenerateExpenseReportContent(document, parameters),
            DocumentType.Contract => GenerateContractContent(document, parameters),
            DocumentType.Receipt => GenerateReceiptContent(document, parameters),
            DocumentType.TaxForm => GenerateTaxFormContent(document, parameters),
            DocumentType.BusinessPlan => GenerateBusinessPlanContent(document, parameters),
            DocumentType.LegalDocument => GenerateLegalDocumentContent(document, parameters),
            _ =>
                $"Generated content for {document.Type} document: {document.Title}\n\nDescription: {document.Description}"
        };
    }

    private string GenerateInvoiceContent(DocumentResponse document, Dictionary<string, object> parameters)
    {
        return $@"
FACTURA

Número: INV-{document.Id.ToString("N")[..8].ToUpper()}
Fecha: {DateTime.Now:dd/MM/yyyy}

DATOS DEL EMISOR:
{GetParameterValue(parameters, "businessName", "Nombre del Negocio")}
CIF: {GetParameterValue(parameters, "cif", "B12345678")}
Dirección: {GetParameterValue(parameters, "address", "Dirección del negocio")}
Teléfono: {GetParameterValue(parameters, "phone", "123-456-789")}
Email: {GetParameterValue(parameters, "email", "contacto@negocio.com")}

DATOS DEL CLIENTE:
{GetParameterValue(parameters, "clientName", "Nombre del Cliente")}
{GetParameterValue(parameters, "clientAddress", "Dirección del Cliente")}
NIF/CIF: {GetParameterValue(parameters, "clientTaxId", "12345678Z")}

DESGLOSE:
Descripción: {document.Description}
Cantidad: {GetParameterValue(parameters, "quantity", "1")}
Precio unitario: {document.Amount ?? 0:F2} {document.Currency}

Subtotal: {document.Amount ?? 0:F2} {document.Currency}
IVA (21%): {(document.Amount ?? 0) * 0.21m:F2} {document.Currency}
TOTAL: {(document.Amount ?? 0) * 1.21m:F2} {document.Currency}

Forma de pago: {GetParameterValue(parameters, "paymentMethod", "Transferencia bancaria")}
Vencimiento: {GetParameterValue(parameters, "dueDate", DateTime.Now.AddDays(30).ToString("dd/MM/yyyy"))}
";
    }

    private string GenerateVATReturnContent(DocumentResponse document, Dictionary<string, object> parameters)
    {
        return $@"
DECLARACIÓN TRIMESTRAL DEL IVA
MODELO 303

Ejercicio: {document.Year}
Período: {document.Quarter}º Trimestre

IDENTIFICACIÓN:
CIF: {GetParameterValue(parameters, "cif", "12345678Z")}
Apellidos y nombre o denominación social: {GetParameterValue(parameters, "businessName", "Autónomo")}

LIQUIDACIÓN:
01. Base imponible: {GetParameterValue(parameters, "taxableBase", "0,00")} €
02. Cuota devengada: {GetParameterValue(parameters, "outputVAT", "0,00")} €
03. Cuota soportada deducible: {GetParameterValue(parameters, "inputVAT", "0,00")} €

RESULTADO DE LA LIQUIDACIÓN:
Resultado: {GetParameterValue(parameters, "result", "0,00")} €

A ingresar: {GetParameterValue(parameters, "amountToPay", "0,00")} €
A compensar: {GetParameterValue(parameters, "amountToCompensate", "0,00")} €

Fecha de presentación: {DateTime.Now:dd/MM/yyyy}
";
    }

    private string GenerateIRPFReturnContent(DocumentResponse document, Dictionary<string, object> parameters)
    {
        return $@"
DECLARACIÓN ANUAL DEL IRPF
MODELO 100

Ejercicio: {document.Year}

DATOS PERSONALES:
DNI: {GetParameterValue(parameters, "dni", "12345678Z")}
Apellidos y nombre: {GetParameterValue(parameters, "fullName", "Nombre Completo")}
Fecha de nacimiento: {GetParameterValue(parameters, "birthDate", "01/01/1980")}

ACTIVIDAD ECONÓMICA:
Epígrafe IAE: {GetParameterValue(parameters, "iaeCode", "861.1")}
Descripción: {GetParameterValue(parameters, "activityDescription", "Actividades jurídicas")}

RENDIMIENTOS DEL TRABAJO Y ACTIVIDADES ECONÓMICAS:
Ingresos íntegros: {GetParameterValue(parameters, "grossIncome", "0,00")} €
Gastos deducibles: {GetParameterValue(parameters, "deductibleExpenses", "0,00")} €
Rendimiento neto: {decimal.Parse(GetParameterValue(parameters, "grossIncome", "0")) - decimal.Parse(GetParameterValue(parameters, "deductibleExpenses", "0")):F2} €

RETENCIONES Y PAGOS A CUENTA:
Retenciones del trabajo: {GetParameterValue(parameters, "workRetentions", "0,00")} €
Pagos fraccionados: {GetParameterValue(parameters, "fractionalPayments", "0,00")} €

RESULTADO:
Cuota íntegra: {GetParameterValue(parameters, "integralQuota", "0,00")} €
Cuota líquida: {GetParameterValue(parameters, "liquidQuota", "0,00")} €
Resultado de la declaración: {GetParameterValue(parameters, "declarationResult", "0,00")} €
";
    }

    private string GenerateSocialSecurityFormContent(DocumentResponse document, Dictionary<string, object> parameters)
    {
        return $@"
RÉGIMEN ESPECIAL DE TRABAJADORES AUTÓNOMOS
LIQUIDACIÓN DE CUOTAS

Período: {document.Quarter}º Trimestre {document.Year}

DATOS DEL COTIZANTE:
Número de afiliación: {GetParameterValue(parameters, "socialSecurityNumber", "123456789012")}
Nombre y apellidos: {GetParameterValue(parameters, "fullName", "Nombre Completo")}
DNI: {GetParameterValue(parameters, "dni", "12345678Z")}

BASE DE COTIZACIÓN:
Base mínima: {GetParameterValue(parameters, "minimumBase", "1.166,70")} €
Base elegida: {GetParameterValue(parameters, "chosenBase", "1.166,70")} €

CUOTAS:
Contingencias comunes: {GetParameterValue(parameters, "commonContingencies", "325,57")} €
Cese de actividad: {GetParameterValue(parameters, "cessationOfActivity", "11,67")} €
Formación profesional: {GetParameterValue(parameters, "professionalTraining", "1,17")} €

TOTAL A INGRESAR: {GetParameterValue(parameters, "totalAmount", "338,41")} €

Forma de pago: {GetParameterValue(parameters, "paymentMethod", "Domiciliación bancaria")}
Fecha de vencimiento: {GetParameterValue(parameters, "dueDate", DateTime.Now.AddDays(30).ToString("dd/MM/yyyy"))}
";
    }

    private string GenerateExpenseReportContent(DocumentResponse document, Dictionary<string, object> parameters)
    {
        return $@"
INFORME DE GASTOS DEDUCIBLES

Período: {GetParameterValue(parameters, "period", $"{document.Quarter}º Trimestre {document.Year}")}
Autónomo: {GetParameterValue(parameters, "businessName", "Nombre del Autónomo")}
CIF: {GetParameterValue(parameters, "cif", "12345678Z")}

DESGLOSE POR CATEGORÍAS:

1. GASTOS DE OFICINA:
   - Material de oficina: {GetParameterValue(parameters, "officeSupplies", "0,00")} €
   - Comunicaciones: {GetParameterValue(parameters, "communications", "0,00")} €
   - Servicios profesionales: {GetParameterValue(parameters, "professionalServices", "0,00")} €

2. GASTOS DE TRANSPORTE:
   - Combustible: {GetParameterValue(parameters, "fuel", "0,00")} €
   - Mantenimiento vehículo: {GetParameterValue(parameters, "vehicleMaintenance", "0,00")} €
   - Transporte público: {GetParameterValue(parameters, "publicTransport", "0,00")} €

3. FORMACIÓN Y DESARROLLO:
   - Cursos y seminarios: {GetParameterValue(parameters, "training", "0,00")} €
   - Libros y publicaciones: {GetParameterValue(parameters, "publications", "0,00")} €

4. OTROS GASTOS:
   - Seguros: {GetParameterValue(parameters, "insurance", "0,00")} €
   - Gastos financieros: {GetParameterValue(parameters, "financialExpenses", "0,00")} €
   - Otros: {GetParameterValue(parameters, "otherExpenses", "0,00")} €

TOTAL GASTOS DEDUCIBLES: {document.Amount:F2} €

IVA soportado deducible: {(document.Amount ?? 0) * 0.21m:F2} €
";
    }

    private string GenerateContractContent(DocumentResponse document, Dictionary<string, object> parameters)
    {
        return $@"
CONTRATO DE PRESTACIÓN DE SERVICIOS

En {GetParameterValue(parameters, "city", "Madrid")}, a {DateTime.Now:dd} de {DateTime.Now:MMMM} de {DateTime.Now:yyyy}

REUNIDOS:

De una parte, {GetParameterValue(parameters, "clientName", "CLIENTE")}, con CIF {GetParameterValue(parameters, "clientCif", "A12345678")}, 
en adelante ""EL CLIENTE"".

De otra parte, {GetParameterValue(parameters, "providerName", "PROVEEDOR")}, con CIF {GetParameterValue(parameters, "providerCif", "12345678Z")}, 
en adelante ""EL PRESTADOR"".

EXPONEN:

Que el PRESTADOR es profesional en {GetParameterValue(parameters, "serviceDescription", "servicios jurídicos")} 
y el CLIENTE desea contratar dichos servicios.

ACUERDAN:

PRIMERO.- OBJETO: {document.Description}

SEGUNDO.- PRECIO: El precio total es de {document.Amount:F2} {document.Currency}, 
que se abonará {GetParameterValue(parameters, "paymentTerms", "al finalizar el trabajo")}.

TERCERO.- PLAZO: {GetParameterValue(parameters, "duration", "30 días naturales")} desde la firma del contrato.

CUARTO.- CONFIDENCIALIDAD: Ambas partes se comprometen a mantener la confidencialidad 
de la información intercambiada.

Y en prueba de conformidad, firman el presente contrato en el lugar y fecha indicados.

EL CLIENTE                    EL PRESTADOR
";
    }

    private string GenerateReceiptContent(DocumentResponse document, Dictionary<string, object> parameters)
    {
        return $@"
RECIBÍ

Recibí de {GetParameterValue(parameters, "payerName", "PAGADOR")}
la cantidad de {document.Amount:F2} {document.Currency}
({GetParameterValue(parameters, "amountInWords", "CANTIDAD EN LETRAS")} {document.Currency})

En concepto de: {document.Description}

Forma de pago: {GetParameterValue(parameters, "paymentMethod", "Efectivo")}

{GetParameterValue(parameters, "city", "Madrid")}, {DateTime.Now:dd/MM/yyyy}

Firmado: {GetParameterValue(parameters, "recipientName", "RECEPTOR")}
DNI: {GetParameterValue(parameters, "recipientDni", "12345678Z")}
";
    }

    private string GenerateTaxFormContent(DocumentResponse document, Dictionary<string, object> parameters)
    {
        return $@"
FORMULARIO FISCAL GENÉRICO

Modelo: {GetParameterValue(parameters, "formModel", "XXX")}
Ejercicio: {document.Year}
Período: {GetParameterValue(parameters, "period", "Anual")}

IDENTIFICACIÓN:
NIF/CIF: {GetParameterValue(parameters, "taxId", "12345678Z")}
Denominación: {GetParameterValue(parameters, "name", "Contribuyente")}

DECLARACIÓN:
{document.Description}

Importe: {document.Amount:F2} {document.Currency}

Lugar y fecha: {GetParameterValue(parameters, "city", "Madrid")}, {DateTime.Now:dd/MM/yyyy}

Firma del declarante
";
    }

    private string GenerateBusinessPlanContent(DocumentResponse document, Dictionary<string, object> parameters)
    {
        return $@"
PLAN DE NEGOCIO

1. RESUMEN EJECUTIVO
Nombre del negocio: {GetParameterValue(parameters, "businessName", "Mi Negocio")}
Sector: {GetParameterValue(parameters, "sector", "Servicios")}
Forma jurídica: {GetParameterValue(parameters, "legalForm", "Autónomo")}

2. DESCRIPCIÓN DEL NEGOCIO
{document.Description}

3. ANÁLISIS DE MERCADO
Mercado objetivo: {GetParameterValue(parameters, "targetMarket", "Mercado local")}
Competencia: {GetParameterValue(parameters, "competition", "Competidores locales")}

4. PLAN FINANCIERO
Inversión inicial: {GetParameterValue(parameters, "initialInvestment", "10.000")} €
Ingresos estimados año 1: {document.Amount:F2} {document.Currency}
Gastos estimados: {GetParameterValue(parameters, "estimatedExpenses", "8.000")} €

5. ESTRATEGIA DE MARKETING
{GetParameterValue(parameters, "marketingStrategy", "Marketing digital y redes sociales")}

Fecha de elaboración: {DateTime.Now:dd/MM/yyyy}
";
    }

    private string GenerateLegalDocumentContent(DocumentResponse document, Dictionary<string, object> parameters)
    {
        return $@"
DOCUMENTO LEGAL

Tipo: {GetParameterValue(parameters, "documentType", "Documento genérico")}
Fecha: {DateTime.Now:dd/MM/yyyy}

PARTES INVOLUCRADAS:
{GetParameterValue(parameters, "parties", "Partes del documento")}

CONTENIDO:
{document.Description}

CLAUSULADO:
{GetParameterValue(parameters, "clauses", "Cláusulas específicas del documento")}

IMPORTE ECONÓMICO: {document.Amount:F2} {document.Currency}

En prueba de conformidad, se firma el presente documento.

Firmas:
";
    }

    private static string GetParameterValue(Dictionary<string, object> parameters, string key, string defaultValue)
    {
        return parameters.TryGetValue(key, out var value) ? value?.ToString() ?? defaultValue : defaultValue;
    }
}