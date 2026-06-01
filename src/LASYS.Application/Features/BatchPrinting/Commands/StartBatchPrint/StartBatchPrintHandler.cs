using LASYS.Application.Features.BatchPrinting.Services;
using MediatR;

namespace LASYS.Application.Features.BatchPrinting.Commands.StartBatchPrint
{
    public sealed class StartBatchPrintHandler : IRequestHandler<StartBatchPrintCommand, Guid>
    {
        private readonly IBatchPrintProcessService _service;

        public StartBatchPrintHandler(IBatchPrintProcessService service)
        {
            _service = service;
        }

        public Task<Guid> Handle(StartBatchPrintCommand request, CancellationToken cancellationToken)
        {
            return _service.StartAsync(request.LabelPrintingContext, request.Quantity);
        }
    }
}
