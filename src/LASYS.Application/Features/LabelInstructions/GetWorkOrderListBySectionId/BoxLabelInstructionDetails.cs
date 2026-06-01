namespace LASYS.Application.Features.LabelInstructions.GetWorkOrderListBySectionId
{
    public sealed record BoxLabelInstructionDetails
    {
        public string? InstructionCode { get; init; }
        public uint? TargetPrintQuantity { get; init; }
        public string? PrintType { get; init; }
        public string? Verdict { get; init; }
        public string? InstructionStatus { get; init; }
        public string? LabelStatus { get; init; }
        public string? ApprovedBy { get; init; }
        public DateTime? DateApproved { get; init; }
    }
}
