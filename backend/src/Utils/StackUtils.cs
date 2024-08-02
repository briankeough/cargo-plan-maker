using CargoMaker.Comparer;
using CargoMaker.Model;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace CargoMaker.Utils {
    public static class StackUtils {

        public static bool TryFindBestFitOnStack(
            decimal maxHeight, 
            ItemStack stack,
            decimal totalWeightLimitOfStack,
            List<ItemToLoad> itemsThatCanGoOnStack, 
            List<ItemToLoad> otherItemListToCheckWhenRemovingItems) 
        {
            var topLayerOnStackToBuildOn = stack.StackLayers[^1]; 

            if (!topLayerOnStackToBuildOn.IsStackableLayer) {
                return false;
            }

            topLayerOnStackToBuildOn.WeightLimit = topLayerOnStackToBuildOn.WeightLimit < totalWeightLimitOfStack ? topLayerOnStackToBuildOn.WeightLimit : totalWeightLimitOfStack;

            var loadItemCandidate = LoadItemCandidateUtils.FindBestFittingItemForSpace(
                stack.Depth, 
                stack.Width, 
                maxHeight - stack.Height, 
                stack.Depth,
                stack.Width, 
                topLayerOnStackToBuildOn.WeightLimit,
                itemsThatCanGoOnStack);

            if (loadItemCandidate != null) {

                var b = true;
                if (loadItemCandidate.ItemToLoad.NSN == "5895012108603EW") {
                    b = false;
                }

                PutSingleItemOnStackInNewLayer(stack, topLayerOnStackToBuildOn, loadItemCandidate);

                if (loadItemCandidate.YDim < (topLayerOnStackToBuildOn.Depth * .6M)) { //if stack layer has reasonably more space (e.g. 40%), add more items to layer
                
                    var newStackLayer = stack.StackLayers[^1]; //newly added layer

                    while(TryAddBestFittingItemToStackLayerOpenSpace(
                        maxHeight - stack.Height + loadItemCandidate.ZDim, //the remaining height from the bottom of the new layer to maxHeight
                        newStackLayer, 
                        topLayerOnStackToBuildOn,
                        stack,
                        itemsThatCanGoOnStack,
                        otherItemListToCheckWhenRemovingItems)){}

                    newStackLayer.IsStackableLayer = StackLayerIsStackable(newStackLayer);
                }
                
                LoadItemCandidateUtils.RemoveItemFromLists(loadItemCandidate, [itemsThatCanGoOnStack, otherItemListToCheckWhenRemovingItems]);

                return true;
            } 
            else {
                return false;
            }
        }

        public static bool TryAddBestFittingItemToStackLayerOpenSpace(
            decimal maxHeight,
            StackLayer stackLayerToAddTo, 
            StackLayer stackLayerBaseForAvailableSpace,
            ItemStack stack,
            List<ItemToLoad> itemsToUse,
            List<ItemToLoad> otherItemListToCheckWhenRemovingItems) 
        {

            if (stackLayerToAddTo.WeightLimit > stackLayerBaseForAvailableSpace.WeightLimit) {
                return false;
            }

            var availableSpaces = CompartmentUtils.GetAvailableSpacesFromLoadedItemsInSpace(
                stackLayerToAddTo.LoadedItems,
                stackLayerBaseForAvailableSpace.Width,
                stackLayerBaseForAvailableSpace.Depth,
                maxHeight,
                stackLayerBaseForAvailableSpace.WeightLimit - stackLayerToAddTo.WeightLimit
            );

            foreach (var availableSpace in availableSpaces) {
                    
                var bestItemThatFitsInSpace = LoadItemCandidateUtils.FindBestFittingItemForSpace(
                    availableSpace.Depth,
                    availableSpace.Width,
                    availableSpace.Height,
                    availableSpace.TargetSpace != null ? availableSpace.TargetSpace.Depth : availableSpace.Depth,
                    availableSpace.TargetSpace != null ? availableSpace.TargetSpace.Width : availableSpace.Width,
                    availableSpace.WeightLimit,
                    itemsToUse
                );

                if (bestItemThatFitsInSpace != null && 
                    SpaceUtils.ItemFitsInSpace(
                        bestItemThatFitsInSpace.XDim, 
                        bestItemThatFitsInSpace.YDim, 
                        bestItemThatFitsInSpace.ZDim,
                        stackLayerToAddTo.Width, 
                        stackLayerToAddTo.Depth, 
                        maxHeight))
                {
                    var xCoord = availableSpace.XCoord + stack.XCoord;
                    var yCoord = availableSpace.YCoord + stack.YCoord;
                    var zCoord = stackLayerToAddTo.ZCoord;

                    var xDim = bestItemThatFitsInSpace.XDim;
                    var yDim = bestItemThatFitsInSpace.YDim;
                    var zDim = bestItemThatFitsInSpace.ZDim;

                    stackLayerToAddTo.LoadedItems.Add(
                        new () {
                            ItemToLoad = bestItemThatFitsInSpace.ItemToLoad,
                            XDim = xDim,
                            YDim = yDim,
                            ZDim = zDim,
                            XCoord = xCoord,
                            YCoord = yCoord,
                            ZCoord = zCoord,
                            DesmosCoords = DesmosUtils.GetDesmosCoords(xCoord, yCoord, zCoord, xDim, yDim, zDim)
                        }
                    );
                    
                    stackLayerToAddTo.WeightLimit += bestItemThatFitsInSpace.ItemToLoad.Weight;
                    stack.Weight += bestItemThatFitsInSpace.ItemToLoad.Weight;
    
                    LoadItemCandidateUtils.RemoveItemFromLists(bestItemThatFitsInSpace, [itemsToUse, otherItemListToCheckWhenRemovingItems]);
                    
                    return true;
                }
            }
            return false;
        }

        public static ItemStack? FindBestFittingStackForSpace(
            AvailableSpace availableSpace,
            List<ItemToLoad> itemsToUseToBuildStack,
            List<ItemToLoad> otherItemListToCheckWhenRemovingItems)
        {
            var maxDepth = availableSpace.Depth;
            var maxWidth = availableSpace.Width;
            var maxHeight = availableSpace.Height;
            var targetDepth = availableSpace.TargetSpace != null ? availableSpace.TargetSpace.Depth : availableSpace.Depth;
            var targetWidth = availableSpace.TargetSpace != null ? availableSpace.TargetSpace.Width : availableSpace.Width;

            ItemStack? bestItemStack = null;
            ItemStack? currentItemStack;

            LoadItemCandidate? itemInBestRotation;

            foreach (var itemToLoad in itemsToUseToBuildStack) {
                
                var itemWidth = itemToLoad.Width;
                var itemDepth = itemToLoad.Depth;
                var itemHeight = itemToLoad.Height;
                var itemWeight = itemToLoad.Weight;

                itemInBestRotation = LoadItemCandidateUtils.GetBestLoadItemCandidateForSpace(
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

                if (itemInBestRotation == null || itemInBestRotation.ItemToLoad.Weight > availableSpace.WeightLimit) { //doesn't fit, or is too heavy
                    continue;
                } 

                currentItemStack = CreateNewStackFromItem(itemInBestRotation, availableSpace.XCoord, availableSpace.YCoord);

                var copyOfitemsAvailableToBuildStack = new List<ItemToLoad>(itemsToUseToBuildStack);
                var copyOfotherItemListToCheckWhenRemovingItems = new List<ItemToLoad>(otherItemListToCheckWhenRemovingItems);
                
                LoadItemCandidateUtils.RemoveItemFromLists(itemInBestRotation, [copyOfitemsAvailableToBuildStack, copyOfotherItemListToCheckWhenRemovingItems]);

                BuildOntoStackToHeightMax(
                    currentItemStack, 
                    availableSpace.WeightLimit,
                    copyOfitemsAvailableToBuildStack,
                    copyOfotherItemListToCheckWhenRemovingItems,
                    maxHeight); 
                
                if (StackIsBetterThanBestStackForSpace(
                    currentItemStack,
                    bestItemStack,
                    targetDepth,
                    targetWidth,
                    maxHeight
                )) {
                    bestItemStack = currentItemStack;
                }
            }
            
            return bestItemStack;
        }

        private static bool StackIsBetterThanBestStackForSpace(
            ItemStack itemStack, 
            ItemStack? bestItemStack,
            decimal targetDepth,
            decimal targetWidth,
            decimal targetHeight)
        {
            if (bestItemStack == null) {
                return true;
            }

            var itemStackTargetDepthDifference = Math.Abs(targetDepth - itemStack.Depth) * 99;
            var bestItemStackTargetDepthDifference = Math.Abs(targetDepth - bestItemStack.Depth) * 99;

            var itemStackTargetWidthDifference = Math.Abs(targetWidth - itemStack.Width) * 89;
            var bestItemStackTargetWidthDifference = Math.Abs(targetWidth - bestItemStack.Width) * 89;

            var itemStackTargetHeightDifference = Math.Abs(targetHeight - itemStack.Height) * 89;
            var bestItemStackTargetHeightDifference = Math.Abs(targetHeight - bestItemStack.Height) * 89;

            var itemStackOverallDifference = itemStackTargetDepthDifference + itemStackTargetWidthDifference + itemStackTargetHeightDifference;
            var bestStackOverallDifference = bestItemStackTargetDepthDifference + bestItemStackTargetWidthDifference + bestItemStackTargetHeightDifference;

            if (itemStackOverallDifference < bestStackOverallDifference) {
                return true;
            }

            return false;
        }
        
        public static void BuildOntoStackWithinHeightRange(
            ItemStack stack, 
            decimal totalWeightLimitOfStack,
            List<ItemToLoad> itemsThatCanGoOnStack,
            List<ItemToLoad> otherItemListToCheckWhenRemovingItems, 
            decimal heightMin, 
            decimal heightMax) 
        { 
            while (TryFindBestFitOnStack(
                heightMax, 
                stack, 
                totalWeightLimitOfStack - stack.Weight,
                itemsThatCanGoOnStack, 
                otherItemListToCheckWhenRemovingItems)) 
            {
                if (stack.Height >= heightMin) {
                    break;
                }
            }
        }

        public static void BuildOntoStackToHeightMax(
            ItemStack stack,
            decimal totalWeightLimitOfStack,
            List<ItemToLoad> itemsThatCanGoOnStack,
            List<ItemToLoad> otherItemListToCheckWhenRemovingItems,
            decimal heightMax) 
        {    
            while (TryFindBestFitOnStack(
                heightMax, 
                stack,
                totalWeightLimitOfStack - stack.Weight,
                itemsThatCanGoOnStack, 
                otherItemListToCheckWhenRemovingItems)) {}
        }

        public static ItemStack CreateNewStackFromItem(LoadItemCandidate loadItem, decimal xCoord, decimal yCoord) {
            
            return new () {
                XCoord = xCoord,
                YCoord = yCoord,
                Width = loadItem.XDim,
                Depth = loadItem.YDim,
                Height = loadItem.ZDim,
                Weight = loadItem.ItemToLoad.Weight,
                StackLayers = [
                    new () {
                        Depth = loadItem.YDim,
                        Width = loadItem.XDim,
                        Height = loadItem.ZDim,
                        ZCoord = 0,
                        IsStackableLayer = true,
                        WeightLimit = loadItem.ItemToLoad.Weight,
                        LoadedItems = [
                            new () {
                                ItemToLoad = loadItem.ItemToLoad,
                                XCoord = xCoord,
                                YCoord = yCoord,
                                ZCoord = 0,
                                XDim = loadItem.XDim,
                                YDim = loadItem.YDim,
                                ZDim = loadItem.ZDim,
                                DesmosCoords = DesmosUtils.GetDesmosCoords(xCoord, yCoord, 0, loadItem.XDim, loadItem.YDim, loadItem.ZDim)
                            }
                        ]
                    }
                ]
            };
        }

        public static void PutSingleItemOnStackInNewLayer (ItemStack stack, StackLayer topLayerOnStackToBuildOn, LoadItemCandidate loadItem) {
            var xCoord = stack.XCoord;
            var yCoord = stack.YCoord;
            var zCoord = topLayerOnStackToBuildOn.ZCoord + topLayerOnStackToBuildOn.Height;

            var xDim = loadItem.XDim;
            var yDim = loadItem.YDim;
            var zDim = loadItem.ZDim;

            stack.StackLayers.Add(new () {
                Width = xDim,
                Depth = yDim,
                Height = zDim,
                ZCoord = zCoord,
                IsStackableLayer = true,
                WeightLimit = loadItem.ItemToLoad.Weight,
                LoadedItems = [
                    new () {
                        ItemToLoad = loadItem.ItemToLoad,
                        XDim = xDim,
                        YDim = yDim,
                        ZDim = zDim,
                        XCoord = xCoord,
                        YCoord = yCoord,
                        ZCoord = zCoord,
                        DesmosCoords = DesmosUtils.GetDesmosCoords(xCoord, yCoord, zCoord, xDim, yDim, zDim)
                    }
                ]
            });

            stack.Height += zDim;
            stack.Weight += loadItem.ItemToLoad.Weight;
        }

        public static bool StackLayerIsStackable(StackLayer stackLayer) {
            decimal itemHeight = -1;
            
            foreach (var item in stackLayer.LoadedItems) {

                if (itemHeight == -1) {
                    itemHeight = item.ZDim;
                }
                if (itemHeight != item.ZDim) {
                    return false;
                }
            };
            return true;
        }
        
    }
}