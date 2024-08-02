using CargoMaker.Comparer;
using CargoMaker.Model;

namespace CargoMaker.Utils {
    public static class CompartmentUtils {

        /// <summary>
        /// Fills section with stacks starting with an item at the back left corner. 
        /// Scans for available spaces and adds best fitting stacks to spaces, repeating until no more stacks fit.
        /// If setBackRowFirst is true the logic will first maintain a back row line as much as possible, working left to right.
        /// </summary>
        /// <param name="section"></param>
        /// <param name="itemsToUseToBuildStacks"></param>
        /// <param name="otherItemListToCheckWhenRemovingItems"></param>
        /// <param name="setBackRowFirst"></param>
        public static void FillSectionWithStacks(
            CompartmentSection section, 
            List<ItemToLoad> itemsToUseToBuildStacks, 
            List<ItemToLoad> otherItemListToCheckWhenRemovingItems,
            bool setBackRowFirst) 
        {
            if (setBackRowFirst) {
                SetInitialAvailableSpaceForBackRowOnSectionUsingFirstStack(section);
                while(TryAddBestFittingStackToBackRowOfSection(section, itemsToUseToBuildStacks, otherItemListToCheckWhenRemovingItems)) {}
            }

            while(TryAddBestFittingStackToSectionOpenSpace(section, itemsToUseToBuildStacks, otherItemListToCheckWhenRemovingItems)) {}
        }

        public static void ShiftZCoordsUpForItemsInSection(CompartmentSection compartmentSection, decimal moveUp) {

            foreach (var stack in compartmentSection.Stacks) {
                foreach (var layer in stack.StackLayers) {
                    foreach (var loadedItem in layer.LoadedItems) {
                        loadedItem.ZCoord += moveUp;
                        loadedItem.DesmosCoords = DesmosUtils.GetDesmosCoords(loadedItem.XCoord, loadedItem.YCoord, loadedItem.ZCoord, loadedItem.XDim, loadedItem.YDim, loadedItem.ZDim);
                    }
                }
            }
        }

        public static void SetSectionAvailableSpacesInCurrentState(CompartmentSection section) {
            section.AvailableSpaces = GetAvailableSpacesFromStacksInSpace(section.Stacks, section.Width, section.Depth, section.Height, section.WeightLimit);
        }

        public static List<AvailableSpace> GetAvailableSpacesFromStacksInSpace(
            List<ItemStack> stacks, 
            decimal spaceWidth, 
            decimal spaceDepth, 
            decimal spaceHeight,
            decimal spaceWeightLimit) 
        {
            var exposedFrontFaces = GetExposedFrontFaceLinesFromStacksInSpace(stacks, spaceWidth);
            return GetAvailableSpacesFromExposedFrontFacesInSpace(exposedFrontFaces, spaceWidth, spaceDepth, spaceHeight, spaceWeightLimit);
        }

        public static List<AvailableSpace> GetAvailableSpacesFromLoadedItemsInSpace(
            List<LoadedItem> items, 
            decimal spaceWidth, 
            decimal spaceDepth, 
            decimal spaceHeight,
            decimal spaceWeightLimit) 
        {
            var exposedFrontFaces = GetExposedFrontFaceLinesFromItemsInSpace(items, spaceWidth);
            return GetAvailableSpacesFromExposedFrontFacesInSpace(exposedFrontFaces, spaceWidth, spaceDepth, spaceHeight, spaceWeightLimit);
        }

        private static List<AvailableSpace> GetAvailableSpacesFromExposedFrontFacesInSpace(
            List<Line> exposedFrontFaces, 
            decimal spaceWidth,
            decimal spaceDepth, 
            decimal spaceHeight,
            decimal spaceWeightLimit) 
        {
            List<AvailableSpace> spaces = [];
            exposedFrontFaces.Sort(new LineStartComparer());

            for (var i = 0; i < exposedFrontFaces.Count; i++) {
                
                var frontFace = exposedFrontFaces[i];

                var availableSpace = new AvailableSpace {
                    YCoord = frontFace.PositionCoord,
                    Depth = spaceDepth - frontFace.PositionCoord,
                    Height = spaceHeight,
                    WeightLimit = spaceWeightLimit,
                };

                if (i > 0) { //build line to left
                    for (var k = i-1; k >= 0; k--) {
                        var previousFrontFace = exposedFrontFaces[k];

                        if (previousFrontFace.PositionCoord <= frontFace.PositionCoord) {
                            if (k == 0) {
                                availableSpace.XCoord = 0;
                            }
                            else {
                                availableSpace.XCoord = previousFrontFace.LineStartCoord;
                            }
                        }
                        else {
                            availableSpace.XCoord = previousFrontFace.LineEndCoord;
                            break;
                        }
                    }
                }
                else {
                    availableSpace.XCoord = 0;
                }

                if (i < exposedFrontFaces.Count-1) { //build line to right
                    for (var k = i+1; k < exposedFrontFaces.Count; k++) {
                        var nextFrontFace = exposedFrontFaces[k];

                        if (nextFrontFace.PositionCoord <= frontFace.PositionCoord) {
                            if (k == exposedFrontFaces.Count-1) {
                                availableSpace.Width = spaceWidth - availableSpace.XCoord;
                            }
                            else {
                                availableSpace.Width = nextFrontFace.LineEndCoord - availableSpace.XCoord;
                            }
                        }
                        else {
                            availableSpace.Width = nextFrontFace.LineStartCoord - availableSpace.XCoord;
                            break;
                        }
                    }
                } 
                else {
                    availableSpace.Width = spaceWidth - availableSpace.XCoord;
                }

                if (!spaces.Contains(availableSpace)) {
                    spaces.Add(availableSpace);
                }
            }
            spaces.Sort(new AvailableSpaceComparer());
            return spaces;
        }

        public static void SetInitialAvailableSpaceForBackRowOnSectionUsingFirstStack(CompartmentSection section) {
            section.AvailableSpaces = [
                new AvailableSpace {
                    Depth = section.Depth,
                    Width = section.Width - section.Stacks[0].Width,
                    Height = section.Height,
                    XCoord = section.Stacks[0].Width,
                    YCoord = 0,
                    TargetSpace = new TargetSpace {
                        Depth = section.Stacks[0].Depth,
                        Width = section.Width - section.Stacks[0].Width
                    }
                }
            ];
        }

        /// <summary>
        /// Adds best fitting stack to section in its current state. 
        /// The available spaces in the section have been previously calculated and set in section.
        /// Returns best fitting stack in best available space. Stack value is null if a stack could not be added.
        /// </summary>
        /// <param name="section"></param>
        /// <param name="itemsToUseToBuildStacks"></param>
        /// <param name="otherItemListToCheckWhenRemovingItems"></param>
        /// <returns></returns>
        private static ItemStack? AddBestFittingStackToSectionInCurrentState(
            CompartmentSection section, 
            List<ItemToLoad> itemsToUseToBuildStacks, 
            List<ItemToLoad> otherItemListToCheckWhenRemovingItems) 
        {
            ItemStack? stack = null;

            var remainingWeightLimitInSection = section.WeightLimit - section.Weight;
            
            foreach (var availableSpace in section.AvailableSpaces) {

                availableSpace.WeightLimit = remainingWeightLimitInSection;

                stack = StackUtils.FindBestFittingStackForSpace(
                    availableSpace,
                    itemsToUseToBuildStacks,
                    otherItemListToCheckWhenRemovingItems); 

                if (stack != null) {
                    break;
                }
            }
            
            if (stack != null) {
                section.AddStackToSection(stack);
                
                foreach(var layer in stack.StackLayers) {
                    foreach(var item in layer.LoadedItems) {
                        LoadItemCandidateUtils.RemoveItemFromLists(item.ItemToLoad, [itemsToUseToBuildStacks, otherItemListToCheckWhenRemovingItems]);
                    }
                }
            }
            return stack;
        }

        public static bool TryAddBestFittingStackToBackRowOfSection(
            CompartmentSection section, 
            List<ItemToLoad> itemsToUseToBuildStack, 
            List<ItemToLoad> otherItemListToCheckWhenRemovingItems)
        {
            var stack = AddBestFittingStackToSectionInCurrentState(section, itemsToUseToBuildStack, otherItemListToCheckWhenRemovingItems);

            if (stack == null) {
                return false;
            }

            SetNextSectionAvailableSpaceForBackRow(section, stack);

            return true;
        }

        public static bool TryAddBestFittingStackToSectionOpenSpace(
            CompartmentSection section, 
            List<ItemToLoad> itemsToUseToBuildStack) 
        {
            SetSectionAvailableSpacesInCurrentState(section);

            var stack = AddBestFittingStackToSectionInCurrentState(section, itemsToUseToBuildStack, []);

            if (stack == null) {
                return false;
            }

            return true;
        }

        public static bool TryAddBestFittingStackToSectionOpenSpace(
            CompartmentSection section, 
            List<ItemToLoad> itemsToUseToBuildStack, 
            List<ItemToLoad> otherItemListToCheckWhenRemovingItems) 
        {
            SetSectionAvailableSpacesInCurrentState(section);

            var stack = AddBestFittingStackToSectionInCurrentState(section, itemsToUseToBuildStack, otherItemListToCheckWhenRemovingItems);

            if (stack == null) {
                return false;
            }

            return true;
        }

        private static void SetNextSectionAvailableSpaceForBackRow(CompartmentSection section, ItemStack stack) {
        
            section.AvailableSpaces = [
                new () {
                    Depth = section.AvailableSpaces[0].Depth,
                    Width = section.AvailableSpaces[0].Width - stack.Width,
                    Height = section.AvailableSpaces[0].Height,
                    XCoord = section.AvailableSpaces[0].XCoord + stack.Width,
                    YCoord = section.AvailableSpaces[0].YCoord,
                    TargetSpace = new () {
                        Depth = section.AvailableSpaces[0].TargetSpace.Depth,
                        Width = section.AvailableSpaces[0].TargetSpace.Width - stack.Width,
                        Height = section.AvailableSpaces[0].TargetSpace.Height
                    }
                }
            ];
        }

        private static List<Line> GetExposedFrontFaceLinesFromStacksAndStartingLine(Line startingLine, List<ItemStack> stacks) {
            
            List<Line> exposedFrontFaces = [startingLine];

            foreach (var stack in stacks) {
                var stackLine = stack.GetStackFrontFaceLine();

                foreach (var line in exposedFrontFaces) {

                    if (stackLine.PositionCoord > line.PositionCoord) {
                        
                        if (stackLine.LineStartCoord >= line.LineEndCoord ||
                            stackLine.LineEndCoord <= line.LineStartCoord) 
                        {
                            continue;
                        }

                        if (stackLine.LineStartCoord <= line.LineStartCoord) { 
                        
                            if (stackLine.LineEndCoord >= line.LineEndCoord) {
                                exposedFrontFaces = []; //no exposed parts of front face
                                break;
                            
                            } else {
                                exposedFrontFaces.Remove(line);
                                exposedFrontFaces.Add(
                                    new () {
                                        LineStartCoord = stackLine.LineEndCoord,
                                        LineEndCoord = line.LineEndCoord,
                                        PositionCoord = line.PositionCoord
                                    }
                                );
                                break;
                            }

                        } else {

                            if (stackLine.LineEndCoord >= line.LineEndCoord) {
                                exposedFrontFaces.Remove(line);
                                exposedFrontFaces.Add(
                                    new () { 
                                        LineStartCoord = line.LineStartCoord,
                                        LineEndCoord = stackLine.LineStartCoord,
                                        PositionCoord = line.PositionCoord
                                    }
                                );
                                break;

                            } else {

                                exposedFrontFaces.Remove(line);
                                exposedFrontFaces.Add(
                                    new () { 
                                        LineStartCoord = line.LineStartCoord,
                                        LineEndCoord = stackLine.LineStartCoord,
                                        PositionCoord = line.PositionCoord
                                    }
                                );
                                exposedFrontFaces.Add(
                                    new () { 
                                        LineStartCoord = stackLine.LineEndCoord,
                                        LineEndCoord = line.LineEndCoord,
                                        PositionCoord = line.PositionCoord
                                    }
                                );
                                break;
                            }
                        }
                    }
                }
            }
            return exposedFrontFaces;
        }

        private static List<Line> GetExposedFrontFaceLinesFromItemsAndStartingLine(Line startingLine, List<LoadedItem> items) {
            
            List<Line> exposedFrontFaces = [startingLine];

            foreach (var item in items) {
                
                var itemFrontFaceLine =  new Line {
                    LineStartCoord = item.XCoord,
                    LineEndCoord = item.XCoord + item.XDim,
                    PositionCoord = item.YCoord + item.YDim
                };

                foreach (var line in exposedFrontFaces) {

                    if (itemFrontFaceLine.PositionCoord > line.PositionCoord) {
                        
                        if (itemFrontFaceLine.LineStartCoord >= line.LineEndCoord ||
                            itemFrontFaceLine.LineEndCoord <= line.LineStartCoord) 
                        {
                            continue;
                        }

                        if (itemFrontFaceLine.LineStartCoord <= line.LineStartCoord) { 
                        
                            if (itemFrontFaceLine.LineEndCoord >= line.LineEndCoord) {
                                exposedFrontFaces = []; //no exposed parts of front face
                                break;
                            
                            } else {
                                exposedFrontFaces.Remove(line);
                                exposedFrontFaces.Add(
                                    new () {
                                        LineStartCoord = itemFrontFaceLine.LineEndCoord,
                                        LineEndCoord = line.LineEndCoord,
                                        PositionCoord = line.PositionCoord
                                    }
                                );
                                break;
                            }

                        } else {

                            if (itemFrontFaceLine.LineEndCoord >= line.LineEndCoord) {
                                exposedFrontFaces.Remove(line);
                                exposedFrontFaces.Add(
                                    new () { 
                                        LineStartCoord = line.LineStartCoord,
                                        LineEndCoord = itemFrontFaceLine.LineStartCoord,
                                        PositionCoord = line.PositionCoord
                                    }
                                );
                                break;

                            } else {

                                exposedFrontFaces.Remove(line);
                                exposedFrontFaces.Add(
                                    new () { 
                                        LineStartCoord = line.LineStartCoord,
                                        LineEndCoord = itemFrontFaceLine.LineStartCoord,
                                        PositionCoord = line.PositionCoord
                                    }
                                );
                                exposedFrontFaces.Add(
                                    new () { 
                                        LineStartCoord = itemFrontFaceLine.LineEndCoord,
                                        LineEndCoord = line.LineEndCoord,
                                        PositionCoord = line.PositionCoord
                                    }
                                );
                                break;
                            }
                        }
                    }
                }
            }
            return exposedFrontFaces;
        }

        private static List<Line> GetExposedFrontFaceLinesFromStacksInSpace(List<ItemStack> stacks, decimal spaceBackLineWidth) {

            List<Line> exposedFrontFaceLines = [];

            foreach (var stack in stacks) {
                exposedFrontFaceLines.AddRange(GetExposedFrontFaceLinesFromStacksAndStartingLine(stack.GetStackFrontFaceLine(), stacks));
            }

            var spaceBackLine = new Line {
                LineStartCoord = 0,
                LineEndCoord = spaceBackLineWidth,
                PositionCoord = 0
            };

            exposedFrontFaceLines.AddRange(GetExposedFrontFaceLinesFromStacksAndStartingLine(spaceBackLine, stacks));

            return exposedFrontFaceLines;
        }

        private static List<Line> GetExposedFrontFaceLinesFromItemsInSpace(List<LoadedItem> items, decimal spaceBackLineWidth) {

            List<Line> exposedFrontFaceLines = [];

            foreach (var item in items) {

                var itemFrontFaceLine =  new Line {
                    LineStartCoord = item.XCoord,
                    LineEndCoord = item.XCoord + item.XDim,
                    PositionCoord = item.YCoord + item.YDim
                };

                exposedFrontFaceLines.AddRange(GetExposedFrontFaceLinesFromItemsAndStartingLine(itemFrontFaceLine, items));
            }

            if (exposedFrontFaceLines.Count == 1 && exposedFrontFaceLines[0].LineEndCoord - exposedFrontFaceLines[0].LineStartCoord == spaceBackLineWidth) {
                return exposedFrontFaceLines;
            }

            var spaceBackLine = new Line {
                LineStartCoord = 0,
                LineEndCoord = spaceBackLineWidth,
                PositionCoord = 0
            };

            exposedFrontFaceLines.AddRange(GetExposedFrontFaceLinesFromItemsAndStartingLine(spaceBackLine, items));

            return exposedFrontFaceLines;
        }
    }
}