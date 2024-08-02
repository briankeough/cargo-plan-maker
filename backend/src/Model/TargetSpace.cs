namespace CargoMaker.Model
{
    public class TargetSpace {

        public decimal Height {get; set;} = default!;
        public decimal Width {get; set;} = default!;
        public decimal Depth {get; set;} = default!;

        public override bool Equals(Object? obj)
        {
            if (obj == null || !(obj is TargetSpace)) {
                return false;
            }
            else {
                var other = (TargetSpace) obj;
                return 
                    this.Height == other.Height && 
                    this.Width == other.Width && 
                    this.Depth == other.Depth;
            }
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Height, Width, Depth);
        }
    }
}
