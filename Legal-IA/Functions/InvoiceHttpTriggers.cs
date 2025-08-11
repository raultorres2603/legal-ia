using Legal_IA.DTOs;
using Legal_IA.Enums;
using Legal_IA.Models;
using Legal_IA.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace Legal_IA.Functions;

public class InvoiceHttpTriggers
{

    [Function("GetInvoicesByCurrentUser")]
    public async Task<IActionResult> GetInvoicesByCurrentUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "invoices/user")]
        HttpRequestData req,
        FunctionContext context,
        [DurableClient] DurableTaskClient client)
    {
        var jwtResult = await JwtValidationHelper.ValidateJwtAsync(req, client);
        if (!JwtValidationHelper.HasRequiredRole(jwtResult, nameof(UserRole.User))) return new UnauthorizedResult();
        if (jwtResult?.UserId == null || !Guid.TryParse(jwtResult.UserId, out var userId))
            return new BadRequestObjectResult("Invalid or missing UserId in JWT");
        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync("InvoiceGetByUserIdOrchestrator", userId);
        var response = await client.WaitForInstanceCompletionAsync(instanceId, true, CancellationToken.None);
        if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
            return new OkObjectResult(response.ReadOutputAs<List<Invoice>>());
        return new StatusCodeResult(500);
    }

    [Function("CreateInvoiceByCurrentUser")]
    public async Task<IActionResult> CreateInvoiceByCurrentUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "invoices/user")]
        HttpRequestData req,
        FunctionContext context,
        [DurableClient] DurableTaskClient client)
    {
        var jwtResult = await JwtValidationHelper.ValidateJwtAsync(req, client);
        if (!JwtValidationHelper.HasRequiredRole(jwtResult, nameof(UserRole.User))) return new UnauthorizedResult();
        var dto = await req.ReadFromJsonAsync<CreateInvoiceRequest>();
        if (dto == null) return new BadRequestResult();
        if (jwtResult?.UserId == null || !Guid.TryParse(jwtResult.UserId, out var userId))
            return new BadRequestObjectResult("Invalid or missing UserId in JWT");
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
        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync("InvoiceCreateOrchestrator", invoice);
        var response = await client.WaitForInstanceCompletionAsync(instanceId, true, CancellationToken.None);
        if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
            return new OkObjectResult(response.ReadOutputAs<Invoice>());
        return new StatusCodeResult(500);
    }

    [Function("PatchInvoiceByCurrentUser")]
    public async Task<IActionResult> PatchInvoiceByCurrentUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "invoices/user/{id}")]
        HttpRequestData req,
        FunctionContext context,
        [DurableClient] DurableTaskClient client,
        string id)
    {
        var jwtResult = await JwtValidationHelper.ValidateJwtAsync(req, client);
        if (!JwtValidationHelper.HasRequiredRole(jwtResult, nameof(UserRole.User))) return new UnauthorizedResult();
        if (jwtResult?.UserId == null || !Guid.TryParse(jwtResult.UserId, out var userId))
            return new BadRequestObjectResult("Invalid or missing UserId in JWT");
        if (!Guid.TryParse(id, out var invoiceId)) return new BadRequestResult();
        var dto = await req.ReadFromJsonAsync<UpdateInvoiceRequest>();
        if (dto == null) return new BadRequestResult();

        var orchestratorInput = new { InvoiceId = invoiceId, UserId = userId, UpdateRequest = dto };
        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync("PatchInvoiceByCurrentUserOrchestrator", orchestratorInput);
        var response = await client.WaitForInstanceCompletionAsync(instanceId, true, CancellationToken.None);
        if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
            return new OkObjectResult(response.ReadOutputAs<Invoice>());
        return new StatusCodeResult(500);
    }

    [Function("DeleteInvoiceByCurrentUser")]
    public async Task<IActionResult> DeleteInvoiceByCurrentUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "invoices/user/{id}")]
        HttpRequestData req,
        FunctionContext context,
        [DurableClient] DurableTaskClient client,
        string id)
    {
        var jwtResult = await JwtValidationHelper.ValidateJwtAsync(req, client);
        if (!JwtValidationHelper.HasRequiredRole(jwtResult, nameof(UserRole.User))) return new UnauthorizedResult();
        if (jwtResult?.UserId == null || !Guid.TryParse(jwtResult.UserId, out var userId))
            return new BadRequestObjectResult("Invalid or missing UserId in JWT");
        if (!Guid.TryParse(id, out var invoiceId)) return new BadRequestResult();
        var orchestratorInput = new { InvoiceId = invoiceId, UserId = userId };
        var instanceId =
            await client.ScheduleNewOrchestrationInstanceAsync("InvoiceDeleteByCurrentUserOrchestrator",
                orchestratorInput);
        var response = await client.WaitForInstanceCompletionAsync(instanceId, true, CancellationToken.None);
        if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
        {
            var result = response.ReadOutputAs<bool>();
            return result ? new OkResult() : new ForbidResult();
        }

        return new StatusCodeResult(500);
    }
}