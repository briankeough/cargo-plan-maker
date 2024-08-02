using CargoMaker.Model;

namespace CargoMaker.Comparer {
    public class LineStartComparer : Comparer<Line>
    {
        public override int Compare(Line? x, Line? y)
        {
            if (x?.LineStartCoord == y?.LineStartCoord) {
                return 0;
            }
            if (x?.LineStartCoord < y?.LineStartCoord) {
                return -1;
                
            } else {
                return 1;
            }
        }
    }
}