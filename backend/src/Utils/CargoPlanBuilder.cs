using CargoMaker.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using CargoMaker.Config;
using CargoMaker.Builder;
using CargoMaker.Comparer;

namespace CargoMaker.Utils
{
    public static class CargoPlanBuilder {

        public static CargoPlan CreateLoadOutPlan(RunRecord runRecord, ILogger log) {

            var itemsToLoad = JsonConvert.DeserializeObject<List<RequestedItemToLoad>>(runRecord.ItemsToLoad)!;
            var destinationLimit = JsonConvert.DeserializeObject<DestinationLimit>(runRecord.Destination);
            var weightLimitOverride = destinationLimit != null ? destinationLimit.MaximumWeight : -1;

            var isu90Items = new List<ItemToLoad>();
            var singlePalletItems = new List<ItemToLoad>();
            var doublePalletItems = new List<ItemToLoad>();
            var unloadedItems = new List<ItemToLoad>();

            var singlePalletContainerConfig = weightLimitOverride > -1 ? 
                ContainerConfig.ContainerWithWeightLimitOverride(ContainerConfigs.Pallet463LSingle, weightLimitOverride) : ContainerConfigs.Pallet463LSingle;

            var doublePalletContainerConfig = weightLimitOverride > -1 ? 
                ContainerConfig.ContainerWithWeightLimitOverride(ContainerConfigs.Pallet463LDouble, weightLimitOverride * 2) : ContainerConfigs.Pallet463LDouble;

            foreach (var item in itemsToLoad) {

                //if source location value starts with 82 (e.g. 820AA) it is an ISU90 item - this logic came from the user when building prototype. update if needed.
                if (item.Location.StartsWith("82") && ItemFitsInContainer(ContainerConfigs.ISU90, item)) 
                { 
                    isu90Items.AddRange(ConvertRequestedItemToLoad(item));
                } 
                else if (ItemFitsInContainer(singlePalletContainerConfig, item))
                { 
                    singlePalletItems.AddRange(ConvertRequestedItemToLoad(item));
                }
                else if (ItemFitsInContainer(doublePalletContainerConfig, item))
                {
                    doublePalletItems.AddRange(ConvertRequestedItemToLoad(item));
                }
                else {
                    unloadedItems.AddRange(ConvertRequestedItemToLoad(item));
                }
            }

            isu90Items.Sort(new SurfaceAreaItemComparer());
            singlePalletItems.Sort(new SurfaceAreaItemComparer());
            doublePalletItems.Sort(new SurfaceAreaItemComparer());

            var isu90ContainerBuilder = new ISU90ContainerBuilder(); 
            var palletBuilder = new PalletBuilder();

            var isu90Containers = isu90ContainerBuilder.BuildContainers(log, ContainerConfigs.ISU90, isu90Items);
            var palletSingleContainers = palletBuilder.BuildContainers(log, singlePalletContainerConfig, singlePalletItems);
            var palletDoubleContainers = palletBuilder.BuildContainers(log, doublePalletContainerConfig, doublePalletItems);

            unloadedItems.AddRange(isu90Items);
            unloadedItems.AddRange(singlePalletItems);
            unloadedItems.AddRange(doublePalletItems);

            return new CargoPlan {
                Isu90Containers = isu90Containers,
                PalletSingleContainers = palletSingleContainers,
                PalletDoubleContainers = palletDoubleContainers,
                UnloadedItems = unloadedItems
            };
        }

        private static bool ItemFitsInContainer(ContainerConfig containerConfig, RequestedItemToLoad itemToLoad) {

            var width = decimal.Parse(itemToLoad.Width);
            var depth = decimal.Parse(itemToLoad.Length);
            var height = decimal.Parse(itemToLoad.Height);
            var weight = decimal.Parse(itemToLoad.Weight);

            return 
                weight < containerConfig.WeightLimit &&
                height <= containerConfig.Height &&
                (
                    (width <= containerConfig.Width && depth <= containerConfig.Depth) || 
                    (width <= containerConfig.Depth && depth <= containerConfig.Width)
                );
        }

        private static List<ItemToLoad> ConvertRequestedItemToLoad(RequestedItemToLoad requestedItemsToLoad) {

            var itemsToLoad = new List<ItemToLoad>();
                
            for (int q=0; q < requestedItemsToLoad.Qty; q++) {
                itemsToLoad.Add(new ItemToLoad() {
                    NSN = requestedItemsToLoad.NSN,
                    Name = requestedItemsToLoad.Name,
                    Depth = decimal.Parse(requestedItemsToLoad.Length),
                    Width = decimal.Parse(requestedItemsToLoad.Width),
                    Height = decimal.Parse(requestedItemsToLoad.Height),
                    Weight = decimal.Parse(requestedItemsToLoad.Weight),
                    Location = requestedItemsToLoad.Location,
                    Qty = requestedItemsToLoad.Qty,
                    CannotFlipOnSide = requestedItemsToLoad.CannotFlipOnSide,
                });
            }

            return itemsToLoad;
        }       
    }
}