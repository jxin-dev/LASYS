namespace LASYS.Application.Common.Models
{
    /// <summary>
    /// Parsed fields extracted from a GS1-128 instruction barcode.
    /// Mirrors GS1_128InstructionBarcode.vb.
    /// </summary>
    public class Gs1BarcodeData
    {
        public int Packaging { get; set; }   // box-type AI
        public string LotNumber { get; set; } = string.Empty;
        public string AssemblyLineSuffix { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public string RawBarcode { get; set; } = string.Empty;
    }
}
