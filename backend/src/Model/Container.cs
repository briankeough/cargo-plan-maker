namespace CargoMaker.Model
{
    public class Container {
        public string Id {get; set;} = default!;
        public List<Compartment> Compartments {get; set;} = default!;
        public decimal Weight {get; set;} = default!;
    }
}
