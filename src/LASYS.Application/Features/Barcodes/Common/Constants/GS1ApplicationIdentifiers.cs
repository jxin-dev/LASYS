namespace LASYS.Application.Features.Barcodes.Common.Constants
{
    public static class GS1ApplicationIdentifiers
    {
        public const string ProductionIndicator = "91";
        public const string InstructionFormIndicator = "92";
        public const string ProductIndicator = "01";
        public const string ExpiraryDateIndicator = "17"; //YYMMDD
        public const string ManufactureDateIndicator = "11"; //YYMMDD EUMDR
        public const string LotNumberIndicator = "10";



        private static class PackagingComponentCodes
        {
            public static readonly Dictionary<int, string> Values = new()
            {
                { 3, "Unit Box / Inner Unit Box" },
                { 4, "Outer Unit Box" },
                { 5, "Carton Box / Inner Carton Box" },
                { 7, "Outer Carton Box" },
            };
        }

    }

 
}
