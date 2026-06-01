namespace LASYS.Application.Features.Barcodes.Common.Constants
{
    public static class LabelBarcodeIndexes
    {
        public const int ProductIndicatorIndex = 0;
        public const int ProductIndicatorLength = 2; //(01)
        public const int PackagingComponentCodeIndex = ProductIndicatorIndex + ProductIndicatorLength;//3,4,5,7

        public const int ExpiryDateIndicatorIndex = 16;
        public const int ExpiryDateIndicatorLength = 2; //(17)
        public const int ExpiryDateIndex = ExpiryDateIndicatorIndex + ExpiryDateIndicatorLength;
        public const int ExpiryDateLength = 6; //YYMMDD

        public const int ManufactureDateIndicatorIndex = 24;
        public const int ManufactureDateIndicatorLength = 2; //(11)
        public const int ManufactureDateIndex = ManufactureDateIndicatorIndex + ManufactureDateIndicatorLength;
        public const int ManufactureDateLength = 6; //YYMMDD


        public const int LotNumberIndicatorIndex = 18;
        public const int LotNumberIndicatorLength = 2; //(10)
        public const int LotNumberIndex = LotNumberIndicatorIndex + LotNumberIndicatorLength;
        public const int LotNumberLength = 6;
        public const int AssemblyLineIndex = LotNumberIndex + LotNumberLength;


    }
}
