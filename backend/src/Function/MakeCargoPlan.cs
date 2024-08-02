using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using CargoMaker.Model;
using Azure;
using ClosedXML.Excel;
using CargoMaker.Utils;


namespace CargoMaker.Function
{
    public class MakeCargoPlan
    {
        private static readonly string ConnectionString = Environment.GetEnvironmentVariable("STORAGE_ACCOUNT_CONNECTION_STRING")!;
        private static readonly string OutputFileContainerName = "cargo-plans";
        private static readonly string RunsTableName = "runs";
        private static readonly string RunsTablePartitionKey = "main";

        private readonly ILogger<MakeCargoPlan> log;

        public MakeCargoPlan(ILogger<MakeCargoPlan> logger)
        {
            log = logger;
        }

        [Function("MakeCargoPlan")]
        public async Task Run(
            [QueueTrigger("run-requests", Connection = "STORAGE_ACCOUNT_CONNECTION_STRING")] string runRequestId)
        {
            var runRecord = new RunRecord();

            try {
                log.LogInformation($"MakeCargoPlan function triggered for run-request with id: {runRequestId}");

                runRecord = await GetRunRecordForRequest(runRequestId);

                log.LogInformation($"Run request found for requestor: {runRecord.Requestor}");

                var cargoLoadOutPlan = CargoPlanBuilder.CreateLoadOutPlan(runRecord, log);
                var cargoPlanXlWorkbook = ExcelUtils.CreateExcelWorkbookFromCargoPlan(cargoLoadOutPlan);
                var cargPlanFileName = $"{ DateTimeOffset.Now:yyyyMMddHHmmss}_cargoplan.xlsx";

                await UploadCargoPlanToStorage(cargoPlanXlWorkbook, cargPlanFileName);
                await UpdateRunRecordAsCompleted(runRecord, cargPlanFileName);
                
            } catch (Exception e){
                log.LogError(e, "Unexpected error making plan");
                await UpdateRunRecordAsFailed(runRecord);
            }
        }

        private static TableClient GetRunsTableClient() {
            var tableServiceClient = new TableServiceClient(ConnectionString);
            
            return tableServiceClient.GetTableClient(
                tableName: RunsTableName
            );
        }

        private static async Task<RunRecord> GetRunRecordForRequest(string runRequestId){
            var runsTableClient = GetRunsTableClient();

            return await runsTableClient.GetEntityAsync<RunRecord>(RunsTablePartitionKey, runRequestId);
        }

        private static async Task UploadCargoPlanToStorage(XLWorkbook cargoPlanXlWorkbook, string cargPlanFileName) {
            var outputFileBlobClient = new BlobContainerClient(ConnectionString, OutputFileContainerName);
            var outputFile = outputFileBlobClient.GetBlobClient(cargPlanFileName);
            
            var outputMemorystream = new MemoryStream();
            cargoPlanXlWorkbook.SaveAs(outputMemorystream);

            outputMemorystream.Position = 0;
            await outputFile.UploadAsync(outputMemorystream);
        }

        private async Task UpdateRunRecordAsCompleted(RunRecord runRecord, string cargPlanFileName) {
            
            if (runRecord == null || runRecord.PartitionKey == null) {
                log.LogError("Run record cannot be updated because it is null");
                return;
            }
            
            var runsTableClient = GetRunsTableClient();

            var updatedRunRecord = new RunRecord()
            {
                PartitionKey = runRecord.PartitionKey,
                RowKey = runRecord.RowKey,
                InputFile = runRecord.InputFile,
                Requestor = runRecord.Requestor,
                ContainerLimits = runRecord.ContainerLimits,
                Destination = runRecord.Destination,
                ItemsToLoad = runRecord.ItemsToLoad,
                PlanFile = cargPlanFileName,
                Status = RunStatus.Completed
            };
            _ = await runsTableClient.UpdateEntityAsync<RunRecord>(updatedRunRecord, ETag.All);
        }

        private async Task UpdateRunRecordAsFailed(RunRecord runRecord) {
            
            if (runRecord == null || runRecord.PartitionKey == null) {
                log.LogError("Run record cannot be updated because it is null");
                return;
            }
            
            var runsTableClient = GetRunsTableClient();

            var updatedRunRecord = new RunRecord()
            {
                PartitionKey = runRecord.PartitionKey,
                RowKey = runRecord.RowKey,
                InputFile = runRecord.InputFile,
                Requestor = runRecord.Requestor,
                ContainerLimits = runRecord.ContainerLimits,
                Destination = runRecord.Destination,
                ItemsToLoad = runRecord.ItemsToLoad,
                Status = RunStatus.Failed
            };
            _ = await runsTableClient.UpdateEntityAsync<RunRecord>(updatedRunRecord, ETag.All);
        }
    }
}
