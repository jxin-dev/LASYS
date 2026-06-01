using LASYS.Application.Common.Models;
using LASYS.Application.Features.LabelInstructions.GetLabelInstructionContext;
using MediatR;

namespace LASYS.Application.Features.BatchPrinting.Commands.StartBatchPrint
{
    public sealed record StartBatchPrintCommand(LabelPrintingContext LabelPrintingContext, int Quantity) : IRequest<Guid>;
    
}
