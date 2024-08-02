using CargoMaker.Config;
using CargoMaker.Model;
using CargoMaker.Utils;
using Microsoft.Extensions.Logging;
using Container = CargoMaker.Model.Container;

namespace CargoMaker.Builder
{
    /// <summary>
    /// Builds 463L (single and double) pallets using best fitting stack builder algorithm.
    /// </summary>
    public class PalletBuilder : IContainerBuilder {

        public List<Container> BuildContainers(ILogger log, ContainerConfig containerConfig, List<ItemToLoad> itemsToLoad)
        { 
            var containers = new List<Container>();
            var container = new Container();

            foreach (var item in itemsToLoad) {
                item.CannotFlipOnSide = true; //for pallets, set all the items to not flip on side
            }

            while (container != null) {
                
                container = BuildContainer(containerConfig, itemsToLoad);

                if (container != null) {
                    containers.Add(container);
                }   
            }
            return containers;
        }

        private static Container? BuildContainer(ContainerConfig containerConfig, List<ItemToLoad> itemsToLoad) 
        {

            var mainSection = new CompartmentSection {
                Id = "PALLET",
                Depth = containerConfig.Depth,
                Height = containerConfig.Height,
                Width = containerConfig.Width,
                Weight = 0,
                WeightLimit = containerConfig.WeightLimit,
                Stacks = []
            };

            while(CompartmentUtils.TryAddBestFittingStackToSectionOpenSpace(mainSection, itemsToLoad)) {}

            if (mainSection.Stacks.Count == 0) {
                return null;
            }

            return new Container {
                Compartments = [
                    new () {
                        Id = "",
                        Sections = [ mainSection ],
                        Weight = mainSection.Weight
                    } 
                ],
                Weight = mainSection.Weight
            };
        }
                   
    }
}