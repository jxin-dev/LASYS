using LASYS.Application.Common.Enums;

namespace LASYS.Application.Features.BatchPrinting.Models
{
    public sealed record SequenceData
    {

        public SequenceData(
          BoxType boxType,
          string printerName,
          string itemCode,
          string lotNo,
          long sequenceNumber,
          string pairedType,
          long batchNumber,
          long setNumber,
          string labelStatus,
          string approvedByUserCode,
          string approvedBySectionId,
          string approvedByIpAddress,
          string approvedByDateTime)
        {
            BoxType = boxType;
            ItemCode = itemCode;
            LotNo = lotNo;
            SequenceNumber = sequenceNumber;
            PairedType = pairedType;
            PrinterName = printerName;
            BatchNumber = batchNumber;
            SetNumber = setNumber;
            LabelStatus = labelStatus;
            ApprovedByUserCode = approvedByUserCode;
            ApprovedBySectionId = approvedBySectionId;
            ApprovedByIpAddress = approvedByIpAddress;
            ApprovedByDateTime = approvedByDateTime;
        }
        public SequenceData(
            BoxType boxType,
            string printerName,
            string itemCode,
            string lotNo,
            long sequenceNumber,
            string pairedType,
            long batchNumber,
            long setNumber,
            string labelStatus,
            string? analyzerResultStatus,
            string? verifierResultStatus,
            string? visualInspectionResultStatus,
            string? printUserCode,
            long? printDateTime,
            string? ncprControlCode,
            string? comment,
            string? approvedByUserCode,
            string? approvedBySectionId,
            string? approvedByIpAddress,
            string? approvedByDateTime,
            string? partitionDate,
            string? createdUserCode,
            string? createdSectionId,
            string? createdIpAddress,
            long? createdDateTime)
        {
            BoxType = boxType;
            ItemCode = itemCode;
            LotNo = lotNo;
            SequenceNumber = sequenceNumber;
            PairedType = pairedType;
            PrinterName = printerName;
            BatchNumber = batchNumber;
            SetNumber = setNumber;
            LabelStatus = labelStatus;
            AnalyzerResultStatus = analyzerResultStatus;
            VerifierResultStatus = verifierResultStatus;
            VisualInspectionResultStatus = visualInspectionResultStatus;
            PrintUserCode = printUserCode;
            PrintDateTime = printDateTime;
            NcprControlCode = ncprControlCode;
            Comment = comment;
            ApprovedByUserCode = approvedByUserCode;
            ApprovedBySectionId = approvedBySectionId;
            ApprovedByIpAddress = approvedByIpAddress;
            ApprovedByDateTime = approvedByDateTime;
            PartitionDate = partitionDate;
            CreatedUserCode = createdUserCode;
            CreatedSectionId = createdSectionId;
            CreatedIpAddress = createdIpAddress;
            CreatedDateTime = createdDateTime;
        }

        public BoxType BoxType { get; private set; }
        public string ItemCode { get; private set; }
        public string LotNo { get; private set; }
        public long SequenceNumber { get; private set; }
        public string PairedType { get; private set; }
        public string PrinterName { get; private set; }
        public long BatchNumber { get; private set; }
        public long SetNumber { get; private set; }
        public string LabelStatus { get; private set; }
        public string? AnalyzerResultStatus { get; private set; }
        public string? VerifierResultStatus { get; private set; }
        public string? VisualInspectionResultStatus { get; private set; }
        public string? PrintUserCode { get; private set; }
        public long? PrintDateTime { get; private set; }
        public string? NcprControlCode { get; private set; }
        public string? Comment { get; private set; }
        public string? ApprovedByUserCode { get; private set; }
        public string? ApprovedBySectionId { get; private set; }
        public string? ApprovedByIpAddress { get; private set; }
        public string? ApprovedByDateTime { get; private set; }
        public string? PartitionDate { get; private set; }
        public string? CreatedUserCode { get; private set; }
        public string? CreatedSectionId { get; private set; }
        public string? CreatedIpAddress { get; private set; }
        public long? CreatedDateTime { get; private set; }

    }
}
