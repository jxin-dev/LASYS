namespace LASYS.Application.Features.LabelProcessing.BarcodeScan
{
    public class BarcodeScanResult
    {
        public string? ItemCodeLabel { get; set; }
        public string? LotNoLabel { get; set; }
        public int BoxType { get; set; }
        public string? Barcode { get; set; }
        public bool IsCaseExists { get; set; }
        public bool IsUbExists { get; set; }
        public bool IsAubExists { get; set; }
        public bool IsAcbExists { get; set; }
        public bool IsOubExists { get; set; }
        public bool IsOcbExists { get; set; }
        public bool IsCbExists { get; set; }
        public bool IsBarcodeUpdated { get; set; }
        public bool IsLabelPrinted { get; set; }
        public bool IsPaired { get; set; }
        public bool PrintType { get; set; }
        public bool IsScrapped { get; set; }
        public bool IsExcess { get; set; }
        public bool IsAdditional { get; set; }
        public bool IsReplacement { get; set; }
        public bool IsReturned { get; set; }
    }


}
