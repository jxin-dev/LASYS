using LASYS.Application.Features.BatchPrinting.Models;
using LASYS.Application.Features.LabelInstructions.GetLabelInstructionContext;
using MediatR;

namespace LASYS.Application.Features.BatchPrinting.Commands.InitializeBatchPrint
{
    public sealed record InitializeBatchPrintCommand(LabelPrintingContext LabelPrintingContext) : IRequest<PrintJobState>;
}
