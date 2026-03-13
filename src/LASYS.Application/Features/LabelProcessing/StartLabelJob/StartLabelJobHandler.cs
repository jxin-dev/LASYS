using LASYS.Application.Features.LabelProcessing.Abstractions;
using LASYS.Application.Features.LabelProcessing.Contracts;
using MediatR;

namespace LASYS.Application.Features.LabelProcessing.StartLabelJob
{
    public class StartLabelJobHandler : IRequestHandler<StartLabelJobCommand>
    {
        private readonly ILabelProcessingService _labelProcessingService;

        public StartLabelJobHandler(ILabelProcessingService labelProcessingService)
        {
            _labelProcessingService = labelProcessingService;
        }

        public async Task Handle(StartLabelJobCommand request, CancellationToken cancellationToken)
        {
            var startRequest = new StartLabelJobRequest
            {
                ItemCode = request.ItemCode,
                StartSequence = request.StartSequence,
                Quantity = request.Quantity,
                LabelData = request.LabelData,
                SequenceVariableName = request.SequenceVariableName,
                BarcodeVariableName = request.BarcodeVariableName
            };

            await _labelProcessingService.StartJobAsync(request.ViewerSize, startRequest);
        }
    }
}
