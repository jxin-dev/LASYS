using LASYS.Application.Common.Enums;
using LASYS.Application.Common.Models;
using LASYS.Application.Features.LabelInstructions.GetLabelInstructionContext;

namespace LASYS.Application.Common.Mappings
{
    public enum BarcodeType
    {
        Normal = 1,
        Roche = 2,
        McKesson = 3,
        PSS = 4,
        RocheGS1 = 5,
        Bulk = 6
    }

    public enum BarcodeCategory
    {
        NA = 1,
        TMC = 2,
        TPC = 3,
        UD = 4,
        Cathether = 5,
        Chemosafe = 6,
        UPC = 7
    }

    public static class NiceLabelDataMappings
    {
        public const string BarcodeDateFormat = "yyMMdd";
        private const string DisplayDateFormat = "yyyy-MM-dd";
        public static NiceLabelVariableCollection ToLabelData(LabelPrintingContext context)
        {
            var labelInstructionDetails = context.LabelInstructionDetails!;
            var productDetails = context.ProductDetails!;
            var masterLabelDetails = context.MasterLabelDetails!;

            var pssBarcode = masterLabelDetails.BoxType switch
            {
                BoxType.CartonBox => $"{productDetails.PssIdNo}1",
                BoxType.UnitBox => $"{productDetails.PssIdNo}2",
                _ => string.Empty
            };

            var boxType = masterLabelDetails.BoxType switch
            {
                BoxType.CartonBox => "5",
                BoxType.OuterCartonBox => "7",
                BoxType.AdditionalCartonBox => "9",
                BoxType.UnitBox => "3",
                BoxType.AdditionalUnitBox => "8",
                BoxType.OuterUnitBox => "4",
                _ => "1"
            };


            var barcodeTypeValue = Enum.Parse<BarcodeType>(productDetails.BarcodeType, ignoreCase: true);
            int barcodeType = (int)barcodeTypeValue;
            //var barcodeNumber = $"{barcodeType}{productDetails.BarcodeNumber}";
            var barcodeNumber = $"{boxType}{productDetails.BarcodeNumber}";

            return new NiceLabelVariableCollection()
                .Add("LOT_NO", labelInstructionDetails.LotNo)
                .Add("GAUGE", productDetails.Gauge)
                .Add("MANUFACTURE_DATE", labelInstructionDetails.ManufactureDate?.ToString(DisplayDateFormat))
                .Add("BARCODE_MANUFACTURE_DATE", labelInstructionDetails.ManufactureDate?.ToString(BarcodeDateFormat))
                .Add("EXPIRY_DATE", labelInstructionDetails.ExpirationDate?.ToString(DisplayDateFormat))
                .Add("BARCODE_EXPIRY_DATE", labelInstructionDetails.ExpirationDate?.ToString(BarcodeDateFormat))
                .Add("BARCODE_NO", barcodeNumber)
                .Add("DEPKES_NUMBER",productDetails.DepkesNumber)
                .Add("PRODUCT_CODE", labelInstructionDetails.ItemCode)
                .Add("DESCRIPTION", productDetails.Description)
                .Add("FLOW_RATE", productDetails.FlowRate)
                .Add("DVR_NUMBER", productDetails.DvrNumber)
                .Add("ROCHE", masterLabelDetails.Roche)
                .Add("PSS_BARCODE", pssBarcode);
            //BOX_NO variable name use for sequence number
        }
      
    }
}




