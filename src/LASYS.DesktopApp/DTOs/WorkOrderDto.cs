namespace LASYS.DesktopApp.DTOs
{
    public record WorkOrderDto
    {
        public LabelInstruction LabelInstruction { get; init; } = new();
        public BarcodeLabel BarcodeLabel { get; init; } = new();
        public BatchInformation BatchInformation { get; init; } = new();
        public PrintingResultInformation PrintingResultInformation { get; init; } = new();
    }
    public record LabelInstruction(string InstructionCode = "Loading...",
                                   string ItemCode = "Loading...",
                                   DateTime? ExpiryDate = null,
                                   string LotNo = "Loading...",
                                   string LabelFile = "Loading...");
    public record BarcodeLabel(int Quantity = 0, int StartSequence = 1);
    public record BatchInformation(bool IsEndOfBatch = false, int BatchNumber = 0, int SetNumber = 0);
    public record PrintingResultInformation(int TargetQuantity = 0, int Remaining = 0, int LabelSample = 0, int TotalPrinted = 0, int TotalPassed = 0, int TotalFailed = 0);


}
