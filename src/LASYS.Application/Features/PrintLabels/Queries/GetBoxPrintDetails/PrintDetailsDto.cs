namespace LASYS.Application.Features.PrintLabels.Queries.GetBoxPrintDetails
{
    public sealed record PrintDetailsDto(
        string ItemCode,
        string LotNo,
        long TotalPassed,
        long TotalFailed,
        long TotalSample,
        long TotalPrinted,
        long NextSequence,
        long BatchNumber,
        long SetNumber);
}
