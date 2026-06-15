using LASYS.Application.Features.BatchPrinting.Models;
using LASYS.Application.Features.BatchPrinting.Services;
using MediatR;

namespace LASYS.Application.Features.BatchPrinting.Commands.InitializeBatchPrint
{
    public sealed class InitializeBatchPrintHandler : IRequestHandler<InitializeBatchPrintCommand, PrintJobState>
    {
        private readonly IBatchPrintProcessService _service;
        public InitializeBatchPrintHandler(IBatchPrintProcessService service)
        {
            _service = service;
        }

        public Task<PrintJobState> Handle(InitializeBatchPrintCommand request, CancellationToken cancellationToken)
        {
            return _service.InitializeAsync(request.LabelPrintingContext);
        }
    }
}
