using LASYS.Application.Interfaces;

namespace LASYS.Application.Services
{
    public class PrintingService : IPrintingService
    {
        public BatchDto CurrentBatch => throw new NotImplementedException();

        public Task<bool> CaptureAndValidateOcrAsync(LabelDto label)
        {
            throw new NotImplementedException();
        }

        public Task PrintLabelAsync(LabelDto label)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ScanBarcodeAsync(LabelDto label)
        {
            throw new NotImplementedException();
        }

        public void SetDecision(UserDecision decision)
        {
            throw new NotImplementedException();
        }

        public Task<UserDecision> WaitForUserDecisionAsync()
        {
            throw new NotImplementedException();
        }
    }
}
