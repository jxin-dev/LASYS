using LASYS.Application.Common.Enums;
using LASYS.Application.Events;
using LASYS.Application.Features.LabelProcessing.Abstractions;
using LASYS.DesktopApp.Views.Forms;
using LASYS.DesktopApp.Views.Interfaces;

namespace LASYS.DesktopApp.Presenters
{
    public class ErrorPresenter
    {
        public ErrorForm View { get; }
        private readonly IErrorView _view;
        private readonly ILabelProcessingService _labelProcessingService;

        public ErrorPresenter(IErrorView view, ILabelProcessingService labelProcessingService)
        {
            _view = view;
            View = (ErrorForm)view;

            _view.DecisionRequested += OnDecisionRequested;
            _labelProcessingService = labelProcessingService;
        }

        public string GetErrorMessage(OperatorDecisionRequiredEventArgs e)
        {
            return BuildMessage(e);
        }
        private string BuildMessage(OperatorDecisionRequiredEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.CustomMessage))
                return e.CustomMessage;

            return e.FailureType switch
            {
                ValidationFailure.BarcodeMismatch =>
                    $"Barcode mismatch.\n\nSequence: {e.SequenceNo}\nScanned: {e.BarcodeResult}",

                ValidationFailure.OcrUnreadable =>
                    $"OCR could not read the label.\n\nSequence: {e.SequenceNo}",

                ValidationFailure.OcrMismatch =>
                    $"OCR mismatch.\n\nSequence: {e.SequenceNo}\nDetected: {e.OcrResult}",

                ValidationFailure.PrinterUnavailable =>
                    "Printer is not available.",

                _ => "A validation error occurred."
            };
        }
        private void OnDecisionRequested(object? sender, OperatorDecision e)
        {
            _labelProcessingService.SetUserDecision(e);
            _view.CloseError();
        }
    }
}
