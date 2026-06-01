namespace LASYS.Application.Features.BatchPrinting.Enums
{
    public enum LabelSequenceStatus
    {
        First,
        Original,
        Last,

        Replacement,
        Additional,
        Returned,
        Qc,
        FailedDuringPrinting,
        FailedAfterPrinting,
        Excess,
        Scrapped,
        Coc
    }
}
