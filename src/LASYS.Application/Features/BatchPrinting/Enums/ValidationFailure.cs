namespace LASYS.Application.Features.BatchPrinting.Enums
{
    public enum ValidationFailure
    {
        FileGenerationFailed,
        PrinterUnavailable,
        BarcodeMismatch,
        OcrUnreadable,
        OcrMismatch
    }
}
