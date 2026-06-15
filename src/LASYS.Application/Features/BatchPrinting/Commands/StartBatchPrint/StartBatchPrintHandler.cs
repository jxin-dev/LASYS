using LASYS.Application.Features.BatchPrinting.Services;
using MediatR;

namespace LASYS.Application.Features.BatchPrinting.Commands.StartBatchPrint
{
    public sealed class StartBatchPrintHandler : IRequestHandler<StartBatchPrintCommand>
    {
        private readonly IBatchPrintProcessService _service;

        public StartBatchPrintHandler(IBatchPrintProcessService service)
        {
            _service = service;
        }

        public Task Handle(StartBatchPrintCommand request, CancellationToken cancellationToken)
        {
            _service.StartAsync(request.JobId, request.Quantity);
            return Task.CompletedTask;
        }
    }
}
