using Legal_IA.DTOs;
using Legal_IA.Shared.Enums;
using Legal_IA.Shared.Models;
using Legal_IA.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;

namespace Legal_IA.Functions;

/// <summary>
///     HTTP-triggered Azure Functions for invoice operations by the current user.
/// </summary>
public class InvoiceHttpTriggers
{
    /// <summary>
    ///     Gets invoices for the current user.
    /// </summary>
    [Function("GetInvoicesByCurrentUser")]
    public async Task<IActionResult> GetInvoicesByCurrentUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "invoices/user")]
        HttpRequestData req,
        FunctionContext context,
        [DurableClient] DurableTaskClient client)
    {
        var (userId, errorResult) = await ValidateAndExtractUserId(req, client);
        if (errorResult != null) return errorResult;
        return await RunOrchestrationAndRespond(client, "InvoiceGetByUserIdOrchestrator", userId);
    }

    /// <summary>
    ///     Creates an invoice for the current user.
    /// </summary>
    [Function("CreateInvoiceByCurrentUser")]
    public async Task<IActionResult> CreateInvoiceByCurrentUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "invoices/user")]
        HttpRequestData req,
        FunctionContext context,
        [DurableClient] DurableTaskClient client)
    {
        var (userId, errorResult) = await ValidateAndExtractUserId(req, client);
        if (errorResult != null) return errorResult;
        var dto = await req.ReadFromJsonAsync<CreateInvoiceRequest>();
        if (dto == null) return new BadRequestResult();
        var invoice = new Invoice
        {
            InvoiceNumber = dto.InvoiceNumber,
            IssueDate = dto.IssueDate,
            ClientName = dto.ClientName,
            ClientNIF = dto.ClientNIF,
            ClientAddress = dto.ClientAddress,
            Subtotal = dto.Subtotal,
            VAT = dto.VAT,
            IRPF = dto.IRPF,
            Total = dto.Total,
            Notes = dto.Notes,
            Status = dto.Status,
            UserId = userId,
            Items = dto.Items.ConvertAll(i => new InvoiceItem
            {
                Description = i.Description,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                VAT = i.VAT,
                IRPF = i.IRPF,
                Total = i.Total
            })
        };
        return await RunOrchestrationAndRespond(client, "InvoiceCreateOrchestrator", invoice, 201);
    }

    /// <summary>
    ///     Updates an invoice for the current user.
    /// </summary>
    [Function("PatchInvoiceByCurrentUser")]
    public async Task<IActionResult> PatchInvoiceByCurrentUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "invoices/user/{id}")]
        HttpRequestData req,
        FunctionContext context,
        [DurableClient] DurableTaskClient client,
        string id)
    {
        var (userId, errorResult) = await ValidateAndExtractUserId(req, client);
        if (errorResult != null) return errorResult;
        if (!Guid.TryParse(id, out var invoiceId)) return new BadRequestResult();
        var dto = await req.ReadFromJsonAsync<UpdateInvoiceRequest>();
        if (dto == null) return new BadRequestResult();
        var orchestratorInput = new { InvoiceId = invoiceId, UserId = userId, UpdateRequest = dto };
        return await RunOrchestrationAndRespond(client, "PatchInvoiceByCurrentUserOrchestrator", orchestratorInput);
    }

    /// <summary>
    ///     Deletes an invoice for the current user.
    /// </summary>
    [Function("DeleteInvoiceByCurrentUser")]
    public async Task<IActionResult> DeleteInvoiceByCurrentUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "invoices/user/{id}")]
        HttpRequestData req,
        FunctionContext context,
        [DurableClient] DurableTaskClient client,
        string id)
    {
        var (userId, errorResult) = await ValidateAndExtractUserId(req, client);
        if (errorResult != null) return errorResult;
        if (!Guid.TryParse(id, out var invoiceId)) return new BadRequestResult();
        var orchestratorInput = new { InvoiceId = invoiceId, UserId = userId };
        var response =
            await client.ScheduleNewOrchestrationInstanceAsync("InvoiceDeleteByCurrentUserOrchestrator",
                orchestratorInput);
        var orchestrationResult = await client.WaitForInstanceCompletionAsync(response, true, CancellationToken.None);
        if (orchestrationResult.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
        {
            var result = orchestrationResult.ReadOutputAs<bool>();
            return result ? new OkResult() : new ForbidResult();
        }

        return new StatusCodeResult(500);
    }

    // --- Private helpers ---

    /// <summary>
    ///     Validates JWT and extracts userId. Returns (userId, errorResult).
    /// </summary>
    private static async Task<(Guid userId, IActionResult errorResult)> ValidateAndExtractUserId(HttpRequestData req,
        DurableTaskClient client)
    {
        var jwtResult = await JwtValidationHelper.ValidateJwtAsync(req, client);
        if (!JwtValidationHelper.HasRequiredRole(jwtResult, nameof(UserRole.User)))
            return (Guid.Empty, new UnauthorizedResult());
        if (jwtResult?.UserId == null || !Guid.TryParse(jwtResult.UserId, out var userId))
            return (Guid.Empty, new BadRequestObjectResult("Invalid or missing UserId in JWT"));
        return (userId, null);
    }

    /// <summary>
    ///     Schedules an orchestration and returns the HTTP response.
    /// </summary>
    private static async Task<IActionResult> RunOrchestrationAndRespond(DurableTaskClient client,
        string orchestratorName, object input, int successStatusCode = 200)
    {
        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(orchestratorName, input);
        var response = await client.WaitForInstanceCompletionAsync(instanceId, true, CancellationToken.None);
        if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
            return new ObjectResult(response.ReadOutputAs<object>()) { StatusCode = successStatusCode };
        return new StatusCodeResult(500);
    }
}