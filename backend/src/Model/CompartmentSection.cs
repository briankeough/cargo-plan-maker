namespace CargoMaker.Model
{
    public class CompartmentSection {

        public string Id {get; set;} = default!;
        public decimal Height {get; set;} = default!;
        public decimal Width {get; set;} = default!;
        public decimal Depth {get; set;} = default!;
        public decimal Weight {get; set;} = default!;
        public decimal WeightLimit {get; set;} = default!;
        public List<ItemStack> Stacks {get; set;} = default!;
        public List<AvailableSpace> AvailableSpaces {get; set;} = default!;
        public decimal LevelingDepth {get; set;} = default!;

        public void AddStackToSection(ItemStack stack) {
            Stacks ??= new ();

            Stacks.Add(stack);
            Weight += stack.Weight;
        }

        public List<LoadedItem> GetLoadedItems () {
            var loadedItems = new List<LoadedItem> ();

            if (Stacks == null) {
                return loadedItems;
            }
            
            foreach (var stack in Stacks) {
                foreach (var layer in stack.StackLayers) {
                    loadedItems.AddRange(layer.LoadedItems);
                }
            }
            return loadedItems;
        }

        public decimal GetHeightOfTallestItemStack () {
            decimal height = 0;
            
            foreach (var stack in Stacks) {
                if (stack.Height > height) {
                    height = stack.Height;
                }
            }

            return height;
        }
    }
}
