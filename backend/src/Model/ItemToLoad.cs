namespace CargoMaker.Model
{
    public class ItemToLoad {
        public string NSN {get; set;} = default!;
        public string Name {get; set;} = default!;
        public decimal Weight { get; set; } = default!;
        public decimal Depth { get; set; } = default!;
        public decimal Width { get; set; } = default!;
        public decimal Height { get; set; } = default!;
        public bool CannotFlipOnSide { get; set; } = default!;
        public int Qty {get; set;} = default!;
        public string Location { get; set; } = default!;
    }
}
