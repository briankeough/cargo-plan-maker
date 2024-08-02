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
    public class DeleteContainerInfo
    {
        private static readonly string ConnectionString = Environment.GetEnvironmentVariable("STORAGE_ACCOUNT_CONNECTION_STRING")!;
        private static readonly string TableName = "containers";
        private static readonly string PartitionKey = "main";


        private readonly ILogger<DeleteContainerInfo> log;

        public DeleteContainerInfo(ILogger<DeleteContainerInfo> logger)
        {
            log = logger;
        }
        
        [Function("DeleteContainerInfo")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "delete-container-info")] HttpRequest req)
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

                ContainerInfo? containerInfo = JsonConvert.DeserializeObject<ContainerInfo>(postBodyJsonString);

                if (containerInfo == null) {
                    return new BadRequestObjectResult(new {message = "json body in incorrect format"});
                }
                
                log.LogInformation($"Deleting from to table storage: {containerInfo.RowKey}");

                var tableServiceClient = new TableServiceClient(ConnectionString);
                var tableClient = tableServiceClient.GetTableClient(
                    tableName: TableName
                );
                
                await tableClient.DeleteEntityAsync(PartitionKey, containerInfo.RowKey);

                return new OkObjectResult(new {id = containerInfo.RowKey, status = "success"});

            } catch (Exception e){
                log.LogError(e, $"Unexpected error deleting from {TableName}.");

                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
