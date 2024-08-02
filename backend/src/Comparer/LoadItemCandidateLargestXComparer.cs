using CargoMaker.Model;

namespace CargoMaker.Comparer {
    public class LoadItemCandidateLargestXComparer : Comparer<LoadItemCandidate>
    {
        public override int Compare(LoadItemCandidate? x, LoadItemCandidate? y)
        {
            if (x?.XDim == y?.XDim) {
                return 0;
            } else if (x?.XDim > y?.XDim) {
                return 1;
            } else {
                return -1;
            }
        }
    }
}