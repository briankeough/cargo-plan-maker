namespace CargoMaker.Config {
    public static class ContainerConfigs {

        public static readonly ContainerConfig ISU90 =  new () { 
            Width = 96,
            Depth = 40, //this is the inside depth dimension of one side of the ISU90
            Height = 84,
            WeightLimit = 10000
        };

        public static readonly ContainerConfig Pallet463LSingle = new () {
            Width = 104,
            Depth = 84,
            Height = 90,
            WeightLimit = 10000
        };

        public static readonly ContainerConfig Pallet463LDouble = new () {
            Width = 104,
            Depth = 168,
            Height = 90,
            WeightLimit = 20000
        };
    }

    public class ContainerConfig {

        public decimal Width {get; set;} = default!;
        public decimal Depth {get; set;} = default!;
        public decimal Height {get; set;} = default!;
        public decimal WeightLimit {get; set;} = default!;

        public static ContainerConfig ContainerWithWeightLimitOverride(ContainerConfig containerConfig, decimal weightLimitOverride) {
            return new ContainerConfig {
                Width = containerConfig.Width,
                Depth = containerConfig.Depth,
                Height = containerConfig.Height,
                WeightLimit = weightLimitOverride,
            };
        }
    }
}