using LASYS.Application.Features.BatchPrinting.Services;
using MediatR;

namespace LASYS.Application.Features.BatchPrinting.Commands.SetBatchPrintDecision
{
    public sealed class SetBatchPrintDecisionHandler : IRequestHandler<SetBatchPrintDecisionCommand>
    {
        private readonly IBatchPrintProcessService _service;

        public SetBatchPrintDecisionHandler(IBatchPrintProcessService service)
        {
            _service = service;
        }

        public Task Handle(SetBatchPrintDecisionCommand request, CancellationToken cancellationToken)
        {
            _service.SetUserDecision(request.Decision);
            return Task.CompletedTask;
        }
    }
}
