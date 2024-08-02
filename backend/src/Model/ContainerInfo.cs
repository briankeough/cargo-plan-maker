using Azure;
using Azure.Data.Tables;

namespace CargoMaker.Model
{
    public record ContainerInfo : ITableEntity
    {
        public string PartitionKey { get; set; } = default!;
        public string RowKey { get; set; } = default!;
        public ETag ETag { get; set; } = default!;
        public DateTimeOffset? Timestamp { get; set; } = default!;
        public string Name { get; set; } = default!;
        public int MaximumWeight { get; set; } = default!;
        public int MaximumLength { get; set; } = default!;
        public int MaximumWidth { get; set; } = default!;
        public int MaximumHeight { get; set; } = default!;
    }
}