using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using CargoMaker.Model;
using Azure.Data.Tables;
using CargoMaker.Utils;


namespace CargoMaker.Api
{
    public class SubmitItemSpecs
    {
        private static readonly string ConnectionString = Environment.GetEnvironmentVariable("STORAGE_ACCOUNT_CONNECTION_STRING")!;
        private static readonly string TableName = "itemspecs";
        private static readonly string PartitionKey = "main";
        private static readonly string NsnCol = "B";
        private static readonly string NameCol = "C";
        private static readonly string WeightCol = "E";
        private static readonly string LengthCol = "F";
        private static readonly string WidthCol = "G";
        private static readonly string HeightCol = "H";
        private static readonly string CannotFlipOnSideCol = "I";

        private readonly ILogger<SubmitItemSpecs> log;

        public SubmitItemSpecs(ILogger<SubmitItemSpecs> logger)
        {
            log = logger;
        }
        
        [Function("SubmitItemSpecs")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "submit-item-specs")] HttpRequest req)
        {
            try {
                log.LogInformation("SubmitItemSpecs function triggered.");

                var formdata = await req.ReadFormAsync();
                var file = req.Form.Files["file"];

                if (file == null){
                    log.LogError("file is null");
                }

                log.LogInformation($"{ file?.FileName } recieved. File size: { file?.Length.ToString() }");

                var worksheet = ExcelUtils.GetExcelWorksheetFromFormFile(log, file!);

                log.LogInformation($"Excel file row count: { worksheet.RowCount }");

                var rowCount = 0;
                var startRow = 1;
                var maxSpacerRows = 10;
                var spacerRowCount = 0;

                var incompleteItems = new List<string>();
                var nigoFormatItems = new List<string>();
                var duplicateNsns = new List<string>();
                var itemSpecsDictionary = new Dictionary<string, ItemSpecs>();
                var nsns = new HashSet<string>();

                foreach (var row in worksheet.Rows()) {

                    if (spacerRowCount > maxSpacerRows) {
                        log.LogInformation("Breaking loop due to spacer row max exceeded");
                        break;
                    }

                    if (rowCount >= startRow) {

                        var nsn = row.Cell(NsnCol) == null ?  "" : row.Cell(NsnCol).GetValue<string>();
                        var name = row.Cell(NameCol) == null ?  "" : row.Cell(NameCol).GetValue<string>();
                        var weight = row.Cell(WeightCol) == null ?  "" : row.Cell(WeightCol).GetValue<string>();
                        var length = row.Cell(LengthCol) == null ?  "" : row.Cell(LengthCol).GetValue<string>();
                        var width = row.Cell(WidthCol) == null ?  "" : row.Cell(WidthCol).GetValue<string>();
                        var height = row.Cell(HeightCol) == null ?  "" : row.Cell(HeightCol).GetValue<string>();
                        var cannotFlipOnSide = row.Cell(CannotFlipOnSideCol) == null ? false : row.Cell(CannotFlipOnSideCol).GetValue<string>().Equals("x");

                        if (string.IsNullOrEmpty(nsn)) {
                            spacerRowCount++;
                        
                        } else {
                            spacerRowCount = 0;

                            if (string.IsNullOrEmpty(name) 
                                || string.IsNullOrEmpty(weight) 
                                || string.IsNullOrEmpty(length)
                                || string.IsNullOrEmpty(width)
                                || string.IsNullOrEmpty(height) 
                            ) {
                                incompleteItems.Add(nsn);

                            } else {
                                try {
                                    decimal.Parse(weight);
                                    decimal.Parse(length);
                                    decimal.Parse(width);
                                    decimal.Parse(height);

                                    if (itemSpecsDictionary.ContainsKey(nsn)) {
                                        itemSpecsDictionary.TryGetValue(nsn, out ItemSpecs? itemSpecs);

                                        if (itemSpecs?.Weight != weight
                                            || itemSpecs?.Length != length
                                            || itemSpecs?.Width != width
                                            || itemSpecs?.Height != height
                                        ) {
                                            duplicateNsns.Add(nsn);
                                        }
                                    } else {
                                        itemSpecsDictionary.Add(nsn, new ItemSpecs()
                                        {
                                            PartitionKey = PartitionKey,
                                            RowKey = Guid.NewGuid().ToString(),
                                            NSN = nsn,
                                            Name = name,
                                            Weight = weight,
                                            Length = length,
                                            Width = width,
                                            Height = height,
                                            CannotFlipOnSide = cannotFlipOnSide
                                        });
                                    }
                                } catch (Exception e){
                                    log.LogInformation(e, "Could not parse item spec values.");
                                    nigoFormatItems.Add(nsn);
                                }
                            }
                        }
                    }
                    rowCount++;
                }

                if (incompleteItems.Count > 0 || nigoFormatItems.Count > 0 || duplicateNsns.Count > 0) {
                    return new OkObjectResult(new {
                        status = "nigo",
                        incompleteItems,
                        nigoFormatItems,
                        duplicateNsns
                    });
                }

                log.LogInformation($"Submitting to table storage: {itemSpecsDictionary.Count} items");

                var tableServiceClient = new TableServiceClient(ConnectionString);
                var tableClient = tableServiceClient.GetTableClient(
                    tableName: TableName
                );

                var allItems = tableClient.Query<ItemSpecs>(x => x.PartitionKey == PartitionKey);
                foreach (var item in allItems)
                {
                    await tableClient.DeleteEntityAsync(PartitionKey, item.RowKey);
                }

                foreach (var itemSpecs in itemSpecsDictionary.Values) {
                    _ = await tableClient.AddEntityAsync<ItemSpecs>(itemSpecs);
                }
                
                return new OkObjectResult(new {
                    status = "success", 
                    count = itemSpecsDictionary.Count
                });

            } catch (Exception e) {
                log.LogError(e, "Unexpected error submitting item specs");

                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
