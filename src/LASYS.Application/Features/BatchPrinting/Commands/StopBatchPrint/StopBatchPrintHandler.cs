using LASYS.Application.Features.BatchPrinting.Services;
using MediatR;

namespace LASYS.Application.Features.BatchPrinting.Commands.StopBatchPrint
{
    public sealed class StopBatchPrintHandler : IRequestHandler<StopBatchPrintCommand>
    {
        private readonly IBatchPrintProcessService _service;
        public StopBatchPrintHandler(IBatchPrintProcessService service)
        {
            _service = service;
        }
        public Task Handle(StopBatchPrintCommand request, CancellationToken cancellationToken)
        {
            _service.Stop(request.JobId);
            return Task.CompletedTask;
        }
    }
}
