using CargoMaker.Comparer;
using CargoMaker.Config;
using CargoMaker.Model;
using CargoMaker.Utils;
using Microsoft.Extensions.Logging;
using Container = CargoMaker.Model.Container;

namespace CargoMaker.Builder
{
    /// <summary>
    /// Builds ISU90 containers using best fitting stack builder algorithm.
    /// Starts building bottom section up into the shelf area, then decides on whether or not add a shelf.
    /// Builds out shelf, then returns to building out bottom. 
    /// Repeat for all comparments.
    /// </summary>
    public class ISU90ContainerBuilder : IContainerBuilder {

        public List<Container> BuildContainers(ILogger log, ContainerConfig containerConfig, List<ItemToLoad> itemsToLoad)
        { 
            var containers = new List<Container>();
            var container = new Container();

            var topShelfItems = new List<ItemToLoad>();
            var bottomItems = new List<ItemToLoad>();
            var unloadedItems = new List<ItemToLoad>();

            foreach (var item in itemsToLoad) {
                if (ItemCanOnlyGoOnShelf(item)) { 
                    topShelfItems.Add(item);
                } 
                else if (ItemCanGoOnShelfOrBottom(item)) {
                    topShelfItems.Add(item);
                    bottomItems.Add(item);
                }
                else {
                    bottomItems.Add(item);
                }
            }

            while (container != null) {
                
                container = BuildContainer(containerConfig, topShelfItems, bottomItems);

                if (container != null) {
                    containers.Add(container);
                }
            }

            unloadedItems.AddRange(topShelfItems);
            unloadedItems.AddRange(bottomItems);
            itemsToLoad.RemoveAll(x => !unloadedItems.Contains(x));

            return containers;
        }

        private static Container? BuildContainer(
            ContainerConfig containerConfig,
            List<ItemToLoad> topShelfItems, 
            List<ItemToLoad> bottomItems)
        {

            if (topShelfItems.Count == 0 && bottomItems.Count == 0) {
                return null;
            }

            if (bottomItems.Count == 0 && topShelfItems.Count != 0) {
                bottomItems = topShelfItems;
            }

            var frontLeftCompartment = BuildCompartment(containerConfig, "A01", topShelfItems, bottomItems);
            var frontRightCompartment = BuildCompartment(containerConfig, "B01", topShelfItems, bottomItems);
            var backLeftCompartment = BuildCompartment(containerConfig, "C01", topShelfItems, bottomItems);
            var backRightCompartment = BuildCompartment(containerConfig, "D01", topShelfItems, bottomItems);

            FlipCoordinatesForRightSide(containerConfig, [ frontRightCompartment, backRightCompartment ]);
            FlipBackCoordinatesBackSide(containerConfig,  [ backLeftCompartment, backRightCompartment ]);
            
            return new Container {
                Compartments = [
                    frontLeftCompartment,
                    frontRightCompartment,
                    backLeftCompartment,
                    backRightCompartment
                ],
                Weight = frontLeftCompartment.Weight + frontRightCompartment.Weight + backLeftCompartment.Weight + backRightCompartment.Weight
            };
        }

        private static Compartment BuildCompartment(
            ContainerConfig containerConfig,
            string compartmentId, 
            List<ItemToLoad> topShelfItems,
            List<ItemToLoad> bottomItems) 
        {
            //NOTE: bottomItems and topShelfItems can contain duplicates of the same item (i.e. items that can go in either place). 
            //So this needs to be handled in deletion logic from the item list to make sure deletion attempts are done in both lists

            var topSection = new CompartmentSection();
            CompartmentDivider? shelf = null;

            var bottomSection = new CompartmentSection {
                Id = compartmentId + "A",
                Depth = containerConfig.Depth,
                Height = containerConfig.Height,
                Width = containerConfig.Width / 2,
                Weight = 0,
                WeightLimit = containerConfig.WeightLimit / 2
            };

            if (bottomItems.Count == 0) {
                return new Compartment () {
                    Sections = [],
                    Dividers = [],
                    Weight = 0
                };
            }

            var firstBottomLoadItemDefault = LoadItemCandidateUtils.CreateLoadItemCandidateFromItemFittingInSection(bottomItems[0], bottomSection);
            
            var firstBottomItemStack = StackUtils.CreateNewStackFromItem(firstBottomLoadItemDefault!, 0, 0);
            LoadItemCandidateUtils.RemoveItemFromLists(firstBottomLoadItemDefault!, [bottomItems, topShelfItems]);
            
            StackUtils.BuildOntoStackWithinHeightRange(
                firstBottomItemStack, 
                bottomSection.WeightLimit - bottomSection.Weight,
                bottomItems,
                topShelfItems,
                containerConfig.Height - ISU90ShelfConfig.ShelfMaxHeight,
                containerConfig.Height - ISU90ShelfConfig.ShelfToCeilingBuffer
            );

            if (topShelfItems.Count > 0) { //add shelf

                topSection = new CompartmentSection {
                    Id = compartmentId + "B",
                    Depth = containerConfig.Depth,
                    Height = containerConfig.Height - ISU90ShelfConfig.ShelfToCeilingBuffer - firstBottomItemStack.Height - ISU90ShelfConfig.ShelfThickness,
                    Width = containerConfig.Width / 2,
                    Weight = 0,
                    WeightLimit = containerConfig.WeightLimit / 2,
                    Stacks = []
                };

                LoadItemCandidate? shelfStartingItem = LoadItemCandidateUtils.CreateLoadItemCandidateFromItemFittingInSection(topShelfItems[0], topSection);

                if (shelfStartingItem != null) {

                    var firstShelfItemStack = StackUtils.CreateNewStackFromItem(shelfStartingItem!, 0, 0);
                    LoadItemCandidateUtils.RemoveItemFromLists(shelfStartingItem!, [topShelfItems, bottomItems]);

                    StackUtils.BuildOntoStackToHeightMax(
                        firstShelfItemStack, 
                        topSection.WeightLimit - topSection.Weight,
                        topShelfItems,
                        bottomItems,
                        topSection.Height
                    );

                    topSection.AddStackToSection(firstShelfItemStack);
                    CompartmentUtils.FillSectionWithStacks(topSection, topShelfItems, bottomItems, false);

                    CleanupTopSection(topSection, topShelfItems);

                    var condensedSectionHeight = topSection.GetHeightOfTallestItemStack() + ISU90ShelfConfig.ShelfToCeilingBuffer;
                    topSection.Height = condensedSectionHeight;
                    bottomSection.Height = containerConfig.Height - topSection.Height - ISU90ShelfConfig.ShelfThickness;

                    CompartmentUtils.ShiftZCoordsUpForItemsInSection(topSection, bottomSection.Height + ISU90ShelfConfig.ShelfThickness);
                    ResetStackInSectionBackDownToBaseLayer(firstBottomItemStack, bottomSection, bottomItems);

                    StackUtils.BuildOntoStackToHeightMax(
                        firstBottomItemStack, 
                        bottomSection.WeightLimit - bottomSection.Weight,
                        bottomItems,
                        topShelfItems,
                        bottomSection.Height
                    );

                    shelf = new CompartmentDivider() {
                        XCoord = 0,
                        YCoord = 0,
                        ZCoord = bottomSection.Height + ISU90ShelfConfig.ShelfThickness,
                        XDim = topSection.Width,
                        YDim = topSection.Depth,
                        ZDim = ISU90ShelfConfig.ShelfThickness,
                        DesmosCoords = DesmosUtils.GetDesmosCoords(
                            0, 
                            0, 
                            bottomSection.Height + ISU90ShelfConfig.ShelfThickness,
                            topSection.Width,
                            topSection.Depth,
                            ISU90ShelfConfig.ShelfThickness)
                    };
                }
            }
            else {

                ResetStackInSectionBackDownToBaseLayer(firstBottomItemStack, bottomSection, bottomItems);

                StackUtils.BuildOntoStackToHeightMax(
                    firstBottomItemStack, 
                    bottomSection.WeightLimit - bottomSection.Weight,
                    bottomItems,
                    topShelfItems,
                    bottomSection.Height
                );
            }

            bottomSection.AddStackToSection(firstBottomItemStack);
            CompartmentUtils.FillSectionWithStacks(bottomSection, bottomItems, topShelfItems, true);

            return new Compartment () {
                Sections = [
                    topSection,
                    bottomSection
                ],
                Dividers = shelf != null ? [shelf] : [],
                Weight = topSection.Weight + bottomSection.Weight
            };
        }

        private static bool ItemCanOnlyGoOnShelf(ItemToLoad itemToLoad) {

            var loadItemCandidate = LoadItemCandidateUtils.GetBestLoadItemCandidateForSpace(
                itemToLoad.Width, 
                itemToLoad.Depth, 
                itemToLoad.Height, 
                ISU90ShelfConfig.Width, 
                ISU90ShelfConfig.Depth, 
                ISU90ShelfItemConfig.ShelfOnlyItemDimThreshold,
                ISU90ShelfConfig.Width, 
                ISU90ShelfConfig.Depth, 
                itemToLoad);

            return loadItemCandidate != null && itemToLoad.Weight <= ISU90ShelfItemConfig.ItemMaxWeight;
        }

        private static bool ItemCanGoOnShelfOrBottom(ItemToLoad itemToLoad) {

            var loadItemCandidate = LoadItemCandidateUtils.GetBestLoadItemCandidateForSpace(
                itemToLoad.Width, 
                itemToLoad.Depth, 
                itemToLoad.Height, 
                ISU90ShelfConfig.Width, 
                ISU90ShelfConfig.Depth, 
                ISU90ShelfItemConfig.ShelfItemDimThreshold,
                ISU90ShelfConfig.Width, 
                ISU90ShelfConfig.Depth, 
                itemToLoad);

            return loadItemCandidate != null && itemToLoad.Weight <= ISU90ShelfItemConfig.ItemMaxWeight;
        }

        private static void FlipCoordinatesForRightSide(ContainerConfig containerConfig, List<Compartment> compartments) {

            foreach (Compartment compartment in compartments) {

                if (compartment == null || compartment.Sections == null || compartment.Sections.Count == 0) {
                    return;
                }

                foreach(var section in compartment.Sections) {

                    if (section == null || section.Stacks == null || section.Stacks.Count == 0) {
                        continue;
                    }

                    foreach(var stack in section.Stacks) {
                        stack.XCoord = containerConfig.Width - stack.XCoord - stack.Width;

                        foreach(var layer in stack.StackLayers) {
                            foreach(var item in layer.LoadedItems) {
                                item.XCoord = containerConfig.Width - item.XCoord - item.XDim;
                                item.DesmosCoords = DesmosUtils.GetDesmosCoords(item.XCoord, item.YCoord, item.ZCoord, item.XDim, item.YDim, item.ZDim);
                            }
                        }
                    }
                }

                if (compartment.Dividers != null && compartment.Dividers.Count > 0) {
                    foreach(var shelf in compartment.Dividers) {
                        shelf.XCoord = containerConfig.Width / 2;
                        shelf.DesmosCoords = DesmosUtils.GetDesmosCoords(shelf.XCoord, shelf.YCoord, shelf.ZCoord, shelf.XDim, shelf.YDim, shelf.ZDim);
                    }
                }
            }
        }

        private static void FlipBackCoordinatesBackSide(ContainerConfig containerConfig, List<Compartment> compartments) {

            foreach (Compartment compartment in compartments) {

                if (compartment == null || compartment.Sections == null || compartment.Sections.Count == 0) {
                    return;
                }

                foreach(var section in compartment.Sections) {

                    if (section == null || section.Stacks == null || section.Stacks.Count == 0) {
                        continue;
                    }

                    foreach(var stack in section.Stacks) {

                        stack.XCoord = containerConfig.Width - stack.XCoord - stack.Width;
                        stack.YCoord = (stack.Depth + stack.YCoord + ISU90ShelfConfig.CenterDividerWallThickness) * -1;

                        foreach(var layer in stack.StackLayers) {
                            foreach(var item in layer.LoadedItems) {
                                item.XCoord = containerConfig.Width - item.XCoord - item.XDim;
                                item.YCoord = (item.YDim + item.YCoord + ISU90ShelfConfig.CenterDividerWallThickness) * -1;
                                item.DesmosCoords = DesmosUtils.GetDesmosCoords(item.XCoord, item.YCoord, item.ZCoord, item.XDim, item.YDim, item.ZDim);
                            }
                        }
                    }
                }

                if (compartment.Dividers != null && compartment.Dividers.Count > 0) {
                    foreach(var shelf in compartment.Dividers) {
                        shelf.XCoord = containerConfig.Width - shelf.XCoord - shelf.XDim;
                        shelf.YCoord = (shelf.YDim + shelf.YCoord + ISU90ShelfConfig.CenterDividerWallThickness) * -1;

                        shelf.DesmosCoords = DesmosUtils.GetDesmosCoords(shelf.XCoord, shelf.YCoord, shelf.ZCoord, shelf.XDim, shelf.YDim, shelf.ZDim);
                    }
                }
            }
        }

        private static void ResetStackInSectionBackDownToBaseLayer(ItemStack stack, CompartmentSection section, List<ItemToLoad> listsOfItemsToAddItemBackTo) {

            var stackStartingWeight = stack.Weight;

            if (stack.StackLayers.Count < 1) {
                return;
            }

            for (var i=0; i < stack.StackLayers.Count; i++) {
                if (i > 0) {
                    var layer = stack.StackLayers[i];

                    foreach (var loadedItemInLayer in layer.LoadedItems) {
                        listsOfItemsToAddItemBackTo.Add(loadedItemInLayer.ItemToLoad);
                    }
                }
            }

            listsOfItemsToAddItemBackTo.Sort(new SurfaceAreaItemComparer());

            stack.StackLayers = [ stack.StackLayers[0] ];

            stack.Width = stack.StackLayers[0].Width;
            stack.Depth = stack.StackLayers[0].Depth;
            stack.Height = stack.StackLayers[0].Height;

            decimal stackNewWeight = 0;

            foreach (var item in stack.StackLayers[0].LoadedItems) {
                stackNewWeight += item.ItemToLoad.Weight;
            }

            stack.Weight = stackNewWeight;
            section.Weight -= stackStartingWeight - stackNewWeight;
        }

        private static void CleanupTopSection(CompartmentSection topSection, List<ItemToLoad> topShelfItems) {
            //if only one stack has two layers and there are more shelf items, remove the layer/item from stack to be used in next section
            var indexesOfStacksWithMultpleLayers = new List<int>();
            
            for (var i = 0; i < topSection.Stacks.Count; i++) {
                if (topSection.Stacks[i].StackLayers.Count > 1) {
                    indexesOfStacksWithMultpleLayers.Add(i);
                }
            }
            if (indexesOfStacksWithMultpleLayers.Count == 1 && topShelfItems.Count > 1) {
                ResetStackInSectionBackDownToBaseLayer(topSection.Stacks[indexesOfStacksWithMultpleLayers[0]], topSection, topShelfItems);
            }
        }

    }
}