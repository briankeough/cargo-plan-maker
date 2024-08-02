using CargoMaker.Model;

namespace CargoMaker.Comparer {
    public class SurfaceAreaItemComparer : Comparer<ItemToLoad>
    {
        public override int Compare(ItemToLoad? x, ItemToLoad? y)
        {
            var xWeight = x!.Weight;
            var xDepth = x!.Depth;
            var xWidth = x!.Width;
            var xHeight = x!.Height;

            var yWeight = y!.Weight;
            var yDepth = y!.Depth;
            var yWidth = y!.Width;
            var yHeight = y!.Height;

            if (x!.NSN == y!.NSN) {
                return 0;
            }
            var xSurfaceArea = xDepth * xWidth;
            var ySurfaceArea = yDepth * yWidth;
            
            if (xSurfaceArea == ySurfaceArea) {
                
                if (xWeight == yWeight) {

                    var xSquareRatio = Math.Abs(xDepth - xWidth);
                    var ySquareRatio = Math.Abs(yDepth - yWidth);

                    if (xSquareRatio == ySquareRatio) {
                        if (xHeight == yHeight) { 
                            return 0; 
                        } else if (xHeight > yHeight) {
                            return -1;
                        } else {
                            return 1;
                        }
                    } else if (xSquareRatio < ySquareRatio) {
                        return -1;
                    } else {
                        return 1;
                    }

                } else if (xWeight > yWeight) {
                    return -1;
                } else {
                    return 1;
                }
            } else if (xSurfaceArea > ySurfaceArea) {
                return -1;
            } else {
                return 1;
            }
        }
    }
}