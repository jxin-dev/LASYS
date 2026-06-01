using MediatR;

namespace LASYS.Application.Features.BatchPrinting.Commands.PauseBatchPrint
{
    public sealed record PauseBatchPrintCommand(Guid JobId) : IRequest;
}
