namespace LASYS.Application.Features.OCRCalibration.GetOcrSupportedItems
{
    public record OcrSupportedItemDto(string ItemCode, uint RevisionNumber, string BoxType, string FilePath);
}
