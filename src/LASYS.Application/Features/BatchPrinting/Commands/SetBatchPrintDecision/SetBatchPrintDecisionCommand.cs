using LASYS.Application.Features.BatchPrinting.Enums;
using MediatR;

namespace LASYS.Application.Features.BatchPrinting.Commands.SetBatchPrintDecision
{
    public sealed record SetBatchPrintDecisionCommand(StepResult Decision): IRequest;
}
