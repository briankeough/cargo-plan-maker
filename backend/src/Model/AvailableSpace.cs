namespace CargoMaker.Model
{
    public class AvailableSpace {

        public decimal Height {get; set;} = default!;
        public decimal Width {get; set;} = default!;
        public decimal Depth {get; set;} = default!;
        public decimal XCoord {get; set;} = default!;
        public decimal YCoord {get; set;} = default!;
        public decimal ZCoord {get; set;} = default!;
        public decimal WeightLimit {get; set;} = default!;
        public TargetSpace TargetSpace {get; set;} = default!;

        public override bool Equals(Object? obj)
        {
            if (obj == null || !(obj is AvailableSpace)) {
                return false;
            }
            else {
                var other = (AvailableSpace) obj;
                return 
                    this.Height == other.Height && 
                    this.Width == other.Width && 
                    this.Depth == other.Depth && 
                    this.XCoord == other.XCoord && 
                    this.YCoord == other.YCoord && 
                    this.ZCoord == other.ZCoord && 
                    this.WeightLimit == other.WeightLimit && 
                    this.TargetSpace == other.TargetSpace;
            }
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Height, Width, Depth, XCoord, YCoord, ZCoord, WeightLimit, TargetSpace);
        }

    }
}
