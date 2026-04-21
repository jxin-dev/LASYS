// =============================================================================
// LASYS – Label Printing (C# Port)
// File   : Models.cs
// Purpose: All domain models / DTOs used across the label-printing flow.
//          Mirrors BoxTypeSelectionBO.vb, LabelStatusObj.vb, and the
//          relevant portions of LabelPrinterDB.vb.
// =============================================================================

using System;
using System.Data;

namespace LASYS.LabelPrinting
{
    // ─────────────────────────────────────────────
    //  Enumerations (from Define namespaces in VB)
    // ─────────────────────────────────────────────

    /// <summary>Packing / box type codes embedded in the GS1-128 barcode.</summary>
    public enum BoxType
    {
        NotSet          = -1,
        UnitBox         = 1,    // UB
        CartonBox       = 2,    // CB
        OuterUnitBox    = 3,    // OUB
        AdditionalUnitBox   = 4,  // AUB
        AdditionalCartonBox = 5,  // ACB
        OuterCartonBox  = 6,    // OCB
        McKessonUB      = 7,
        CaseLabel       = 8,
        QC              = 9,
        AubQC           = 10,
        OubQC           = 11,
        COC             = 12,
    }

    /// <summary>Print-type / label-status codes (Original, Additional, etc.).</summary>
    public enum LabelPrintType
    {
        NotSet      = 0,
        Original    = 1,
        Excess      = 2,
        Additional  = 3,
        Replacement = 4,
        Returned    = 5,
        QC          = 6,
        COC         = 7,
        FailedDuringPrinting = 99,
    }

    // ─────────────────────────────────────────────
    //  BoxTypeSelectionBO  (mirrors BoxTypeSelectionBO.vb)
    // ─────────────────────────────────────────────

    /// <summary>
    /// Carries all state determined during a barcode scan so that the UI can
    /// decide which printing path to follow.
    /// </summary>
    public class BoxTypeSelection
    {
        public string   ItemCode        { get; set; } = string.Empty;
        public string   LotNo           { get; set; } = string.Empty;
        public string   Barcode         { get; set; } = string.Empty;
        public BoxType  BoxType         { get; set; } = BoxType.NotSet;
        public int      PrintType       { get; set; } = 0;

        // Flags – existence of prior print records
        public bool     IsCaseExists    { get; set; }
        public bool     IsUbExists      { get; set; }
        public bool     IsOubExists     { get; set; }
        public bool     IsOcbExists     { get; set; }
        public bool     IsCbExists      { get; set; }
        public bool     IsAubExists     { get; set; }
        public bool     IsAcbExists     { get; set; }

        // Flags – instruction status
        public bool     IsBarcodeUpdated { get; set; }
        public bool     IsLabelPrinted   { get; set; }
        public bool     IsPaired         { get; set; }
        public bool     IsScrapped       { get; set; }

        // Print-type decision flags
        public bool     IsExcess         { get; set; }
        public bool     IsAdditional     { get; set; }
        public bool     IsReplacement    { get; set; }
        public bool     IsReturned       { get; set; }
    }

    // ─────────────────────────────────────────────
    //  WorkOrderItem  (one row in the work-order grid)
    // ─────────────────────────────────────────────

    /// <summary>Represents a single work-order row shown in the listing grid.</summary>
    public class WorkOrderItem
    {
        public string   ItemCode        { get; set; }
        public string   LotNo           { get; set; }
        public DateTime ExpiryDate      { get; set; }
        public int      ProdQty         { get; set; }

        // UB columns
        public string   UbLiCode        { get; set; }
        public int      UbQty           { get; set; }
        public string   UbPrintType     { get; set; }
        public string   UbVerdict       { get; set; }
        public string   UbLiStatus      { get; set; }
        public string   UbLabelStatus   { get; set; }

        // CB columns
        public string   CbLiCode        { get; set; }
        public int      CbQty           { get; set; }
        public string   CbPrintType     { get; set; }
        public string   CbVerdict       { get; set; }
        public string   CbLiStatus      { get; set; }
        public string   CbLabelStatus   { get; set; }
    }

    // ─────────────────────────────────────────────
    //  PrintData  (mirrors LabelPrinterDB.vb)
    // ─────────────────────────────────────────────

    /// <summary>
    /// Container for all data required to perform a single label-print job.
    /// Built in WorkOrderListingForm.SetData() and passed to LabelPrintForm.
    /// </summary>
    public class PrintData
    {
        // Identity
        public string       ItemCode            { get; set; } = string.Empty;
        public string       LotNo               { get; set; } = string.Empty;
        public string       InstructionCode     { get; set; } = string.Empty;

        // Print configuration
        public BoxType      LabelType           { get; set; } = BoxType.NotSet;
        public LabelPrintType LabelStatus       { get; set; } = LabelPrintType.NotSet;
        public int          PrintLabelPrintType { get; set; }
        public bool         IsPaired            { get; set; }
        public bool         IsQcSample          { get; set; }

        // Quantities / sequence
        public long         ProductQuantity     { get; set; }
        public long         TotalCase           { get; set; }
        public long         TotalPassedCase     { get; set; }
        public long         TotalFailedCase     { get; set; }
        public int          TotalSampleCase     { get; set; }
        public long         LastSequence        { get; set; }
        public long         TargetQuantity      { get; set; }
        public long         TotalPassed         { get; set; }
        public long         SequenceNumber      { get; set; }

        // Dates
        public DateTime     ExpiryDate          { get; set; } = DateTime.MinValue;
        public DateTime     ProductionDate      { get; set; } = DateTime.MinValue;
        public string       MfgDate             { get; set; } = string.Empty;

        // Miscellaneous
        public string       PssNumber           { get; set; } = string.Empty;
        public int          RevisionNumber      { get; set; }
        public string       ProductBarcode      { get; set; } = string.Empty;
        public string       ApprovedUserCode    { get; set; } = string.Empty;
        public int          ApprovedSectionId   { get; set; }

        // Result rows to be saved after printing
        public DataTable    ResultData          { get; set; }
    }

    // ─────────────────────────────────────────────
    //  GS1-128 parsed barcode data
    // ─────────────────────────────────────────────

    /// <summary>
    /// Parsed fields extracted from a GS1-128 instruction barcode.
    /// Mirrors GS1_128InstructionBarcode.vb.
    /// </summary>
    public class Gs1BarcodeData
    {
        public int      Packaging           { get; set; }   // box-type AI
        public string   LotNumber           { get; set; } = string.Empty;
        public string   AssemblyLineSuffix  { get; set; } = string.Empty;
        public string   ItemCode            { get; set; } = string.Empty;
        public DateTime ExpiryDate          { get; set; }
        public string   RawBarcode          { get; set; } = string.Empty;
    }
}
