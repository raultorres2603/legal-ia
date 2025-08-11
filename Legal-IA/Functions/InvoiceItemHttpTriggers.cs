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
    [Function("GetInvoiceItems")]
    public async Task<IActionResult> GetInvoiceItems(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "invoice-items/users")]
        HttpRequestData req,
        FunctionContext context,
        [DurableClient] DurableTaskClient client)
    {
        var jwtResult = await JwtValidationHelper.ValidateJwtAsync(req, client);
        if (!JwtValidationHelper.HasRequiredRole(jwtResult, nameof(UserRole.Admin))) return new UnauthorizedResult();
        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync("InvoiceItemGetAllOrchestrator", null);
        var response = await client.WaitForInstanceCompletionAsync(instanceId, true, CancellationToken.None);
        if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
        {
            var items = response.ReadOutputAs<List<InvoiceItem>>();
            if (items == null || items.Count == 0) return new NotFoundResult();
            return new OkObjectResult(items);
        }

        return new StatusCodeResult(500);
    }

    [Function("GetInvoiceItemById")]
    public async Task<IActionResult> GetInvoiceItemById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "invoice-items/users/{id}")]
        HttpRequestData req,
        FunctionContext context,
        [DurableClient] DurableTaskClient client,
        string id)
    {
        var jwtResult = await JwtValidationHelper.ValidateJwtAsync(req, client);
        if (!JwtValidationHelper.HasRequiredRole(jwtResult, nameof(UserRole.Admin))) return new UnauthorizedResult();
        if (!Guid.TryParse(id, out var guid)) return new BadRequestResult();
        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync("InvoiceItemGetByIdOrchestrator", guid);
        var response = await client.WaitForInstanceCompletionAsync(instanceId, true, CancellationToken.None);
        if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
        {
            var item = response.ReadOutputAs<InvoiceItem>();
            return item == null ? new NotFoundResult() : new OkObjectResult(item);
        }

        return new StatusCodeResult(500);
    }

    [Function("CreateInvoiceItem")]
    public async Task<IActionResult> CreateInvoiceItem(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "invoice-items")]
        HttpRequestData req,
        FunctionContext context,
        [DurableClient] DurableTaskClient client)
    {
        var jwtResult = await JwtValidationHelper.ValidateJwtAsync(req, client);
        if (!JwtValidationHelper.HasRequiredRole(jwtResult, nameof(UserRole.Admin))) return new UnauthorizedResult();
        var item = await req.ReadFromJsonAsync<InvoiceItem>();
        if (item == null) return new BadRequestResult();
        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync("InvoiceItemCreateOrchestrator", item);
        var response = await client.WaitForInstanceCompletionAsync(instanceId, true, CancellationToken.None);
        if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
        {
            var created = response.ReadOutputAs<InvoiceItem>();
            if (created == null) return new StatusCodeResult(500);
            return new OkObjectResult(created);
        }

        return new StatusCodeResult(500);
    }

    [Function("PatchInvoiceItem")]
    public async Task<IActionResult> PatchInvoiceItem(
        [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "invoice-items/users/{id}")]
        HttpRequestData req,
        FunctionContext context,
        [DurableClient] DurableTaskClient client,
        string id)
    {
        var jwtResult = await JwtValidationHelper.ValidateJwtAsync(req, client);
        if (!JwtValidationHelper.HasRequiredRole(jwtResult, nameof(UserRole.Admin))) return new UnauthorizedResult();
        if (!Guid.TryParse(id, out var guid)) return new BadRequestResult();
        var dto = await req.ReadFromJsonAsync<UpdateInvoiceItemRequest>();
        if (dto == null) return new BadRequestResult();
        var orchestratorInput = new { InvoiceItemId = guid, UpdateRequest = dto };
        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync("PatchInvoiceItemOrchestrator", orchestratorInput);
        var response = await client.WaitForInstanceCompletionAsync(instanceId, true, CancellationToken.None);
        if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
        {
            var updated = response.ReadOutputAs<InvoiceItem>();
            if (updated == null) return new NotFoundResult();
            return new OkObjectResult(updated);
        }
        return new StatusCodeResult(500);
    }


    [Function("DeleteInvoiceItem")]
    public async Task<IActionResult> DeleteInvoiceItem(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "invoice-items/users/{id}")]
        HttpRequestData req,
        FunctionContext context,
        [DurableClient] DurableTaskClient client,
        string id)
    {
        var jwtResult = await JwtValidationHelper.ValidateJwtAsync(req, client);
        if (!JwtValidationHelper.HasRequiredRole(jwtResult, nameof(UserRole.Admin))) return new UnauthorizedResult();
        if (!Guid.TryParse(id, out var guid)) return new BadRequestResult();
        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync("InvoiceItemDeleteOrchestrator", id);
        var response = await client.WaitForInstanceCompletionAsync(instanceId, true, CancellationToken.None);
        if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
        {
            var deleted = response.ReadOutputAs<bool>();
            if (!deleted) return new NotFoundResult();
            return new OkResult();
        }

        return new StatusCodeResult(500);
    }

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
        var item = await req.ReadFromJsonAsync<InvoiceItem>();
        if (item == null) return new BadRequestResult();
        if (jwtResult?.UserId == null || !Guid.TryParse(jwtResult.UserId, out var userId))
            return new BadRequestObjectResult("Invalid or missing UserId in JWT");
        // Optionally, link item to invoice owned by user (add validation here if needed)
        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync("InvoiceItemCreateOrchestrator", item);
        var response = await client.WaitForInstanceCompletionAsync(instanceId, true, CancellationToken.None);
        if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
        {
            var created = response.ReadOutputAs<InvoiceItem>();
            if (created == null) return new StatusCodeResult(500);
            // Optionally, set UserId on created item if not already set
            return new OkObjectResult(response.ReadOutputAs<InvoiceItem>());
        }

        return new StatusCodeResult(500);
    }

    [Function("UpdateInvoiceItemByUser")]
    public async Task<IActionResult> UpdateInvoiceItemByUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "invoice-items/user/{id}")]
        HttpRequestData req,
        FunctionContext context,
        [DurableClient] DurableTaskClient client,
        string id)
    {
        var jwtResult = await JwtValidationHelper.ValidateJwtAsync(req, client);
        if (!JwtValidationHelper.HasRequiredRole(jwtResult, nameof(UserRole.User))) return new UnauthorizedResult();
        if (!Guid.TryParse(id, out var itemId)) return new BadRequestResult();
        var dto = await req.ReadFromJsonAsync<DTOs.UpdateInvoiceItemRequest>();
        if (dto == null) return new BadRequestResult();
        if (jwtResult?.UserId == null || !Guid.TryParse(jwtResult.UserId, out var userId))
            return new BadRequestObjectResult("Invalid or missing UserId in JWT");
        var input = new { ItemId = itemId, UserId = userId, UpdateRequest = dto };
        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync("PatchInvoiceItemOrchestrator", input);
        var response = await client.WaitForInstanceCompletionAsync(instanceId, true, CancellationToken.None);
        if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
        {
            var updated = response.ReadOutputAs<InvoiceItem>();
            if (updated == null) return new NotFoundResult();
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
        if (jwtResult?.UserId == null || !Guid.TryParse(jwtResult.UserId, out var userId))
            return new BadRequestObjectResult("Invalid or missing UserId in JWT");
        var input = new { ItemId = itemId, UserId = userId };
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