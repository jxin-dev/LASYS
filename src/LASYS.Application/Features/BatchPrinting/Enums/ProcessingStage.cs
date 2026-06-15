namespace LASYS.Application.Features.BatchPrinting.Enums
{
    public enum ProcessingStage
    {
        None = 0,
        Generated = 1,
        Printed = 2,
        BarcodeValidated = 3,
        OcrValidated = 4,
        Saved = 5
    }
}
