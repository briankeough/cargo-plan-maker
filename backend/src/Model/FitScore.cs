namespace CargoMaker.Model
{
    public class FitScore {
        public decimal DepthFitScore {get; set;} = default!; 
        public decimal WidthFitScore {get; set;} = default!;         
        public decimal HeightFitScore {get; set;} = default!; 
        public decimal VolumeFitScore {get; set;} = default!; 
        public decimal SurfaceAreaFitScore {get; set;} = default!;
    }
}
