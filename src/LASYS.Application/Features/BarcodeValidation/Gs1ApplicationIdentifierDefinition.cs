namespace LASYS.Application.Features.BarcodeValidation
{
    public static class Gs1ApplicationIdentifierDefinition
    {
        public static readonly Dictionary<string, int> Label = new()
        {
            { "01", 14 }, 
            { "17", 6 }, // Expiry Date
            { "11", 6 }, // Manufacture Date - this is only applicable if eumdr
            { "10", 6 }, // Lot Number - need to included the line length. need to analyze bec. the line length is not fixed: example the line is X or AB
        };

        public static readonly Dictionary<string, int> Instruction = new()
        {
            { "91", 13 },
            { "92", 0 }, //not yet identified the length check it on the source code and sample barcode for instruction barcode 
            { "01", 0 }, //not yet identified the length check it on the source code and sample barcode for instruction barcode
            { "17", 0 }, //not yet identified the length check it on the source code and sample barcode for instruction barcode
            { "11", 0 }, //not yet identified the length check it on the source code and sample barcode for instruction barcode
            { "10", 0 } //not yet identified the length check it on the source code and sample barcode for instruction barcode

        };
    }
}
