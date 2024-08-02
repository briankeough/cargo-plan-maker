namespace CargoMaker.Model
{
    public class StackLayer {
        public decimal ZCoord {get; set;} = default!;
        public decimal Width {get; set;} = default!;
        public decimal Depth {get; set;} = default!;
        public decimal Height {get; set;} = default!;
        public decimal WeightLimit {get; set;} = default!; //used to determine weight limit for next layer to stack on this layer
        public bool IsStackableLayer {get; set;} = default!;
        public List<LoadedItem> LoadedItems {get; set;} = default!;
    }
}
