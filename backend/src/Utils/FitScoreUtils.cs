using CargoMaker.Model;

namespace CargoMaker.Utils {

    public static class FitScoreUtils {

        public static FitScore CalculateFitScore(decimal itemX, decimal itemY, decimal itemZ, decimal spaceX, decimal spaceY, decimal spaceZ) {

            var itemSurfaceArea = itemX * itemY;
            var itemVolume = itemSurfaceArea * itemZ;

            var spaceSurfaceArea = spaceX * spaceY;
            var spaceVolume = spaceSurfaceArea * spaceZ;

            return new FitScore {
                VolumeFitScore = Math.Abs(spaceVolume - itemVolume),
                SurfaceAreaFitScore = Math.Abs(spaceSurfaceArea - itemSurfaceArea),
                WidthFitScore = Math.Abs(spaceX - itemX),
                DepthFitScore = Math.Abs(spaceY - itemY),
                HeightFitScore = Math.Abs(spaceZ - itemZ)
            };
        }

        public static bool CandidateIsBetterFitThanOtherCandidate(LoadItemCandidate candidate, LoadItemCandidate? otherCandidate) {
           
            var candidateTotal2dScore = (99 * candidate.FitScore.SurfaceAreaFitScore) + (99 * candidate.FitScore.DepthFitScore) + (99 * candidate.FitScore.WidthFitScore);
            var otherCandidateTotal2dScore = (99 * otherCandidate?.FitScore.SurfaceAreaFitScore) + (99 * otherCandidate?.FitScore.DepthFitScore) + (99 * otherCandidate?.FitScore.WidthFitScore);
            
            if (candidateTotal2dScore == otherCandidateTotal2dScore) {
                return candidate.ZDim > otherCandidate?.ZDim;
            }

            return candidateTotal2dScore < otherCandidateTotal2dScore;
        }
    }
}