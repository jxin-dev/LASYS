namespace LASYS.Application.Common.Enums
{
    /// <summary>Print-type / label-status codes (Original, Additional, etc.).</summary>
    public enum LabelPrintType
    {
        NotSet = 0,
        Original = 1,
        Excess = 2,
        Additional = 3,
        Replacement = 4,
        Returned = 5,
        QC = 6,
        COC = 7,
        FailedDuringPrinting = 99,
    }
}
