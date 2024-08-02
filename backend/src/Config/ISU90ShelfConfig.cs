namespace CargoMaker.Config
{
    public static class ISU90ShelfConfig {
        public static readonly decimal ShelfMinHeight = 12;
        public static readonly decimal ShelfMaxHeight = 35;
        public static readonly decimal Width = ContainerConfigs.ISU90.Width / 2;
        public static readonly decimal Depth = ContainerConfigs.ISU90.Depth;
        public static readonly decimal ShelfThickness = 1;
        public static readonly decimal ShelfToCeilingBuffer = 1;
        public static readonly decimal CenterDividerWallThickness = 1;
    }   
}