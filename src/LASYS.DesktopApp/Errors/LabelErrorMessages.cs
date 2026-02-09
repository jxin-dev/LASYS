using LASYS.DesktopApp.Enums;
using LASYS.DesktopApp.Events;

namespace LASYS.DesktopApp.Errors
{
    public static class LabelErrorMessages
    {
        private static readonly Dictionary<LabelOperationType, string> DefaultMessages =
        new()
        {
          {
                LabelOperationType.PrinterNotAvailable,
                "Printer is not available.\nPlease check the connection and try again."
            },
            {
                LabelOperationType.BarcodeMismatch,
                "Scanned barcode does not match."
            },
            {
                LabelOperationType.OcrNotReadable,
                "OCR could not read the label.\nEnsure the label is clear and properly printed."
            },
            {
                LabelOperationType.OcrIncorrect,
                "OCR result does not match the expected sequence."
            }
        };
        public static string GetMessage(LabelOperationFailedEventArgs args)
        {
            // Prefer custom message if provided
            var message = !string.IsNullOrWhiteSpace(args.CustomMessage)
                ? args.CustomMessage
                : DefaultMessages.GetValueOrDefault(
                    args.OperationType,
                    "An unknown label operation error occurred."
                  );

            // Append sequence number if available
            if (args.SequenceNo.HasValue)
            {
                // Format as 5-digit number: 00001, 00012, 00123
                string formattedSeq = args.SequenceNo.Value.ToString("D5");
                message += $"\n(Sequence No: {formattedSeq})";
            }

            // Append barcode result
            if (args.OperationType == LabelOperationType.BarcodeMismatch &&
                !string.IsNullOrWhiteSpace(args.BarcodeResult))
            {
                message += $"\nBarcode Result: {args.BarcodeResult}";
            }

            // Append OCR result
            if (args.OperationType == LabelOperationType.OcrIncorrect &&
                !string.IsNullOrWhiteSpace(args.OcrResult))
            {
                message += $"\nOCR Result: {args.OcrResult}";
            }

            return message;
        }
    }
}
