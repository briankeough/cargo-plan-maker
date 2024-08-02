using Microsoft.Extensions.Logging;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using CargoMaker.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;


namespace CargoMaker.Api
{
    public class GetDownloadTokenConfig
    {
        private static readonly string ConnectionString = Environment.GetEnvironmentVariable("STORAGE_ACCOUNT_CONNECTION_STRING")!;
        private static readonly string TableName = "sastoken";
        private static readonly string PartitionKey = "main";

        private readonly ILogger<RequestCargoPlan> log;

        public GetDownloadTokenConfig(ILogger<RequestCargoPlan> logger)
        {
            log = logger;
        }
        
        [Function("GetDownloadTokenConfig")]
        public IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "get-download-token-config")] HttpRequest req)
        {
            try {
                var tableServiceClient = new TableServiceClient(ConnectionString);
                var tableClient = tableServiceClient.GetTableClient(
                    tableName: TableName
                );

                var allElementsPageable = tableClient.Query<DownloadToken>(x => x.PartitionKey == PartitionKey);

                var tokenConfig = new DownloadTokenConfig();

                foreach (var item in allElementsPageable)
                {
                    if (item.RowKey == "input-file") {
                        tokenConfig.InputFile = item.Token;
                    }
                    else if (item.RowKey == "cargo-plan") {
                        tokenConfig.PlanFile = item.Token;
                    }
                }
                return new OkObjectResult(tokenConfig);
                
            } catch (Exception e){
                log.LogError(e, $"Unexpected error getting plan file.");

                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
