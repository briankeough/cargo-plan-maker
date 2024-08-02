namespace CargoMaker.Model
{
    public class RequestedItemToLoad {
        public string NSN {get; set;} = default!;
        public string Name {get; set;} = default!;
        public int Qty {get; set;} = default!;
        public string Location {get; set;} = default!;
        public string Weight { get; set; } = default!;
        public string Length { get; set; } = default!;
        public string Width { get; set; } = default!;
        public string Height { get; set; } = default!;
        public bool CannotFlipOnSide { get; set; } = default!;
    }
}
