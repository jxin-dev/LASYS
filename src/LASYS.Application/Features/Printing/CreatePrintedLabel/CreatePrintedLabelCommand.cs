using LASYS.Application.Common.Enums;
using LASYS.Application.Common.Results;
using MediatR;

namespace LASYS.Application.Features.Printing.CreatePrintedLabel
{
    public sealed record CreatePrintedLabelCommand(BoxType BoxType, LabelPrint LabelPrint) : IRequest<Result<Unit>>
    {

    }
}
