namespace LASYS.Application.Common.Models
{
    public sealed record LabelInstructionDetails
    {
        public string InstructionCode { get; set; } = string.Empty;
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
        public string PrintType { get; init; } = string.Empty;
        public string ApprovedByUserCode { get; init; } = string.Empty;
        public string ApprovedBySectionId { get; init; } = string.Empty;
        public string ApprovedByIpAddress { get; init; } = string.Empty;
        public string ApprovedByDateTime { get; init; } = string.Empty;



    }
}
