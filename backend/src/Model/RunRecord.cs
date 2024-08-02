using Azure;
using Azure.Data.Tables;

namespace CargoMaker.Model
{
    public record RunRecord : ITableEntity
    {
        public string PartitionKey { get; set; } = default!;
        public string RowKey { get; set; } = default!;
        public ETag ETag { get; set; } = default!;
        public DateTimeOffset? Timestamp { get; set; } = default!;
        public string Requestor { get; set; } = default!;
        public RunStatus Status { get; set; } = default!;
        public string InputFile { get; set; } = default!;
        public string PlanFile { get; set; } = default!;
        public string ItemsToLoad { get; set; } = default!;
        public string ContainerLimits { get; set; } = default!;
        public string? Destination { get; set; } = default!;
    }
}