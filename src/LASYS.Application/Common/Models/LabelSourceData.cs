namespace LASYS.Application.Common.Models
{
    public sealed record LabelSourceData(
        string NiceLabelTemplatePath, LabelData LabelData, PrintData Print);


    public sealed record LabelData(
        string InstructionCode,
        string ItemCode,
        string LotNoWithAssemblyLine,
        string BarcodeNo,
        string Gauge,
        DateTime? ManufactureDate,
        DateTime ExpiryDate);

    public sealed record PrintData(
        long SetNumber,
        long BatchNumber,
        long StartSequenceNumber,
        int QuantityToPrint);
}
