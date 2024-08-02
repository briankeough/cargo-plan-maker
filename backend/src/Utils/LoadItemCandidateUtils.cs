using CargoMaker.Model;

namespace CargoMaker.Utils {
    public static class LoadItemCandidateUtils {

        public static LoadItemCandidate? FindBestFittingItemForSpace(
            decimal maxDepth, 
            decimal maxWidth, 
            decimal maxHeight, 
            decimal targetDepth, 
            decimal targetWidth,
            decimal weightLimit,
            List<ItemToLoad> itemsToLoad)
        {
            LoadItemCandidate? bestLoadItemCandidate = null;
            LoadItemCandidate? currentLoadItemCandidate;

            foreach (var itemToLoad in itemsToLoad) {
                
                var itemWidth = itemToLoad.Width;
                var itemDepth = itemToLoad.Depth;
                var itemHeight = itemToLoad.Height;
                var itemWeight = itemToLoad.Weight;

                 if (itemWeight > weightLimit + 1) { //too heavy
                    continue;
                }

                currentLoadItemCandidate = GetBestLoadItemCandidateForSpace(
                    itemWidth, 
                    itemDepth, 
                    itemHeight, 
                    maxWidth, 
                    maxDepth, 
                    maxHeight,
                    targetDepth,
                    targetWidth,
                    itemToLoad
                );

                if (currentLoadItemCandidate != null && CandidateIsBetterFitThanOtherCandidate(currentLoadItemCandidate, bestLoadItemCandidate)) {
                    bestLoadItemCandidate = currentLoadItemCandidate;
                }   
            }    
            
            return bestLoadItemCandidate;
        }

        public static LoadItemCandidate? CreateLoadItemCandidateFromItemFittingInSection(ItemToLoad itemToLoad, CompartmentSection section) {
            
            var loadItemCandidate = GetBestLoadItemCandidateForSpace(
                itemToLoad.Width, 
                itemToLoad.Depth,
                itemToLoad.Height, 
                section.Width,
                section.Depth, 
                section.Height,
                section.Depth,
                section.Width,
                itemToLoad
            );
            
            return loadItemCandidate;
        }

        private static List<LoadItemCandidate> GetLoadItemCandidatesWithAnyRotation(
            decimal itemWidth, 
            decimal itemDepth, 
            decimal itemHeight, 
            decimal spaceMaxX, 
            decimal spaceMaxY, 
            decimal spaceMaxZ, 
            decimal spaceTargetX,
            decimal spaceTargetY) 
        {

            List<LoadItemCandidate> candidates = [];

            var wdh = GetLoadItemCandidate(itemWidth, itemDepth, itemHeight, spaceMaxX, spaceMaxY, spaceMaxZ, spaceTargetX, spaceTargetY); //wdh
            var whd = GetLoadItemCandidate(itemWidth, itemHeight, itemDepth, spaceMaxX, spaceMaxY, spaceMaxZ, spaceTargetX, spaceTargetY); //whd
            var dwh = GetLoadItemCandidate(itemDepth, itemWidth, itemHeight, spaceMaxX, spaceMaxY, spaceMaxZ, spaceTargetX, spaceTargetY); //dwh
            var dhw = GetLoadItemCandidate(itemDepth, itemHeight, itemWidth, spaceMaxX, spaceMaxY, spaceMaxZ, spaceTargetX, spaceTargetY); //dhw
            var hwd = GetLoadItemCandidate(itemHeight, itemWidth, itemDepth, spaceMaxX, spaceMaxY, spaceMaxZ, spaceTargetX, spaceTargetY); //hwd
            var hdw = GetLoadItemCandidate(itemHeight, itemDepth, itemWidth, spaceMaxX, spaceMaxY, spaceMaxZ, spaceTargetX, spaceTargetY); //hdw

            if (wdh != null) {
                candidates.Add(wdh);
            }
            if (whd != null) {
                candidates.Add(whd);
            }
            if (dwh != null) {
                candidates.Add(dwh);
            }
            if (dhw != null) {
                candidates.Add(dhw);
            }
            if (hwd != null) {
                candidates.Add(hwd);
            }
            if (hdw != null) {
                candidates.Add(hdw);
            }

            return candidates;   
        }

        private static LoadItemCandidate? GetBestLoadItemCandidateWithAnyRotation(
            decimal itemWidth, 
            decimal itemDepth, 
            decimal itemHeight, 
            decimal spaceMaxX, 
            decimal spaceMaxY, 
            decimal spaceMaxZ, 
            decimal spaceTargetX,
            decimal spaceTargetY) 
        {
            return GetBestLoadItemCandidate(GetLoadItemCandidatesWithAnyRotation(
                itemWidth,
                itemDepth, 
                itemHeight, 
                spaceMaxX, 
                spaceMaxY, 
                spaceMaxZ, 
                spaceTargetX,
                spaceTargetY
            ));
        }

        private static List<LoadItemCandidate> GetLoadItemCandidatesNoFlipping(
            decimal itemWidth, 
            decimal itemDepth, 
            decimal itemHeight, 
            decimal spaceMaxX, 
            decimal spaceMaxY, 
            decimal spaceMaxZ, 
            decimal spaceTargetX,
            decimal spaceTargetY) 
        {
            
            List<LoadItemCandidate> candidates = [];

            var wdh = GetLoadItemCandidate(itemWidth, itemDepth, itemHeight, spaceMaxX, spaceMaxY, spaceMaxZ, spaceTargetX, spaceTargetY); //wdh
            var dwh = GetLoadItemCandidate(itemDepth, itemWidth, itemHeight, spaceMaxX, spaceMaxY, spaceMaxZ, spaceTargetX, spaceTargetY); //dwh

            if (wdh != null) {
                candidates.Add(wdh);
            }
            if (dwh != null) {
                candidates.Add(dwh);
            }

            return candidates;
        }

        public static LoadItemCandidate? GetBestLoadItemCandidateForSpace(
            decimal itemWidth, 
            decimal itemDepth, 
            decimal itemHeight, 
            decimal spaceMaxX, 
            decimal spaceMaxY, 
            decimal spaceMaxZ, 
            decimal spaceTargetX,
            decimal spaceTargetY,
            ItemToLoad itemToLoad) 
        {
            LoadItemCandidate? candidate;

            if (itemToLoad.CannotFlipOnSide) {
                candidate = GetBestLoadItemCandidateNoFlipping(
                    itemWidth, 
                    itemDepth, 
                    itemHeight, 
                    spaceMaxX, 
                    spaceMaxY, 
                    spaceMaxZ,
                    spaceTargetX,
                    spaceTargetY);

            } else {
                candidate = GetBestLoadItemCandidateWithAnyRotation(
                    itemWidth, 
                    itemDepth, 
                    itemHeight, 
                    spaceMaxX, 
                    spaceMaxY, 
                    spaceMaxZ,
                    spaceTargetX,
                    spaceTargetY);
            }

            if (candidate != null) {
                candidate.ItemToLoad = itemToLoad;
            }

            return candidate;
        }

        private static LoadItemCandidate? GetBestLoadItemCandidateNoFlipping(
            decimal itemWidth, 
            decimal itemDepth, 
            decimal itemHeight, 
            decimal spaceMaxX, 
            decimal spaceMaxY, 
            decimal spaceMaxZ, 
            decimal spaceTargetX,
            decimal spaceTargetY) 
        {
            return GetBestLoadItemCandidate(GetLoadItemCandidatesNoFlipping(
                itemWidth, 
                itemDepth, 
                itemHeight, 
                spaceMaxX, 
                spaceMaxY, 
                spaceMaxZ, 
                spaceTargetX,
                spaceTargetY)
            );
        }

        public static LoadItemCandidate? GetBestLoadItemCandidate(List<LoadItemCandidate> candidates) {
            LoadItemCandidate? bestLoadItemCandidate = null;

            foreach (var candidate in candidates) { 
                if (CandidateIsBetterFitThanOtherCandidate(candidate, bestLoadItemCandidate)) {
                    bestLoadItemCandidate = candidate;
                }
            }
            return bestLoadItemCandidate;
        }

        public static bool CandidateIsBetterFitThanOtherCandidate(LoadItemCandidate candidate, LoadItemCandidate? otherLoadItemCandidate) {
            return otherLoadItemCandidate is null || FitScoreUtils.CandidateIsBetterFitThanOtherCandidate(candidate, otherLoadItemCandidate);
        }

        public static LoadItemCandidate? GetLoadItemCandidate(
            decimal itemX, 
            decimal itemY, 
            decimal itemZ, 
            decimal spaceMaxX, 
            decimal spaceMaxY, 
            decimal spaceMaxZ,
            decimal spaceTargetX, 
            decimal spaceTargetY) 
        {
            LoadItemCandidate? loadItemCandidate = null;

            if (SpaceUtils.ItemFitsInSpace(itemX, itemY, itemZ, spaceMaxX, spaceMaxY, spaceMaxZ)) {
 
                loadItemCandidate = new() {
                    XDim = itemX,
                    YDim = itemY,
                    ZDim = itemZ,
                    FitScore = FitScoreUtils.CalculateFitScore(itemX, itemY, itemZ, spaceTargetX, spaceTargetY, spaceMaxZ)
                };
            }
            return loadItemCandidate;
        }

        public static void RemoveItemFromLists(LoadItemCandidate loadItemCandidate, List<List<ItemToLoad>> listOfListsOfItems) {
            RemoveItemFromLists(loadItemCandidate.ItemToLoad, listOfListsOfItems);
        }

        public static void RemoveItemFromLists(ItemToLoad itemToLoad, List<List<ItemToLoad>> listOfListsOfItems) {
            listOfListsOfItems.ForEach(list => list.Remove(itemToLoad));
        }

    }
}