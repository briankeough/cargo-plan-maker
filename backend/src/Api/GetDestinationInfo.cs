using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using CargoMaker.Model;


namespace CargoMaker.Api
{
    public class GetDestinationInfo
    {
        private static readonly string ConnectionString = Environment.GetEnvironmentVariable("STORAGE_ACCOUNT_CONNECTION_STRING")!;
        private static readonly string TableName = "destinations";

        private readonly ILogger<RequestCargoPlan> log;

        public GetDestinationInfo(ILogger<RequestCargoPlan> logger)
        {
            log = logger;
        }
        
        [Function("GetDestinationInfo")]
        public IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "get-destination-info")] HttpRequest req)
        {
            try {
                log.LogInformation("GetDestinationInfo function triggered.");
                var allElements = new List<DestinationInfo>();

                var tableServiceClient = new TableServiceClient(ConnectionString);
                var tableClient = tableServiceClient.GetTableClient(
                    tableName: TableName
                );

                var allElementsPageable = tableClient.Query<DestinationInfo>(x => x.PartitionKey == "main");

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
