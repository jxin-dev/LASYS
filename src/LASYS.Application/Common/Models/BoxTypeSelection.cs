using LASYS.Application.Common.Enums;

namespace LASYS.Application.Common.Models
{
    /// <summary>
    /// Carries all state determined during a barcode scan so that the UI can
    /// decide which printing path to follow.
    /// </summary>
    public class BoxTypeSelection
    {
        public string ItemCode { get; set; } = string.Empty;
        public string LotNo { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public BoxType BoxType { get; set; } = BoxType.NotSet;
        public int PrintType { get; set; } = 0;

        // Flags – existence of prior print records
        public bool IsCaseExists { get; set; }
        public bool IsUbExists { get; set; }
        public bool IsOubExists { get; set; }
        public bool IsOcbExists { get; set; }
        public bool IsCbExists { get; set; }
        public bool IsAubExists { get; set; }
        public bool IsAcbExists { get; set; }

        // Flags – instruction status
        public bool IsBarcodeUpdated { get; set; }
        public bool IsLabelPrinted { get; set; }
        public bool IsPaired { get; set; }
        public bool IsScrapped { get; set; }

        // Print-type decision flags
        public bool IsExcess { get; set; }
        public bool IsAdditional { get; set; }
        public bool IsReplacement { get; set; }
        public bool IsReturned { get; set; }
    }
}
