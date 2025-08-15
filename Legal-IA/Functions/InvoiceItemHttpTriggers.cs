using Legal_IA.DTOs;
using Legal_IA.Enums;
using Legal_IA.Models;
using Legal_IA.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;

namespace Legal_IA.Functions;

public class InvoiceItemHttpTriggers
{
    [Function("GetInvoiceItemsByCurrentUser")]
    public async Task<IActionResult> GetInvoiceItemsByCurrentUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "invoice-items/user")]
        HttpRequestData req,
        FunctionContext context,
        [DurableClient] DurableTaskClient client)
    {
        var jwtResult = await JwtValidationHelper.ValidateJwtAsync(req, client);
        if (!JwtValidationHelper.HasRequiredRole(jwtResult, nameof(UserRole.User)))
            return new UnauthorizedResult();
        if (jwtResult?.UserId == null || !Guid.TryParse(jwtResult.UserId, out var userId))
            return new BadRequestObjectResult("Invalid or missing UserId in JWT");
        var instanceId =
            await client.ScheduleNewOrchestrationInstanceAsync("InvoiceItemGetByUserIdOrchestrator", userId);
        var response = await client.WaitForInstanceCompletionAsync(instanceId, true, CancellationToken.None);
        if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
        {
            var items = response.ReadOutputAs<List<InvoiceItem>>();
            if (items == null || items.Count == 0) return new NotFoundResult();
            return new OkObjectResult(items);
        }

        return new StatusCodeResult(500);
    }

    [Function("CreateInvoiceItemByCurrentUser")]
    public async Task<IActionResult> CreateInvoiceItemByCurrentUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "invoice-items/user")]
        HttpRequestData req,
        FunctionContext context,
        [DurableClient] DurableTaskClient client)
    {
        var jwtResult = await JwtValidationHelper.ValidateJwtAsync(req, client);
        if (!JwtValidationHelper.HasRequiredRole(jwtResult, nameof(UserRole.User))) return new UnauthorizedResult();
        var items = await req.ReadFromJsonAsync<List<InvoiceItem>>();
        if (items == null || items.Count == 0) return new BadRequestResult();
        if (jwtResult?.UserId == null || !Guid.TryParse(jwtResult.UserId, out var userId))
            return new BadRequestObjectResult("Invalid or missing UserId in JWT");
        // Optionally, link items to invoice owned by user (add validation here if needed)
        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync("InvoiceItemCreateOrchestrator", items);
        var response = await client.WaitForInstanceCompletionAsync(instanceId, true, CancellationToken.None);
        if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
        {
            var created = response.ReadOutputAs<List<InvoiceItem>>();
            if (created == null) return new StatusCodeResult(500);
            return new OkObjectResult(created);
        }
        return new StatusCodeResult(500);
    }

    [Function("UpdateInvoiceItemByUser")]
    public async Task<IActionResult> UpdateInvoiceItemByUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "invoice-items/user")]
        HttpRequestData req,
        FunctionContext context,
        [DurableClient] DurableTaskClient client)
    {
        var jwtResult = await JwtValidationHelper.ValidateJwtAsync(req, client);
        if (!JwtValidationHelper.HasRequiredRole(jwtResult, nameof(UserRole.User))) return new UnauthorizedResult();
        var dtos = await req.ReadFromJsonAsync<List<BatchUpdateInvoiceItemRequest>>();
        if (dtos == null || dtos.Count == 0) return new BadRequestResult();
        if (jwtResult?.UserId == null || !Guid.TryParse(jwtResult.UserId, out _))
            return new BadRequestObjectResult("Invalid or missing UserId in JWT");
        var input = new { jwtResult.UserId, UpdateRequests = dtos };
        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync("PatchInvoiceItemOrchestrator", input);
        var response = await client.WaitForInstanceCompletionAsync(instanceId, true, CancellationToken.None);
        if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
        {
            var updated = response.ReadOutputAs<List<InvoiceItem>>();
            if (updated == null || updated.Count == 0) return new NotFoundResult();
            return new OkObjectResult(updated);
        }

        return new StatusCodeResult(500);
    }

    [Function("DeleteInvoiceItemByUser")]
    public async Task<IActionResult> DeleteInvoiceItemByUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "invoice-items/user/{id}")]
        HttpRequestData req,
        FunctionContext context,
        [DurableClient] DurableTaskClient client,
        string id)
    {
        var jwtResult = await JwtValidationHelper.ValidateJwtAsync(req, client);
        if (!JwtValidationHelper.HasRequiredRole(jwtResult, nameof(UserRole.User))) return new UnauthorizedResult();
        if (!Guid.TryParse(id, out var itemId)) return new BadRequestResult();
        if (jwtResult?.UserId == null || !Guid.TryParse(jwtResult.UserId, out _))
            return new BadRequestObjectResult("Invalid or missing UserId in JWT");
        var input = new { ItemId = itemId, jwtResult.UserId };
        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync("InvoiceItemDeleteOrchestrator", input);
        var response = await client.WaitForInstanceCompletionAsync(instanceId, true, CancellationToken.None);
        if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
        {
            var deleted = response.ReadOutputAs<bool>();
            if (!deleted) return new NotFoundResult();
            return new OkResult();
        }

        return new StatusCodeResult(500);
    }
}