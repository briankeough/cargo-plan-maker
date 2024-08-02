using CargoMaker.Model;

namespace CargoMaker.Comparer {
    public class AvailableSpaceComparer : Comparer<AvailableSpace>
    {
        public override int Compare(AvailableSpace? x, AvailableSpace? y)
        {
            if (x?.YCoord == y?.YCoord) {
                if (x?.XCoord <= y?.YCoord) {
                    return -1;
                } 
                else {
                    return 1;
                }
            }
            if (x?.YCoord < y?.YCoord) {
                return -1;
                
            } else {
                return 1;
            }
        }
    }
}