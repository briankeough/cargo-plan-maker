using CargoMaker.Model;

namespace CargoMaker.Comparer {
    public class AvailableSpaceStackTopSpaceComparer : Comparer<AvailableSpace>
    {
        public override int Compare(AvailableSpace? x, AvailableSpace? y)
        {
            if (x?.XCoord == y?.XCoord) {
                
                if (x?.YCoord < y?.YCoord) {
                    return -1;
                } 
                if (x?.YCoord > y?.YCoord) {
                    return 1;
                }

                var xSurfaceArea = x?.Depth * x?.Width;
                var ySurfaceArea = y?.Depth * y?.Width;

                if (xSurfaceArea > ySurfaceArea) {
                    return -1;
                }
                if (xSurfaceArea < ySurfaceArea) {
                    return 1;
                }

                if (x?.Width > y?.Width) {
                    return -1;
                }
                if (x?.Width < y?.Width) {
                    return 1;
                }

                if (x?.Height > y?.Height) {
                    return -1;
                }
                if (x?.Height < y?.Height) {
                    return 1;
                }

                return 0;
            }
            if (x?.XCoord < y?.XCoord) {
                return -1;
                
            } 
            else {
                return 1;
            }
        }
    }
}