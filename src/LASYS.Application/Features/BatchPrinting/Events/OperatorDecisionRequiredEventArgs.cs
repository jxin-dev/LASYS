using LASYS.Application.Features.BatchPrinting.Enums;

namespace LASYS.Application.Features.BatchPrinting.Events
{
    public class OperatorDecisionRequiredEventArgs : EventArgs
    {
        public ValidationFailure FailureType { get; private set; }
        public string SequenceNo { get; private set; }
        public int PairNumber { get; private set; }
        public int TotalPairs { get; private set; }
        public string? BarcodeResult { get; private set; }
        public string? OcrResult { get; private set; }

        public OperatorDecisionRequiredEventArgs(ValidationFailure failureType, string sequenceNo)
        {
            FailureType = failureType;
            SequenceNo = sequenceNo;
            PairNumber = 1;
            TotalPairs = 1;
        }

        public OperatorDecisionRequiredEventArgs(
            ValidationFailure failureType,
            string sequenceNo,
            int pairNumber,
            int totalPairs,
            string? barcodeResult = null,
            string? ocrResult = null)
        {
            FailureType = failureType;
            SequenceNo = sequenceNo;
            PairNumber = pairNumber;
            TotalPairs = totalPairs;
            BarcodeResult = barcodeResult;
            OcrResult = ocrResult;
        }

        public string GetMessage()
        {
            return FailureType switch
            {
                ValidationFailure.FileGenerationFailed => $"Label generation failed for sequence {SequenceNo}.",
                ValidationFailure.PrinterUnavailable => $"Printer is unavailable for sequence {SequenceNo}.\n{FormatPairText()}",
                ValidationFailure.BarcodeMismatch => $"Barcode validation failed on {SequenceNo}.\n{FormatPairText()}\n{AppendBarcodeDetails()}",
                ValidationFailure.OcrMismatch => $"OCR validation failed on {SequenceNo}\n{FormatPairText()}\n{AppendOcrDetails()}",
                ValidationFailure.SaveFailed => $"Saving printed label failed on {SequenceNo}.\n{FormatPairText}",
                ValidationFailure.ScannerNotDetected => $"Barcode scanner was not detected.\nPlease check that the scanner is connected and try again.",
                ValidationFailure.CameraNotDetected => "Camera was not detected.\nPlease check that the camera is connected and try again.",
                ValidationFailure.OcrCalibrationNotFound => $"Barcode scanner was not detected.\nPlease check that the scanner is connected and try again.",
                _ => $"Validation failure of type {FailureType} occurred for sequence {SequenceNo}."
            };
        }
        private string AppendBarcodeDetails()
        {
            // No barcode detected at all
            if (string.IsNullOrWhiteSpace(BarcodeResult))
            {
                return " (No barcode was detected by the scanner).";
            }

            // Barcode was read but mismatched
            return $" (Scanned value = '{BarcodeResult}').";
        }
        private string AppendOcrDetails()
        {
            // OCR read nothing
            if (string.IsNullOrWhiteSpace(OcrResult))
            {
                return " (No text was detected from the image).";
            }

            // OCR returned something (actual mismatch case)
            return $" (Detected text = '{OcrResult}').";
        }
        private string FormatPairText()
        {
            if (TotalPairs <= 1)
                return string.Empty;

            return $" ({ToOrdinal(PairNumber)} label of paired set)";
        }
        private static string ToOrdinal(int number)
        {
            return (number % 100) switch
            {
                11 or 12 or 13 => $"{number}th",
                _ => (number % 10) switch
                {
                    1 => $"{number}st",
                    2 => $"{number}nd",
                    3 => $"{number}rd",
                    _ => $"{number}th"
                }
            };
        }
    }
}
