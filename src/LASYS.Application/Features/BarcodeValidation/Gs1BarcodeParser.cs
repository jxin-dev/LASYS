namespace LASYS.Application.Features.BarcodeValidation
{
    public class Gs1BarcodeParser
    {
        public BarcodeValidationResult Parse(string barcode, BarcodeType type)
        {
            if (string.IsNullOrEmpty(barcode))
            {
                return BarcodeValidationResult.Failure("Barcode is empty.");
            }

            switch (type)
            {
                case BarcodeType.Label:
                    return ParseLabelBarcode(barcode);

                case BarcodeType.Instruction:
                    return ParseInstructionBarcode(barcode);

                default:
                    return BarcodeValidationResult.Failure(
                        $"Unsupported barcode type: {type}");
            }
        }
        private BarcodeValidationResult ParseLabelBarcode(string barcode)
        {
            return ParseBarcode(barcode, Gs1ApplicationIdentifierDefinition.Label);
        }
        private BarcodeValidationResult ParseInstructionBarcode(string barcode)
        {
            return ParseBarcode(barcode, Gs1ApplicationIdentifierDefinition.Instruction);
        }
        private BarcodeValidationResult ParseBarcode(string barcode, Dictionary<string, int> aiDefinitions)
        {
            var applicationIdentifiers = new Dictionary<string, string>();

            int position = 0;

            while (position < barcode.Length)
            {
                if (position + 2 > barcode.Length)
                {
                    return BarcodeValidationResult.Failure("Invalid barcode format.");
                }

                string ai = barcode.Substring(position, 2);

                position += 2;

                if (!aiDefinitions.TryGetValue(ai, out int length))
                {
                    return BarcodeValidationResult.Failure($"Unsupported Application Identifier: {ai}");
                }

                if (position + length > barcode.Length)
                {
                    return BarcodeValidationResult.Failure($"Invalid length for AI {ai}");
                }

                string value = barcode.Substring(position, length);

                applicationIdentifiers.Add(ai, value);

                position += length;
            }

            return BarcodeValidationResult.Success(applicationIdentifiers);
        }
    }
}
