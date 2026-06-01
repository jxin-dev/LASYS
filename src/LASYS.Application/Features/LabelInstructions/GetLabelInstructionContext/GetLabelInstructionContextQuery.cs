using LASYS.Application.Common.Enums;
using LASYS.Application.Common.Results;
using MediatR;

namespace LASYS.Application.Features.LabelInstructions.GetLabelInstructionContext
{
    public sealed record GetLabelInstructionContextQuery(string ItemCode, string LotNo, uint MasterRevision, BoxType BoxType) 
        : IRequest<Result<LabelPrintingContext>>;
}
