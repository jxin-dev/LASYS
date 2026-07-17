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
            { "91", new() { Length = 14 } }, //Boxtype eq. 03/05/07 ect. + Barcode No.
            { "92", new() { Length = 6 } }, // Expiry Date 
            { "10", new() { IsVariableLength = true } } // Lot Number
        };
    }
}
