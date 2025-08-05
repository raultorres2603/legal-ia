using Legal_IA.DTOs;
using Legal_IA.Enums;
using Legal_IA.Models;
using Legal_IA.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;

namespace Legal_IA.Functions;

public class InvoiceHttpTriggers
{
    [Function("GetInvoices")]
    public async Task<IActionResult> GetInvoices(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "invoices")]
        HttpRequestData req,
        FunctionContext context,
        [DurableClient] DurableTaskClient client)
    {
        var jwtResult = await JwtValidationHelper.ValidateJwtAsync(req, client);
        if (!JwtValidationHelper.HasRequiredRole(jwtResult, nameof(UserRole.Admin))) return new UnauthorizedResult();
        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync("InvoiceGetAllOrchestrator", null);
        var response = await client.WaitForInstanceCompletionAsync(instanceId, true, CancellationToken.None);
        if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
            return new OkObjectResult(response.ReadOutputAs<List<Invoice>>());
        return new StatusCodeResult(500);
    }

    [Function("GetInvoiceById")]
    public async Task<IActionResult> GetInvoiceById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "invoices/{id}")]
        HttpRequestData req,
        FunctionContext context,
        [DurableClient] DurableTaskClient client,
        string id)
    {
        var jwtResult = await JwtValidationHelper.ValidateJwtAsync(req, client);
        if (!JwtValidationHelper.HasRequiredRole(jwtResult, nameof(UserRole.Admin))) return new UnauthorizedResult();
        if (!Guid.TryParse(id, out var guid)) return new BadRequestResult();
        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync("InvoiceGetByIdOrchestrator", guid);
        var response = await client.WaitForInstanceCompletionAsync(instanceId, true, CancellationToken.None);
        if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
        {
            var invoice = response.ReadOutputAs<Invoice>();
            return invoice == null ? new NotFoundResult() : new OkObjectResult(invoice);
        }

        return new StatusCodeResult(500);
    }

    [Function("CreateInvoice")]
    public async Task<IActionResult> CreateInvoice(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "invoices")]
        HttpRequestData req,
        FunctionContext context,
        [DurableClient] DurableTaskClient client)
    {
        var jwtResult = await JwtValidationHelper.ValidateJwtAsync(req, client);
        if (!JwtValidationHelper.HasRequiredRole(jwtResult, nameof(UserRole.Admin))) return new UnauthorizedResult();
        var invoice = await req.ReadFromJsonAsync<Invoice>();
        if (invoice == null) return new BadRequestResult();
        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync("InvoiceCreateOrchestrator", invoice);
        var response = await client.WaitForInstanceCompletionAsync(instanceId, true, CancellationToken.None);
        if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
            return new OkObjectResult(response.ReadOutputAs<Invoice>());
        return new StatusCodeResult(500);
    }

    [Function("UpdateInvoice")]
    public async Task<IActionResult> UpdateInvoice(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "invoices/{id}")]
        HttpRequestData req,
        FunctionContext context,
        [DurableClient] DurableTaskClient client,
        string id)
    {
        var jwtResult = await JwtValidationHelper.ValidateJwtAsync(req, client);
        if (!JwtValidationHelper.HasRequiredRole(jwtResult, nameof(UserRole.Admin))) return new UnauthorizedResult();
        if (!Guid.TryParse(id, out var guid)) return new BadRequestResult();
        var invoice = await req.ReadFromJsonAsync<Invoice>();
        if (invoice == null) return new BadRequestResult();
        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync("InvoiceUpdateOrchestrator", invoice);
        var response = await client.WaitForInstanceCompletionAsync(instanceId, true, CancellationToken.None);
        if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
            return new OkObjectResult(response.ReadOutputAs<Invoice>());
        return new StatusCodeResult(500);
    }

    [Function("DeleteInvoice")]
    public async Task<IActionResult> DeleteInvoice(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "invoices/{id}")]
        HttpRequestData req,
        FunctionContext context,
        [DurableClient] DurableTaskClient client,
        string id)
    {
        var jwtResult = await JwtValidationHelper.ValidateJwtAsync(req, client);
        if (!JwtValidationHelper.HasRequiredRole(jwtResult, nameof(UserRole.Admin))) return new UnauthorizedResult();
        if (!Guid.TryParse(id, out var guid)) return new BadRequestResult();
        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync("InvoiceDeleteOrchestrator", guid);
        var response = await client.WaitForInstanceCompletionAsync(instanceId, true, CancellationToken.None);
        if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
            return new OkResult();
        return new StatusCodeResult(500);
    }

    [Function("GetInvoicesByCurrentUser")]
    public async Task<IActionResult> GetInvoicesByCurrentUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "invoices/user")] HttpRequestData req,
        FunctionContext context,
        [DurableClient] DurableTaskClient client)
    {
        var jwtResult = await JwtValidationHelper.ValidateJwtAsync(req, client);
        if (!JwtValidationHelper.HasRequiredRole(jwtResult, nameof(UserRole.User)))
            return new UnauthorizedResult();
        if (jwtResult?.UserId == null || !Guid.TryParse(jwtResult.UserId, out var userId))
            return new BadRequestObjectResult("Invalid or missing UserId in JWT");
        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync("InvoiceGetByUserIdOrchestrator", userId);
        var response = await client.WaitForInstanceCompletionAsync(instanceId, true, System.Threading.CancellationToken.None);
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
        var invoice = await req.ReadFromJsonAsync<Invoice>();
        if (invoice == null) return new BadRequestResult();
        if (jwtResult.UserId == null || !Guid.TryParse(jwtResult.UserId, out var userId))
            return new BadRequestObjectResult("Invalid or missing UserId in JWT");
        invoice.UserId = userId;
        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync("InvoiceCreateOrchestrator", invoice);
        var response = await client.WaitForInstanceCompletionAsync(instanceId, true, System.Threading.CancellationToken.None);
        if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
            return new OkObjectResult(response.ReadOutputAs<Invoice>());
        return new StatusCodeResult(500);
    }
}