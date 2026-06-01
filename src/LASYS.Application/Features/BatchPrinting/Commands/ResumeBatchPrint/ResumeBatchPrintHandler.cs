using LASYS.Application.Features.BatchPrinting.Services;
using MediatR;

namespace LASYS.Application.Features.BatchPrinting.Commands.ResumeBatchPrint
{
    public sealed class ResumeBatchPrintHandler : IRequestHandler<ResumeBatchPrintCommand>
    {
        private readonly IBatchPrintProcessService _service;
        public ResumeBatchPrintHandler(IBatchPrintProcessService service)
        {
            _service = service;
        }
        public Task Handle(ResumeBatchPrintCommand request, CancellationToken cancellationToken)
        {
            _service.Resume(request.JobId);
            return Task.CompletedTask;
        }
    }
}
