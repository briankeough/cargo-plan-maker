namespace CargoMaker.Model
{
    public class LoadItemCandidate {
        public ItemToLoad ItemToLoad {get; set;} = default!;
        public decimal XDim {get; set;} = default!;
        public decimal YDim {get; set;} = default!;
        public decimal ZDim {get; set;} = default!;
        public FitScore FitScore {get; set;} = default!;
        
    }

}
