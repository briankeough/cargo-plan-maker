using Azure;
using Azure.Data.Tables;

namespace CargoMaker.Model
{
    public record DownloadToken : ITableEntity
    {
        public string PartitionKey { get; set; } = default!;
        public string RowKey { get; set; } = default!;
        public ETag ETag { get; set; } = default!;
        public DateTimeOffset? Timestamp { get; set; } = default!;
        public string Token { get; set; } = default!;
    }
}