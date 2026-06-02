using LASYS.Application.Features.BatchPrinting.Commands.SetBatchPrintDecision;
using LASYS.Application.Features.BatchPrinting.Enums;
using LASYS.Application.Features.BatchPrinting.Events;
using LASYS.DesktopApp.Views.Forms;
using LASYS.DesktopApp.Views.Interfaces;
using MediatR;

namespace LASYS.DesktopApp.Presenters
{
    public class ErrorPresenter
    {
        public ErrorForm View { get; }
        private readonly IErrorView _view;
        private readonly IMediator _mediator;

        public ErrorPresenter(IErrorView view, IMediator mediator)
        {
            _view = view;
            _mediator = mediator;

            View = (ErrorForm)view;

            _view.DecisionRequested += OnDecisionRequested;
        }

        private async void OnDecisionRequested(object? sender, StepResult e)
        {
            await _mediator.Send(new SetBatchPrintDecisionCommand(e));
            _view.CloseError();
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
     
    }
}
