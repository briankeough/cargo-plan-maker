using Azure;
using Azure.Data.Tables;

namespace CargoMaker.Model
{
    public record DestinationLimit
    {
        public string Name { get; set; } = default!;
        public int MaximumWeight { get; set; } = default!;
    }
}