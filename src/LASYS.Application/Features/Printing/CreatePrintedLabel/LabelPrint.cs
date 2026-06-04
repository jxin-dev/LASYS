namespace LASYS.Application.Features.Printing.CreatePrintedLabel
{
    public sealed record LabelPrint
    {
        public required string ItemCode { get; set; }
        public required string LotNo { get; set; }
        public required int SequenceNumber { get; set; }
        public required string PairedType { get; set; } //Single, Paired1, Paired2
        public string? PrinterName { get; set; }

        public string? BatchNumber { get; set; }
        public int? SetNumber { get; set; }

        public string? LabelStatus { get; set; }

        public string? AnalyzerResultStatus { get; set; }
        public string? VerifierResultStatus { get; set; }
        public string? VisualInsResultStatus { get; set; }

        public string? PrintUserCode { get; set; }
        public DateTime? PrintDatetime { get; set; }

        public string? NcprControlCode { get; set; }
        public string? Comment { get; set; }

        public string? ApprovedByUserCode { get; set; }
        public string? ApprovedBySectionId { get; set; }
        public string? ApprovedByIpAddress { get; set; }
        public DateTime? ApprovedByDatetime { get; set; }

        public DateTime? PartitionDate { get; set; }

        public string? CreatedUserCode { get; set; }
        public string? CreatedSectionId { get; set; }
        public string? CreatedIpAddress { get; set; }
        public DateTime? CreatedDatetime { get; set; }

        public string? LastUpdateUserCode { get; set; }
        public string? LastUpdateSectionId { get; set; }
        public string? LastUpdateIpAddress { get; set; }
        public DateTime? LastUpdateDatetime { get; set; }
    }
}
