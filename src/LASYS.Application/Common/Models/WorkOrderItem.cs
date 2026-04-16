namespace LASYS.Application.Common.Models
{
    /// <summary>Represents a single work-order row shown in the listing grid.</summary>
    public class WorkOrderItem
    {
        public string? ItemCode { get; set; }
        public string? LotNo { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int ProdQty { get; set; }

        // UB columns
        public string? UbLiCode { get; set; }
        public int UbQty { get; set; }
        public string? UbPrintType { get; set; }
        public string? UbVerdict { get; set; }
        public string? UbLiStatus { get; set; }
        public string? UbLabelStatus { get; set; }

        // CB columns
        public string? CbLiCode { get; set; }
        public int CbQty { get; set; }
        public string? CbPrintType { get; set; }
        public string? CbVerdict { get; set; }
        public string? CbLiStatus { get; set; }
        public string? CbLabelStatus { get; set; }
    }
}
