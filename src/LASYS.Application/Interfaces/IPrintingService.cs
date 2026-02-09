namespace LASYS.Application.Interfaces
{
    public enum UserDecision { Retry, Skip, StopBatch }
    public interface IPrintingService
    {
        Task PrintLabelAsync(LabelDto label);
        Task<bool> ScanBarcodeAsync(LabelDto label);
        Task<bool> CaptureAndValidateOcrAsync(LabelDto label);

        Task<UserDecision> WaitForUserDecisionAsync();
        void SetDecision(UserDecision decision);

        BatchDto CurrentBatch { get; }
    }

    public record LabelDto();
    public record BatchDto();

}
