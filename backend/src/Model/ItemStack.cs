namespace CargoMaker.Model
{
    public class ItemStack {
        public decimal XCoord {get; set;} = default!;
        public decimal Width {get; set;} = default!;
        public decimal YCoord {get; set;} = default!;
        public decimal Depth {get; set;} = default!;
        public decimal Height {get; set;} = default!;
        public decimal Weight {get; set;} = default!;
        public List<StackLayer> StackLayers {get; set;} = default!;
        public decimal FitScore {get; set;} = default!;

        public Line GetStackFrontFaceLine() {
            return new Line {
                LineStartCoord = XCoord,        //x coord start
                LineEndCoord = XCoord + Width,  //x coord end
                PositionCoord = YCoord + Depth  //y coord
            };
        }
    }
}
