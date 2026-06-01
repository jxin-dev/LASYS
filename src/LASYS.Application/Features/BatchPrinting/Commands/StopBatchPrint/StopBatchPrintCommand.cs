using MediatR;

namespace LASYS.Application.Features.BatchPrinting.Commands.StopBatchPrint
{
    public sealed record StopBatchPrintCommand(Guid JobId) : IRequest;
}
