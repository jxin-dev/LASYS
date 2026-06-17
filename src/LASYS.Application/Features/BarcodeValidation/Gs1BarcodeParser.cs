namespace LASYS.Application.Features.BarcodeValidation
{
    public class Gs1BarcodeParser
    {
        public BarcodeValidationResult Parse(string? barcode, BarcodeContentType type, bool isEumdr)
        {
            if (string.IsNullOrEmpty(barcode))
            {
                return BarcodeValidationResult.Failure("Barcode is empty.");
            }

            switch (type)
            {
                case BarcodeContentType.Label:
                    return ParseLabelBarcode(barcode, isEumdr);

                case BarcodeContentType.Instruction:
                    return ParseInstructionBarcode(barcode, isEumdr);

                default:
                    return BarcodeValidationResult.Failure(
                        $"Unsupported barcode type: {type}");
            }
        }
        private BarcodeValidationResult ParseLabelBarcode(string barcode, bool isEumdr)
        {
            return ParseBarcode(barcode, Gs1ApplicationIdentifierDefinition.Label, isEumdr);
        }
        private BarcodeValidationResult ParseInstructionBarcode(string barcode, bool isEumdr)
        {
            return ParseBarcode(barcode, Gs1ApplicationIdentifierDefinition.Instruction, isEumdr);
        }
        private BarcodeValidationResult ParseBarcode(string barcode, Dictionary<string, Gs1AiDefinition> aiDefinitions, bool isEumdr = false)
        {
            var applicationIdentifiers = new Dictionary<string, string>();

            int position = 0;

            while (position < barcode.Length)
            {
                if (position + 2 > barcode.Length)
                {
                    return BarcodeValidationResult.Failure(
                        "Invalid barcode format.");
                }

                var ai = barcode.Substring(position, 2);

                position += 2;

                if (!aiDefinitions.TryGetValue(ai, out var definition))
                {
                    return BarcodeValidationResult.Failure(
                        $"Unsupported Application Identifier: {ai}");
                }

                if (definition.IsVariableLength)
                {
                    var nextAiPosition = -1;

                    foreach (var knownAi in aiDefinitions.Keys)
                    {
                        var candidate =
                            barcode.IndexOf(
                                knownAi,
                                position,
                                StringComparison.Ordinal);

                        if (candidate > position)
                        {
                            if (nextAiPosition == -1 ||
                                candidate < nextAiPosition)
                            {
                                nextAiPosition = candidate;
                            }
                        }
                    }

                    string value;

                    if (nextAiPosition == -1)
                    {
                        value = barcode.Substring(position);
                        position = barcode.Length;
                    }
                    else
                    {
                        value = barcode.Substring(
                            position,
                            nextAiPosition - position);

                        position = nextAiPosition;
                    }

                    applicationIdentifiers.Add(ai, value);

                    continue;
                }

                if (position + definition.Length > barcode.Length)
                {
                    return BarcodeValidationResult.Failure(
                        $"Invalid length for AI {ai}");
                }

                var fixedValue = barcode.Substring(position, definition.Length);

                applicationIdentifiers.Add(ai, fixedValue);

                position += definition.Length;
            }

            if (isEumdr && !applicationIdentifiers.ContainsKey("11"))
            {
                return BarcodeValidationResult.Failure("Manufacture Date (AI 11) is required for EUMDR labels.");
            }

            if (!applicationIdentifiers.ContainsKey("10"))
            {
                return BarcodeValidationResult.Failure("Lot Number (AI 10) is required.");
            }

            return BarcodeValidationResult.Success(applicationIdentifiers);
        }
    }
}
