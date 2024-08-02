using CargoMaker.Model;

namespace CargoMaker.Comparer {
    public class CompartmentSectionHeightComparer : Comparer<CompartmentSection>
    {
        public override int Compare(CompartmentSection? x, CompartmentSection? y)
        {
            if (x?.Height == y?.Height) {
                return 0;
            } else if (x?.Height > y?.Height) {
                return 1;
            } else {
                return -1;
            }
        }
    }
}