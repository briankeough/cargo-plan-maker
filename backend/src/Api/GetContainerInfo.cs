using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using CargoMaker.Model;


namespace CargoMaker.Api
{
    public class GetContainerInfo
    {
        private static readonly string ConnectionString = Environment.GetEnvironmentVariable("STORAGE_ACCOUNT_CONNECTION_STRING")!;
        private static readonly string TableName = "containers";

        private readonly ILogger<RequestCargoPlan> log;

        public GetContainerInfo(ILogger<RequestCargoPlan> logger)
        {
            log = logger;
        }
        
        [Function("GetContainerInfo")]
        public IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "get-container-info")] HttpRequest req)
        {
            try {
                log.LogInformation("GetContainerInfo function triggered.");
                var allElements = new List<ContainerInfo>();

                var tableServiceClient = new TableServiceClient(ConnectionString);
                var tableClient = tableServiceClient.GetTableClient(
                    tableName: TableName
                );

                var allElementsPageable = tableClient.Query<ContainerInfo>(x => x.PartitionKey == "main");

                foreach (var item in allElementsPageable)
                {
                    allElements.Add(item);
                }

                var sortedElements = allElements.OrderByDescending(x => x.Timestamp);
                return new OkObjectResult(sortedElements);

            } catch (Exception e){
                log.LogError(e, $"Unexpected error getting {TableName} info.");

                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
