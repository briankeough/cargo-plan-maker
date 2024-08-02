using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using CargoMaker.Model;
using Newtonsoft.Json;


namespace CargoMaker.Api
{
    public class GetRunHistory
    {
        private static readonly string ConnectionString = Environment.GetEnvironmentVariable("STORAGE_ACCOUNT_CONNECTION_STRING")!;
        private static readonly string TableName = "runs";
        private static readonly int ExpiredRunMinutes = 5;

        private readonly ILogger<RequestCargoPlan> log;

        public GetRunHistory(ILogger<RequestCargoPlan> logger)
        {
            log = logger;
        }
        
        [Function("GetRunHistory")]
        public IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "get-run-history")] HttpRequest req)
        {
            try {
                log.LogInformation("GetRunHistory function triggered.");
                var displayRecords = new List<RunRecordDisplay>();

                var tableServiceClient = new TableServiceClient(ConnectionString);
                var tableClient = tableServiceClient.GetTableClient(
                    tableName: TableName
                );

                var allElementsPageable = tableClient.Query<RunRecord>(x => x.PartitionKey == "main");

                foreach (var item in allElementsPageable)
                {
                    displayRecords.Add(new RunRecordDisplay {
                        Id = item.RowKey,
                        Requestor = item.Requestor,
                        Status = item.Status.ToString(),
                        Timestamp = item.Timestamp,
                        InputFile = item.InputFile,
                        PlanFile = item.PlanFile,
                        Destination = string.IsNullOrEmpty(item.Destination) ? null : JsonConvert.DeserializeObject<DestinationLimit>(item.Destination),
                        ContainerLimits = string.IsNullOrEmpty(item.ContainerLimits) ? null : JsonConvert.DeserializeObject<List<ContainerLimit>>(item.ContainerLimits),
                        ItemsToLoad = JsonConvert.DeserializeObject<List<ItemToLoad>>(item.ItemsToLoad)!,
                    });
                }

                var sortedDisplayRecords = displayRecords.OrderByDescending(x => x.Timestamp);

                foreach(var record in sortedDisplayRecords)
                {
                    if ((record.Status == RunStatus.Processing.ToString() || record.Status == RunStatus.Requested.ToString())
                        && DateTimeOffset.Compare(record.Timestamp.Value!, DateTimeOffset.Now.AddMinutes(- ExpiredRunMinutes)) <= 0 )
                    {
                        record.Status = RunStatus.Expired.ToString();
                    }
                }

                return new OkObjectResult(sortedDisplayRecords);

            } catch (Exception e){
                log.LogError(e, $"Unexpected error getting {TableName} info.");

                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
