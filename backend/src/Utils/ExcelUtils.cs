using CargoMaker.Config;
using CargoMaker.Model;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CargoMaker.Utils {

    public static class ExcelUtils {

        public static IXLWorksheet GetExcelWorksheetFromFormFile(ILogger log, IFormFile formFile) {
            var memoryStream = formFile?.OpenReadStream();
            
            var workbook = new XLWorkbook(memoryStream);
            var worksheet = workbook.Worksheet(1);

            return worksheet;
        }

        public static XLWorkbook CreateExcelWorkbookFromCargoPlan(CargoPlan cargoPlan) {
            var workbook = new XLWorkbook();

            if (cargoPlan.UnloadedItems.Count > 0) {
                var unloadedItemsWorksheet = workbook.Worksheets.Add("Unloaded Items");

                for (var i=0; i<cargoPlan.UnloadedItems.Count; i++) {
                    unloadedItemsWorksheet.Row(i+1).Cell("A").Value = cargoPlan.UnloadedItems[i].NSN;
                }
            }

            var manifestWorksheet = workbook.Worksheets.Add("Manifest");
            AddCargoPlanWorksheetHeaderRow(manifestWorksheet);

            int manifestCurrentRow = 2;
        
            for (var i=0; i<cargoPlan.Isu90Containers.Count; i++) {
                var container = cargoPlan.Isu90Containers[i];
                var worksheet = workbook.Worksheets.Add($"ISU90 {i+1}");

                manifestCurrentRow++;
                manifestWorksheet.Row(manifestCurrentRow).Cell("A").Value = $"ISU90 {i+1} - (Total Weight: {container.Weight})";
                manifestCurrentRow++;
                
                AddCargoPlanWorksheetHeaderRow(worksheet);
                worksheet.Row(1).Cell("L").Value = $"Total Weight: {container.Weight}";
                
                int currentRow = 2;
                
                foreach (var compartment in container.Compartments) {
                    foreach (var section in compartment.Sections) {
                        if (section != null) {
                            foreach (var loadedItem in section.GetLoadedItems()) {
                                AddDataRowForItem(worksheet, currentRow, compartment, section, loadedItem, $"{OrgConfig.ORG}{OrgConfig.SHP} {i+1}");
                                currentRow++;

                                AddDataRowForItem(manifestWorksheet, manifestCurrentRow, compartment, section, loadedItem, $"{OrgConfig.ORG}{OrgConfig.SHP} {i+1}");
                                manifestCurrentRow++;
                            }
                        }
                    }
                    if (compartment.Dividers != null) {
                        foreach (var divider in compartment.Dividers) {
                            AddDividerCoords(worksheet, currentRow, divider);
                            currentRow++;
                        }
                    }
                }
                AddISU90EdgeCoords(worksheet, currentRow); //to be done at end, doesn't increment currentRow
            }
            
            for (var i=0; i < cargoPlan.PalletSingleContainers.Count; i++) {
                var container = cargoPlan.PalletSingleContainers[i];
                var worksheet = workbook.Worksheets.Add($"Pallet (Single) {i+1}");

                manifestCurrentRow++;
                manifestWorksheet.Row(manifestCurrentRow).Cell("A").Value = $"Pallet (Single) {i+1} - (Total Weight: {container.Weight})";
                manifestCurrentRow++;
                
                AddCargoPlanWorksheetHeaderRow(worksheet);
                worksheet.Row(1).Cell("L").Value = $"Total Weight: {container.Weight}";

                int currentRow = 2;

                foreach (var compartment in container.Compartments) {
                    foreach (var section in compartment.Sections) {
                        foreach (var loadedItem in section.GetLoadedItems()) {
                            AddDataRowForItem(worksheet, currentRow, compartment, section, loadedItem);
                            currentRow++;

                            AddDataRowForItem(manifestWorksheet, manifestCurrentRow, compartment, section, loadedItem);
                            manifestCurrentRow++;
                        }
                    }
                }
            }

            for (var i=0; i < cargoPlan.PalletDoubleContainers.Count; i++) {
                var container = cargoPlan.PalletDoubleContainers[i];
                var worksheet = workbook.Worksheets.Add($"Pallet (Double) {i+1}");
                
                manifestCurrentRow++;
                manifestWorksheet.Row(manifestCurrentRow).Cell("A").Value = $"Pallet (Double) {i+1} - (Total Weight: {container.Weight})";
                manifestCurrentRow++;

                AddCargoPlanWorksheetHeaderRow(worksheet);
                worksheet.Row(1).Cell("L").Value = $"Total Weight: {container.Weight}";

                int currentRow = 2;

                foreach (var compartment in container.Compartments) {
                    foreach (var section in compartment.Sections) {
                        foreach (var loadedItem in section.GetLoadedItems()) {
                            AddDataRowForItem(worksheet, currentRow, compartment, section, loadedItem);
                            currentRow++;

                            AddDataRowForItem(manifestWorksheet, manifestCurrentRow, compartment, section, loadedItem);
                            manifestCurrentRow++;
                        }
                    }
                }
            }

            return workbook;
        }

        private static void AddCargoPlanWorksheetHeaderRow(IXLWorksheet worksheet) {
            worksheet.Row(1).Cell("A").Value = "ORG";
            worksheet.Row(1).Cell("B").Value = "SHP";
            worksheet.Row(1).Cell("C").Value = "STOCK NUMBER";
            worksheet.Row(1).Cell("D").Value = "NOMENCLATURE";
            worksheet.Row(1).Cell("E").Value = "DEP QTY";
            worksheet.Row(1).Cell("F").Value = "LOCATION";
            worksheet.Row(1).Cell("G").Value = "DESMOS COORDINATES";
            worksheet.Row(1).Cell("H").Value = "LENGTH";
            worksheet.Row(1).Cell("I").Value = "WIDTH";
            worksheet.Row(1).Cell("J").Value = "HEIGHT";
            worksheet.Row(1).Cell("K").Value = "WEIGHT";

            worksheet.Row(1).Height = 44.9;
            worksheet.Row(1).Style.Font.SetBold();

            StyleFirstCellInHeader(worksheet, "A");
            StyleMiddleCellInHeader(worksheet, "B");
            StyleMiddleCellInHeader(worksheet, "C");
            StyleMiddleCellInHeader(worksheet, "D");
            StyleMiddleCellInHeader(worksheet, "E");
            StyleMiddleCellInHeader(worksheet, "F");
            StyleMiddleCellInHeader(worksheet, "G");
            StyleMiddleCellInHeader(worksheet, "H");
            StyleMiddleCellInHeader(worksheet, "I");
            StyleMiddleCellInHeader(worksheet, "J");
            StyleLastCellInHeader(worksheet, "K");
        }

        private static void StyleFirstCellInHeader(IXLWorksheet worksheet, string cell) {
            worksheet.Row(1).Cell(cell).Style.Fill.BackgroundColor = XLColor.FromHtml("#E9E8E3");
            worksheet.Row(1).Cell(cell).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            worksheet.Row(1).Cell(cell).Style.Border.BottomBorderColor = XLColor.Black;
            worksheet.Row(1).Cell(cell).Style.Border.TopBorder = XLBorderStyleValues.Thin;
            worksheet.Row(1).Cell(cell).Style.Border.TopBorderColor = XLColor.Black;
            worksheet.Row(1).Cell(cell).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            worksheet.Row(1).Cell(cell).Style.Border.LeftBorderColor = XLColor.Black;
            worksheet.Row(1).Cell(cell).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Row(1).Cell(cell).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        private static void StyleMiddleCellInHeader(IXLWorksheet worksheet, string cell) {
            worksheet.Row(1).Cell(cell).Style.Fill.BackgroundColor = XLColor.FromHtml("#E9E8E3");
            worksheet.Row(1).Cell(cell).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            worksheet.Row(1).Cell(cell).Style.Border.BottomBorderColor = XLColor.Black;
            worksheet.Row(1).Cell(cell).Style.Border.TopBorder = XLBorderStyleValues.Thin;
            worksheet.Row(1).Cell(cell).Style.Border.TopBorderColor = XLColor.Black;
            worksheet.Row(1).Cell(cell).Style.Border.LeftBorder = XLBorderStyleValues.Dotted;
            worksheet.Row(1).Cell(cell).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Row(1).Cell(cell).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        private static void StyleLastCellInHeader(IXLWorksheet worksheet, string cell) {
            worksheet.Row(1).Cell(cell).Style.Fill.BackgroundColor = XLColor.FromHtml("#E9E8E3");
            worksheet.Row(1).Cell(cell).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            worksheet.Row(1).Cell(cell).Style.Border.BottomBorderColor = XLColor.Black;
            worksheet.Row(1).Cell(cell).Style.Border.TopBorder = XLBorderStyleValues.Thin;
            worksheet.Row(1).Cell(cell).Style.Border.TopBorderColor = XLColor.Black;
            worksheet.Row(1).Cell(cell).Style.Border.LeftBorder = XLBorderStyleValues.Dotted;
            worksheet.Row(1).Cell(cell).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Row(1).Cell(cell).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Row(1).Cell(cell).Style.Border.RightBorder = XLBorderStyleValues.Thin;
            worksheet.Row(1).Cell(cell).Style.Border.RightBorderColor = XLColor.Black;
        }

        private static void StyleCellInDataRow(IXLWorksheet worksheet, int row) {
            worksheet.Row(row).Cell("A").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            worksheet.Row(row).Cell("B").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            worksheet.Row(row).Cell("C").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            worksheet.Row(row).Cell("D").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            worksheet.Row(row).Cell("E").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            worksheet.Row(row).Cell("F").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            worksheet.Row(row).Cell("G").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            worksheet.Row(row).Cell("H").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            worksheet.Row(row).Cell("I").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            worksheet.Row(row).Cell("J").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            worksheet.Row(row).Cell("K").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        private static void AddDataRowForItem(IXLWorksheet worksheet, int row, Compartment compartment, CompartmentSection section, LoadedItem loadedItem, string locationPrefix = "") {
            var item = loadedItem.ItemToLoad;

            worksheet.Row(row).Cell("A").Value = OrgConfig.ORG;
            worksheet.Row(row).Cell("B").Value = OrgConfig.SHP;
            worksheet.Row(row).Cell("C").Value = item.NSN;
            worksheet.Row(row).Cell("D").Value = item.Name;
            worksheet.Row(row).Cell("E").Value = item.Qty;
            worksheet.Row(row).Cell("F").Value = $"{ locationPrefix }{ compartment.Id }{ section.Id }";
            worksheet.Row(row).Cell("G").Value = loadedItem.DesmosCoords;
            worksheet.Row(row).Cell("H").Value = item.Depth;
            worksheet.Row(row).Cell("I").Value = item.Width;
            worksheet.Row(row).Cell("J").Value = item.Height;
            worksheet.Row(row).Cell("K").Value = item.Weight;

            StyleCellInDataRow(worksheet, row);
        }

        private static void AddDividerCoords(IXLWorksheet worksheet, int row, CompartmentDivider divider) {
            worksheet.Row(row).Cell("G").Value = divider.DesmosCoords;
        }

        private static void AddISU90EdgeCoords(IXLWorksheet worksheet, int row) {
            worksheet.Row(row).Cell("G").Value = """0<x<102\left\{-40<y<-39\right\}\left\{0<z<4\right\}""";
            worksheet.Row(row + 1).Cell("G").Value = """0<x<102\left\{-8<y\ <\ 0\right\}\left\{83<z<84\right\}""";
            worksheet.Row(row + 2).Cell("G").Value = """0<x<102\left\{41<y<42\right\}\left\{0<z<4\right\}""";
            worksheet.Row(row + 3).Cell("G").Value = """0<x<102\left\{1<y\ <\ 9\right\}\left\{83<z<84\right\}""";
        }
    }
}