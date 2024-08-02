using Azure;
using Azure.Data.Tables;

namespace CargoMaker.Model
{
    public record DestinationInfo : ITableEntity
    {
        public string PartitionKey { get; set; } = default!;
        public string RowKey { get; set; } = default!;
        public ETag ETag { get; set; } = default!;
        public DateTimeOffset? Timestamp { get; set; } = default!;
        public string Name { get; set; } = default!;
        public int MaximumWeight { get; set; } = default!;
    }
}