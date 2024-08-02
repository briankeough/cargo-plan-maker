using CargoMaker.Model;

namespace CargoMaker.Comparer {
    public class WeightItemComparer : Comparer<ItemToLoad>
    {
        public override int Compare(ItemToLoad? x, ItemToLoad? y)
        {
            var xWeight = x!.Weight;
            var yWeight = y!.Weight;

            if (x?.NSN == y?.NSN) {
                return 0;
            }
            if (xWeight == yWeight) {

                var surfaceAreaItemComparer = new SurfaceAreaItemComparer();
                return surfaceAreaItemComparer.Compare(x, y);

            } else if (xWeight > yWeight) {
                return -1;
            } else {
                return 1;
            }
        }
    }
}