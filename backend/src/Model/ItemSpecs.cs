using Azure;
using Azure.Data.Tables;

namespace CargoMaker.Model
{
    public record ItemSpecs : ITableEntity
    {
        public string PartitionKey { get; set; } = default!;
        public string RowKey { get; set; } = default!;
        public ETag ETag { get; set; } = default!;
        public DateTimeOffset? Timestamp { get; set; } = default!;
        public string NSN { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Weight { get; set; } = default!;
        public string Length { get; set; } = default!;
        public string Width { get; set; } = default!;
        public string Height { get; set; } = default!;
        public bool CannotFlipOnSide { get; set; } = default!;
    }
}