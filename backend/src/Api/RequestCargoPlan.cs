using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using Azure.Data.Tables;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using CargoMaker.Model;
using Newtonsoft.Json;
using CargoMaker.Utils;


namespace CargoMaker.Api
{
    public class RequestCargoPlan
    {
        private static readonly string ConnectionString = Environment.GetEnvironmentVariable("STORAGE_ACCOUNT_CONNECTION_STRING")!;
        private static readonly string InputFileContainerName = "input-files";
        private static readonly string RunsTableName = "runs";
        private static readonly string ItemSpecTableName = "itemspecs";
        private static readonly string DestinationsTableName = "destinations";
        private static readonly string PartitionKey = "main";
        private static readonly string RunRequestsQueueName = "run-requests";
        private static readonly string NsnCol = "H";
        private static readonly string QuantityCol = "K";
        private static readonly string LocationCol = "O";
        private static readonly string NSNheaderName = "STOCK NUMBER";


        private readonly ILogger<RequestCargoPlan> log;

        public RequestCargoPlan(ILogger<RequestCargoPlan> logger)
        {
            log = logger;
        }
        
        [Function("RequestCargoPlan")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "make-plan")] HttpRequest req)
        {
            try {
                log.LogInformation("RequestMakeCargoPlan function triggered.");

                var formdata = await req.ReadFormAsync();
                var file = req.Form.Files["file"];
                var requestor = req.Form["requestor"];
                var destination = req.Form["destination"];

                if (file == null || string.IsNullOrEmpty(requestor)) {
                    return new OkObjectResult(new {
                        status = "nigo",
                        validationError = "Input file and requestor are required"
                    });
                }

                log.LogInformation($"{ file?.FileName } recieved. File size: { file?.Length.ToString() }");

                var worksheet = ExcelUtils.GetExcelWorksheetFromFormFile(log, file!);
                log.LogInformation($"Excel file row count: { worksheet.RowCount }");

                var rowCount = 0;
                var startRow = 1;
                var maxSpacerRows = 10;
                var spacerRowCount = 0;

                var itemsWithNoSpecs = new List<string>();
                var inputNsnsMissing = new List<string>();
                var itemIgoNsns = new List<string>();
                var inputFileNsns = new List<string>();
                var duplicateNsns = new List<string>();
                var itemsToLoad = new List<RequestedItemToLoad>();

                foreach (var row in worksheet.Rows()) {

                    if (spacerRowCount > maxSpacerRows) {
                        log.LogInformation("Breaking loop due to spacer row max exceeded");
                        break;
                    }

                    if (rowCount >= startRow) {

                        var nsn = row.Cell(NsnCol) == null ?  "" : row.Cell(NsnCol).GetValue<string>();
                        var qty = row.Cell(QuantityCol) == null ?  "" : row.Cell(QuantityCol).GetValue<string>();
                        var location = row.Cell(LocationCol) == null ?  "" : row.Cell(LocationCol).GetValue<string>();

                        if (string.IsNullOrEmpty(nsn)) {
                            spacerRowCount++;
                        } 
                        else if (string.Equals(NSNheaderName, nsn, StringComparison.OrdinalIgnoreCase)) {
                            continue;
                            
                        } else {
                            spacerRowCount = 0;

                            if (!inputFileNsns.Contains(nsn)) {
                                inputFileNsns.Add(nsn);
                            }

                            if (!string.IsNullOrEmpty(qty))
                            {
                                try {
                                    int.Parse(qty);
                                } catch (Exception e){
                                    log.LogInformation(e, $"Qty could not be parsed at row: {row.RowNumber}, col: {QuantityCol}");
                                    continue;
                                }

                                if (itemIgoNsns.Contains(nsn)) {
                                    duplicateNsns.Add(nsn);
                                }
                                else {
                                    itemIgoNsns.Add(nsn);
                                }

                                itemsToLoad.Add(new RequestedItemToLoad{
                                    NSN = nsn,
                                    Qty = int.Parse(qty),
                                    Location = location
                                });
                            }
                        }
                    }
                    rowCount++;
                }

                var itemSpecsDictionary = new Dictionary<string, ItemSpecs>();

                var tableServiceClient = new TableServiceClient(ConnectionString);
                var itemSpecsTableClient = tableServiceClient.GetTableClient(
                    tableName: ItemSpecTableName
                );

                var allElementsPageable = itemSpecsTableClient.Query<ItemSpecs>(x => x.PartitionKey == PartitionKey);

                foreach (var item in allElementsPageable)
                {
                    itemSpecsDictionary.Add(item.NSN, item);
                }

                foreach(var n in itemIgoNsns) {
                    if (!itemSpecsDictionary.ContainsKey(n)) {
                        itemsWithNoSpecs.Add(n);
                    }
                }

                foreach(var n in inputFileNsns) {
                    if (!itemIgoNsns.Contains(n)) {
                        inputNsnsMissing.Add(n);
                    }
                }

                if (itemsWithNoSpecs.Count > 0 || inputNsnsMissing.Count > 0 || duplicateNsns.Count > 0) {
                    return new OkObjectResult(new {
                        status = "nigo",
                        itemsWithNoSpecs,
                        inputNsnsMissing,
                        duplicateNsns
                    });
                }

                foreach(var item in itemsToLoad) {
                    itemSpecsDictionary.TryGetValue(item.NSN, out var itemSpecs);

                    item.Name = itemSpecs!.Name;
                    item.Weight = itemSpecs!.Weight;
                    item.Length = itemSpecs!.Length;
                    item.Width = itemSpecs!.Width;
                    item.Height = itemSpecs!.Height;
                    item.CannotFlipOnSide = itemSpecs!.CannotFlipOnSide;
                }

                var inputFileName = $"{ DateTimeOffset.Now.ToString("yyyyMMddHHmmss") }_{ file?.FileName }";

                //upload input file
                var blobClient = new BlobContainerClient(ConnectionString, InputFileContainerName);
                var blob = blobClient.GetBlobClient(inputFileName);
                await blob.UploadAsync(file?.OpenReadStream());

                //insert run record
                var runsTableClient = tableServiceClient.GetTableClient(
                    tableName: RunsTableName
                );

                var runId = Guid.NewGuid().ToString();
                log.LogInformation($"Generated row key: {runId}");

                var containerLimits = new List<ContainerLimit>();
                DestinationLimit? destinationLimit = null;

                if (!string.IsNullOrEmpty(destination) && destination != "NONE") {
                    var destinationsTableClient = tableServiceClient.GetTableClient(
                        tableName: DestinationsTableName
                    );

                    var destinationsInfoList = destinationsTableClient.Query<DestinationInfo>(x => x.PartitionKey == "main");

                    foreach (var dest in destinationsInfoList)
                    {
                        if (dest.Name == destination){
                            destinationLimit = new DestinationLimit{
                                Name = dest.Name,
                                MaximumWeight = dest.MaximumWeight
                            };
                        }
                    }
                }

                var runRecord = new RunRecord()
                {
                    PartitionKey = PartitionKey,
                    RowKey = runId,
                    Requestor = requestor.ToString(),
                    InputFile = inputFileName,
                    Status = RunStatus.Processing,
                    ContainerLimits = JsonConvert.SerializeObject(containerLimits),
                    Destination = JsonConvert.SerializeObject(destinationLimit),
                    ItemsToLoad = JsonConvert.SerializeObject(itemsToLoad)
                };
                await runsTableClient.AddEntityAsync<RunRecord>(runRecord);

                var queueClient = new QueueClient(ConnectionString, RunRequestsQueueName, new QueueClientOptions
                {
                    MessageEncoding = QueueMessageEncoding.Base64
                });
                await queueClient.SendMessageAsync(runId); //insert run request to queue

                return new OkObjectResult(new {status = "success"});

            } catch (Exception e){
                log.LogError(e, "Unexpected error requesting plan");

                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
