namespace CargoMaker.Model
{
    public class Compartment {
        public string Id {get; set;} = default!;
        public decimal Depth {get; set;} = default!;
        public decimal Width {get; set;} = default!;
        public decimal Height {get; set;} = default!;
        public decimal Weight {get; set;} = default!;
        public List<CompartmentSection> Sections {get; set;} = default!;
        public List<CompartmentDivider> Dividers {get; set;} = default!;
    }

}
