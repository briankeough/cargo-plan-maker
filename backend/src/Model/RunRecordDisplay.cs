using Azure;

namespace CargoMaker.Model
{
    public record RunRecordDisplay
    {
        public string Id { get; set; } = default!;
        public DateTimeOffset? Timestamp { get; set; } = default!;
        public string Requestor { get; set; } = default!;
        public string Status { get; set; } = default!;
        public string InputFile { get; set; } = default!;
        public string PlanFile { get; set; } = default!;
        public List<ItemToLoad> ItemsToLoad { get; set; } = default!;
        public List<ContainerLimit>? ContainerLimits { get; set; } = default!;
        public DestinationLimit? Destination { get; set; } = default!;
    }
}