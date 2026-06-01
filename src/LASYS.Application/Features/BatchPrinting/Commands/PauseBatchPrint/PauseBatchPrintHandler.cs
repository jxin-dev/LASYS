using LASYS.Application.Features.BatchPrinting.Services;
using MediatR;

namespace LASYS.Application.Features.BatchPrinting.Commands.PauseBatchPrint
{
    public sealed class PauseBatchPrintHandler : IRequestHandler<PauseBatchPrintCommand>
    {
        private readonly IBatchPrintProcessService _service;
        public PauseBatchPrintHandler(IBatchPrintProcessService service)
        {
            _service = service;
        }
        public Task Handle(PauseBatchPrintCommand request, CancellationToken cancellationToken)
        {
            _service.Pause(request.JobId);
            return Task.CompletedTask;
        }
    }
}
