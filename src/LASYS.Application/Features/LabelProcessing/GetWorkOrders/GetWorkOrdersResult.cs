namespace LASYS.Application.Features.LabelProcessing.GetWorkOrders
{
    public class GetWorkOrdersResult
    {
        public string? ItemCode { get; set; }
        public string? LotNo { get; set; }
        public string? ExpDate { get; set; }
        public string? PrintType { get; set; }
        public string? Verdict { get; set; }
        public string? DateApproved { get; set; }
        public int ProdQty { get; set; }
        public int MasterLabelRevisionNo { get; set; }
        public int LabelInsRevisionNo { get; set; }
        public string? UB_LI_Code { get; set; }
        public int UB_Qty { get; set; }
        public string? UB_LI_Status { get; set; }
        public string? AUB_LI_Code { get; set; }
        public int AUB_Qty { get; set; }
        public string? AUB_LI_Status { get; set; }
        public string? OUB_LI_Code { get; set; }
        public int OUB_Qty { get; set; }
        public string? OUB_LI_Status { get; set; }
        public string? CB_LI_Code { get; set; }
        public int CB_Qty { get; set; }
        public string? CB_LI_Status { get; set; }
        public string? ACB_LI_Code { get; set; }
        public int ACB_Qty { get; set; }
        public string? ACB_LI_Status { get; set; }
        public string? OCB_LI_Code { get; set; }
        public int OCB_Qty { get; set; }
        public string? OCB_LI_Status { get; set; }
        public bool CASE_OCR { get; set; }
        public bool UB_OCR { get; set; }
        public bool OUB_OCR { get; set; }
        public bool CB_OCR { get; set; }
        public bool OCB_OCR { get; set; }
        public bool AUB_OCR { get; set; }
        public bool ACB_OCR { get; set; }
    }
}
