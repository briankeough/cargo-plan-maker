using CargoMaker.Comparer;
using CargoMaker.Model;

namespace CargoMaker.Utils {
    public static class SpaceUtils {
        
        public static bool ItemFitsInSpace(decimal itemX, decimal itemY, decimal itemZ, decimal spaceX, decimal spaceY, decimal spaceZ)  {
            return itemX <= spaceX && itemY <= spaceY && itemZ <= spaceZ;
        }
        
    }
}