using LASYS.Application.Common.Results;
using MediatR;

namespace LASYS.Application.Features.OCRCalibration.PrintLabel
{
    public record PrintLabelCommand(string ItemCode, int RevisionNumber, string BoxType, string FilePath) : IRequest<Result<Unit>>;
}
