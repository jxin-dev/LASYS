using MediatR;

namespace LASYS.Application.Features.BatchPrinting.Commands.StartBatchPrint
{
    public sealed record StartBatchPrintCommand(Guid JobId, int Quantity) : IRequest;
    
}
