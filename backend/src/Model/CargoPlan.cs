namespace CargoMaker.Model
{
    public class CargoPlan {
        public List<Container> Isu90Containers {get; set;} = default!;
        public List<Container> PalletSingleContainers {get; set;} = default!;
        public List<Container> PalletDoubleContainers {get; set;} = default!;
        public List<ItemToLoad> UnloadedItems {get; set;} = default!;
    }
}
