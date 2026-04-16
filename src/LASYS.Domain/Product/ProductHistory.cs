namespace LASYS.Domain.Product
{
    public class ProductHistory
    {
        public int? BarcodeType { get; set; }
        public int? ItemCode { get; set; }
        public int? MasterLabelRevisionNumber { get; set; }
        public string? ItemName { get; set; }
        public string? ItemGroupTypeCode { get; set; }
        public string? MarketCode { get; set; }
        public string? SectionAssignments { get; set; }
        public int? UbReportTypeCode { get; set; }
        public int? CbReportTypeCode { get; set; }
        public int? ItemType { get; set; }
        public string? CalendarType { get; set; }
        public string? Description { get; set; }
        public string? TipType { get; set; }
        public string? WithCoc { get; set; }
        public string? WithSterilization { get; set; }
        public int? DaysBeforeExpiration { get; set; }
        public int? DaysBfrXprtnFrBrcd { get; set; }
        public int? DaysBeforeSterilization { get; set; }
        public string? PairedCbBoxType { get; set; }
        public bool? PrintUbInstructionFlag { get; set; }
        public bool? PrintCbInstructionFlag { get; set; }
        public bool? PrintCaseLabelFlag { get; set; }
        public bool? PrintQcSampleFlag { get; set; }
        public string? BarcodeCategory { get; set; }
        public int? UbQuantity { get; set; }
        public int? CbQuantity { get; set; }
        public int? UbPerCbQuantity { get; set; }
        public int? CbPerPallete { get; set; }
        public int? QcSampleQuantity { get; set; }
        public int? SbQuantity { get; set; }
        public string? UbBarcodeNumber { get; set; }
        public string? CbBarcodeNumber { get; set; }
        public string? PqeControlNumberCode { get; set; }
        public string? DvrNumber { get; set; }
        public string? DepkesNumber { get; set; }
        public string? CeMark { get; set; }
        public string? SrType { get; set; }
        public string? Gauge { get; set; }
        public string? NeedleGauge { get; set; }
        public string? NeedleSize { get; set; }
        public string? TransferInstruction { get; set; }
        public string? PssIdNo { get; set; }
        public string? FlowRate { get; set; }
        public string? Description1 { get; set; }
        public string? Description2 { get; set; }
        public bool? LabelFlag { get; set; }
        public bool? LabelTypeUbFlag { get; set; }
        public bool? LabelTypeCbFlag { get; set; }
        public bool? LabelTypeCaseFlag { get; set; }
        public decimal? LabelWidth { get; set; }
        public decimal? LabelHeight { get; set; }
        public bool? CombinedBoxTypeFlag { get; set; }
        public bool? ActiveFlag { get; set; }
        public string? Custom1 { get; set; }
        public string? Custom2 { get; set; }
        public string? Custom3 { get; set; }
        public string? Custom4 { get; set; }
        public string? Custom5 { get; set; }
        public string? Custom6 { get; set; }
        public string? Custom7 { get; set; }
        public string? Custom8 { get; set; }
        public string? Custom9 { get; set; }
        public string? Custom10 { get; set; }

        public string? CreatedUserCode { get; set; }
        public int? CreatedSectionId { get; set; }
        public string? CreatedIpAddress { get; set; }
        public DateTime? CreatedDatetime { get; set; }

        public string? LastUpdateUserCode { get; set; }
        public int? LastUpdateSectionId { get; set; }
        public string? LastUpdateIpAddress { get; set; }
        public DateTime? LastUpdateDatetime { get; set; }

        public int? OubReportTypeCode { get; set; }
        public bool? PrintOubInstructionFlag { get; set; }
        public int? OubQuantity { get; set; }
        public string? OubBarcodeNumber { get; set; }
        public bool? LabelTypeOubFlag { get; set; }

        public int? OcbReportTypeCode { get; set; }
        public bool? PrintOcbInstructionFlag { get; set; }
        public int? OcbQuantity { get; set; }
        public string? OcbBarcodeNumber { get; set; }
        public bool? LabelTypeOcbFlag { get; set; }

        public int? AubReportTypeCode { get; set; }
        public bool? PrintAubInstructionFlag { get; set; }
        public int? AubQuantity { get; set; }
        public string? AubBarcodeNumber { get; set; }
        public bool? LabelTypeAubFlag { get; set; }

        public int? AcbReportTypeCode { get; set; }
        public bool? PrintAcbInstructionFlag { get; set; }
        public int? AcbQuantity { get; set; }
        public string? AcbBarcodeNumber { get; set; }
        public bool? LabelTypeAcbFlag { get; set; }

        public bool? LabelTypeEuMdrFlag { get; set; }
    }
}
