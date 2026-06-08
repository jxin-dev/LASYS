namespace LASYS.Application.Features.Products
{
    public sealed record ProductDetails
    {
        public string ItemName { get; private set; } = string.Empty;
        public string ItemGroupTypeCode { get; private set; } = string.Empty;
        public string MarketCode { get; private set; } = string.Empty;
        public string SectionAssignments { get; private set; } = string.Empty;
        public string ItemType { get; private set; } = string.Empty;
        public string CalendarType { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public string TipType { get; private set; } = string.Empty;
        public string WithCoc { get; private set; } = string.Empty;
        public bool IsPairedBoxType { get; private set; } = false;
        public string WithSterilization { get; private set; } = string.Empty;
        public string BarcodeType { get; private set; } = string.Empty;
        public string BarcodeCategory { get; private set; } = string.Empty;
        public uint UbPerCbQuantity { get; private set; }
        public uint CbPerPallete { get; private set; }
        public uint QcSampleQuantity { get; private set; }
        public uint SbQuantity { get; private set; }
        public string ReportType { get; private set; } = string.Empty;
        public string BarcodeNumber { get; private set; } = string.Empty;
        public uint Quantity { get; private set; }
        public bool IsEumdr { get; private set; }
        public string PqeControlNumberCode { get; private set; } = string.Empty;
        public string DvrNumber { get; private set; } = string.Empty;
        public string DepkesNumber { get; private set; } = string.Empty;
        public string CeMark { get; private set; } = string.Empty;
        public string SrType { get; private set; } = string.Empty;
        public string Gauge { get; private set; } = string.Empty;
        public string NeedleGauge { get; private set; } = string.Empty;
        public string NeedleSize { get; private set; } = string.Empty;
        public string TransferInstruction { get; private set; } = string.Empty;
        public string PssIdNo { get; private set; } = string.Empty;
        public string FlowRate { get; private set; } = string.Empty;
        public string? Description1 { get; private set; }
        public string? Description2 { get; private set; }
        public string? Custom1 { get; private set; }
        public string? Custom2 { get; private set; }
        public string? Custom3 { get; private set; }
        public string? Custom4 { get; private set; }
        public string? Custom5 { get; private set; }
        public string? Custom6 { get; private set; }
        public string? Custom7 { get; private set; }
        public string? Custom8 { get; private set; }
        public string? Custom9 { get; private set; }
        public string? Custom10 { get; private set; }
    }
}
