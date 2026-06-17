namespace LASYS.Application.Features.BarcodeValidation
{
    public static class Gs1ApplicationIdentifierDefinition
    {
        public static readonly Dictionary<string, Gs1AiDefinition> Label = new()
        {
            { "01", new() { Length = 14 } }, // GTIN
            { "17", new() { Length = 6 } },  // Expiry Date
            { "11", new() { Length = 6 } },  // Manufacture Date
            { "10", new() { IsVariableLength = true } } // Lot Number
        };

        public static readonly Dictionary<string, Gs1AiDefinition> Instruction = new()
        {
            //{ "91", 13 },
            //{ "92", 0 }, //not yet identified the length check it on the source code and sample barcode for instruction barcode 
            //{ "01", 0 }, //not yet identified the length check it on the source code and sample barcode for instruction barcode
            //{ "17", 0 }, //not yet identified the length check it on the source code and sample barcode for instruction barcode
            //{ "11", 0 }, //not yet identified the length check it on the source code and sample barcode for instruction barcode
            //{ "10", 0 } //not yet identified the length check it on the source code and sample barcode for instruction barcode

        };
    }
}
