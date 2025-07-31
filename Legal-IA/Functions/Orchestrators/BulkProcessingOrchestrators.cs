using Legal_IA.DTOs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace Legal_IA.Functions.Orchestrators;

/// <summary>
///     Bulk processing orchestrator functions
/// </summary>
public class BulkProcessingOrchestrators
{
    private readonly ILogger<BulkProcessingOrchestrators> _logger;

    public BulkProcessingOrchestrators(ILogger<BulkProcessingOrchestrators> logger)
    {
        _logger = logger;
    }

    [Function("BulkDocumentProcessingOrchestrator")]
    public async Task<List<DocumentResponse>> BulkDocumentProcessingOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var documentIds = context.GetInput<List<Guid>>()!;
        var results = new List<DocumentResponse>();

        try
        {
            // Process documents in parallel with a maximum degree of parallelism
            const int maxParallelism = 5;

            for (var i = 0; i < documentIds.Count; i += maxParallelism)
            {
                var batch = documentIds.Skip(i).Take(maxParallelism);
                var batchTasks = batch.Select(id =>
                    context.CallActivityAsync<DocumentResponse>("ProcessSingleDocumentActivity", id));

                var batchResults = await Task.WhenAll(batchTasks);
                results.AddRange(batchResults);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk document processing orchestration");
            throw;
        }
    }
}