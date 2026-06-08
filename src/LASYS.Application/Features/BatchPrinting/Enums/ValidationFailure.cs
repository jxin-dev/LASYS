namespace LASYS.Application.Features.BatchPrinting.Enums
{
    public enum ValidationFailure
    {
        SaveFailed,
        FileGenerationFailed,
        PrinterUnavailable,
        BarcodeMismatch,
        OcrUnreadable,
        OcrMismatch
    }
}
