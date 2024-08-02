namespace CargoMaker.Utils {
    public static class DesmosUtils {

        public static string GetDesmosCoords(decimal itemXCoord, decimal itemYCoord, decimal itemZCoord, decimal xDim, decimal yDim, decimal zDim) {
            return $"{ itemXCoord }<x<{ itemXCoord + xDim }\\left\\{{{ (itemYCoord + yDim) * -1 }<y<{ itemYCoord * -1 }\\right\\}}\\left\\{{{ itemZCoord }<z<{ itemZCoord + zDim }\\right\\}}";
        }
    }
}