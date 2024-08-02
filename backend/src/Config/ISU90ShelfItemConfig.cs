namespace CargoMaker.Config
{
    public static class ISU90ShelfItemConfig {
        public static readonly decimal ItemMaxWeight = 30;
        public static readonly decimal ShelfOnlyItemDimThreshold = 12; //items that should only go on shelf
        public static readonly decimal ShelfItemDimThreshold = 17; //items that can go on shelf or not on shelf
    }   
}