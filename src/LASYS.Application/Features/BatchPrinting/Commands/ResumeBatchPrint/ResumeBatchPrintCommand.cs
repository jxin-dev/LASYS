using MediatR;

namespace LASYS.Application.Features.BatchPrinting.Commands.ResumeBatchPrint
{
    public sealed record ResumeBatchPrintCommand(Guid JobId) : IRequest;
}
