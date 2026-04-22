using LASYS.Application.Common.Results;
using MediatR;

namespace LASYS.Application.Features.OCRCalibration.GetOcrLabelFilePath
{
    public record GetOcrLabelFilePathQuery(string ItemCode, uint RevisionNumber, string BoxType) : IRequest<Result<string?>>;
}
