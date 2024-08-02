using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using CargoMaker.Model;
using System.Text;
using Newtonsoft.Json;


namespace CargoMaker.Api
{
    public class SubmitDestinationInfo
    {
        private static readonly string ConnectionString = Environment.GetEnvironmentVariable("STORAGE_ACCOUNT_CONNECTION_STRING")!;
        private static readonly string TableName = "destinations";
        private static readonly string PartitionKey = "main";


        private readonly ILogger<SubmitDestinationInfo> log;

        public SubmitDestinationInfo(ILogger<SubmitDestinationInfo> logger)
        {
            log = logger;
        }
        
        [Function("SubmitDestinationInfo")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "submit-destination-info")] HttpRequest req)
        {
            try {
                string postBodyJsonString;
                
                using (StreamReader readStream = new(req.Body, Encoding.UTF8))
                {
                    postBodyJsonString = await readStream.ReadToEndAsync();

                    if (postBodyJsonString == null) {
                        return new BadRequestObjectResult(new {message = "no post body provided"});
                    }
                }

                DestinationInfo? destinationInfo = JsonConvert.DeserializeObject<DestinationInfo>(postBodyJsonString);

                if (destinationInfo == null) {
                    return new BadRequestObjectResult(new {message = "json body in incorrect format"});
                }
                
                log.LogInformation($"Submitting to table storage: {postBodyJsonString}");

                var tableServiceClient = new TableServiceClient(ConnectionString);
                var tableClient = tableServiceClient.GetTableClient(
                    tableName: TableName
                );
                var id = Guid.NewGuid().ToString();

                destinationInfo.PartitionKey = PartitionKey;
                destinationInfo.RowKey = id;

                await tableClient.AddEntityAsync<DestinationInfo>(destinationInfo);

                return new OkObjectResult(new {id = id, status = "success"});

            } catch (Exception e){
                log.LogError(e, $"Unexpected error submitting to {TableName} info.");

                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
