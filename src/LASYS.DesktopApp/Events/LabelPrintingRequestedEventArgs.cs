namespace LASYS.DesktopApp.Events
{
    public class LabelPrintingRequestedEventArgs : EventArgs
    {
        public int WorkOrderId { get; }
        public string? ItemCode { get; }
        public string? LotNo { get; }
        public string? PrintType { get; }
        public string? InstructionCode { get; }
        public LASYS.Application.Common.Enums.BoxType? LabelType { get; }

        public string? UbLiCode { get; }
        public string? AubLiCode { get; }
        public string? OubLiCode { get; }
        public string? CbLiCode { get; }
        public string? AcbLiCode { get; }
        public string? OcbLiCode { get; }

        public LabelPrintingRequestedEventArgs(int workOrderId)
        {
            WorkOrderId = workOrderId;
        }

        public LabelPrintingRequestedEventArgs(string itemCode,
                                               string lotNo,
                                               string instructionCode,
                                               LASYS.Application.Common.Enums.BoxType labelType,
                                               string? printType = null,
                                               string? ubLiCode = null,
                                               string? aubLiCode = null,
                                               string? oubLiCode = null,
                                               string? cbLiCode = null,
                                               string? acbLiCode = null,
                                               string? ocbLiCode = null)
        {
            ItemCode = itemCode;
            LotNo = lotNo;
            InstructionCode = instructionCode;
            LabelType = labelType;
            PrintType = printType;
            UbLiCode = ubLiCode;
            AubLiCode = aubLiCode;
            OubLiCode = oubLiCode;
            CbLiCode = cbLiCode;
            AcbLiCode = acbLiCode;
            OcbLiCode = ocbLiCode;
        }
    }
}
