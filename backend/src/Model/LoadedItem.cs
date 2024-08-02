namespace CargoMaker.Model
{
    public class LoadedItem {
        public ItemToLoad ItemToLoad {get; set;} = default!;
        public decimal XCoord {get; set;} = default!;
        public decimal YCoord {get; set;} = default!;
        public decimal ZCoord {get; set;} = default!;
        public decimal XDim {get; set;} = default!;
        public decimal YDim {get; set;} = default!;
        public decimal ZDim {get; set;} = default!;
        public string DesmosCoords {get; set;} = default!;
    }
}
