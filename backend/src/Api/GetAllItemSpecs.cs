using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using CargoMaker.Model;


namespace CargoMaker.Api
{
    public class GetAllItemSpecs
    {
        private static readonly string ConnectionString = Environment.GetEnvironmentVariable("STORAGE_ACCOUNT_CONNECTION_STRING")!;
        private static readonly string TableName = "itemspecs";
        private static readonly string PartitionKey = "main";

        private readonly ILogger<RequestCargoPlan> log;

        public GetAllItemSpecs(ILogger<RequestCargoPlan> logger)
        {
            log = logger;
        }
        
        [Function("GetAllItemSpecs")]
        public IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "get-all-item-specs")] HttpRequest req)
        {
            try {
                log.LogInformation("GetAllItemSpecs function triggered.");
                var allElements = new List<ItemSpecs>();

                var tableServiceClient = new TableServiceClient(ConnectionString);
                var tableClient = tableServiceClient.GetTableClient(
                    tableName: TableName
                );

                var allElementsPageable = tableClient.Query<ItemSpecs>(x => x.PartitionKey == PartitionKey);

                foreach (var item in allElementsPageable)
                {
                    allElements.Add(item);
                }

                var sortedElements = allElements.OrderBy(x => x.NSN).Reverse();

                return new OkObjectResult(sortedElements);

            } catch (Exception e) {
                log.LogError(e, $"Unexpected error getting {TableName} info.");

                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
