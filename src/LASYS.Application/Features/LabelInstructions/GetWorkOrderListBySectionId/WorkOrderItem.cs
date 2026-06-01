using LASYS.Application.Common.Enums;

namespace LASYS.Application.Features.LabelInstructions.GetWorkOrderListBySectionId
{
    public sealed record WorkOrderItem
    {
        //public string AvailableBoxTypes { get; init; } = string.Empty;
        public string SectionAssignments { get; init; } = string.Empty;
        public string LineCode { get; init; } = string.Empty;
        public string ItemCode { get; init; } = string.Empty;
        public string LotNo { get; init; } = string.Empty;
        public uint LabelInsRevNumber { get; init; }
        public uint MasterLabelRevNumber { get; init; }
        public string Udi { get; init; } = string.Empty;
        public DateTime? ManufactureDate { get; init; }
        public DateTime? ExpirationDate { get; init; }
        public DateTime? ProductionDate { get; init; }
        public DateTime? SterilizationDate { get; init; }
        public int TargetProductionQuantity { get; init; }
        public IReadOnlyDictionary<BoxType, BoxLabelInstructionDetails>? Details { get; init; }
        public IReadOnlyCollection<BoxType> AvailableBoxTypes => Details?.Keys.ToList() ?? [];
    }
}
